﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Poller.Database;
using Poller.Extensions;
using Poller.Helper;
using Poller.Model.Response;
using Poller.OAuth;
using Poller.Poller;
using Poller.Taboola.Model;

namespace Poller.Taboola
{
    internal class TaboolaPoller : IPollerRefreshAdvertisementData, IPollerDataSyncback, IPollerCreateOrUpdateObjects, IDisposable
    {
        private readonly object dbLock = new object();
        private readonly ILogger _logger;
        private readonly DbProvider _provider;
        private readonly IMemoryCache _cache;
        private readonly HttpManager _client;
        private readonly TaboolaPollerOptions _options;

        public TaboolaPoller(ILogger logger, TaboolaPollerOptions options, DbProvider provider, IMemoryCache cache)
        {
            _logger = logger;
            _options = options;
            _provider = provider;
            _cache = cache;

            _client = new HttpManager(_options.BaseUrl)
            {
                TokenUri = "oauth/token",
                RefreshUri = "oauth/token",
                AuthorizationProvider = new OAuthAuthorizationProvider
                {
                    ClientId = _options.OAuth2.ClientId,
                    ClientSecret = _options.OAuth2.ClientSecret,
                    Username = _options.OAuth2.Username,
                    Password = _options.OAuth2.Password,
                }
            };
        }

        /// <summary>
        /// Run the remote query and catch all exceptions where before letting
        /// them propagate upwards.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Object of TResult.</returns>
        protected async Task<TResult> RemoteQueryAndLogAsync<TResult>(HttpMethod method, string url, CancellationToken cancellationToken = default) // TODO: remove default
            where TResult : class
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogTrace($"Executing {method} {url}");

                return await _client.RemoteQueryAsync<TResult>(method, url, cancellationToken);
            }
            catch (Exception e) when (e as OperationCanceledException == null && e as TaskCanceledException == null)
            {
                _logger.LogError($"{url}: {e.Message}");
                throw e;
            }
        }

        /// <summary>
        /// Gets the Top Campaign Reports for a specific date as specified
        /// in the Backstage documentation, deserializes them and  inserts them into the database
        /// </summary>
        private Task<AdItemList> GetTopCampaignReportAsync(string account, CancellationToken token)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["end_date"] = query["start_date"] = DateTime.Now.ToString("yyyy-MM-dd");

            var url = $"api/1.0/{account}/reports/top-campaign-content/dimensions/item_breakdown?{query}";

            return RemoteQueryAndLogAsync<AdItemList>(HttpMethod.Get, url, token);
        }

        private Task<AccountList> GetAllAccounts(CancellationToken token)
        {
            var url = $"api/1.0/users/current/allowed-accounts/";

            return RemoteQueryAndLogAsync<AccountList>(HttpMethod.Get, url, token);
        }

        private Task<CampaignList> GetAllCampaigns(string account, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns";

            return RemoteQueryAndLogAsync<CampaignList>(HttpMethod.Get, url, token);
        }

        private async Task GetCampaign(string account, string campaign, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns/{campaign}";

            var result = await RemoteQueryAndLogAsync<Campaign>(HttpMethod.Get, url, token);
        }

        private Task<Campaign> CreateCampaign(string account, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns/";

            return RemoteQueryAndLogAsync<Campaign>(HttpMethod.Post, url, token);
        }

        private async Task UpdateCampaignStatus(string account, string campaign, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns/{campaign}";

            var result = await RemoteQueryAndLogAsync<TopCampaignReport>(HttpMethod.Put, url, token);
        }

        private Task<Campaign> DeleteCampaign(string account, string campaign, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns/{campaign}";

            return RemoteQueryAndLogAsync<Campaign>(HttpMethod.Delete, url, token);
        }

        private Task<AdItemList> GetCampaignAllItems(string account, string campaign, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns/{campaign}/items";

            return RemoteQueryAndLogAsync<AdItemList>(HttpMethod.Get, url, token);
        }

        private Task<AdItem> GetCampaignItem(string account, string campaign, string item, CancellationToken token)
        {
            var url = $"api/1.0/{account}/campaigns/{campaign}/items/{item}";

            return RemoteQueryAndLogAsync<AdItem>(HttpMethod.Get, url, token);
        }

        private async Task CommitCampaignItems(AdItemList aditems, CancellationToken token)
        {
            if (aditems == null || aditems.Items.Count() <= 0) { return; }

            var sql = @"
                INSERT INTO
	                public.ad_item AS INCLUDED (secondary_id, ad_group, title, url, content, cpc, spent, clicks, impressions, actions, details, status)
                VALUES
                    (
                        @Id,
                        2,
                        LEFT(@Title, 128),
                        @Url,
                        @Content,
                        @Cpc,
                        @Spent,
                        @Clicks,
                        @Impressions,
                        @Actions,
                        NULL,
                        CAST (@StatusText AS ad_item_status)
                    )
                ON CONFLICT (secondary_id) DO UPDATE
                SET
                    title = EXCLUDED.title,
	                cpc = GREATEST(INCLUDED.cpc, EXCLUDED.cpc),
	                spent = GREATEST(INCLUDED.spent, EXCLUDED.spent),
	                clicks = GREATEST(INCLUDED.clicks, EXCLUDED.clicks),
	                impressions = GREATEST(INCLUDED.impressions, EXCLUDED.impressions),
	                actions = GREATEST(INCLUDED.actions, EXCLUDED.actions),
                    details = EXCLUDED.details,
                    status = EXCLUDED.status";

            using (var connection = _provider.ConnectionScope())
            {
                await connection.ExecuteAsync(new CommandDefinition(sql, aditems.Items, cancellationToken: token));
            }
        }

        private async Task CommitAccounts(AccountList accounts, CancellationToken token)
        {
            if (accounts == null || accounts.Items.Count() <= 0) { return; }

            foreach (var item in accounts.Items)
            {
                // FUTURE: Improve
                item.Details = Json.Serialize(new AccountDetails
                {
                    PartnerTypes = item.PartnerTypes,
                    Type = item.Type,
                    CampaignTypes = item.CampaignTypes,
                });
            }

            var sql = @"
                INSERT INTO
	                public.account(publisher, name, currency, details)
                VALUES
                    ('taboola', @AccountId, @Currency, @Details::json)
                ON CONFLICT (name) DO NOTHING";

            using (var connection = _provider.ConnectionScope())
            {
                await connection.ExecuteAsync(new CommandDefinition(sql, accounts.Items, cancellationToken: token));
            }
        }

        private async Task CommitCampaigns(CampaignList campaigns, CancellationToken token)
        {
            if (campaigns == null || campaigns.Items.Count() <= 0) { return; }

            foreach (var item in campaigns.Items)
            {
                if (item.Cpc > item.DailyCap)
                {
                    item.DailyCap = null;
                }
            }

            var item2 = campaigns.Items.First();
            item2.Delivery2 = AdDelivery.Accelerated;

            var sql = @"
                INSERT INTO
	                public.campaign(name, branding_text, location_include, language, initial_cpc, budget, budget_daily, delivery, start_date, end_date, utm, campaign_group, note, publisher_id)
                VALUES
                    (@Name, @Branding, '{0}', '{XXX}', @Cpc, @SpendingLimit, @Delivery2::delivery, @DailyCap, COALESCE(@StartDate, CURRENT_TIMESTAMP), @EndDate, @Utm, 48389, @Note, @Id)
                ON CONFLICT (publisher_id) DO NOTHING";

            using (var connection = _provider.ConnectionScope())
            {
                await connection.ExecuteAsync(new CommandDefinition(sql, item2, cancellationToken: token));
            }
        }

        private async Task<IEnumerable<Account>> FetchAdvertiserAccounts(CancellationToken token)
        {
            var sql = @"
                SELECT
                    id, publisher, name, currency
	            FROM
                    public.account
                WHERE
                    (details::json#>>'{partner_types}')::jsonb ? 'ADVERTISER'";

            using (var connection = _provider.ConnectionScope())
            {
                return await connection.QueryAsync<Account>(new CommandDefinition(sql, cancellationToken: token));
            }
        }

        private Task<IEnumerable<Account>> FetchAdvertiserAccountsForCache(CancellationToken token)
        {
            return _cache.GetOrCreateAsync("AdvertiserAccounts", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);
                return await FetchAdvertiserAccounts(token);
            });
        }

        public async Task RefreshAdvertisementDataAsync(PollerContext context, CancellationToken token)
        {
            var accounts = await FetchAdvertiserAccountsForCache(token);
            foreach (var account in accounts.ToList().Shuffle())
            {
                var result = await GetTopCampaignReportAsync(account.Name, token);
                await CommitCampaignItems(result, token);
            }

            //context.Interval = TimeSpan.FromMinutes(15);
        }

        public async Task DataSyncbackAsync(PollerContext context, CancellationToken token)
        {
            // Accounts almmost never change.
            if (context.RunCount % 4 == 0 && false)
            {
                _logger.LogInformation("Syncback account information");

                var result = await GetAllAccounts(token);
                await CommitAccounts(result, token);
            }

            var accounts = await FetchAdvertiserAccountsForCache(token);
            foreach (var account in accounts.ToList().Shuffle().Take(2))
            {
                var result = await GetAllCampaigns(account.Name, token);
                //await CommitCampaigns(result, token);

                // Reorder the items with the idea of risk spreading.
                foreach (var item in result.Items.ToList().Shuffle().Take(100))
                {
                    try
                    {
                        var result2 = await GetCampaignAllItems(account.Name, item.Id, token);
                        await CommitCampaignItems(result2, token);

                        // Prevent spamming.
                        await Task.Delay(250, token);

                        context.MarkProgress(token);
                    }
                    catch (TaskCanceledException)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        public Task CreateOrUpdateObjectsAsync(PollerContext context, CancellationToken token)
        {
            //

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}

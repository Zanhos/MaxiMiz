﻿using Poller.Helper;
using Poller.Taboola.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AccountCore = Maximiz.Model.Entity.Account;
using AccountTaboola = Poller.Taboola.Model.Account;

namespace Poller.Taboola.Mapper
{

    /// <summary>
    /// Implements our extensions to convert
    /// accounts.
    /// </summary>
    internal static class HardCodedMapperAccount
    {

        private static readonly string DefaultAccountId = "invalid account id";
        private static readonly string DefaultPublisher = "taboola";
        private static readonly string DefaultName = "default name";
        private static readonly string DefaultCurrency = "invalid";

        /// <summary>
        /// Convert taboola account to core model.
        /// </summary>
        /// <param name="mapper">Base object</param>
        /// <param name="account">The object to convert</param>
        /// <returns>The converted object</returns>
        public static AccountCore TaboolaToCore(
            this HardCodedMapper mapper, AccountTaboola account)
        {
            string details = Json.Serialize(new AccountDetails
            {
                Id = account.Id,
                PartnerTypes = account.PartnerTypes?.Select(
                    s => s.ToLowerInvariant()).ToArray(),
                Type = account.Type.ToLower(),
                CampaignTypes = account.CampaignTypes?.Select(
                    s => s.ToLowerInvariant()).ToArray(),
            });

            return new AccountCore
            {
                SecondaryId = account.AccountId ?? DefaultAccountId,
                Publisher = DefaultPublisher,
                Name = account.Name ?? DefaultName,
                Currency = account.Currency ?? DefaultCurrency,
                Details = details
            };
        }

        /// <summary>
        /// Convert core model to taboola account.
        /// </summary>
        /// <param name="mapper">Base object</param>
        /// <param name="account">The object to convert</param>
        /// <returns>The converted object</returns>
        public static AccountTaboola CoreToTaboola(
            this HardCodedMapper mapper, AccountCore account)
        {
            AccountDetails details = Json.Deserialize
                <AccountDetails>(account.Details);

            return new AccountTaboola
            {
                Name = account.Name ?? DefaultName,
                AccountId = account.SecondaryId ?? DefaultAccountId,
                PartnerTypes = details.PartnerTypes,
                Type = details.Type,
                Currency = account.Currency ?? DefaultCurrency,
                CampaignTypes = details.CampaignTypes
            };
        }

    }
}

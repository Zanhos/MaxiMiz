using Poller.Helper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Poller.OAuth
{

    /// <summary>
    /// This actually sends our requests with integrated
    /// authorization. All requests will be sent to the 
    /// base url, which is set in the <see cref="Uris"/>.
    /// </summary>
    internal class OAuthHttpClient : HttpClient
    {

        /// <summary>
        /// The authorization ticket containing our tokens.
        /// </summary>
        private OAuthTicket _ticket;

        /// <summary>
        /// Contains our url and token endpoints.
        /// </summary>
        private readonly Uris _uris;

        /// <summary>
        /// Contains our client secrets and credentials.
        /// </summary>
        private readonly OAuthAuthorizationProvider _authorizationProvider;

        /// <summary>
        /// Contains our credential key-value pairs.
        /// </summary>
        private readonly Dictionary<string, string> _credentials;

        /// <summary>
        /// Constructor which sets our default headers.
        /// </summary>
        /// <param name="uris">Base url and token endpoints</param>
        public OAuthHttpClient(Uris uris, OAuthAuthorizationProvider authorizationProvider)
        {
            _uris = uris;
            _authorizationProvider = authorizationProvider;
            DefaultRequestHeaders.UserAgent.ParseAdd("Poller.Host");

            _credentials = new Dictionary<string, string>
            {
                {OAuthGrantType.ClientId, _authorizationProvider.ClientId},
                {OAuthGrantType.ClientSecret, _authorizationProvider.ClientSecret},
            };

            BaseAddress = _uris.GetBaseAsUri();
        }

        /// <summary>
        /// This invokes functions that add authentication and send the request.
        /// </summary>
        /// <param name="request">Http request"</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="HttpResponseMessage"/>The response</returns>
        public override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AttachTokenAuthentication(request).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// This invokes functions that add authentication and send the request.
        /// 
        /// TODO We don't need this.
        /// </summary>
        /// <param name="request">Http request"</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="HttpResponseMessage"/>The response</returns>
        public async Task<HttpResponseMessage> PostAsync(
            HttpRequestMessage request, HttpContent content,
            CancellationToken cancellationToken)
        {
            request.Content = content;
            await AttachTokenAuthentication(request).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken);
        }


        /// <summary>
        /// This checks if our authorization ticket exists and 
        /// is still valid and gets us a new one if it isn't.
        /// After that the ticket is attached to the request.
        /// </summary>
        /// <param name="httpRequest">The request</param>
        /// <returns>The request with authentication</returns>
        protected async Task AttachTokenAuthentication(HttpRequestMessage httpRequest)
        {
            if (_ticket == null)
            {
                _ticket = await SendAuthorizeRequestAsync("password");
            }
            else if (!_ticket.IsValid)
            {
                _ticket = await SendTokenRefreshRequestAsync();
            }

            // Attach the bearer token to the request
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                OAuthAuthenticationType.Bearer, _ticket.AccessToken);
        }

        /// <summary>
        /// Send OAuth authorization request.
        /// </summary>
        /// <param name="grantType">Grant type.</param>
        /// <param name="tokenUri">Endpoint.</param>
        /// <returns>Task with ticket</returns>
        protected async virtual Task<OAuthTicket> SendAuthorizeRequestAsync(
            string grantType, string tokenUri = null)
        {
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                tokenUri ?? _uris.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent
                (new Dictionary<string, string>(_credentials)
                {
                    {OAuthGrantType.Username, _authorizationProvider.Username},
                    {OAuthGrantType.Password, _authorizationProvider.Password},
                    {OAuthGrantType.GrantType, grantType},
                })
            })
            using (var httpResponse = await base.SendAsync(httpRequest))
            {
                return await Json.DeserializeAsync<OAuthTicket>(httpResponse);
            }
        }

        /// <summary>
        /// Send an OAuth refresh token request.
        /// </summary>
        /// <param name="grantType">Grant type.</param>
        /// <param name="tokenUri">Endpoint.</param>
        /// <returns>Task with ticket</returns>
        protected async virtual Task<OAuthTicket> SendTokenRefreshRequestAsync(
            string grantType = OAuthGrantType.RefreshToken, string refreshUri = null)
        {
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                refreshUri ?? _uris.RefreshEndpoint)
            {
                Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>(_credentials)
                {
                    {OAuthGrantType.RefreshToken, _ticket.RefreshToken},
                    {OAuthGrantType.GrantType, grantType},
                })
            })
            using (var httpResponse = await base.SendAsync(httpRequest))
            {
                return await Json.DeserializeAsync<OAuthTicket>(httpResponse);
            }
        }
    }
}

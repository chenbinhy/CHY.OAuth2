using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHY.OAuth2.Client.OAuth2
{
    public class BearerTokenHttpMessageHandler:DelegatingHandler
    {
        public BearerTokenHttpMessageHandler(string bearerToken, HttpMessageHandler innerHandler)
            :base(innerHandler)
        {
            this.BearerToken = bearerToken;
        }

        public BearerTokenHttpMessageHandler(ClientBase client, IAuthorizationState authorization, HttpMessageHandler innerHandler)
            :base(innerHandler)
        {
            this.Client = client;
            this.Authorization = authorization;
        }

        public string BearerToken { get; private set; }

        public IAuthorizationState Authorization { get; private set; }
        public ClientBase Client { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string bearerToken = this.BearerToken;
            if(bearerToken == null)
            {
                ErrorUtilities.VerifyProtocol(!this.Authorization.AccessTokenExpirationUtc.HasValue || this.Authorization.AccessTokenExpirationUtc >= DateTime.UtcNow || this.Authorization.RefreshToken != null, ClientStrings.AuthorizationExpired);
                if(this.Authorization.AccessTokenExpirationUtc.HasValue && this.Authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow)
                {
                    ErrorUtilities.VerifyProtocol(this.Authorization.RefreshToken != null, ClientStrings.AccessTokenRefreshFailed);
                    this.Client.RefreshAuthorizationAsync(this.Authorization, cancellationToken: cancellationToken);
                }
                bearerToken = this.Authorization.AccessToken;
            }
            request.Headers.Authorization = new AuthenticationHeaderValue(Protocol.BearerHttpAuthorizationScheme, bearerToken);
            return base.SendAsync(request, cancellationToken);
        }
    }
}

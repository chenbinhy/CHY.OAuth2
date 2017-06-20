using CHY.OAuth2.Client.OAuth2.ChannelElements;
using CHY.OAuth2.Client.OAuth2.Messages;
using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CHY.OAuth2.Client.OAuth2
{
    public class ClientBase
    {
        protected ClientBase(AuthorizationServerDescription authorizationServer, string clientIdentifier = null, ClientCredentialApplicator clientCredentialApplicator = null, IHostFactories hostFactories = null)
        {
            this.AuthorizationServer = authorizationServer;
            this.Channel = new OAuth2ClientChannel(hostFactories);
            this.ClientIdentifier = clientIdentifier;
            this.ClientCredentialApplicator = clientCredentialApplicator;
        }

        public AuthorizationServerDescription AuthorizationServer { get; private set; }

        public Channel Channel { get;  set; }

        public string ClientIdentifier
        {
            get { return this.OAuthChannel.ClientIdentifier; }
            set { this.OAuthChannel.ClientIdentifier = value; }
        }

        public ClientCredentialApplicator ClientCredentialApplicator
        {
            get { return this.OAuthChannel.ClientCredentialApplicator; }
            set { this.OAuthChannel.ClientCredentialApplicator = value; }
        }

        public XmlDictionaryReaderQuotas JsonReaderQuotas
        {
            get { return this.OAuthChannel.JsonReaderQuotas; }
        }

        public IOAuth2ChannelWithClient OAuthChannel
        {
            get { return (IOAuth2ChannelWithClient)this.Channel; }
        }

        public static void AuthorizeRequest(HttpWebRequest request, string accessToken)
        {
            AuthorizeRequest(request.Headers, accessToken);
        }

        public static void AuthorizeRequest(WebHeaderCollection requestHeaders, string accessToken)
        {
            OAuthUtilities.AuthorizeWithBearerToken(requestHeaders, accessToken);
        }

        public Task AuthorizeRequestAsync(HttpWebRequest request, IAuthorizationState authorization, CancellationToken cancellationToken)
        {
            return this.AuthorizeRequestAsync(request.Headers, authorization, cancellationToken);
        }

        public async Task AuthorizeRequestAsync(WebHeaderCollection requestHeaders, IAuthorizationState authorization, CancellationToken cancellationToken)
        {
            ErrorUtilities.VerifyProtocol(!authorization.AccessTokenExpirationUtc.HasValue || authorization.AccessTokenExpirationUtc >= DateTime.UtcNow || authorization.RefreshToken != null, ClientStrings.AuthorizationExpired);

            if(authorization.AccessTokenExpirationUtc.HasValue && authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow)
            {
                ErrorUtilities.VerifyProtocol(authorization.RefreshToken != null, ClientStrings.AccessTokenRefreshFailed);
                await this.RefreshAuthorizationAsync(authorization, cancellationToken: cancellationToken);
            }
            AuthorizeRequest(requestHeaders, authorization.AccessToken);
        }

        public DelegatingHandler CreateAuthorizingHandler(string bearerAccessToken, HttpMessageHandler innerHandler = null)
        {
            return new BearerTokenHttpMessageHandler(bearerAccessToken, innerHandler ?? new HttpClientHandler());
        }

        public DelegatingHandler CreateAuthorizingHandler(IAuthorizationState authorization, HttpMessageHandler innerHandler = null)
        {
            return new BearerTokenHttpMessageHandler(this, authorization, innerHandler ?? new HttpClientHandler());
        }

        public async Task<bool> RefreshAuthorizationAsync(IAuthorizationState authorization, TimeSpan? skipIfUsefulLifeExceeds = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(skipIfUsefulLifeExceeds.HasValue && authorization.AccessTokenExpirationUtc.HasValue)
            {
                TimeSpan usefulLifeRemaining = authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
                if(usefulLifeRemaining > skipIfUsefulLifeExceeds.Value)
                {
                    Logger.OAuth.DebugFormat("Skipping token refresh step because access token's remaining life is {0}, which exceeds {1}.", usefulLifeRemaining, skipIfUsefulLifeExceeds.Value);
                    return false;
                }
            }
            var request = new AccessTokenRefreshRequestC(this.AuthorizationServer)
            {
                ClientIdentifier = this.ClientIdentifier,
                RefreshToken = authorization.RefreshToken
            };
            this.ApplyClientCredential(request);
            var response = await this.Channel.RequestAsync<AccessTokenSuccessResponse>(request, cancellationToken);
            UpdateAuthorizationWithResponse(authorization, response);

            return true;
        }

        public async Task<IAuthorizationState> GetScopedAccessTokenAsync(string refreshToken, HashSet<string> scope, CancellationToken cancellationToken)
        {
            var request = new AccessTokenRefreshRequestC(this.AuthorizationServer)
            {
                ClientIdentifier = this.ClientIdentifier,
                RefreshToken = refreshToken
            };
            this.ApplyClientCredential(request);
            var response = await this.Channel.RequestAsync<AccessTokenSuccessResponse>(request, cancellationToken);
            var authorization = new AuthorizationState();
            UpdateAuthorizationWithResponse(authorization, response);

            return authorization;
        }

        public Task<IAuthorizationState> ExchangeUserCredentialForTokenAsync(string userName, string password, IEnumerable<string> scopes = null, CancellationToken cancellationToken=default(CancellationToken))
        {
            var request = new AccessTokenResourceOwnerPasswordCredentialsRequest(this.AuthorizationServer.TokenEndpoint, this.AuthorizationServer.Version)
            {
                RequestingUserName = userName,
                Password = password
            };

            return this.RequestAccessTokenAsync(request, scopes, cancellationToken);
        }

        public Task<IAuthorizationState> GetClientAccessTokenAsync(IEnumerable<string> scopes = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new AccessTokenClientCredentialsRequest(this.AuthorizationServer.TokenEndpoint, this.AuthorizationServer.Version);

            return this.RequestAccessTokenAsync(request, scopes, cancellationToken);
        }

        public static void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, AccessTokenSuccessResponse accessTokenSuccess)
        {
            authorizationState.AccessToken = accessTokenSuccess.AccessToken;
            authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
            authorizationState.AccessTokenIssueDateUtc = DateTime.UtcNow;
            if(accessTokenSuccess.RefreshToken != null)
            {
                authorizationState.RefreshToken = accessTokenSuccess.RefreshToken;
            }
            if ((authorizationState.Scope == null || authorizationState.Scope.Count == 0) && accessTokenSuccess.Scope != null)
            {
                authorizationState.Scope.ResetContents(accessTokenSuccess.Scope);
            }

            authorizationState.SaveChanges();
        }

        public static void UpdateAuthorizationWithResponse(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAccessTokenResponse accessTokenSuccess)
        {
            authorizationState.AccessToken = accessTokenSuccess.AccessToken;
            authorizationState.AccessTokenExpirationUtc = DateTime.UtcNow + accessTokenSuccess.Lifetime;
            authorizationState.AccessTokenIssueDateUtc = DateTime.UtcNow;
            if(accessTokenSuccess.Scope != null && accessTokenSuccess.Scope != authorizationState.Scope)
            {
                if(authorizationState.Scope != null)
                {
                    Logger.OAuth.InfoFormat(
                                           "Requested scope of \"{0}\" changed to \"{1}\" by authorization server.",
                                           authorizationState.Scope,
                                           accessTokenSuccess.Scope);
                }
                authorizationState.Scope.ResetContents(accessTokenSuccess.Scope);
            }
            authorizationState.SaveChanges();
        }

        public async Task UpdateAuthorizationWithResponseAsync(IAuthorizationState authorizationState, EndUserAuthorizationSuccessAuthCodeResponse authorizationSuccess, CancellationToken cancellationToken)
        {
            var accessTokenRequest = new AccessTokenAuthorizationCodeRequestC(this.AuthorizationServer)
            {
                ClientIdentifier = this.ClientIdentifier,
                Callback = authorizationState.Callback,
                AuthorizationCode= authorizationSuccess.AuthorizationCode
            };
            this.ApplyClientCredential(accessTokenRequest);
            IProtocolMessage accessTokenResponse = await this.Channel.RequestAsync(accessTokenRequest, cancellationToken);
            var accessTokenSuccess = accessTokenResponse as AccessTokenSuccessResponse;
            var failedAccessTokenResponse = accessTokenResponse as AccessTokenFailedResponse;
            if(accessTokenSuccess != null)
            {
                UpdateAuthorizationWithResponse(authorizationState, accessTokenSuccess);
            }
            else
            {
                authorizationState.Delete();
                string error = failedAccessTokenResponse != null ?failedAccessTokenResponse.Error:"(unknown)";
                ErrorUtilities.ThrowProtocol(ClientStrings.CannotObtainAccessTokenWithReason, error);
            }
        }

        /// <summary>
        /// 没有密码返回null，否则返回网络凭据
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        protected static ClientCredentialApplicator DefaultSecretApplicator(string secret)
        {
            return secret == null ? ClientCredentialApplicator.NoSecret() : ClientCredentialApplicator.NetworkCredential(secret);
        }

        protected void ApplyClientCredential(AuthenticatedClientRequestBase request)
        {
            if(this.ClientCredentialApplicator != null)
            {
                this.ClientCredentialApplicator.ApplyClientCredential(this.ClientIdentifier, request);
            }
        }

        private static double ProporttionalLifeRemaining(IAuthorizationState authorization)
        {
            TimeSpan totalLifetime = authorization.AccessTokenExpirationUtc.Value - authorization.AccessTokenIssueDateUtc.Value;
            TimeSpan elapsedLifetime = DateTime.UtcNow - authorization.AccessTokenIssueDateUtc.Value;
            double proportionLifetimeRemaining = 1 - (elapsedLifetime.TotalSeconds / totalLifetime.TotalSeconds);

            return proportionLifetimeRemaining;
        }

        private async Task<IAuthorizationState> RequestAccessTokenAsync(ScopedAccessTokenRequest request, IEnumerable<string> scopes, CancellationToken cancellationToken)
        {
            var authorizationState = new AuthorizationState(scopes);
            request.ClientIdentifier = this.ClientIdentifier;
            this.ApplyClientCredential(request);
            request.Scope.UnionWith(authorizationState.Scope);

            var response = await this.Channel.RequestAsync(request, cancellationToken);
            var success = response as AccessTokenSuccessResponse;
            var failure = response as AccessTokenFailedResponse;
            ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
            if(success != null)
            {
                authorizationState.Scope.Clear();
                UpdateAuthorizationWithResponse(authorizationState, success);
            }
            else
            {
                Logger.OAuth.Info("Credentials rejected by the Authorization Server.");
                authorizationState.Delete();
            }
            return authorizationState;
        }

    }
}

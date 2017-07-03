using CHY.OAuth2.AuthorizationServer.Configuration;
using CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements;
using CHY.OAuth2.AuthorizationServer.OAuth2.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Common;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CHY.OAuth2.AuthorizationServer.OAuth2
{
    /// <summary>
    /// 授权服务
    /// </summary>
    public class AuthorizationServer
    {
        private static readonly IScopeSatisfiedCheck DefaultScopeSatisfiedCheck = new StandardScopeSatisfiedCheck(); // 范围检测
        private readonly List<ClientAuthenticationModule> clientAuthenticationModules = new List<ClientAuthenticationModule>(); // 客户端认证模块
        private readonly ClientAuthenticationModule aggregatingClientAuthenticationModule;

        public AuthorizationServer(IAuthorizationServerHost authorizationServer)
        {
            this.aggregatingClientAuthenticationModule = new AggregatingClientCredentialReader(this.clientAuthenticationModules);
            this.Channel = new OAuth2AuthorizationServerChannel(authorizationServer, this.aggregatingClientAuthenticationModule);
            this.clientAuthenticationModules.AddRange(OAuth2AuthorizationServerSection.Configuration.ClientAuthenticationModules.CreateInstances(true, null));
            this.ScopeSatisfiedCheck = DefaultScopeSatisfiedCheck;
        }

        public Channel Channel { get; set; }

        public IAuthorizationServerHost AuthorizationServerServices
        {
            get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).AuthorizationServer; }
        }

        public IList<ClientAuthenticationModule> ClientAuthenticationModules
        {
            get { return this.clientAuthenticationModules; }
        }

        public IScopeSatisfiedCheck ScopeSatisfiedCheck
        {
            get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).ScopeSatisfiedCheck; }
            set { ((IOAuth2ChannelWithAuthorizationServer)this.Channel).ScopeSatisfiedCheck = value; }
        }

        public async Task<EndUserAuthorizationRequest> ReadAuthorizationRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = await this.Channel.TryReadFromRequestAsync<EndUserAuthorizationRequest>(request, cancellationToken);
            if(message != null)
            {
                if(message.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode)
                {
                    var client = this.AuthorizationServerServices.GetClientOrThrow(message.ClientIdentifier);
                    ErrorUtilities.VerifyProtocol(client.HasNonEmptySecret, Protocol.EndUserAuthorizationRequestErrorCodes.UnauthorizedClient);
                }
            }

            return message;
        }

        public Task<EndUserAuthorizationRequest> ReadAuthorizationRequestAsync(
            HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            request = request ?? this.Channel.GetRequestFromContext();
            return this.ReadAuthorizationRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
        }

        public Task<EndUserAuthorizationRequest> ReadAuthorizationRequestAsync(Uri requestUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return this.ReadAuthorizationRequestAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> HandleTokenRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            IProtocolMessage responseMessage;
            try
            {
                AccessTokenRequestBase requestMessage = await this.Channel.TryReadFromRequestAsync<AccessTokenRequestBase>(request, cancellationToken);
                if(requestMessage != null)
                {
                    var accessTokenResult = this.AuthorizationServerServices.CreateAccessToken(requestMessage);
                    ErrorUtilities.VerifyHost(accessTokenResult != null, "IAuthorizationServerHost.CreateAccessToken must not return null.");
                    IAccessTokenRequestInternal  accessRequestInternal = requestMessage;
                    accessRequestInternal.AccessTokenResult = accessTokenResult;
                    var successResponseMessage = this.PrepareAccessTokenResponse(requestMessage, accessTokenResult.AllowRefreshToken);
                    successResponseMessage.Lifetime = accessTokenResult.AccessToken.Lifetime;
                    var authCarryingRequest = requestMessage as IAuthorizationCarryingRequest;
                    if(authCarryingRequest != null)
                    {
                        accessTokenResult.AccessToken.ApplyAuthorization(authCarryingRequest.AuthorizationDescription);
                        IAccessTokenIssuingResponse accessTokenIssuingResponse = successResponseMessage;
                        accessTokenIssuingResponse.AuthorizationDescription = accessTokenResult.AccessToken;
                    }
                    responseMessage = successResponseMessage;
                }
                else
                {
                    responseMessage = new AccessTokenFailedResponse(){Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest};
                }
            }catch(TokenEndpointProtocolException ex)
            {
                responseMessage = ex.GetResponse();
            }
            catch(ProtocolException)
            {
                responseMessage = new AccessTokenFailedResponse() { Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest };
            }

            return await this.Channel.PrepareResponseAsync(responseMessage, cancellationToken);
        }

        public Task<HttpResponseMessage> HandleTokenRequestAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            request = request??this.Channel.GetRequestFromContext();
            return this.HandleTokenRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
        }

        public EndUserAuthorizationFailedResponse PrepareRejectAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, Uri callback = null)
        {
            if(callback == null)
            {
                callback = this.GetCallback(authorizationRequest);
            }
            var response = new EndUserAuthorizationFailedResponse(callback, authorizationRequest);

            return response;
        }

        public EndUserAuthorizationSuccessResponseBase PrepareApproveAuthorizationRequest(EndUserAuthorizationRequest authorizationRequest, string userName, IEnumerable<string> scopes = null, Uri callback = null)
        {
            if(callback == null)
            {
                callback = this.GetCallback(authorizationRequest);
            }
            var client = this.AuthorizationServerServices.GetClientOrThrow(authorizationRequest.ClientIdentifier);
            EndUserAuthorizationSuccessResponseBase response;
            switch(authorizationRequest.ResponseType)
            {
                case EndUserAuthorizationResponseType.AccessToken:
                    IAccessTokenRequestInternal accessRequestInternal = (EndUserAuthorizationImplicitRequest)authorizationRequest;
                    var accessTokenResult = this.AuthorizationServerServices.CreateAccessToken(accessRequestInternal);
                    ErrorUtilities.VerifyHost(accessTokenResult != null, "IAuthorizationServerHost.CreateAccessToken must not return null.");
                    accessRequestInternal.AccessTokenResult = accessTokenResult;
                    var implicitGrantResponse = new EndUserAuthorizationSuccessAccessTokenResponse(callback, authorizationRequest);
                    implicitGrantResponse.Lifetime = accessTokenResult.AccessToken.Lifetime;
                    accessTokenResult.AccessToken.ApplyAuthorization(implicitGrantResponse.Scope, userName, implicitGrantResponse.Lifetime);

                    IAccessTokenCarryingRequest tokenCarryingResponse = implicitGrantResponse;
                    tokenCarryingResponse.AuthorizationDescription = accessTokenResult.AccessToken;

                    response = implicitGrantResponse;
                    break;
                case EndUserAuthorizationResponseType.AuthorizationCode:
                    var authCodeResponse = new EndUserAuthorizationSuccessAuthCodeResponseAS(callback, authorizationRequest);
                    IAuthorizationCodeCarryingRequest codeCarryingResponse = authCodeResponse;
                    codeCarryingResponse.AuthorizationDescription = new AuthorizationCode(
                        authorizationRequest.ClientIdentifier,
                        authorizationRequest.Callback,
                        authCodeResponse.Scope,
                        userName
                    );
                    response = authCodeResponse;
                    break;
                default:
                    throw ErrorUtilities.ThrowInternal("Unexpected response type.");
            }
            response.AuthorizingUserName = userName;
            if(scopes != null)
            {
                response.Scope.ResetContents(scopes);
            }

            return response;
        }

        public IAuthorizationDescription DecodeRefreshToken(string refreshToken)
        {
            var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServerServices.CryptoKeyStore);
            var token = new RefreshToken();
            refreshTokenFormatter.Deserialize(token, refreshToken);

            return token;
        }

        protected Uri GetCallback(EndUserAuthorizationRequest authorizationRequest)
        {
            var client = this.AuthorizationServerServices.GetClientOrThrow(authorizationRequest.ClientIdentifier);
            if(authorizationRequest.Callback != null)
            {
                return authorizationRequest.Callback;
            }

            Uri defaultCallback = client.DefaultCallback;
            ErrorUtilities.VerifyProtocol(defaultCallback != null, AuthServerStrings.NoCallback);
            return defaultCallback;
        }

        private AccessTokenSuccessResponse PrepareAccessTokenResponse(AccessTokenRequestBase request, bool allowRefreshToken = true)
        {
            if(allowRefreshToken)
            {
                if(request is AccessTokenClientCredentialsRequest)
                {
                    Logger.OAuth.Debug("Suppressing refresh token in access token response because the grant type used by the client disallows it.");
                    allowRefreshToken = false;
                }
            }
            var tokenRequest = (IAuthorizationCarryingRequest)request;
            var accessTokenRequest = (IAccessTokenRequestInternal)request;
            var response = new AccessTokenSuccessResponse(request)
            {
                HasRefreshToken = allowRefreshToken
            };
            response.Scope.ResetContents(tokenRequest.AuthorizationDescription.Scope);

            return response;
        }
    }
}


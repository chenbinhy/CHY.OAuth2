using CHY.OAuth2.AuthorizationServer.OAuth2.Messages;
using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Common;
using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.AuthorizationServer.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CHY.OAutho2.Core.Messaging;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.ClientAuthorization.OAuth2;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class MessageValidationBindingElement:AuthServerBindingElementBase
    {
        private readonly ClientAuthenticationModule clientAuthenticationModule;

        public MessageValidationBindingElement(ClientAuthenticationModule clientAuthenticationModule)
        {
            this.clientAuthenticationModule = clientAuthenticationModule;
        }

        public override MessageProtections Protection
        {
            get { return MessageProtections.None; }
        }

        public override Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            var accessTokenResponse = message as AccessTokenSuccessResponse;
            if(accessTokenResponse != null)
            {
                var directResponseMessage = (IDirectResponseProtocolMessage)accessTokenResponse;
                var accessTokenRequest = (AccessTokenRequestBase)directResponseMessage.OriginatingRequest;
                ErrorUtilities.VerifyProtocol(accessTokenRequest.GrantType != GrantType.ClientCredentials || accessTokenResponse.RefreshToken == null, OAuthStrings.NoGrantNoRefreshToken);
            }

            return MessageProtectionTasks.Null;
        }

        public override Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            bool applied = false;
            var clientCredentialOnly = message as AccessTokenClientCredentialsRequest;
            var authenticatedClientRequest = message as AuthenticatedClientRequestBase;
            var accessTokenRequest = authenticatedClientRequest as AccessTokenRequestBase;
            var resourceOwnerPasswordCarrier = message as AccessTokenResourceOwnerPasswordCredentialsRequest;
            if(authenticatedClientRequest != null)
            {
                string clientIdentifier;
                var result = this.clientAuthenticationModule.TryAuthenticateClient(this.AuthServerChannel.AuthorizationServer, authenticatedClientRequest, out clientIdentifier);
                switch(result)
                {
                    case ClientAuthenticationResult.ClientAuthenticated:
                        break;
                    case ClientAuthenticationResult.NoAuthenticationRecognized:
                    case ClientAuthenticationResult.ClientIdNotAuthenticated:
                        AuthServerUtilities.TokenEndpointVerify(resourceOwnerPasswordCarrier != null, accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.InvalidClient, this.clientAuthenticationModule, AuthServerStrings.ClientSecretMismatch);
                        break;
                    default:
                        AuthServerUtilities.TokenEndpointVerify(false, accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.InvalidClient, this.clientAuthenticationModule, AuthServerStrings.ClientSecretMismatch);
                        break;
                }
                authenticatedClientRequest.ClientIdentifier = result == ClientAuthenticationResult.NoAuthenticationRecognized? null : clientIdentifier;
                accessTokenRequest.ClientAuthenticated = result == ClientAuthenticationResult.ClientAuthenticated;
                applied = true;
            }

            if(resourceOwnerPasswordCarrier != null)
            {
                try
                {
                    var authorizaResult = this.AuthorizationServer.CheckAuthorizeResourceOwnerCredentialGrant(
                        resourceOwnerPasswordCarrier.RequestingUserName, resourceOwnerPasswordCarrier.Password, resourceOwnerPasswordCarrier);
                    if(authorizaResult.IsApproved)
                    {
                        resourceOwnerPasswordCarrier.CredentialsValidated = true;
                        resourceOwnerPasswordCarrier.RequestingUserName = authorizaResult.CanonicalUserName;
                        resourceOwnerPasswordCarrier.Scope.ResetContents(authorizaResult.ApprovedScope);
                    }
                    else
                    {
                        Logger.OAuth.ErrorFormat(
                            "Resource owner password credential for user \"{0}\" rejected by authorization server host.",
                            resourceOwnerPasswordCarrier.RequestingUserName);
                        throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.InvalidGrant, AuthServerStrings.InvalidResourceOwnerPasswordCredential);
                    }
                }catch(NotSupportedException)
                {
                    throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.UnsupportedGrantType);
                }
                catch (NotImplementedException)
                {
                    throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.UnsupportedGrantType);
                }
                applied = true;
            }
            else if(clientCredentialOnly != null)
            {
                try
                {
                    var authorizeResult = this.AuthorizationServer.CheckAuthorizeClientCredentialsGrant(clientCredentialOnly);
                    if(!authorizeResult.IsApproved)
                    {
                        Logger.OAuth.ErrorFormat(
                            "Client credentials grant access request for client \"{0}\" rejected by authorization server host.",
                            clientCredentialOnly.ClientIdentifier);
                        throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.UnauthorizedClient);
                    }
                    clientCredentialOnly.Scope.ResetContents(authorizeResult.ApprovedScope);
                }
                catch (NotSupportedException)
                {
                    throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.UnsupportedGrantType);
                }
                catch (NotImplementedException)
                {
                    throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.UnsupportedGrantType);
                }
            }
            else
            {
                var authorizationRequest = message as EndUserAuthorizationRequest;
                if(authorizationRequest != null)
                {
                    var client = this.AuthorizationServer.GetClientOrThrow(authorizationRequest.ClientIdentifier);
                    ErrorUtilities.VerifyProtocol(authorizationRequest.Callback == null || client.IsCallbackAllowed(authorizationRequest.Callback), AuthServerStrings.ClientCallbackDisallowed, authorizationRequest.Callback);
                    ErrorUtilities.VerifyProtocol(authorizationRequest.Callback != null || client.DefaultCallback != null, AuthServerStrings.NoCallback);
                    applied = true;
                }

                var request = message as AccessTokenAuthorizationCodeRequestAS;
                if(request != null)
                {
                    IAuthorizationCodeCarryingRequest tokenRequest = request;
                    tokenRequest.AuthorizationDescription.VerifyCallback(request.Callback);
                    applied = true;
                }
                var authCarrier = message as IAuthorizationCarryingRequest;
                if(authCarrier != null)
                {
                    var accessRequest = authCarrier as AccessTokenRequestBase;
                    if(accessRequest != null)
                    {
                        AuthServerUtilities.TokenEndpointVerify(string.Equals(accessRequest.ClientIdentifier, authCarrier.AuthorizationDescription.ClientIdentifier, StringComparison.Ordinal), accessRequest, Protocol.AccessTokenRequestErrorCodes.InvalidClient);
                        var scopedAccessRequest = accessRequest as ScopedAccessTokenRequest;
                        if(scopedAccessRequest != null)
                        {
                            if(!this.AuthServerChannel.ScopeSatisfiedCheck.IsScopeSatisfied(requiredScope:scopedAccessRequest.Scope, grantedScope:authCarrier.AuthorizationDescription.Scope))
                            {
                                Logger.OAuth.ErrorFormat("The requested access scope (\"{0}\") exceeds the grant scope (\"{1}\").", scopedAccessRequest.Scope, authCarrier.AuthorizationDescription.Scope);
                                throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.InvalidScope, AuthServerStrings.AccessScopeExceedsGrantScope);
                            }
                        }
                    }
                    if(!this.AuthorizationServer.IsAuthorizationValid(authCarrier.AuthorizationDescription))
                    {
                        Logger.OAuth.Error("Rejecting access token request because the IAuthorizationServerHost.IsAuthorizationValid method returned false.");
                        throw new TokenEndpointProtocolException(accessTokenRequest, Protocol.AccessTokenRequestErrorCodes.InvalidGrant);
                    }
                    applied = true;
                }
            }

            return applied?MessageProtectionTasks.None:MessageProtectionTasks.Null;
        }
    }
}

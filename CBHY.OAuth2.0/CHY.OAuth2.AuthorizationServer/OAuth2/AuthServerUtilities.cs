using CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements;
using CHY.OAuth2.ClientAuthorization.OAuth2;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.OAuth2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2
{
    public static class AuthServerUtilities
    {
        public static IClientDescription GetClientOrThrow(this IAuthorizationServerHost authorizationServer, string clientIdentifier)
        {
            try
            {
                var result = authorizationServer.GetClient(clientIdentifier);
                ErrorUtilities.VerifyHost(result != null, OAuthStrings.ResultShouldNotBeNull, authorizationServer.GetType().FullName, "GetClient(string)");

                return result;
            }
            catch (KeyNotFoundException ex)
            {
                throw ErrorUtilities.Wrap(ex, AuthServerStrings.ClientOrTokenSecretNotFound);
            }
            catch (ArgumentException ex)
            {
                throw ErrorUtilities.Wrap(ex, AuthServerStrings.ClientOrTokenSecretNotFound);
            }
        }

        public static void TokenEndpointVerify(bool condition, AccessTokenRequestBase requestMessage, string error, ClientAuthenticationModule authenticationModule = null, string unformattedDescription = null, params object[] args)
        {
            if(!condition)
            {
                string description = unformattedDescription != null ? string.Format(CultureInfo.CurrentCulture, unformattedDescription, args) : null;
                string wwwAuthenticateHeader = null;
                if(authenticationModule != null)
                {
                    wwwAuthenticateHeader = authenticationModule.AuthenticateHeader;
                }

                throw new TokenEndpointProtocolException(requestMessage, error, description, authenticateHeader: wwwAuthenticateHeader);
            }
        }
    }
}

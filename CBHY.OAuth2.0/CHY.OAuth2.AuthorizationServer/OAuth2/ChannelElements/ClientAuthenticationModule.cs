using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public abstract class ClientAuthenticationModule
    {
        protected ClientAuthenticationModule()
        {

        }

        public virtual string AuthenticateHeader
        {
            get { return null; }
        }

        public abstract ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, AuthenticatedClientRequestBase requestMessage, out string clientIdentifier);

        protected static ClientAuthenticationResult TryAuthenticateClientBySecret(IAuthorizationServerHost authorizationServerHost, string clientIdentifier, string clientSecret)
        {
            if (!string.IsNullOrEmpty(clientIdentifier))
            {
                var client = authorizationServerHost.GetClient(clientIdentifier);
                if (client != null)
                {
                    if (!string.IsNullOrEmpty(clientSecret))
                    {
                        if (client.IsValidClientSecret(clientSecret))
                        {
                            return ClientAuthenticationResult.ClientAuthenticated;
                        }
                        else
                        {
                            return ClientAuthenticationResult.ClientAuthenticationRejected;
                        }
                    }
                    else
                    {
                        return ClientAuthenticationResult.ClientIdNotAuthenticated;
                    }
                }
                else
                {
                    return ClientAuthenticationResult.ClientAuthenticationRejected;
                }
            }
            else
            {
                return ClientAuthenticationResult.NoAuthenticationRecognized;
            }
            
        }
    }
}

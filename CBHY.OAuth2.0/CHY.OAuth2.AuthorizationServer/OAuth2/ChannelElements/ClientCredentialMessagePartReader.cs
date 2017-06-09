using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class ClientCredentialMessagePartReader:ClientAuthenticationModule
    {
        public override ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, AuthenticatedClientRequestBase requestMessage, out string clientIdentifier)
        {
            clientIdentifier = requestMessage.ClientIdentifier;
            return TryAuthenticateClientBySecret(authorizationServerHost, requestMessage.ClientIdentifier, requestMessage.ClientSecret);
        }
    }
}

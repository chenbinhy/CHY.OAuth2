using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class ClientCredentialHttpBasicReader:ClientAuthenticationModule
    {
        public override string AuthenticateHeader
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Basic realm=\"{0}\"", this.Realm);
            }
        }

        public string Realm { get; set; }

        public override ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, AuthenticatedClientRequestBase requestMessage, out string clientIdentifier)
        {
            var credential = OAuthUtilities.ParseHttpBasicAuth(requestMessage.Headers);
            if(credential != null)
            {
                clientIdentifier = credential.UserName;
                return TryAuthenticateClientBySecret(authorizationServerHost, credential.UserName, credential.Password);
            }
            clientIdentifier = null;
            return ClientAuthenticationResult.NoAuthenticationRecognized;
        }
    }
}

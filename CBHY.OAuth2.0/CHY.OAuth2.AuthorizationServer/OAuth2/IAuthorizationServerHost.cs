using CHY.OAuth2.AuthorizationServer.OAuth2.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging.Bindings;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2
{
    public interface IAuthorizationServerHost
    {
        ICryptoKeyStore CryptoKeyStore { get; }

        INonceStore NonceStore { get; }

        AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage);
        IClientDescription GetClient(string clientIdentifier);

        bool IsAuthorizationValid(IAuthorizationDescription authorization);

        AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest);

        AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest);
    }
}

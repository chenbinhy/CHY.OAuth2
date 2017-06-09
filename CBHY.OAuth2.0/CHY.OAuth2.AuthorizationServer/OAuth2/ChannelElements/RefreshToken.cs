using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHY.OAuth2.Core.Messaging.Bindings;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class RefreshToken:AuthorizationDataBag
    {
        public const string RefreshTokenKeyBucket = "https://localhost/dnoa/oauth_refresh_token";

        public RefreshToken()
        {

        }

        public RefreshToken(IAuthorizationDescription authorization)
        {
            this.ClientIdentifier = authorization.ClientIdentifier;
            this.UtcCreationDate = authorization.UtcIssued;
            this.User = authorization.User;
            this.Scope.ResetContents(authorization.Scope);
        }

        public static IDataBagFormatter<RefreshToken> CreateFormatter(ICryptoKeyStore cryptoKeyStore)
        {
            return new UriStyleMessageFormatter<RefreshToken>(cryptoKeyStore, RefreshTokenKeyBucket, signed: true, encrypted: true);
        }
    }
}

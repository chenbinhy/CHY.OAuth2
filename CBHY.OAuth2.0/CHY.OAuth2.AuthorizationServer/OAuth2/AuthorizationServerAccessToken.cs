using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Bindings;
using CHY.OAuth2.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2
{
    public class AuthorizationServerAccessToken:AccessToken
    {
        public AuthorizationServerAccessToken() { }

        public RSACryptoServiceProvider AccessTokenSigningKey { get; set; }

        public RSACryptoServiceProvider ResourceServerEncryptionKey { get; set; }

        public ICryptoKeyStore SymmetricKeyStore { get; set; }

        public override string Serialize()
        {
            ErrorUtilities.VerifyHost(this.AccessTokenSigningKey != null || this.SymmetricKeyStore != null, AuthServerStrings.AccessTokenSigningKeyMissing);
            IDataBagFormatter<AccessToken> formatter;
            if(this.AccessTokenSigningKey != null)
            {
                formatter = CreateFormatter(this.AccessTokenSigningKey, this.ResourceServerEncryptionKey);
            }
            else
            {
                formatter = CreateFormatter(this.SymmetricKeyStore);
            }

            return formatter.Serialize(this);
        }
    }
}

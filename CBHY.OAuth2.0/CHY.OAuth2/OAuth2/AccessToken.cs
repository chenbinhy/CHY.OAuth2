using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Bindings;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2
{
    public class AccessToken:AuthorizationDataBag
    {
        public AccessToken() { }

        [MessagePart(Encoder = typeof(TimespanSecondsEncoder))]
        public TimeSpan? Lifetime { get; set; }

        protected override Type BagType
        {
            get
            {
                return typeof(AccessToken);
            }
        }

        public static  IDataBagFormatter<AccessToken> CreateFormatter(RSACryptoServiceProvider signingKey, RSACryptoServiceProvider encryptingKey)
        {
            return new UriStyleMessageFormatter<AccessToken>(signingKey, encryptingKey);
        }

        public static IDataBagFormatter<AccessToken> CreateFormatter(ICryptoKeyStore symmetricKeyStore)
        {
            return new UriStyleMessageFormatter<AccessToken>(symmetricKeyStore, bucket: "AccessTokens", signed: true, encrypted: true);
        }

        public void ApplyAuthorization(IAuthorizationDescription authorization)
        {
            this.ClientIdentifier = authorization.ClientIdentifier;
            this.UtcCreationDate = authorization.UtcIssued;
            this.User = authorization.User;
            this.Scope.ResetContents(authorization.Scope);
        }

        public void ApplyAuthorization(IEnumerable<string> scopes, string username, TimeSpan? lifetime)
        {
            this.Scope.ResetContents(scopes);
            this.User = username;
            this.Lifetime = lifetime;
            this.UtcCreationDate = DateTime.UtcNow;
        }

        public virtual string Serialize()
        {
            throw new NotSupportedException();
        }

        protected override void EnsureValidMessage()
        {
            base.EnsureValidMessage();
            if(this.Lifetime.HasValue)
            {
                DateTime expirationDate = this.UtcCreationDate + this.Lifetime.Value;
                if(expirationDate < DateTime.UtcNow)
                {
                    throw new ExpiredMessageException(expirationDate, this.ContainingMessage);
                }
            }
        }
    }
}

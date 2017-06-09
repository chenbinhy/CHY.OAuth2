using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class AccessTokenRefreshRequest:ScopedAccessTokenRequest
    {
        protected AccessTokenRefreshRequest(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {

        }

        [MessagePart(Protocol.refresh_token, IsRequired = true)]
        public string RefreshToken { get; set; }

        public override GrantType GrantType
        {
            get { return Messages.GrantType.RefreshToken; }
        }
    }
}

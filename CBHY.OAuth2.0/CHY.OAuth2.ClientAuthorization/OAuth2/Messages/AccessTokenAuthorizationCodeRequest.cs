using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class AccessTokenAuthorizationCodeRequest :AccessTokenRequestBase
    {
        protected AccessTokenAuthorizationCodeRequest(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {

        }

        public override GrantType GrantType
        {
            get { return Messages.GrantType.AuthorizationCode; }
        }

        [MessagePart(Protocol.code, IsRequired=true)]
        public string AuthorizationCode { get; set; }

        [MessagePart(Protocol.redirect_uri, IsRequired=false)]
        public Uri Callback { get; set; }

        protected override HashSet<string> RequestedScope
        {
            get { return ((IAuthorizationCarryingRequest)this).AuthorizationDescription.Scope; }
        }
    }
}

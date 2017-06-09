using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public abstract class ScopedAccessTokenRequest:AccessTokenRequestBase
    {
        internal ScopedAccessTokenRequest(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
        }

        [MessagePart(Protocol.scope, IsRequired=false, Encoder = typeof(ScopeEncoder))]
        public HashSet<string> Scope { get; private set; }

        protected override HashSet<string> RequestedScope
        {
            get { return this.Scope; }
        }
    }
}

using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.ChannelElements
{
    public abstract class AuthorizationDataBag:DataBag, IAuthorizationDescription
    {
        protected AuthorizationDataBag()
        {
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
        }

        [MessagePart]
        public string ClientIdentifier { get; set; }

        public DateTime UtcIssued
        {
            get { return this.UtcCreationDate; }
        }

        [MessagePart]
        public string User { get; set; }

        [MessagePart(Encoder=typeof(ScopeEncoder))]
        public HashSet<string> Scope { get; private set; }
    }
}

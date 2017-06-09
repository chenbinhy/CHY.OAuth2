using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public abstract class EndUserAuthorizationSuccessResponseBase:MessageBase,IMessageWithClientState
    {
        public EndUserAuthorizationSuccessResponseBase(Uri clientCallback, Version version)
            :base(version, MessageTransport.Indirect, clientCallback)
        {
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
        }

        public EndUserAuthorizationSuccessResponseBase(Uri clientCallback, EndUserAuthorizationRequest request)
            :base(request, clientCallback)
        {
            ((IMessageWithClientState)this).ClientState = request.ClientState;
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            this.Scope.ResetContents(request.Scope);
        }

        [MessagePart(Protocol.state, IsRequired=false)]
        string IMessageWithClientState.ClientState { get; set; }

        public ICollection<string> Scope { get; protected set;}

        public string AuthorizingUserName { get; set; }
    }
}

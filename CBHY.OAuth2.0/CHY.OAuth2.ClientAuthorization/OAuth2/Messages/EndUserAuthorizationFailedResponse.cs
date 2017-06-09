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
    public class EndUserAuthorizationFailedResponse:MessageBase,IMessageWithClientState
    {
        public EndUserAuthorizationFailedResponse(Uri clientCallback,Version version)
            :base(version, MessageTransport.Indirect, clientCallback)
        {
        }

        public EndUserAuthorizationFailedResponse(Uri clientCallback, EndUserAuthorizationRequest request)
            :base(((IProtocolMessage)request).Version, MessageTransport.Indirect, clientCallback)
        {
            ((IMessageWithClientState)this).ClientState = request.ClientState;
        }

        [MessagePart(Protocol.error, IsRequired=true)]
        public string Error { get; set; }

        [MessagePart(Protocol.error_description, IsRequired=false)]
        public string ErrorDescription { get; set; }

        [MessagePart(Protocol.error_uri, IsRequired=false)]
        public Uri ErrorUri { get; set; }

        [MessagePart(Protocol.state, IsRequired=false)]
        string IMessageWithClientState.ClientState { get; set; }
    }
}

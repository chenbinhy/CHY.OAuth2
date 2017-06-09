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
    public class EndUserAuthorizationSuccessAuthCodeResponse:EndUserAuthorizationSuccessResponseBase
    {
        public EndUserAuthorizationSuccessAuthCodeResponse(Uri clientCallback, Version version)
            :base(clientCallback, version)
        {
        }

        public EndUserAuthorizationSuccessAuthCodeResponse(Uri clientCallback, EndUserAuthorizationRequest request)
            :base(clientCallback,request)
        {
            ((IMessageWithClientState)this).ClientState = request.ClientState;
        }

        [MessagePart(Protocol.code, IsRequired=true)]
        public string AuthorizationCode { get; set; }
    }
}

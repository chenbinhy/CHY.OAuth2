using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class EndUserAuthorizationSuccessAccessTokenResponse:EndUserAuthorizationSuccessResponseBase, IAccessTokenIssuingResponse, IHttpIndirectResponse
    {
        public EndUserAuthorizationSuccessAccessTokenResponse(Uri clientCallback, Version version)
            :base(clientCallback, version)
        {
            this.TokenType = Protocol.AccessTokenTypes.Bearer;
        }

        public EndUserAuthorizationSuccessAccessTokenResponse(Uri clientCallback, EndUserAuthorizationRequest request)
            :base(clientCallback, request)
        {
            ((IMessageWithClientState)this).ClientState = request.ClientState;
            this.TokenType = Protocol.AccessTokenTypes.Bearer;
        }

        AccessToken IAccessTokenCarryingRequest.AuthorizationDescription { get; set; }

        IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription
        {
            get { return ((IAccessTokenCarryingRequest)this).AuthorizationDescription; }
        }

        bool IHttpIndirectResponse.Include301RedirectPayloadInFragment
        {
            get { return true; }
        }

        TimeSpan? IAccessTokenIssuingResponse.Lifetime
        {
            get { return this.Lifetime; }
            set { this.Lifetime = value; }
        }

        [MessagePart(Protocol.token_type, IsRequired=true)]
        public string TokenType { get; internal set; }

        [MessagePart(Protocol.access_token, IsRequired=true)]
        public string AccessToken { get; set; }

        public new ICollection<string> Scope
        {
            get { return base.Scope; }
            protected set { base.Scope = value; }
        }

        [MessagePart(Protocol.expires_in, IsRequired=false, Encoder=typeof(TimespanSecondsEncoder))]
        public TimeSpan? Lifetime { get; set; }
    }
}

using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.OAuth2.Messages;
using CHY.OAuth2.Core.Configuration;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using CHY.OAuth2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class EndUserAuthorizationRequest:MessageBase
    {
        [MessagePart(Protocol.response_type, IsRequired = true, Encoder = typeof(EndUserAuthorizationResponseTypeEncoder))]
        private const EndUserAuthorizationResponseType ResponseTypeConst = EndUserAuthorizationResponseType.AuthorizationCode;

        protected EndUserAuthorizationRequest(Uri authorizationEndpoint, Version version)
            :base(version, MessageTransport.Indirect, authorizationEndpoint)
        {
            this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest;
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
        }

        public virtual EndUserAuthorizationResponseType ResponseType
        {
            get { return ResponseTypeConst; }
        }

        [MessagePart(Protocol.client_id, IsRequired=true)]
        public string ClientIdentifier { get; set; }

        [MessagePart(Protocol.redirect_uri, IsRequired=false)]
        public Uri Callback { get; set; }

        [MessagePart(Protocol.state, IsRequired=false)]
        public string ClientState { get; set; }

        [MessagePart(Protocol.scope, IsRequired=false, Encoder=typeof(ScopeEncoder))]
        public HashSet<string> Scope { get; private set; }

        protected override void EnsureValidMessage()
        {
 	            base.EnsureValidMessage();
            ErrorUtilities.VerifyProtocol(
				DotNetOpenAuthSection.Messaging.RelaxSslRequirements || this.Recipient.IsTransportSecure(),
				OAuthStrings.HttpsRequired);
			ErrorUtilities.VerifyProtocol(this.Callback == null || this.Callback.IsAbsoluteUri, this, OAuthStrings.AbsoluteUriRequired, Protocol.redirect_uri);
        }
    }
}

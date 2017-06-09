using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.Core.Configuration;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using CHY.OAuth2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public abstract class AccessTokenRequestBase : AuthenticatedClientRequestBase, IAccessTokenRequestInternal
    {
        protected AccessTokenRequestBase(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {
            this.HttpMethods = HttpDeliveryMethods.PostRequest;
        }

        HashSet<string> IAccessTokenRequest.Scope
        {
            get { return this.RequestedScope; }
        }

        public bool ClientAuthenticated { get;  set; }

        IAccessTokenResult IAccessTokenRequestInternal.AccessTokenResult { get; set; }

        public virtual string UserName
        {
            get
            {
                IAccessTokenRequestInternal request = this;
                if(request.AccessTokenResult != null && request.AccessTokenResult.AccessToken != null)
                {
                    return request.AccessTokenResult.AccessToken.User;
                }

                return null;
            }
        }

        [MessagePart(Protocol.grant_type, IsRequired=true, Encoder=typeof(GrantTypeEncoder))]
        public abstract GrantType GrantType { get; }

        protected abstract HashSet<string> RequestedScope { get; }

        protected override void EnsureValidMessage()
        {
            base.EnsureValidMessage();
            ErrorUtilities.VerifyProtocol(
                DotNetOpenAuthSection.Messaging.RelaxSslRequirements || this.Recipient.IsTransportSecure(),
                OAuthStrings.HttpsRequired);
        }
    }
}

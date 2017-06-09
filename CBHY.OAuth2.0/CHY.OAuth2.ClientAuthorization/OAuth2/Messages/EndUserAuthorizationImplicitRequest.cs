using CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    [Serializable]
    public class EndUserAuthorizationImplicitRequest:EndUserAuthorizationRequest, IAccessTokenRequestInternal
    {
        [MessagePart(Protocol.response_type, IsRequired = true, Encoder = typeof(EndUserAuthorizationResponseTypeEncoder))]
        private const EndUserAuthorizationResponseType ResponseTypeConst = EndUserAuthorizationResponseType.AccessToken;

        protected EndUserAuthorizationImplicitRequest(Uri authorizationEndpoint, Version version)
            :base(authorizationEndpoint, version)
        {

        }

        public override EndUserAuthorizationResponseType ResponseType
        {
            get
            {
                return ResponseTypeConst;
            }
        }

        IAccessTokenResult IAccessTokenRequestInternal.AccessTokenResult { get; set; }

        string IAccessTokenRequest.UserName
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

        bool IAccessTokenRequest.ClientAuthenticated
        {
            get { return false; }
        }
    }
}

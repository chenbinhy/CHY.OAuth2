using CHY.OAuth2.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class AccessTokenFailedResponse:MessageBase, IHttpDirectResponse
    {
        private readonly bool invalidClientCredentialsInAuthorizationHeader;
        private readonly WebHeaderCollection headers = new WebHeaderCollection();
        public AccessTokenFailedResponse(AccessTokenRequestBase request)
            :base(request)
        {

        }

        public AccessTokenFailedResponse(AccessTokenRequestBase request, bool invalidClientCredentialsInAuthorizationHeader)
            :base(request)
        {
            this.invalidClientCredentialsInAuthorizationHeader = invalidClientCredentialsInAuthorizationHeader;
        }

        public AccessTokenFailedResponse(Version version = null)
            :base(version??Protocol.Default.Version)
        {

        }

        HttpStatusCode IHttpDirectResponse.HttpStatusCode
        {
            get { return this.invalidClientCredentialsInAuthorizationHeader ? HttpStatusCode.Unauthorized : HttpStatusCode.BadRequest; }
        }

        public WebHeaderCollection Headers
        {
            get { return this.headers; }
        }

        [MessagePart(Protocol.error, IsRequired=true)]
        public string Error { get; set; }

        [MessagePart(Protocol.error_description, IsRequired=false)]
        public string ErrorDescription { get; set; }

        [MessagePart(Protocol.error_uri, IsRequired=false)]
        public Uri ErrorUri { get; set; }
    }
}

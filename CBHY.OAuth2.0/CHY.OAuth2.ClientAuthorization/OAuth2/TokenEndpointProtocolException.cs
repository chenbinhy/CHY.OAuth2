using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAutho2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2
{
    public class TokenEndpointProtocolException:ProtocolException
    {
        private readonly AccessTokenRequestBase requestMessage;
        private readonly string authenticateHeader;

        public TokenEndpointProtocolException(AccessTokenRequestBase requestMessage, string error, string description = null, Uri moreInformation = null, string authenticateHeader = null)
            :base(string.Format(CultureInfo.CurrentUICulture, ClientAuthorizationStrings.TokenEndpointErrorFormat, error, description))
        {
            this.requestMessage = requestMessage;
            this.Error = error;
            this.Description = description;
            this.MoreInformation = moreInformation;
            this.authenticateHeader = authenticateHeader;
        }

        public TokenEndpointProtocolException(Exception innerException)
            :base(Protocol.AccessTokenRequestErrorCodes.InvalidRequest, innerException)
        {
            this.Error = Protocol.AccessTokenRequestErrorCodes.InvalidRequest;
        }

        public string Error { get; private set; }

        public string Description { get; private set; }
        public Uri MoreInformation { get; private set; }

        public IDirectResponseProtocolMessage GetResponse()
        {
            var response = this.requestMessage != null ? new AccessTokenFailedResponse(this.requestMessage, this.authenticateHeader != null)
                : new AccessTokenFailedResponse();
            response.Error = this.Error;
            response.ErrorDescription = this.Description;
            response.ErrorUri = this.MoreInformation;
            if(this.authenticateHeader != null)
            {
                response.Headers.Add(HttpRequestHeaders.WwwAuthenticate, this.authenticateHeader);
            }

            return response;
        }
    }

}

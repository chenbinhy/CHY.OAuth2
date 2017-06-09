using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.Messages
{
    public class UnauthorizedResponse:MessageBase,IHttpDirectResponse
    {
        private readonly WebHeaderCollection headers = new WebHeaderCollection();

        protected UnauthorizedResponse(Version version = null)
            :base(version??Protocol.Default.Version)
        {

        }

        protected UnauthorizedResponse(IDirectedProtocolMessage request)
            :base(request)
        {

        }

        public HttpStatusCode HttpStatusCode { get; set; }

        public WebHeaderCollection Headers
        {
            get { return this.headers; }
        }

        [MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.ErrorCode)]
        public string ErrorCode { get; set; }

        [MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.ErrorDescription)]
        public string ErrorDescription { get; set; }

        [MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.ErrorUri)]
        public Uri ErrorUri { get; set; }

        [MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.Realm)]
        public string Realm { get; set; }

        [MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.Scope, Encoder=typeof(ScopeEncoder))]
        public HashSet<string> Scope { get; set; }

        public virtual string Scheme
        {
            get {return Protocol.BearerHttpAuthorizationScheme;}
        }

        public static UnauthorizedResponse InvalidRequest(ProtocolException exception, Version version = null)
        {
            var message = new UnauthorizedResponse(version)
            {
                ErrorCode = Protocol.BearerTokenErrorCodes.InvalidRequest,
                ErrorDescription = exception.Message,
                HttpStatusCode = HttpStatusCode.BadRequest,
            };

            return message;
        }

        public static UnauthorizedResponse InvalidToken(IDirectedProtocolMessage request, ProtocolException exception)
        {
            var message = new UnauthorizedResponse(request){
                ErrorCode = Protocol.BearerTokenErrorCodes.InvalidToken,
                ErrorDescription = exception.Message,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };

            return message;
        }

        public static UnauthorizedResponse InsufficientScope(IDirectedProtocolMessage request, HashSet<string> requiredScopes)
        {
            var message = new UnauthorizedResponse(request)
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
                ErrorCode = Protocol.BearerTokenErrorCodes.InsufficientScope,
                Scope = requiredScopes,
            };
            return message;
        }

        protected override void EnsureValidMessage()
        {
 	            base.EnsureValidMessage();
            VerifyErrorCodeOrDescription(this.ErrorCode, Protocol.BearerTokenUnauthorizedResponseParameters.ErrorCode);
            VerifyErrorCodeOrDescription(this.ErrorDescription, Protocol.BearerTokenUnauthorizedResponseParameters.ErrorDescription);
            VerifyErrorUri(this.ErrorUri);
            ErrorUtilities.VerifyProtocol(
				this.ErrorCode != null || this.ErrorDescription != null || this.ErrorUri != null || this.Realm != null || this.Scope != null,
				OAuthStrings.BearerTokenUnauthorizedAtLeastOneParameterRequired);
        }

        private static void VerifyErrorCodeOrDescription(string value, string parameterName)
        {
            if(value != null)
            {
                for(int i = 0;i <value.Length; i++)
                {
                    char ch = value[i];
                    if(!((ch >= '\x20' && ch <= '\x21') || (ch >= '\x23' && ch <= '\x5B') || (ch >= '\x5D' && ch <= '\x7E')))
                    {
                        ErrorUtilities.ThrowProtocol(OAuthStrings.ParameterContainsIllegalCharacters, parameterName, ch);
                    }
                }
            }
        }

        private static void VerifyErrorUri(Uri valueUri)
        {
            if(valueUri != null)
            {
                ErrorUtilities.VerifyProtocol(valueUri.IsAbsoluteUri, OAuthStrings.AbsoluteUriRequired);
                string value = valueUri.AbsoluteUri;
                for(int i = 0; i  < value.Length; i++)
                {
                    char ch = value[i];
                    if(!(ch == '\x21' || (ch >= '\x23' && ch <= '\x5B') || (ch >= '\x5D' && ch <= '\x7E')))
                    {
                        ErrorUtilities.ThrowProtocol(OAuthStrings.ParameterContainsIllegalCharacters, Protocol.BearerTokenUnauthorizedResponseParameters.ErrorUri, ch);
                    }
                }
            }
        }
    }
}

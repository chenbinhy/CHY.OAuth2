using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
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
    public class AccessTokenSuccessResponse:MessageBase, IHttpDirectResponse, IAccessTokenIssuingResponse
    {
        public AccessTokenSuccessResponse(AccessTokenRequestBase request)
            :base(request)
        {
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            this.TokenType = Protocol.AccessTokenTypes.Bearer;
        }

        HttpStatusCode IHttpDirectResponse.HttpStatusCode
        {
            get { return HttpStatusCode.OK; }
        }

        WebHeaderCollection IHttpDirectResponse.Headers
        {
            get
            {
                return new WebHeaderCollection{
                    {HttpResponseHeader.CacheControl, "no-store"},
                    {HttpResponseHeader.Pragma, "no-cache"},
                };
            }
        }

        [MessagePart(Protocol.access_token, IsRequired=true)]
        public string AccessToken { get;  set; }

        [MessagePart(Protocol.token_type, IsRequired = false)]
        public string TokenType { get;  set; }

        [MessagePart(Protocol.expires_in, IsRequired =false, Encoder=typeof(TimespanSecondsEncoder))]
        public TimeSpan? Lifetime { get;  set; }

        [MessagePart(Protocol.refresh_token, IsRequired=false)]
        public string RefreshToken {get;  set;}

        [MessagePart(Protocol.scope, IsRequired=false, Encoder = typeof(ScopeEncoder))]
        public HashSet<string> Scope {get; private set;}

        TimeSpan? IAccessTokenIssuingResponse.Lifetime
        {
            get {return this.Lifetime;}
            set {this.Lifetime = value;}
        }

        IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription{
            get {return ((IAccessTokenCarryingRequest)this).AuthorizationDescription;}
        }

        AccessToken IAccessTokenCarryingRequest.AuthorizationDescription { get; set; }

        string IAccessTokenCarryingRequest.AccessToken{
            get {return this.AccessToken;}
            set {this.AccessToken = value;}
        }

        public bool HasRefreshToken {get;set;}

        protected override void EnsureValidMessage()
        {
 	        base.EnsureValidMessage();
            ErrorUtilities.VerifyProtocol(!this.HasRefreshToken || !(this.OriginatingRequest is AccessTokenClientCredentialsRequest), ClientAuthorizationStrings.RefreshTokenInappropriateForRequestType, this.OriginatingRequest.GetType().Name);
        }
    }
}

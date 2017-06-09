using CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.Messages
{
    public class AccessTokenAuthorizationCodeRequestAS:AccessTokenAuthorizationCodeRequest, IAuthorizationCodeCarryingRequest
    {
        public AccessTokenAuthorizationCodeRequestAS(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {

        }

        string IAuthorizationCodeCarryingRequest.Code
        {
            get { return this.AuthorizationCode; }
            set { this.AuthorizationCode = value; }
        }

        AuthorizationCode IAuthorizationCodeCarryingRequest.AuthorizationDescription { get; set; }

        IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription
        {
            get { return ((IAuthorizationCodeCarryingRequest)this).AuthorizationDescription; }
        }
    }
}

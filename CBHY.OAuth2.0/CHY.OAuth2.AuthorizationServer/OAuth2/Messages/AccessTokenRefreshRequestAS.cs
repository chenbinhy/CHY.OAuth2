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
    public class AccessTokenRefreshRequestAS:AccessTokenRefreshRequest, IRefreshTokenCarryingRequest
    {
        public AccessTokenRefreshRequestAS(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {

        }

        string IRefreshTokenCarryingRequest.RefreshToken
        {
            get { return this.RefreshToken; }
            set { this.RefreshToken = value; }
        }

        RefreshToken IRefreshTokenCarryingRequest.AuthorizationDescription { get; set; }

        IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription
        {
            get { return ((IRefreshTokenCarryingRequest)this).AuthorizationDescription; }
        }
    }
}

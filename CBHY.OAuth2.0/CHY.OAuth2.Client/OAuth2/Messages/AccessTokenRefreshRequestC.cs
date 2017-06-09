using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Client.OAuth2.Messages
{
    public class AccessTokenRefreshRequestC:AccessTokenRefreshRequest
    {
        public AccessTokenRefreshRequestC(AuthorizationServerDescription authorizationServer)
            :base(authorizationServer.TokenEndpoint, authorizationServer.Version)
        {
        }
    }
}

using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.Messages
{
    public class AccessTokenResult:IAccessTokenResult
    {
        public AccessTokenResult(AuthorizationServerAccessToken accessToken)
        {
            this.AllowRefreshToken = true;
            this.AccessToken = accessToken;
        }

        public bool AllowRefreshToken { get; set; }

        public AuthorizationServerAccessToken AccessToken { get; private set; }

        AccessToken IAccessTokenResult.AccessToken
        {
            get { return this.AccessToken; }
        }
    }
}

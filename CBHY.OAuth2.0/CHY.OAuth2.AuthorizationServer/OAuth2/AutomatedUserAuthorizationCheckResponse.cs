using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2
{
    public class AutomatedUserAuthorizationCheckResponse:AutomatedAuthorizationCheckResponse
    {
        public AutomatedUserAuthorizationCheckResponse(IAccessTokenRequest accessRequest, bool approved, string canonicalUserName)
            :base(accessRequest, approved)
        {
            this.CanonicalUserName = canonicalUserName;
        }

        public string CanonicalUserName { get; private set; }
    }
}

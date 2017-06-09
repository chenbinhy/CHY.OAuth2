using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2
{
    public class AutomatedAuthorizationCheckResponse
    {
        public AutomatedAuthorizationCheckResponse(IAccessTokenRequest accessRequest, bool approved)
        {
            this.IsApproved = approved;
            this.ApprovedScope = new HashSet<string>(accessRequest.Scope);
        }

        public bool IsApproved { get; private set; }

        public HashSet<string> ApprovedScope { get; private set; }
    }
}

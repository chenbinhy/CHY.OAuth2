using CHY.OAuth2.OAuth2;
using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Client.OAuth2
{
    [Serializable]
    public class AuthorizationState:IAuthorizationState
    {
        public AuthorizationState(IEnumerable<string> scopes = null)
        {
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            if(scopes != null)
            {
                this.Scope.AddRange(scopes);
            }
        }

        public Uri Callback { get; set; }

        public string RefreshToken { get; set; }

        public string AccessToken { get; set; }

        public DateTime? AccessTokenExpirationUtc { get; set; }

        public DateTime? AccessTokenIssueDateUtc { get; set; }

        public HashSet<string> Scope { get; private set; }

        public bool IsDeleted { get; set; }
        public virtual void Delete()
        {
            this.IsDeleted = true;
        }
        public virtual void SaveChanges()
        {

        }
    }
}

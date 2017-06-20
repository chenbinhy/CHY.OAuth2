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

        /// <summary>
        /// 回调地址
        /// </summary>
        public Uri Callback { get; set; }

        /// <summary>
        /// 刷新token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? AccessTokenExpirationUtc { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? AccessTokenIssueDateUtc { get; set; }

        /// <summary>
        /// 授权范围
        /// </summary>
        public HashSet<string> Scope { get; private set; }

        /// <summary>
        /// 是否删除
        /// </summary>
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

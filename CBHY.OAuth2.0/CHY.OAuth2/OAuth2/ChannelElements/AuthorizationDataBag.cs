using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.ChannelElements
{
    /// <summary>
    /// 授权数据袋
    /// </summary>
    public abstract class AuthorizationDataBag:DataBag, IAuthorizationDescription
    {
        protected AuthorizationDataBag()
        {
            this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
        }

        /// <summary>
        /// 客户端标识
        /// </summary>
        [MessagePart]
        public string ClientIdentifier { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime UtcIssued
        {
            get { return this.UtcCreationDate; }
        }

        /// <summary>
        /// 用户
        /// </summary>
        [MessagePart]
        public string User { get; set; }

        /// <summary>
        /// 范围
        /// </summary>
        [MessagePart(Encoder=typeof(ScopeEncoder))]
        public HashSet<string> Scope { get; private set; }
    }
}

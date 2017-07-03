using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    /// <summary>
    /// 信息
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 版本
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// 附加数据
        /// </summary>
        IDictionary<string, string> ExtraData { get; }

        /// <summary>
        /// 验证信息
        /// </summary>
        void EnsureValidMessage();
    }
}

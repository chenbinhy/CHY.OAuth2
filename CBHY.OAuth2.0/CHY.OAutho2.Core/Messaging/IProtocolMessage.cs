using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    /// <summary>
    /// 协议数据
    /// </summary>
    public interface IProtocolMessage:IMessage
    {
        /// <summary>
        /// 保护类型
        /// </summary>
        MessageProtections RequiredProtection { get; }

        /// <summary>
        /// 传输类型
        /// </summary>
        MessageTransport Transport { get; }
    }
}

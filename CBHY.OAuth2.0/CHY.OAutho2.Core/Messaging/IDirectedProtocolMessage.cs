using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    /// <summary>
    /// 直接协议信息
    /// </summary>
    public interface IDirectedProtocolMessage:IProtocolMessage
    {
        /// <summary>
        /// 传输方式
        /// </summary>
        HttpDeliveryMethods HttpMethods { get; }

        /// <summary>
        /// 接收地址
        /// </summary>
        Uri Recipient { get; }
    }
}

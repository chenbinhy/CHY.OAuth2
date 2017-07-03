using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    /// <summary>
    /// 格式化数据袋
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataBagFormatter<in T> where T:DataBag
    {
        string Serialize(T message);

        void Deserialize(T message, string data, IProtocolMessage containingMessage = null, string messagePartName = null);
    }
}

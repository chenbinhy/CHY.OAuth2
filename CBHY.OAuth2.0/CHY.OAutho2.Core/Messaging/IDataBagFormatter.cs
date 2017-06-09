using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public interface IDataBagFormatter<in T> where T:DataBag
    {
        string Serialize(T message);

        void Deserialize(T message, string data, IProtocolMessage containingMessage = null, string messagePartName = null);
    }
}

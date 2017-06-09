using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public interface IMessageFactory
    {
        IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields);

        IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields);
    }
}

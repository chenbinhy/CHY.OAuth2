using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public interface IChannelBindingElement
    {
        Channel Channel { get; set; }

        MessageProtections Protection { get; }

        MessageProtections? ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken);

        MessageProtections? ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken);
    }
}

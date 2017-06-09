using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class ChannelEventArgs:EventArgs
    {
        public ChannelEventArgs(IProtocolMessage message)
        {
            this.Message = message;
        }

        public IProtocolMessage Message { get; private set; }
    }
}

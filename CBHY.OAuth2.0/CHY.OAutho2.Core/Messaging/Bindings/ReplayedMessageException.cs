using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Bindings
{
    public class ReplayedMessageException : ProtocolException
    {
        public ReplayedMessageException(IProtocolMessage faultedMessage) : base(MessagingStrings.ReplayAttackDetected, faultedMessage) { }

        protected ReplayedMessageException(
            SerializationInfo info,
            StreamingContext context
            ):base(info, context)
        {

        }
    }
}

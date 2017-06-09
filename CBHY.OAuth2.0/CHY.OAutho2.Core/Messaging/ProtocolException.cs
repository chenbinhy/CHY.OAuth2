using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class ProtocolException:Exception
    {
        public ProtocolException()
        {

        }

        public ProtocolException(string message):base(message)
        {

        }

        public ProtocolException(string message, Exception inner):base(message, inner)
        {

        }

        public ProtocolException(string message, IProtocolMessage faultedMessage, Exception innerException = null)
            :base(message, innerException)
        {
            this.FaultedMessage = faultedMessage;
        }

        protected ProtocolException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
            throw new NotImplementedException();
        }

        public IProtocolMessage FaultedMessage { get; private set; }
    }
}

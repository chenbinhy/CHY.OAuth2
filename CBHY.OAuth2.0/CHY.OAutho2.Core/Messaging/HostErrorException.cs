using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class HostErrorException : Exception
    {
        public HostErrorException()
        {

        }

        public HostErrorException(string message)
            : base(message)
        {

        }

        public HostErrorException(string message, Exception inner)
            : base(message, inner)
        { }

        protected HostErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

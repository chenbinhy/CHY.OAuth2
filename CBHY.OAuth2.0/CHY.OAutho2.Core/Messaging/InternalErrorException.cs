using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException() { }

        public InternalErrorException(string message) : base(message) { }

        public InternalErrorException(string message, Exception inner) : base(message, inner) { }

        protected InternalErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

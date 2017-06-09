using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class HttpResponseMessageWithOriginal : HttpResponseMessage
    {
        public HttpResponseMessageWithOriginal(IMessage originalMessage, HttpStatusCode statusCode = HttpStatusCode.OK)
            :base(statusCode)
        {
            this.OriginalMessage = originalMessage;
        }

        public IMessage OriginalMessage { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public interface IHttpDirectResponse
    {
        HttpStatusCode HttpStatusCode { get; }
        WebHeaderCollection Headers { get; }
    }
}

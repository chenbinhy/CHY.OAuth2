using CHY.OAuth2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Common
{
    public class DefaultOAuthHostFactories:IHostFactories
    {
        public virtual HttpMessageHandler CreateHttpMessageHandler()
        {
            var handler = new HttpClientHandler();
            return handler;
        }

        public HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            handler = handler ?? this.CreateHttpMessageHandler();
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.Add(Util.LibraryVersionHeader);
            return client;
        }
    }
}

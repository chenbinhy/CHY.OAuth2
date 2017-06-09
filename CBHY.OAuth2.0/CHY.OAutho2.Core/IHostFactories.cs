using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core
{
    public interface IHostFactories
    {
        HttpMessageHandler CreateHttpMessageHandler();

        HttpClient CreateHttpClient(HttpMessageHandler handler = null);
    }
}

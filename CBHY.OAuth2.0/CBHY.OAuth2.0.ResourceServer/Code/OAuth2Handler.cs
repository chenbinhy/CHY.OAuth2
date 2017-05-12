using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;

namespace CBHY.OAuth2.OAuthResourceServer.Code
{
    public class OAuth2Handler:DelegatingHandler
    {
        private readonly ResourceServer m_ResourceServer;

        public OAuth2Handler(ResourceServer resourceServer)
        {
            m_ResourceServer = resourceServer;
        }
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            HttpContextBase httpContext;
            string userName;
            HashSet<string> scope;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
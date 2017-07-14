using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CHY.OAuth2.Core.Messaging;

namespace CHY.OAuth2.Mvc
{
    public static class MvcExtensions
    {
        public static ActionResult AsActionResult(this HttpResponseMessage response)
        {
            return new HttpResponseMessageActionResult(response);
        }

        private class HttpResponseMessageActionResult : ActionResult
        {
            private readonly HttpResponseMessage response;

            internal HttpResponseMessageActionResult(HttpResponseMessage response)
            {
                this.response = response;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                this.response.SendAsync(context.HttpContext);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBHY.OAuth2.AuthorizetionServer.Code
{
    public class HttpHeaderAttribute:ActionFilterAttribute
    {
        public HttpHeaderAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.AppendHeader(this.Name, this.Value);
            base.OnResultExecuted(filterContext);
        }
    }
}
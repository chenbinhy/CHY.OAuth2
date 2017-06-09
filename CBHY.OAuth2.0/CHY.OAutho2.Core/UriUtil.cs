using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace CHY.OAuth2.Core
{
    public static class UriUtil
    {
        public static bool QueryStringContainPrefixedParameters(this Uri uri, string prefix)
        {
            if(uri == null)
            {
                return false;
            }
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            Assumes.True(nvc != null);

            return nvc.Keys.OfType<string>().Any(key => key.StartsWith(prefix, StringComparison.Ordinal));
        }

        public static bool IsTransportSecure(this Uri uri)
        {
            return string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        }

        public static string ToStringWithImpliedPorts(this UriBuilder builder)
        {
            if((builder.Port == 80 && string.Equals(builder.Scheme, "http", StringComparison.OrdinalIgnoreCase)) ||
                (builder.Port == 443 && string.Equals(builder.Scheme, "https", StringComparison.OrdinalIgnoreCase)))
            {
                string url = builder.ToString();
                string result = Regex.Replace(url, @"^(https?://[^:]+):\d+", m => m.Groups[1].Value, RegexOptions.IgnoreCase);
                Assumes.True(result != null);
                return result;
            }
            else
            {
                return builder.ToString();
            }
        }

        public static void ValidateResolvableUrl(Page page, bool designMode, string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return;
            }
            if(page != null && !designMode)
            {
                Assumes.True(page.Request != null);
                string relativeUrl = page.ResolveClientUrl(value);
                Assumes.True(page.Request.Url != null);
                Assumes.True(relativeUrl != null);
                new Uri(page.Request.Url, relativeUrl);
            }
            else
            {
                if(Regex.IsMatch(value, @"^https?://"))
                {
                    new Uri(value);
                }
                else if(value.StartsWith("~/", StringComparison.Ordinal))
                {

                }
                else
                {
                    throw new UriFormatException();
                }
            }
        }
    }
}

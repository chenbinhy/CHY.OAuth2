using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBHY.OAuth2.AuthorizetionServer.Code
{
    internal static class Utilities
    {
        internal static DateTime AsUtc(this DateTime value)
        {
            if(value.Kind == DateTimeKind.Unspecified)
            {
                return new DateTime(value.Ticks, DateTimeKind.Utc);
            }

            return value.ToUniversalTime();
        }
    }
}
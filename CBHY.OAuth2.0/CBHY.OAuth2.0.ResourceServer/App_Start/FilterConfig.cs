using System.Web;
using System.Web.Mvc;

namespace CBHY.OAuth2.OAuthResourceServer
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

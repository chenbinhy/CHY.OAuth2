using System.Web;
using System.Web.Mvc;

namespace CBHY.OAuth2.AuthorizetionServer
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

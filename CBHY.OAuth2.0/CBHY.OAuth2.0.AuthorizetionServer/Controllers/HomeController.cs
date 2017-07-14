using CBHY.OAuth2.AuthorizetionServer.Code;
using CHY.BaseFramework.Authorization;
using CHY.BaseFramework.DAL;
using OAuth2.Models.CHYAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CBHY.OAuth2.AuthorizetionServer.Controllers
{
    public class HomeController : Controller
    {
        private IRepository<User> m_UserRep;

        public HomeController(IRepository<User> userRep)
        {
            m_UserRep = userRep;
        }

        [GlobalAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogOn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        public string Test()
        {
            return "";
        }

        public ActionResult AjaxLogOn(string returnUrl)
        {
            string name = "chenbin";
            string displayName = "陈斌";

            var user = m_UserRep.Get(s => s.OpenIDClaimedIdentifier == name);
            if (user == null)
            {
                m_UserRep.Add(new User() { OpenIDClaimedIdentifier = name, OpenIDFriendlyIdentifier = displayName });
            }
            SystemUser systemUser = new SystemUser() { Email = "chenbin@qianduan.com", LoginTime = DateTime.Now, Mobile = "13450260080", RealName = "陈斌", UserId = "1", UserName = "chenbin", UserType = 1 };
            AuthorizeHelper.SignIn(systemUser);
            //FormsAuthentication.SetAuthCookie("sampleconsumer", false);

            return this.Redirect(returnUrl ?? Url.Action("Index", "Home"));
        }
      
    }
}
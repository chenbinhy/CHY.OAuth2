using CHY.BaseFramework.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CHY.BaseFramework;

namespace CBHY.OAuth2.AuthorizetionServer.Code
{
    public class GlobalAuthorizeAttribute:AuthorizeAttribute
    {
        private string m_ActionUrl;
        private string m_RawUrl;
        private string m_UnauthorizedMessage;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if(httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            // 判断是否超级管理员
            var user = AuthorizeHelper.GetCurrentUser();
            if(user == null)
            {
                return false;// AuthResult(false, httpContext, 401);
            }

            //if(user.UserType == (int)UserType.超级管理员)
            //{
            //    return this.permissionService.IsSupperAdmin(user.UserId);
            //}

            //var permissions = this.permissionService.GetPermissionCache(user.UserId);

            //// 判断菜单权限
            //if(!HasMenuPermission(permissions))
            //{
            //    return AuthResult(false, httpContext, 403);
            //}

            //// 判断Action权限
            //if(!HasActionPermission(permissions))
            //{
            //    return AuthResult(false, httpContext, 403);
            //}

            return true;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //this.m_ActionUrl = PermissionUtil.CurrentPermissionUrl(filterContext);
            //this.m_RawUrl = PermissionUtil.CurrentUrl(filterContext);

            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = GetJsonResult("您的登陆状态已超时，请重新登录");
                return;
            }
            if(filterContext.HttpContext.Response.StatusCode == 401 && filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = GetJsonResult("您的登陆状态已超时，请重新登录");
                return;
            }
            if(filterContext.HttpContext.Response.StatusCode == 403)
            {
                filterContext.Result = new ContentResult{ Content=m_UnauthorizedMessage};
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }

        private bool HasMenuPermission(string permissions)
        {
            //var menus = this.permissionService.GetAllMenusCached();
            //var url = ",{0},".Fmt(this.m_RawUrl);
            //if(!menus.Containers(url) || this.m_RawUrl == "/")
            //{
            //    return true;
            //}
            //if(!permissions.Contains(url))
            //{
            //    m_UnauthorizedMessage = "您没有访问菜单{0}的权限".Fmt(url.Trim(','));
            //    return false;
            //}

            return true;
        }

        private bool HasActionPermission(string permissions)
        {
            //var code = string.Empty;
            //if(!PermissionUtil.PermissionUrls.TryGetValue(this.m_ActionUrl, out code))
            //{
            //    return true;
            //}
            //if(!permissions.Contains(",{0},".Fmt(code))
            //{
            //    m_UnauthorizedMessage = "您没有权限({0})".Fmt((PermCode)int.Parse(code));

            //    return false;
            //}

            return true;
        }

        private bool AuthResult(bool result, HttpContextBase httpContext, int statusCode)
        {
            httpContext.Response.StatusCode = statusCode;

            return result;
        }

        private JsonResult GetJsonResult(string message)
        {
            return new JsonResult
            {
                Data = new { Success=false, Message=message},
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
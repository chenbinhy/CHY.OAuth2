﻿using CBHY.OAuth2.Models;
using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CBHY.OAuth2.Controllers
{
    public class CusAuthController : Controller
    {
        private List<AuthItemModel> m_AuthItemModelList;
        /// <summary>
        /// The OAuth 2.0 client object to use to obtain authorization and authorize outgoing HTTP requests.
        /// </summary>
        private static readonly WebServerClient Client;

        /// <summary>
        /// The details about the sample OAuth-enabled WCF service that this sample client calls into.
        /// </summary>
        private static AuthorizationServerDescription authServerDescription = new AuthorizationServerDescription
        {
            TokenEndpoint = new Uri("https://authorizationserver.oauth.com/OAuth/Token"),
            AuthorizationEndpoint = new Uri("https://authorizationserver.oauth.com/OAuth/Authorize"),
        };

        /// <summary>
        /// Gets or sets the authorization details for the logged in user.
        /// </summary>
        /// <value>The authorization details.</value>
        /// <remarks>
        /// Because this is a sample, we simply store the authorization information in memory with the user session.
        /// A real web app should store at least the access and refresh tokens in this object in a database associated with the user.
        /// </remarks>
        private IAuthorizationState Authorization
        {
            get { return (AuthorizationState)Session["Authorization"]; }
            set { Session["Authorization"] = value;}
        }

        static CusAuthController()
        {
            Client = new WebServerClient(authServerDescription, "sampleconsumer", "samplesecret");
        }

        public CusAuthController()
        {
            m_AuthItemModelList = new List<AuthItemModel>() { 
                new AuthItemModel(){ Name="获取名称", Item="https://resourceserver.oauth.com/api/Person/Name"},
                new AuthItemModel(){ Name="获取年龄", Item="https://resourceserver.oauth.com/api/Person/Age"},
                new AuthItemModel(){ Name="获取邮箱", Item="https://resourceserver.oauth.com/api/Person/Email"}
            };
            ViewBag.AuthItemModelList = m_AuthItemModelList;
        }

        // GET: CusAuth
        public ActionResult Index()
        {
           

            return View();
        }

        public void GetAuthorization(string code)
        {
             if(!string.IsNullOrEmpty(code))
            {
                var authorization = Client.ProcessUserAuthorization();
                if (authorization != null)
                {
                    // We are receiving an authorization response.  Store it and associate it with this user.
                    Authorization = authorization;
                    Response.Redirect(Request.Path); // get rid of the /?code= parameter
                }
            }

            if(Authorization != null)
            {
                foreach(var li in m_AuthItemModelList.Where(s => Authorization.Scope.Contains(s.Item)))
                {
                    li.IsAuthed = true;
                }
                if(Authorization.AccessTokenExpirationUtc.HasValue)
                {
                    TimeSpan timeLeft = Authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
                    ViewBag.ExpireTime = string.Format(CultureInfo.CurrentCulture, "(access token expires in {0} minutes)", Math.Round(timeLeft.TotalMinutes, 1));
                }
            }
            string[] scopes = new string[] { "https://resourceserver.oauth.com/api/Person/Name", "https://resourceserver.oauth.com/api/Person/Age" };
            Client.RequestUserAuthorization(scopes);
        }

        public JsonResult GetName()
        {
            string result = this.CallService("https://resourceserver.oauth.com/api/Person/Name");

            return Json(result);
        }

        private string CallService(string url)
        {
            if (Authorization == null)
            {
                throw new InvalidOperationException("No access token!");
            }

            if(Authorization.AccessTokenExpirationUtc.HasValue)
            {
                // 刷新access token
                Client.RefreshAuthorization(Authorization, TimeSpan.FromSeconds(30));
            }

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            ClientBase.AuthorizeRequest(httpRequest, Authorization.AccessToken);
            var response = httpRequest.GetResponse();
            string result = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}
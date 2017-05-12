using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using CBHY.OAuth2.AuthorizetionServer.Code;
using System.Net;
using CHY.BaseFramework.DAL;
using OAuth2.Models.CHYAuth;
using OAuthAuthorizationServer.Code;
using CBHY.OAuth2.AuthorizetionServer.Models;
using CHY.BaseFramework.Authorization;

namespace CBHY.OAuth2.AuthorizetionServer.Controllers
{
        [GlobalAuthorize]
    public class OAuthController : Controller
    {
        private AuthorizationServer m_AuthorizationServer;
        private IRepository<Client> m_ClientRep;
        private IRepository<User> m_UserRep;
        private IRepository<ClientAuthorization> m_ClientAuthRep;

        public OAuthController(AuthorizationServer authorizationServer, IRepository<Client> clientRep,
            IRepository<User> userRep, IRepository<ClientAuthorization> clientAuthRep)
        {
            m_AuthorizationServer = authorizationServer;
            m_ClientRep = clientRep;
            m_UserRep = userRep;
            m_ClientAuthRep = clientAuthRep;
        }

        public ActionResult Token()
        {
            return this.m_AuthorizationServer.HandleTokenRequest(this.Request).AsActionResult();
        }

        public ActionResult Authorize()
        {
            var pendingRequest = this.m_AuthorizationServer.ReadAuthorizationRequest();
            if(pendingRequest == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request");
            }
            var clientInfo = m_ClientRep.Get(s => s.ClientIdentifier == pendingRequest.ClientIdentifier);

            if(((OAuth2AuthorizationServer)this.m_AuthorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest))
            {
                var approval = this.m_AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);

                return this.m_AuthorizationServer.Channel.PrepareResponse(approval).AsActionResult();
            }

            var model = new AccountAuthorizeModel
            {
                ClientApp = clientInfo.Name,
                Scope = pendingRequest.Scope,
                AuthorizationRequest = pendingRequest
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AuthorizeResponse(bool isApproved)
        {
            var pendingRequest = this.m_AuthorizationServer.ReadAuthorizationRequest();
            if (pendingRequest == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
            }
            IDirectedProtocolMessage response;
            if (isApproved)
            {
                var clientInfo = m_ClientRep.Get(s => s.ClientIdentifier == pendingRequest.ClientIdentifier);
                var user = AuthorizeHelper.GetCurrentUser();
                var userInfo = m_UserRep.Get(s => s.OpenIDClaimedIdentifier == user.UserName);
                m_ClientAuthRep.Add(new ClientAuthorization()
                {
                    Scope = OAuthUtilities.JoinScopes(pendingRequest.Scope),
                    UserId = userInfo.UserId,
                    CreatedOnUtc = DateTime.UtcNow,
                    ClientId = clientInfo.ClientId
                });

                response = this.m_AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, user.UserName);
            }
            else
            {
                response = this.m_AuthorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
            }

            return this.m_AuthorizationServer.Channel.PrepareResponse(response).AsActionResult();
        }
    }
}
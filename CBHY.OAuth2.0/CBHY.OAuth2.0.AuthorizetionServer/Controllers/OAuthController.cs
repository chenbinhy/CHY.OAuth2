using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CBHY.OAuth2.AuthorizetionServer.Code;
using CHY.OAuth2.Core.Messaging;
using System.Net;
using CHY.BaseFramework.DAL;
using OAuth2.Models.CHYAuth;
using OAuthAuthorizationServer.Code;
using CBHY.OAuth2.AuthorizetionServer.Models;
using CHY.BaseFramework.Authorization;
using CHY.OAuth2.AuthorizationServer.OAuth2;
using System.Net.Http;
using CHY.OAuth2.Mvc;
using System.Threading.Tasks;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.OAuth2;

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

        public async Task<ActionResult> Token()
        {
            var request = await m_AuthorizationServer.HandleTokenRequestAsync(this.Request, this.Response.ClientDisconnectedToken);
            Response.ContentType = request.Content.Headers.ContentType.ToString();
            return request.AsActionResult();
        }

        public async Task<ActionResult> Authorize()
        {
            EndUserAuthorizationRequest pendingRequest = await this.m_AuthorizationServer.ReadAuthorizationRequestAsync(Request, Response.ClientDisconnectedToken);
            if(pendingRequest == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request");
            }
            var clientInfo = m_ClientRep.Get(s => s.ClientIdentifier == pendingRequest.ClientIdentifier);

            if(((OAuth2AuthorizationServer)this.m_AuthorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest))
            {
                var approval = this.m_AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);

                var response = await this.m_AuthorizationServer.Channel.PrepareResponseAsync(approval, Response.ClientDisconnectedToken);
                Response.ContentType = response.Content.Headers.ContentType.ToString();
                return response.AsActionResult();
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
        public async Task<ActionResult> AuthorizeResponse(bool isApproved)
        {
            var pendingRequest = await this.m_AuthorizationServer.ReadAuthorizationRequestAsync(Request, Response.ClientDisconnectedToken);
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
            var res = await this.m_AuthorizationServer.Channel.PrepareResponseAsync(response, Response.ClientDisconnectedToken);
            Response.ContentType = res.Content.Headers.ContentType.ToString();
            return res.AsActionResult();
        }
    }
}
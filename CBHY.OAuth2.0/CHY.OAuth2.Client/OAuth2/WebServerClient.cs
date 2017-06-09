using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Net.Http.Headers;
using CHY.OAuth2.Client.OAuth2.Messages;

namespace CHY.OAuth2.Client.OAuth2
{
    /// <summary>
    /// Web应用程序客户端
    /// </summary>
    public class WebServerClient:ClientBase
    {
        private const string XsrfCookieName = "DotNetOpenAuth.WebServerClient.XSRF-Session";

        public WebServerClient(AuthorizationServerDescription authorizationServer,string clientIdentifier = null, string clientSecret = null, IHostFactories hostFactories = null)
            :base(authorizationServer, clientIdentifier, DefaultSecretApplicator(clientSecret), hostFactories)
        {

        }

        public WebServerClient(AuthorizationServerDescription authorizationServer, string clientIdentifier, ClientCredentialApplicator clientCredentialApplicator, IHostFactories hostFactories = null)
            :base(authorizationServer, clientIdentifier, clientCredentialApplicator, hostFactories)
        {

        }

        public IClientAuthorizationTracker AuthorizationTracker { get; set; }

        public Task<HttpResponseMessage> PrepareRequestUserAuthorizationAsync(IEnumerable<string> scopes = null, Uri returnTo = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var authorizationState = new AuthorizationState(scopes)
            {
                Callback = returnTo
            };
            return this.PrepareRequestUserAuthorizationAsync(authorizationState, cancellationToken);
        }

        public async Task<HttpResponseMessage> PrepareRequestUserAuthorizationAsync(IAuthorizationState authorization, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(authorization.Callback == null)
            {
                authorization.Callback = this.Channel.GetRequestFromContext().GetPublicFacingUrl()
                    .StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(EndUserAuthorizationSuccessResponseBase), Protocol.Default.Version))
                    .StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(typeof(EndUserAuthorizationFailedResponse), Protocol.Default.Version));
                authorization.SaveChanges();
            }
            var request = new EndUserAuthorizationRequestC(this.AuthorizationServer)
            {
                ClientIdentifier = this.ClientIdentifier,
                Callback = authorization.Callback
            };
            request.Scope.ResetContents(authorization.Scope);
            var cookies = new List<CookieHeaderValue>();
            if(this.AuthorizationTracker == null)
            {
                string xsrfKey = MessagingUtilities.GetNonCryptoRandomDataAsBase64(16, useWeb64: true);
                cookies.Add(new CookieHeaderValue(XsrfCookieName, xsrfKey)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                });
                request.ClientState = xsrfKey;
            }
            var response = await this.Channel.PrepareResponseAsync(request, cancellationToken);
            response.Headers.AddCookies(cookies);

            return response;
        }

        public Task<IAuthorizationState> ProcessUserAuthorizationAsync(
            HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            request = request ?? this.Channel.GetRequestFromContext();
            return this.ProcessUserAuthorizationAsync(request.AsHttpRequestMessage(), cancellationToken);
        }

        public async Task<IAuthorizationState> ProcessUserAuthorizationAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await this.Channel.TryReadFromRequestAsync<IMessageWithClientState>(request, cancellationToken);
            if(response != null)
            {
                Uri callback = request.RequestUri.StripMessagePartsFromQueryString(this.Channel.MessageDescriptions.Get(response));
                IAuthorizationState authorizationState;
                if(this.AuthorizationTracker != null)
                {
                    authorizationState = this.AuthorizationTracker.GetAuthorizationState(callback, response.ClientState);
                    ErrorUtilities.VerifyProtocol(authorizationState != null, ClientStrings.AuthorizationResponseUnexpectedMismatch);
                }
                else
                {
                    var xsrfCookieValue = (from cookieHeader in request.Headers.GetCookies()
                                           from cookie in cookieHeader.Cookies
                                           where cookie.Name == XsrfCookieName
                                           select cookie.Value).FirstOrDefault();
                    ErrorUtilities.VerifyProtocol(xsrfCookieValue != null && string.Equals(response.ClientState, xsrfCookieValue, StringComparison.Ordinal), ClientStrings.AuthorizationResponseUnexpectedMismatch);
                    authorizationState = new AuthorizationState { Callback = callback };
                }
                var success = response as EndUserAuthorizationSuccessAuthCodeResponse;
                var failure = response as EndUserAuthorizationFailedResponse;
                ErrorUtilities.VerifyProtocol(success != null || failure != null, MessagingStrings.UnexpectedMessageReceivedOfMany);
                if(success != null)
                {
                    await this.UpdateAuthorizationWithResponseAsync(authorizationState, success, cancellationToken);
                }
                else
                {
                    Logger.OAuth.Info("User refused to grant the requested authorization at the Authorization Server.");
                    authorizationState.Delete();
                }
                return authorizationState;
            }
            return null;
        }
    }
}

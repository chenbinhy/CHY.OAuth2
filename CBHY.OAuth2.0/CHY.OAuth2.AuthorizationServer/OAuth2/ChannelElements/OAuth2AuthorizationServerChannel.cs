using CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CHY.OAuth2.AuthorizationServer.OAuth2.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class OAuth2AuthorizationServerChannel:OAuth2ChannelBase, IOAuth2ChannelWithAuthorizationServer
    {
        private static readonly Type[] MessageTypes = new Type[]{
            typeof(AccessTokenRefreshRequestAS),
            typeof(AccessTokenAuthorizationCodeRequestAS),
            typeof(AccessTokenResourceOwnerPasswordCredentialsRequest),
            typeof(AccessTokenClientCredentialsRequest),
            typeof(EndUserAuthorizationRequest),
            typeof(EndUserAuthorizationImplicitRequest),
            typeof(EndUserAuthorizationFailedResponse),
        };

        public OAuth2AuthorizationServerChannel(IAuthorizationServerHost authorizationServer, ClientAuthenticationModule clientAuthenticationModule)
            : base(MessageTypes, InitializeBindingElements(authorizationServer, clientAuthenticationModule))
        {
            this.AuthorizationServer = authorizationServer;
        }

        public IAuthorizationServerHost AuthorizationServer { get; private set; }

        public IScopeSatisfiedCheck ScopeSatisfiedCheck { get; set; }

        protected override IDictionary<string, string> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response)
        {
            var webResponse = new HttpResponseMessage();
            ApplyMessageTemplate(response, webResponse);
            string json = this.SerializeAsJson(response);
            webResponse.Content = new StringContent(json, Encoding.UTF8, JsonEncoded);

            return webResponse;
        }

        protected override IDirectedProtocolMessage ReadFromRequestCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(!string.IsNullOrEmpty(request.RequestUri.Fragment))
            {
                var fields = HttpUtility.ParseQueryString(request.RequestUri.Fragment.Substring(1)).ToDictionary();
                MessageReceivingEndpoint recipient;
                try
                {
                    recipient = request.GetRecipient();
                }
                catch(ArgumentException ex)
                {
                    Logger.Messaging.WarnFormat("Unrecognized HTTP request: " + ex.ToString());
                    return null;
                }
                return (IDirectedProtocolMessage)this.Receive(fields, recipient);
            }
            var result = base.ReadFromRequestCoreAsync(request, cancellationToken);
            return result;
        }

        private static IChannelBindingElement[] InitializeBindingElements(IAuthorizationServerHost authorizationServer, ClientAuthenticationModule clientAuthenticationModule)
        {
            var bindingElements = new List<IChannelBindingElement>();
            bindingElements.Add(new MessageValidationBindingElement(clientAuthenticationModule));
            bindingElements.Add(new TokenCodeSerializationBindingElement());

            return bindingElements.ToArray();
        }
    }
}

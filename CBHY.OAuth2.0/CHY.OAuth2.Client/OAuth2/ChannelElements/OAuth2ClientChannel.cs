using CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.OAuth2.Messages;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CHY.OAuth2.Client.OAuth2.ChannelElements
{
    public class OAuth2ClientChannel : OAuth2ChannelBase, IOAuth2ChannelWithClient
    {
        private static readonly Type[] MessageTypes = new Type[]{
            typeof(AccessTokenSuccessResponse),
            typeof(AccessTokenFailedResponse),
            typeof(EndUserAuthorizationSuccessAuthCodeResponse),
            typeof(EndUserAuthorizationSuccessAccessTokenResponse),
            typeof(EndUserAuthorizationFailedResponse),
            typeof(UnauthorizedResponse),
        };

        public OAuth2ClientChannel(IHostFactories hostFactories)
            :base(MessageTypes, hostFactories:hostFactories)
        {

        }

        public string ClientIdentifier { get; set; }

        public ClientCredentialApplicator ClientCredentialApplicator { get; set; }

        public XmlDictionaryReaderQuotas JsonReaderQuotas
        {
            get { return this.XmlDictionaryReaderQuotas; }
        }

        protected override HttpRequestMessage CreateHttpRequest(IDirectedProtocolMessage request)
        {
            HttpRequestMessage httpRequest;
            if((request.HttpMethods & HttpDeliveryMethods.GetRequest) != 0)
            {
                httpRequest = InitializeRequestAsGet(request);
            }
            else if((request.HttpMethods & HttpDeliveryMethods.PostRequest) != 0)
            {
                httpRequest = InitializeRequestAsPost(request);
            }
            else
            {
                throw new NotSupportedException();
            }

            return httpRequest;
        }

        protected override async Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, System.Threading.CancellationToken cancellationToken)
        {
            string body = await response.Content.ReadAsStringAsync();
            var contentType = response.Content.Headers.ContentType.MediaType;
            if(contentType == JsonEncoded || contentType == JsonTextEncoded)
            {
                return this.DeserializeFromJson(body);
            }
            else if(contentType == HttpFormUrlEncoded || contentType == PlainTextEncoded)
            {
                return HttpUtility.ParseQueryString(body).ToDictionary();
            }
            else
            {
                throw ErrorUtilities.ThrowProtocol(ClientStrings.UnexpectedResponseContentType, contentType);
            }
        }

        protected override Task<IDirectedProtocolMessage> ReadFromRequestCoreAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.Method, request.RequestUri.AbsoluteUri);
            var fields = HttpUtility.ParseQueryString(request.RequestUri.Query).ToDictionary();
            string fragment = request.RequestUri.Fragment;
            if(!string.IsNullOrEmpty(fragment))
            {
                foreach(var pair in HttpUtility.ParseQueryString(fragment.Substring(1)).AsKeyValuePairs())
                {
                    fields.Add(pair.Key, pair.Value);
                }
            }
            MessageReceivingEndpoint recipient;
            try
            {
                recipient = request.GetRecipient();
            }
            catch(ArgumentException ex)
            {
                Logger.Messaging.WarnFormat("Unrecognized HTTP request:", ex);
                return null;
            }

            return Task.FromResult((IDirectedProtocolMessage)this.Receive(fields, recipient));
        }

        protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response)
        {
            throw new NotImplementedException();
        }

        protected override void PrepareHttpWebRequest(HttpRequestMessage request)
        {
            base.PrepareHttpWebRequest(request);
            if(this.ClientCredentialApplicator != null)
            {
                this.ClientCredentialApplicator.ApplyClientCredential(this.ClientIdentifier, request);
            }
        }
    }
}

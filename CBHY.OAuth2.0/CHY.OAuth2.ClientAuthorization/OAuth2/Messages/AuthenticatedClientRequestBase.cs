using CHY.OAuth2.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.Messages
{
    public abstract class AuthenticatedClientRequestBase:MessageBase, IHttpDirectRequest
    {
        private readonly System.Net.Http.Headers.HttpRequestHeaders headers = new HttpRequestMessage().Headers;

        protected AuthenticatedClientRequestBase(Uri tokenEndpoint, Version version)
            :base(version, MessageTransport.Direct, tokenEndpoint)
        {
        }

        [MessagePart(Protocol.client_id, IsRequired = false)]
        public string ClientIdentifier { get; set; }

        [MessagePart(Protocol.client_secret, IsRequired=false, IsSecuritySensitive=true)]
        public string ClientSecret { get; set; }

        public System.Net.Http.Headers.HttpRequestHeaders Headers
        {
            get { return this.headers; }
        }
    }
}

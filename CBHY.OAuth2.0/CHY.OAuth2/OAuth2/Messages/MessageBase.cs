using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.Messages
{
    public class MessageBase:IDirectedProtocolMessage, IDirectResponseProtocolMessage
    {
        private Dictionary<string, string> extraData = new Dictionary<string, string>();

        private IDirectedProtocolMessage originationRequest;

        private Version version;

        private MessageTransport messageTransport;

        protected MessageBase(Version version)
        {
            this.messageTransport = MessageTransport.Direct;
            this.version = version;
            this.HttpMethods = HttpDeliveryMethods.GetRequest;
        }

        protected MessageBase(IDirectedProtocolMessage request, Uri recipient = null)
        {
            this.originationRequest = request;
            this.messageTransport = request.Transport;
            this.version = request.Version;
            this.Recipient = recipient;
            this.HttpMethods = HttpDeliveryMethods.GetRequest;
        }

        protected MessageBase(Version version, MessageTransport messageTransport, Uri recipient)
        {
            this.version = version;
            this.messageTransport = messageTransport;
            this.Recipient = recipient;
            this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest;
        }

        Version IMessage.Version
        {
            get { return this.Version; }
        }

        public IDictionary<string, string> ExtraData
        {
            get { return this.extraData; }
        }

        MessageProtections IProtocolMessage.RequiredProtection
        {
            get { return RequiredProtection; }
        }

        MessageTransport IProtocolMessage.Transport
        {
            get { return this.Transport; }
        }

        HttpDeliveryMethods IDirectedProtocolMessage.HttpMethods
        {
            get { return this.HttpMethods; }
        }

        Uri IDirectedProtocolMessage.Recipient
        {
            get { return this.Recipient; }
        }

        IDirectedProtocolMessage IDirectResponseProtocolMessage.OriginatingRequest
        {
            get { return this.OriginatingRequest; }
        }

        protected static MessageProtections RequiredProtection
        {
            get { return MessageProtections.None; }
        }

        protected MessageTransport Transport
        {
            get { return this.messageTransport; }
        }

        protected Version Version
        {
            get { return this.version; }
        }

        protected HttpDeliveryMethods HttpMethods { get; set; }

        protected IDirectedProtocolMessage OriginatingRequest
        {
            get { return this.originationRequest; }
        }

        protected Uri Recipient { get; private set; }

        void IMessage.EnsureValidMessage()
        {
            this.EnsureValidMessage();
        }

        protected virtual void EnsureValidMessage()
        {

        }
    }
}

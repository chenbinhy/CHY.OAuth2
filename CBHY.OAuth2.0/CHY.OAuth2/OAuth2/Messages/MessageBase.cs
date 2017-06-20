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

        /// <summary>
        /// 版本
        /// </summary>
        Version IMessage.Version
        {
            get { return this.Version; }
        }

        /// <summary>
        /// 附加数据
        /// </summary>
        public IDictionary<string, string> ExtraData
        {
            get { return this.extraData; }
        }

        /// <summary>
        /// 保护方式
        /// </summary>
        MessageProtections IProtocolMessage.RequiredProtection
        {
            get { return RequiredProtection; }
        }

        /// <summary>
        /// 访问方式
        /// </summary>
        MessageTransport IProtocolMessage.Transport
        {
            get { return this.Transport; }
        }

        /// <summary>
        /// http or https
        /// </summary>
        HttpDeliveryMethods IDirectedProtocolMessage.HttpMethods
        {
            get { return this.HttpMethods; }
        }

        /// <summary>
        /// 接收地址
        /// </summary>
        Uri IDirectedProtocolMessage.Recipient
        {
            get { return this.Recipient; }
        }

        /// <summary>
        /// 原始请求
        /// </summary>
        IDirectedProtocolMessage IDirectResponseProtocolMessage.OriginatingRequest
        {
            get { return this.OriginatingRequest; }
        }

        /// <summary>
        /// 保护方式
        /// </summary>
        protected static MessageProtections RequiredProtection
        {
            get { return MessageProtections.None; }
        }

        /// <summary>
        /// 传输方式
        /// </summary>
        protected MessageTransport Transport
        {
            get { return this.messageTransport; }
        }

        /// <summary>
        /// 版本
        /// </summary>
        protected Version Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// 请求方式
        /// </summary>
        protected HttpDeliveryMethods HttpMethods { get; set; }

        /// <summary>
        /// 原始请求
        /// </summary>
        protected IDirectedProtocolMessage OriginatingRequest
        {
            get { return this.originationRequest; }
        }

        /// <summary>
        /// 接收者
        /// </summary>
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

using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CHY.OAuth2.Core.Common;
using System.Web;
using System.Net;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace CHY.OAuth2.Core.Messaging
{
    public abstract class Channel:IDisposable
    {
        public static readonly Encoding PostEntityEncoding = new UTF8Encoding(false);
        public static readonly XmlDictionaryReaderQuotas DefaultUntrustedXmlDictionaryReaderQuotas = new XmlDictionaryReaderQuotas
        {
            MaxArrayLength = 1,
            MaxDepth = 2,
            MaxBytesPerRead = 8 * 1024,
            MaxStringContentLength = 16*1024,
        };

        public const string HttpFormUrlEncoded = "application/x-www-form-urlencoded";
        public const string JsonEncoded = "application/json";
        public const string JsonTextEncoded = "text/javascript";
        public const string PlainTextEncoded = "text/plain";
        public static readonly ContentType HttpFormUrlEncodedContentType = new ContentType(HttpFormUrlEncoded) { CharSet = PostEntityEncoding.WebName };
        private const string RedirectResponseBodyFormat = @"<html><head><title>Object moved</title></head><body>
                                                            <h2>Object moved to <a href=""{0}"">here</a>.</h2>
                                                            </body></html>";
        private readonly List<IChannelBindingElement> outgoingBindingElements = new List<IChannelBindingElement>();
        private readonly List<IChannelBindingElement> incomingBindingElements = new List<IChannelBindingElement>();
        private const string IndirectMessageFormPostFormat = @"
                                                                <html>
                                                                <head>
                                                                </head>
                                                                <body onload=""document.body.style.display = 'none'; var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; document.getElementById('openid_message').submit()"">
                                                                <form id=""openid_message"" action=""{0}"" method=""post"" accept-charset=""UTF-8"" enctype=""application/x-www-form-urlencoded"" onSubmit=""var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; return true;"">
                                                                {1}
	                                                                <input id=""submit_button"" type=""submit"" value=""Continue"" />
                                                                </form>
                                                                </body>
                                                                </html>
                                                                ";
        private static MessageDescriptionCollection defaultMessageDescriptions = new MessageDescriptionCollection();
        private MessageDescriptionCollection messageDescriptions = defaultMessageDescriptions;
        private IMessageFactory messageTypeProvider;
        private int maximumIndirectMessageUrlLength = Configuration.DotNetOpenAuthSection.Messaging.MaximumIndirectMessageUrlLength;
        protected Channel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements, IHostFactories hostFactories)
        {
            this.messageTypeProvider = messageTypeProvider;
            this.HostFactories = hostFactories;
            this.XmlDictionaryReaderQuotas = DefaultUntrustedXmlDictionaryReaderQuotas;
            this.outgoingBindingElements = new List<IChannelBindingElement>(ValidateAndPrepareBindingElements(bindingElements));
            this.incomingBindingElements = new List<IChannelBindingElement>(this.outgoingBindingElements);
            this.incomingBindingElements.Reverse();
            foreach(var element in this.outgoingBindingElements)
            {
                element.Channel = this;
            }
        }

        public event EventHandler<ChannelEventArgs> Sending;

        public IHostFactories HostFactories { get; private set; }

        public int MaximumIndirectMessageUrlLength
        {
            get { return this.maximumIndirectMessageUrlLength; }
            set { this.maximumIndirectMessageUrlLength = value; }
        }
        public virtual MessageDescriptionCollection MessageDescriptions
        {
            get { return this.messageDescriptions; }
            set { this.messageDescriptions = value; }
        }

        public IMessageFactory MessageFactoryTestHook
        {
            get { return this.MessageFactory; }
        }

        public Action<IProtocolMessage> OutgoingMessageFilter { get; set; }
        public Action<IProtocolMessage> IncomingMessageFilter { get; set; }
        public ReadOnlyCollection<IChannelBindingElement> BindingElements
        {
            get
            {
                var result = this.outgoingBindingElements.AsReadOnly();
                Assumes.True(result != null);
                return result;
            }
        }

        public ReadOnlyCollection<IChannelBindingElement> OutgoingBindingElements
        {
            get { return this.outgoingBindingElements.AsReadOnly(); }
        }

        public ReadOnlyCollection<IChannelBindingElement> IncomingBindingElements
        {
            get
            {
                return this.incomingBindingElements.AsReadOnly();
            }
        }

        public bool IsDisposed { get; set; }

        protected virtual IMessageFactory MessageFactory
        {
            get { return this.messageTypeProvider; }
            set { this.messageTypeProvider = value; }
        }

        protected virtual XmlDictionaryReaderQuotas XmlDictionaryReaderQuotas { get; set; }

        public async Task<HttpResponseMessage> PrepareResponseAsync(IProtocolMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.ProcessOutgoingMessageAsync(message, cancellationToken);
            Logger.Channel.DebugFormat("Sending message: {0}", message.GetType().Name);

            HttpResponseMessage result;
            switch(message.Transport)
            {
                case MessageTransport.Direct:
                    result = this.PrepareDirectResponse(message);
                    break;
                case MessageTransport.Indirect:
                    var directedMessage = message as IDirectedProtocolMessage;
                    ErrorUtilities.VerifyArgumentNamed(
						directedMessage != null,
						"message",
						MessagingStrings.IndirectMessagesMustImplementIDirectedProtocolMessage,
						typeof(IDirectedProtocolMessage).FullName);
					ErrorUtilities.VerifyArgumentNamed(
						directedMessage.Recipient != null,
						"message",
						MessagingStrings.DirectedMessageMissingRecipient);
                    result = this.PrepareIndirectResponse(directedMessage);
                    break;
                default:
                    throw ErrorUtilities.ThrowArgumentNamed(
                        "message",
                        MessagingStrings.UnrecognizedEnumValue,
                        "Transport",
                        message.Transport);
            }
            result.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { 
                NoCache = true,
                NoStore = true,
                MaxAge = TimeSpan.Zero,
                MustRevalidate = true
            };
            result.Headers.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            return result;
        }

        /// <summary>
        /// 读取httpRequest信息转换为TRequest
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="httpRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TRequest> TryReadFromRequestAsync<TRequest>(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
            where TRequest:class, IProtocolMessage
        {
            IProtocolMessage untypeRequest = await this.ReadFromRequestAsync(httpRequest, cancellationToken);
            if(untypeRequest == null)
            {
                return null;
            }
            var request = untypeRequest as TRequest;
            ErrorUtilities.VerifyProtocol(request != null, MessagingStrings.UnexpectedMessageReceived, typeof(TRequest), untypeRequest.GetType());

            return request;
        }

        public async Task<TRequest> ReadFromRequestAsync<TRequest>(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
            where TRequest:class, IProtocolMessage
        {
            TRequest request = await this.TryReadFromRequestAsync<TRequest>(httpRequest, cancellationToken);
            ErrorUtilities.VerifyProtocol(request != null, MessagingStrings.ExpectedMessageNotReceived, typeof(TRequest));
            return request;
        }

        /// <summary>
        /// HttpRequestMessage装换未IDirectedProtocolMessage
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IDirectedProtocolMessage> ReadFromRequestAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            if (Logger.Channel.IsInfoEnabled() && httpRequest.RequestUri != null)
            {
                Logger.Channel.InfoFormat("Scanning incoming request for messages: {0}", httpRequest.RequestUri.AbsoluteUri);
            }
            IDirectedProtocolMessage requestMessage = await this.ReadFromRequestCoreAsync(httpRequest, cancellationToken);
            if(requestMessage != null)
            {
                Logger.Channel.DebugFormat("Incoming request received: {0}", requestMessage.GetType().Name);
                var directRequest = requestMessage as IHttpDirectRequest;
                if(directRequest != null)
                {
                    foreach(var header in httpRequest.Headers)
                    {
                        directRequest.Headers.Add(header.Key, header.Value);
                    }
                }
                await this.ProcessIncomingMessageAsync(requestMessage, cancellationToken);
            }
            return requestMessage;
        }

        public async Task<TResponse> RequestAsync<TResponse>(IDirectedProtocolMessage requestMessage, CancellationToken cancellationToken)
            where TResponse:class, IProtocolMessage
        {
            IProtocolMessage response = await this.RequestAsync(requestMessage, cancellationToken);
            ErrorUtilities.VerifyProtocol(response != null, MessagingStrings.ExpectedMessageNotReceived, typeof(TResponse));
            var expectedResponse = response as TResponse;
            ErrorUtilities.VerifyProtocol(expectedResponse != null, MessagingStrings.UnexpectedMessageReceived, typeof(TResponse), response.GetType());

            return expectedResponse;
        }
        public async Task<IProtocolMessage> RequestAsync(IDirectedProtocolMessage requestMessage, CancellationToken cancellationToken)
        {
            await this.ProcessOutgoingMessageAsync(requestMessage, cancellationToken);
            Logger.Channel.DebugFormat("Sending {0} request.", requestMessage.GetType().Name);
            var responseMessage = await this.RequestCoreAsync(requestMessage, cancellationToken);
            ErrorUtilities.VerifyProtocol(responseMessage != null, MessagingStrings.ExpectedMessageNotReceived, typeof(IProtocolMessage).Name);
            Logger.Channel.DebugFormat("Received {0} response.", responseMessage.GetType().Name);
            await this.ProcessIncomingMessageAsync(responseMessage, cancellationToken);

            return responseMessage;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task ProcessIncomingMessageTestHookAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            return this.ProcessIncomingMessageAsync(message, cancellationToken);
        }

        public HttpRequestMessage CreateHttpRequestTestHook(IDirectedProtocolMessage request)
        {
            return this.CreateHttpRequest(request);
        }

        public HttpResponseMessage PrepareDirectResponseTestHook(IProtocolMessage response)
        {
            return this.PrepareDirectResponse(response);
        }

        public Task<IDictionary<string, string>> ReadFromResponseCoreAsyncTestHook(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return this.ReadFromResponseCoreAsyncTestHook(response, cancellationToken);
        }

        public Task ProcessOutgoingMessageTestHookAsync(IProtocolMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.ProcessOutgoingMessageAsync(message, cancellationToken);
        }

        /// <summary>
        /// Form表单数据转换键值对
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<KeyValuePair<string, string>>> ParseUrlEncodedFormContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request.Content !=null && request.Content.Headers.ContentType !=null
                && request.Content.Headers.ContentType.MediaType.Equals(HttpFormUrlEncoded))
            {
                return HttpUtility.ParseQueryString(await request.Content.ReadAsStringAsync()).AsKeyValuePairs();
            }
            
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public virtual HttpContextBase GetHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }

        /// <summary>
        /// 封装当前请求上下文为HttpRequestWrapper
        /// </summary>
        /// <returns></returns>
        public virtual HttpRequestBase GetRequestFromContext()
        {
            Assumes.True(HttpContext.Current.Request.Url != null);
            Assumes.True(HttpContext.Current.Request.RawUrl != null);

            return new HttpRequestWrapper(HttpContext.Current.Request);
        }

        protected static MultipartFormDataContent InitializeMultipartFormDataContent(IMessageWithBinaryData requestMessageWithBinaryData)
        {
            var content = new MultipartFormDataContent();
            foreach(var part in requestMessageWithBinaryData.BinaryData)
            {
                if(string.IsNullOrEmpty(part.Name))
                {
                    content.Add(part.Content);
                }
                else if(string.IsNullOrEmpty(part.FileName))
                {
                    content.Add(part.Content, part.Name);
                }
                else
                {
                    content.Add(part.Content, part.Name, part.FileName);
                }
            }
            return content;
        }

        protected static bool HttpMethodHasEntity(HttpMethod httpMethod)
        {
            if(httpMethod == HttpMethod.Get ||
                httpMethod == HttpMethod.Head ||
                httpMethod == HttpMethod.Delete ||
                httpMethod == HttpMethod.Options)
            {
                return false;
            }
            else if(httpMethod == HttpMethod.Post ||
                httpMethod == HttpMethod.Put ||
                string.Equals(httpMethod.Method, "PATCH", StringComparison.Ordinal))
            {
                return true;
            }
            else
            {
                throw ErrorUtilities.ThrowArgumentNamed("httpMethod", MessagingStrings.UnsupportedHttpVerb, httpMethod);
            }
        }

        protected static void ApplyMessageTemplate(IMessage message, HttpResponseMessage response)
        {
            var httpMessage = message as IHttpDirectResponse;
            if(httpMessage != null)
            {
                response.StatusCode = httpMessage.HttpStatusCode;
                foreach(string headerName in httpMessage.Headers)
                {
                    response.Headers.Add(headerName, httpMessage.Headers[headerName]);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(IDisposable bindingElement in this.BindingElements.OfType<IDisposable>())
                {
                    bindingElement.Dispose();
                }

                this.IsDisposed = true;
            }
        }

        protected virtual void OnSending(IProtocolMessage message)
        {
            var sending = this.Sending;
            if(sending != null)
            {
                sending(this, new ChannelEventArgs(message));
            }
        }

        protected virtual async Task<IProtocolMessage> RequestCoreAsync(IDirectedProtocolMessage request, CancellationToken cancellationToken)
        {
            if(this.OutgoingMessageFilter != null)
            {
                this.OutgoingMessageFilter(request);
            }

            var webRequest = this.CreateHttpRequest(request);
            var directRequest = request as IHttpDirectRequest;
            if(directRequest != null)
            {
                foreach(var header in directRequest.Headers)
                {
                    webRequest.Headers.Add(header.Key, header.Value);
                }
            }

            try
            {
                // 防止报根据验证过程，远程证书无效
                ServicePointManager.ServerCertificateValidationCallback =delegate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                using (var httpClient = this.HostFactories.CreateHttpClient())
                {
                    using(var response = await httpClient.SendAsync(webRequest, cancellationToken))
                    {
                        if(response.Content != null)
                        {
                            var responseFields = await this.ReadFromResponseCoreAsync(response, cancellationToken);
                            if(responseFields != null)
                            {
                                var responseMessage = this.MessageFactory.GetNewResponseMessage(request, responseFields);
                                if(responseMessage != null)
                                {
                                    this.OnReceivingDirectResponse(response, responseMessage);
                                    var messageAccessor = this.MessageDescriptions.GetAccessor(responseMessage);
                                    messageAccessor.Deserialize(responseFields);

                                    return responseMessage;
                                }
                            }
                        }
                        if(!response.IsSuccessStatusCode)
                        {
                            var errorContent = (response.Content != null) ? await response.Content.ReadAsStringAsync() : null;
                            Logger.Http.ErrorFormat(
                                "Error received in HTTP response: {0} {1}\n{2}", (int)response.StatusCode, response.ReasonPhrase, errorContent);
                            response.EnsureSuccessStatusCode();
                        }
                        return null;
                    }
                }
            }
            catch(HttpRequestException requestException){
                throw ErrorUtilities.Wrap(requestException, "Error sending HTTP request or receiving response.");
            }
        }

        protected virtual void OnReceivingDirectResponse(HttpResponseMessage response, IDirectResponseProtocolMessage message)
        {

        }

        /// <summary>
        /// HttpRequestMessage转换为IDirectedProtocolMessage，填充HttpRequestMessage参数值
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<IDirectedProtocolMessage> ReadFromRequestCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.Method, request.RequestUri.AbsoluteUri);
            var fields = new Dictionary<string, string>();
            fields.AddRange(await ParseUrlEncodedFormContentAsync(request, cancellationToken));
            if(fields.Count == 0 && request.Method.Method != "POST")
            {
                fields.AddRange(HttpUtility.ParseQueryString(request.RequestUri.Query).AsKeyValuePairs());
            }
            MessageReceivingEndpoint recipient;
            try
            {
                recipient = request.GetRecipient();
            }
            catch(ArgumentException ex)
            {
                Logger.Messaging.WarnFormat("Unrecognized HTTP request: {0}", ex);
                return null;
            }
            IDirectedProtocolMessage result = null;
            result = (IDirectedProtocolMessage)this.Receive(fields, recipient);
              
            return result;
        }

        protected virtual IProtocolMessage Receive(Dictionary<string,string> fields, MessageReceivingEndpoint recipient)
        {
            this.FilterReceivedFields(fields);
            IProtocolMessage message = this.MessageFactory.GetNewRequestMessage(recipient, fields); // 创建对象实例
            if (message == null)
            {
                return null;
            }
            var directedMessage = message as IDirectedProtocolMessage;
            ErrorUtilities.VerifyProtocol(recipient == null || (directedMessage != null && (recipient.AllowedMethods & directedMessage.HttpMethods) != 0), MessagingStrings.UnsupportedHttpVerbForMessageType, message.GetType().Name, recipient.AllowedMethods);

            var messageAccessor = this.MessageDescriptions.GetAccessor(message); // 简单获取MessageDictionary
            messageAccessor.Deserialize(fields); // 给对象填充参数值

            return message;
        }

        protected virtual HttpResponseMessage PrepareIndirectResponse(IDirectedProtocolMessage message)
        {
            Assumes.True(message != null && message.Recipient != null);
            var messageAccessor = this.messageDescriptions.GetAccessor(message);
            Assumes.True(message != null && message.Recipient != null);
            var fields = messageAccessor.Serialize();
            HttpResponseMessage response = null;
            bool tooLargeForGet = false;
            if((message.HttpMethods & HttpDeliveryMethods.GetRequest) == HttpDeliveryMethods.GetRequest)
            {
                bool payloadInFragment = false;
                var httpIndirect = message as IHttpIndirectResponse;
                if(httpIndirect != null)
                {
                    payloadInFragment = httpIndirect.Include301RedirectPayloadInFragment;
                }
                response = this.Create301RedirectResponse(message, fields, payloadInFragment);
                tooLargeForGet = response.Headers.Location.PathAndQuery.Length > this.MaximumIndirectMessageUrlLength;
            }
            if(tooLargeForGet)
            {
                ErrorUtilities.VerifyProtocol(
                    (message.HttpMethods & HttpDeliveryMethods.PostRequest) == HttpDeliveryMethods.PostRequest,
                    MessagingStrings.MessageExceedsGetSizePostNotAllowed);
            }
            if(response == null || tooLargeForGet)
            {
                response = this.CreateFormPostResponse(message, fields);
            }

            return response;
        }

        /// <summary>
        /// 重定向
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fields"></param>
        /// <param name="payloadInFragment"></param>
        /// <returns></returns>
        protected virtual HttpResponseMessage Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string,string> fields, bool payloadInFragment = false)
        {
            UriBuilder builder = new UriBuilder(message.Recipient);
            if(payloadInFragment)
            {
                builder.AppendFragmentArgs(fields);
            }
            else
            {
                builder.AppendQueryArgs(fields);
            }
            Logger.Http.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
            HttpResponseMessage response = new HttpResponseMessageWithOriginal(message)
            {
                StatusCode = HttpStatusCode.Redirect,
                Content = new StringContent(string.Format(CultureInfo.InvariantCulture, RedirectResponseBodyFormat, builder.Uri.AbsoluteUri))
            };
            response.Headers.Location = builder.Uri;
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html") { CharSet = "utf-8" };
            return response;
        }

        protected virtual HttpResponseMessage CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string,string> fields)
        {
            using(StringWriter bodyWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                StringBuilder hiddenFields = new StringBuilder();
                foreach(var field in fields)
                {
                    hiddenFields.AppendFormat("\t<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />\r\n", HttpUtility.HtmlEncode(field.Key), HttpUtility.HtmlEncode(field.Value));
                }
                bodyWriter.WriteLine(
                    IndirectMessageFormPostFormat,
                    HttpUtility.HtmlEncode(message.Recipient.AbsoluteUri),
                    hiddenFields
                    );
                bodyWriter.Flush();
                HttpResponseMessage response = new HttpResponseMessageWithOriginal(message)
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(bodyWriter.ToString())
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");

                return response;
            }
        }

        protected abstract Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken);

        protected virtual HttpRequestMessage CreateHttpRequest(IDirectedProtocolMessage request)
        {
            throw new NotImplementedException();
        }

        protected abstract HttpResponseMessage PrepareDirectResponse(IProtocolMessage response);

        protected virtual string SerializeAsJson(IMessage message)
        {
            return MessagingUtilities.SerializeAsJson(message, this.MessageDescriptions);
        }

        protected virtual IDictionary<string,string> DeserializeFromJson(string json)
        {
            var dictionary = new Dictionary<string, string>();
            using(var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), this.XmlDictionaryReaderQuotas))
            {
                MessageSerializer.DeserializeJsonAsFlatDictionary(dictionary, jsonReader);
            }

            return dictionary;
        }

        protected async Task ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            Logger.Channel.DebugFormat("Preparing to send {0} ({1}) message.", message.GetType().Name, message.Version);
            this.OnSending(message);
            IMessageWithEvents eventedMessage = message as IMessageWithEvents;
            if(eventedMessage != null)
            {
                eventedMessage.OnSending();
            }
            MessageProtections appliedProtection = MessageProtections.None;
            foreach(IChannelBindingElement bindingElement in this.outgoingBindingElements)
            {
                Assumes.True(bindingElement.Channel != null);
                MessageProtections? elementProtection = await bindingElement.ProcessOutgoingMessageAsync(message, cancellationToken);
                if(elementProtection.HasValue)
                {
                    Logger.Bindings.DebugFormat("Binding element {0} applied to message.", bindingElement.GetType().FullName);
                    ErrorUtilities.VerifyProtocol((appliedProtection & elementProtection.Value) == 0, MessagingStrings.TooManyBindingsOfferingSameProtection, elementProtection.Value);
                    appliedProtection |= elementProtection.Value;
                }
                else
                {
                    Logger.Bindings.DebugFormat("Binding element {0} did not apply to message.", bindingElement.GetType().FullName);
                }
            }
            if((message.RequiredProtection & appliedProtection) != message.RequiredProtection)
            {
                throw new UnprotectedMessageException(message, appliedProtection);
            }
            this.EnsureValidMessageParts(message);
            message.EnsureValidMessage();
            if(this.OutgoingMessageFilter != null)
            {
                this.OutgoingMessageFilter(message);
            }
            if (Logger.Channel.IsInfoEnabled()) // 日志
            {
                var directedMessage = message as IDirectedProtocolMessage;
                string recipient = (directedMessage != null && directedMessage.Recipient != null) ? directedMessage.Recipient.AbsoluteUri : "<response>";
                var messageAccessor = this.MessageDescriptions.GetAccessor(message);
                Logger.Channel.InfoFormat(
                    "Prepared outgoing {0} ({1}) message for {2}: {3}{4}",
                    message.GetType().Name,
                    message.Version,
                    recipient,
                    Environment.NewLine,
                    messageAccessor.ToStringDeferred());
            }
        }

        protected virtual HttpRequestMessage InitializeRequestAsGet(IDirectedProtocolMessage requestMessage)
        {
            var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
            var fields = messageAccessor.Serialize();
            UriBuilder builder = new UriBuilder(requestMessage.Recipient);
            MessagingUtilities.AppendQueryArgs(builder, fields);
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
            this.PrepareHttpWebRequest(httpRequest);

            return httpRequest;
        }

        protected virtual HttpRequestMessage InitializeRequestAsHead(IDirectedProtocolMessage requestMessage)
        {
            var request = this.InitializeRequestAsGet(requestMessage);
            request.Method = HttpMethod.Head;

            return request;
        }

        protected virtual HttpRequestMessage InitializeRequestAsPost(IDirectedProtocolMessage requestMessage)
        {
            var messageAccessor = this.MessageDescriptions.GetAccessor(requestMessage);
            var fields = messageAccessor.Serialize();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestMessage.Recipient);
            this.PrepareHttpWebRequest(httpRequest);

            var requestMessageWithBinaryData = requestMessage as IMessageWithBinaryData;
            if(requestMessageWithBinaryData != null && requestMessageWithBinaryData.SendAsMultipart)
            {
                var content = InitializeMultipartFormDataContent(requestMessageWithBinaryData);
                foreach(var field in fields)
                {
                    content.Add(new StringContent(field.Value), field.Key);
                }

                httpRequest.Content = content;
            }
            else 
            {
                ErrorUtilities.VerifyProtocol(requestMessageWithBinaryData == null || requestMessageWithBinaryData.BinaryData.Count == 0, MessagingStrings.BinaryDataRequiresMultipart);
                httpRequest.Content = new FormUrlEncodedContent(fields);
            }

            return httpRequest;
        }

        protected virtual HttpRequestMessage InitializeRequestAsPut(IDirectedProtocolMessage requestMessage)
        {
            var request = this.InitializeRequestAsGet(requestMessage);
            request.Method = HttpMethod.Put;

            return request;
        }

        protected virtual HttpRequestMessage InitializeRequestAsDelete(IDirectedProtocolMessage requestMessage)
        {
            var request = this.InitializeRequestAsGet(requestMessage);
            request.Method = HttpMethod.Delete;

            return request;
        }

        protected virtual async Task ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            if (Logger.Channel.IsInfoEnabled()) // 日志记录
            {
                var messageAccessor = this.MessageDescriptions.GetAccessor(message, true);
                Logger.Channel.InfoFormat(
                    "Processing incoming {0} ({1}) message:{2}{3}",
                    message.GetType().Name,
                    message.Version,
                    Environment.NewLine,
                    messageAccessor.ToStringDeferred());
            }
            if(this.IncomingMessageFilter != null)
            {
                this.IncomingMessageFilter(message);
            }

            MessageProtections appliedProtection = MessageProtections.None;
            foreach(IChannelBindingElement bindingElement in this.IncomingBindingElements)
            {
                Assumes.True(bindingElement.Channel != null);
                MessageProtections? elementProtection = await bindingElement.ProcessIncomingMessageAsync(message, cancellationToken);
                if(elementProtection.HasValue)
                {
                    Logger.Bindings.DebugFormat("Binding element {0} applied to message.", bindingElement.GetType().FullName);
                    if((appliedProtection & elementProtection.Value) != 0)
                    {
                        Logger.Bindings.WarnFormat(MessagingStrings.TooManyBindingsOfferingSameProtection, elementProtection.Value);
                    }
                    appliedProtection |= elementProtection.Value;
                }
                else
                {
                    Logger.Bindings.DebugFormat("Binding element {0} did not apply to message.", bindingElement.GetType().FullName);
                }
            }
            if((message.RequiredProtection & appliedProtection) != message.RequiredProtection)
            {
                throw new UnprotectedMessageException(message, appliedProtection);
            }

            IMessageWithEvents eventedMessage = message as IMessageWithEvents;
            if(eventedMessage != null)
            {
                eventedMessage.OnReceiving();
            }

            if (Logger.Channel.IsDebugEnabled()) // 日志记录
            {
                var messageAccessor = this.MessageDescriptions.GetAccessor(message);
                Logger.Channel.DebugFormat(
                    "After binding element processing, the received {0} ({1}) message is: {2}{3}",
                    message.GetType().Name,
                    message.Version,
                    Environment.NewLine,
                    messageAccessor.ToStringDeferred());
            }

            message.EnsureValidMessage();
        }

        protected virtual void FilterReceivedFields(IDictionary<string,string> fields)
        {

        }

        protected virtual void PrepareHttpWebRequest(HttpRequestMessage request)
        {

        }

        protected void CustomizeBindingElementOrder(IEnumerable<IChannelBindingElement> outgoingOrder, IEnumerable<IChannelBindingElement> incomingOrder)
        {
            ErrorUtilities.VerifyArgument(this.IsBindingElementOrderValid(outgoingOrder), MessagingStrings.InvalidCustomBindingElementOrder);
            ErrorUtilities.VerifyArgument(this.IsBindingElementOrderValid(incomingOrder), MessagingStrings.InvalidCustomBindingElementOrder);
            this.outgoingBindingElements.Clear();
            this.outgoingBindingElements.AddRange(outgoingOrder);
            this.incomingBindingElements.Clear();
            this.incomingBindingElements.AddRange(incomingOrder);
        }

        private static IEnumerable<IChannelBindingElement> ValidateAndPrepareBindingElements(IEnumerable<IChannelBindingElement> elements)
        {
            if(elements == null)
            {
                return new IChannelBindingElement[0];
            }
            var transformationElements = new List<IChannelBindingElement>(
                elements.Where(element => element.Protection == MessageProtections.None)
                );
            var protectionElements = new List<IChannelBindingElement>(
                elements.Where(element => element.Protection != MessageProtections.None)
                );

            bool wasLastProtectionPresent = true;
            foreach(MessageProtections protectionKind in Enum.GetValues(typeof(MessageProtections))) // 保护级别，执行后一种保护时，也必须执行第一种
            {
                if(protectionKind == MessageProtections.None)
                {
                    continue;
                }
                int countProtectionsOfThisKind = protectionElements.Count(element => (element.Protection & protectionKind) == protectionKind);
                ErrorUtilities.VerifyProtocol(!(countProtectionsOfThisKind > 0 && !wasLastProtectionPresent), MessagingStrings.RequiredProtectionMissing, protectionKind);
                wasLastProtectionPresent = countProtectionsOfThisKind > 0;
            }
            var orderedList = new List<IChannelBindingElement>(transformationElements);
            orderedList.AddRange(protectionElements.OrderBy(element => element.Protection, BindingElementOutgoingMessageApplicationOrder));

            return orderedList.AsEnumerable();
        }

        private static int BindingElementOutgoingMessageApplicationOrder(MessageProtections protection1, MessageProtections protection2)
        {
            ErrorUtilities.VerifyInternal(protection1 != MessageProtections.None || protection2 != MessageProtections.None, "This comparison function should only be used to compare protection binding elements.  Otherwise we change the order of user-defined message transformations.");

            return -((int)protection1).CompareTo((int)protection2);
        }

        private void EnsureValidMessageParts(IProtocolMessage message)
        {
            MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(message);
            MessageDescription description = this.MessageDescriptions.Get(message);
            description.EnsureMessagePartsPassBasicValidation(dictionary);
        }

        private bool IsBindingElementOrderValid(IEnumerable<IChannelBindingElement> order)
        {
            if(order.Count() != this.OutgoingBindingElements.Count)
            {
                return false;
            }
            if (order.Any(el => !this.OutgoingBindingElements.Contains(el)))
            {
                return false;
            }

            return true;
        }
    }
}

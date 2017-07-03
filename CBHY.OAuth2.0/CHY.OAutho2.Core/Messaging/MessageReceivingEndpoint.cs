using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    /// <summary>
    /// 消息接收终结点
    /// </summary>
    public class MessageReceivingEndpoint
    {
        public MessageReceivingEndpoint(string locationUri, HttpDeliveryMethods method)
            :this(new Uri(locationUri), method)
        {

        }

        public MessageReceivingEndpoint(Uri location, HttpDeliveryMethods method)
        {
            this.Location = location;
            this.AllowedMethods = method;
        }

        public Uri Location { get; private set; }

        public HttpDeliveryMethods AllowedMethods { get; private set; }
    }
}

using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public abstract class StandardMessageFactoryChannel:Channel
    {
        private readonly ICollection<Type> messageTypes;
        private readonly ICollection<Version> versions;
        protected StandardMessageFactoryChannel(ICollection<Type> messageTypes, ICollection<Version> versions, IHostFactories hostFactories, IChannelBindingElement[] bindingElements = null)
            :base(new StandardMessageFactory(), bindingElements??new IChannelBindingElement[0], hostFactories)
        {
            this.messageTypes = messageTypes;
            this.versions = versions;
            this.StandardMessageFactory.AddMessageTypes(GetMessageDescriptions(this.messageTypes, this.versions, this.MessageDescriptions));
        }

        public StandardMessageFactory StandardMessageFactory
        {
            get { return (Messaging.StandardMessageFactory)this.MessageFactory; }
            set { this.MessageFactory = value; }
        }

        public sealed override MessageDescriptionCollection MessageDescriptions
        {
            get
            {
                return base.MessageDescriptions;
            }
            set
            {
                base.MessageDescriptions = value;
                var factory = new StandardMessageFactory();
                factory.AddMessageTypes(GetMessageDescriptions(this.messageTypes, this.versions, value));
                this.MessageFactory = factory;
            }
        }

        protected sealed override IMessageFactory MessageFactory
        {
            get
            {
                return (StandardMessageFactory)base.MessageFactory;
            }
            set
            {
                StandardMessageFactory newValue = (StandardMessageFactory)value;
                base.MessageFactory = value;
            }
        }

        private static IEnumerable<MessageDescription> GetMessageDescriptions(ICollection<Type> messageTypes, ICollection<Version> versions, MessageDescriptionCollection descriptionsCache)
        {
            var messageDescriptions = new List<MessageDescription>(messageTypes.Count * versions.Count);
            messageDescriptions.AddRange(from version in versions
                                         from messageType in messageTypes
                                         select descriptionsCache.Get(messageType, version));

            return messageDescriptions;
        }
    }
}

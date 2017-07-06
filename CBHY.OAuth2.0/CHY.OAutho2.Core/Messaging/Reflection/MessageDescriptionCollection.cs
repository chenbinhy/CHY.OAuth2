using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    public class MessageDescriptionCollection : IEnumerable<MessageDescription>
    {
        private readonly Dictionary<MessageTypeAndVersion, MessageDescription> reflectedMessageTypes = new Dictionary<MessageTypeAndVersion, MessageDescription>();
        public MessageDescriptionCollection()
        {

        }

        public IEnumerator<MessageDescription> GetEnumerator()
        {
            lock (this.reflectedMessageTypes)
            {
                return this.reflectedMessageTypes.Values.ToList().GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// 根据消息类型和版本获取消息描述
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="messageVersion"></param>
        /// <returns></returns>
        public MessageDescription Get(Type messageType, Version messageVersion)
        {
            MessageTypeAndVersion key = new MessageTypeAndVersion(messageType, messageVersion);
            MessageDescription result;
            lock(this.reflectedMessageTypes)
            {
                this.reflectedMessageTypes.TryGetValue(key, out result);
            }
            if(result == null)
            {
                var newDescription = new MessageDescription(messageType, messageVersion);
                lock(this.reflectedMessageTypes)
                {
                    if(!this.reflectedMessageTypes.TryGetValue(key, out result))
                    {
                        this.reflectedMessageTypes[key] = result = newDescription;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据IMessage获取消息描述
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public MessageDescription Get(IMessage message)
        {
            return this.Get(message.GetType(), message.Version);
        }

        /// <summary>
        /// 获取消息字典
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public MessageDictionary GetAccessor(IMessage message)
        {
            return this.GetAccessor(message, false);
        }

        public MessageDictionary GetAccessor(IMessage message, bool getOriginalValues)
        {
            return this.Get(message).GetDictionary(message, getOriginalValues);
        }

        private struct MessageTypeAndVersion
        {
            private readonly Type type;
            private readonly Version version;

            public MessageTypeAndVersion(Type messageType, Version messageVersion)
            {
                this.type = messageType;
                this.version = messageVersion;
            }

            public Type Type
            {
                get { return this.type; }
            }

            public Version Version
            {
                get { return this.version; }
            }

            public static bool operator ==(MessageTypeAndVersion first, MessageTypeAndVersion second)
            {
                return first.Equals(second);
            }

            public static bool operator !=(MessageTypeAndVersion first, MessageTypeAndVersion second)
            {
                return !first.Equals(second);
            }

            public override bool Equals(object obj)
            {
                if(obj is MessageTypeAndVersion)
                {
                    MessageTypeAndVersion other = (MessageTypeAndVersion)obj;
                    return this.type == other.type && this.version == other.version;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return this.type.GetHashCode();
            }
        }
    }
}

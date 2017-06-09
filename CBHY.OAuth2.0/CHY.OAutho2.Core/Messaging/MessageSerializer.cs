using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CHY.OAuth2.Core.Messaging
{
    public class MessageSerializer
    {
        private readonly Type messageType;

        private MessageSerializer(Type messageType)
        {
            this.messageType = messageType;
        }

        public static MessageSerializer Get(Type messageType)
        {
            return new MessageSerializer(messageType);
        }

        public static void DeserializeJsonAsFlatDictionary(IDictionary<string, string> messageDictionary, XmlDictionaryReader reader)
        {
            reader.Read();
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.EndElement)
                {
                    continue;
                }
                string key = reader.Name;
                reader.Read();
                string value = reader.ReadContentAsString();
                messageDictionary[key] = value;
            }
        }

        public static void Serialize(MessageDictionary messageDictionary, XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("root");
            writer.WriteAttributeString("type", "object");
            foreach(var pair in messageDictionary)
            {
                bool include = false;
                string type = "string";
                MessagePart partDescription;
                if(messageDictionary.Description.Mapping.TryGetValue(pair.Key, out partDescription))
                {
                    Assumes.True(partDescription != null);
                    if(partDescription.IsRequired || partDescription.IsNondefaultValueSet(messageDictionary.Message))
                    {
                        include = true;
                        Type formattingType = partDescription.PreferredFormattingType;
                        if(IsNumeric(formattingType))
                        {
                            type = "number";
                        }
                        else if(formattingType.IsAssignableFrom(typeof(bool)))
                        {
                            type = "boolean";
                        }
                    }
                }
                else
                {
                    include = true;
                }
                if(include)
                {
                    writer.WriteStartElement(pair.Key);
                    writer.WriteAttributeString("type", type);
                    writer.WriteString(pair.Value);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public static void Deserialize(MessageDictionary messageDictionary, XmlDictionaryReader reader)
        {
            DeserializeJsonAsFlatDictionary(messageDictionary, reader);
            messageDictionary.Description.EnsureMessagePartsPassBasicValidation(messageDictionary);
            messageDictionary.Message.EnsureValidMessage();
        }

        public IDictionary<string, string> Serialize(MessageDictionary messageDictionary)
        {
            var result = new Dictionary<string, string>();
            foreach(var pair in messageDictionary)
            {
                MessagePart partDescription;
                if(messageDictionary.Description.Mapping.TryGetValue(pair.Key, out partDescription))
                {
                    Assumes.True(partDescription != null);
                    if(partDescription.IsRequired || partDescription.IsNondefaultValueSet(messageDictionary.Message))
                    {
                        result.Add(pair.Key, pair.Value);
                    }
                }
                else
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }

        public void Deserialize(IDictionary<string, string> fields, MessageDictionary messageDictionary)
        {
            var messageDescription = messageDictionary.Description;
            messageDescription.EnsureMessagePartsPassBasicValidation(fields);
            try
            {
                foreach(var pair in fields)
                {
                    messageDictionary[pair.Key] = pair.Value;
                }
            }
            catch(ArgumentException ex)
            {
                throw ErrorUtilities.Wrap(ex, MessagingStrings.ErrorDeserializingMessage, this.messageType.Name);
            }
            messageDictionary.Message.EnsureValidMessage();
            var originalPayloadMessage = messageDictionary.Message as IMessageOriginalPayload;
            if(originalPayloadMessage != null)
            {
                originalPayloadMessage.OriginalPayload = fields;
            }
        }

        private static bool IsNumeric(Type type)
        {
            return type.IsAssignableFrom(typeof(double))
                || type.IsAssignableFrom(typeof(float))
                || type.IsAssignableFrom(typeof(short))
                || type.IsAssignableFrom(typeof(int))
                || type.IsAssignableFrom(typeof(long))
                || type.IsAssignableFrom(typeof(ushort))
                || type.IsAssignableFrom(typeof(uint))
                || type.IsAssignableFrom(typeof(ulong));
        }
    }
}

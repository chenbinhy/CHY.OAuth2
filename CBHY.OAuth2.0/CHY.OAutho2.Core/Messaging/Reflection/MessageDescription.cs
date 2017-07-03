using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CHY.OAuth2.Core.Common;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    /// <summary>
    /// 消息描述
    /// </summary>
    public class MessageDescription
    {
        /// <summary>
        /// 映射
        /// </summary>
        private Dictionary<string, MessagePart> mapping;

        public MessageDescription(Type messageType, Version messageVersion)
        {
            this.MessageType = messageType;
            this.MessageVersion = messageVersion;
            this.ReflectMessageType();
        }

        public IDictionary<string, MessagePart> Mapping
        {
            get { return this.mapping; }
        }

        public Version MessageVersion { get; private set; }

        public Type MessageType { get; private set; }

        public ConstructorInfo[] Constructors { get; private set; }

        public override string ToString()
        {
            return this.MessageType.Name + " (" + this.MessageVersion + ")";
        }

        public MessageDictionary GetDictionary(IMessage message)
        {
            return this.GetDictionary(message, false);
        }

        public MessageDictionary GetDictionary(IMessage message, bool getOriginalValues)
        {
            return new MessageDictionary(message, this, getOriginalValues);
        }

        public void EnsureMessagePartsPassBasicValidation(IDictionary<string, string> parts)
        {
            try
            {
                this.CheckRequiredMessagePartsArePresent(parts.Keys, true);
                this.CheckRequiredProtocolMessagePartsAreNotEmpty(parts, true);
                this.CheckMessagePartsConstantValues(parts, true);
            }
            catch(ProtocolException)
            {
                Logger.Messaging.ErrorFormat("Error while performing basic validation of {0} ({3}) with these message parts:{1}{2}",
                    this.MessageType.Name, Environment.NewLine, parts.ToStringDeferred(), this.MessageVersion);
                throw;
            }
        }

        public bool CheckMessagePartsPassBasicValidation(IDictionary<string, string> parts)
        {
            return this.CheckRequiredMessagePartsArePresent(parts.Keys, false) &&
                this.CheckRequiredProtocolMessagePartsAreNotEmpty(parts, false) &&
                this.CheckMessagePartsConstantValues(parts, false);
        }

        private bool CheckRequiredMessagePartsArePresent(IEnumerable<string> keys, bool throwOnFailure)
        {
            var missingKeys = (from part in this.Mapping.Values 
                               where part.IsRequired && !keys.Contains(part.Name)
                               select part.Name).ToArray();
            if(missingKeys.Length > 0)
            {
                if(throwOnFailure)
                {
                    ErrorUtilities.ThrowProtocol(
                        MessagingStrings.RequiredParametersMissing,
                        this.MessageType.FullName,
                        string.Join(", ", missingKeys)
                        );
                }
                else
                {
                    Logger.Messaging.DebugFormat(
                        MessagingStrings.RequiredParametersMissing,
                        this.MessageType.FullName,
                        missingKeys.ToStringDeferred()
                        );
                        return false;
                }
            }
            return true;
        }

        private bool CheckRequiredProtocolMessagePartsAreNotEmpty(IDictionary<string,string> partValues, bool throwOnFailure)
        {
            string value;
            var emptyValueKeys = (from part in this.Mapping.Values
                                  where !part.AllowEmpty && partValues.TryGetValue(part.Name, out value) && value != null && value.Length == 0
                                  select part.Name).ToArray();
            if(emptyValueKeys.Length > 0)
            {
                if(throwOnFailure){
                    ErrorUtilities.ThrowProtocol(
                        MessagingStrings.RequiredNonEmptyParameterWasEmpty,
                        this.MessageType.FullName,
                        string.Join(", ", emptyValueKeys)
                        );
                }
                else
                {
                    Logger.Messaging.DebugFormat(
                        MessagingStrings.RequiredNonEmptyParameterWasEmpty,
                        this.MessageType.FullName,
                        emptyValueKeys.ToStringDeferred()
                        );
                    return false;
                }
            }
            return true;
        }

        private bool CheckMessagePartsConstantValues(IDictionary<string,string> partValues, bool throwOnFailure)
        {
            var badConstantValues = (from part in this.Mapping.Values
                                     where part.IsConstantValueAvailableStatically
                                     where partValues.ContainsKey(part.Name)
                                     where !string.Equals(partValues[part.Name], part.StaticConstantValue, StringComparison.Ordinal)
                                     select part.Name).ToArray();
            if(badConstantValues.Length > 0)
            {
                if(throwOnFailure)
                {
                    ErrorUtilities.ThrowProtocol(
                        MessagingStrings.RequiredMessagePartConstantIncorrect,
                        this.MessageType.FullName,
                        string.Join(", ", badConstantValues)
                        );
                }
                else
                {
                    Logger.Messaging.DebugFormat(
                        MessagingStrings.RequiredMessagePartConstantIncorrect,
                        this.MessageType.FullName,
                        badConstantValues.ToStringDeferred()
                        );

                    return false;
                }
            }
            return true;
        }

        private void ReflectMessageType()
        {
            this.mapping = new Dictionary<string, MessagePart>();
            Type currentType = this.MessageType;
            do
            {
                foreach (MemberInfo member in currentType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (member is PropertyInfo || member is FieldInfo)
                    {
                        MessagePartAttribute partAttribute =
                            (from a in member.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>()
                             orderby a.MinVersionValue descending
                             where a.MinVersionValue <= this.MessageVersion
                             where a.MaxVersionValue >= this.MessageVersion
                             select a).FirstOrDefault();
                        if (partAttribute != null)
                        {
                            MessagePart part = new MessagePart(member, partAttribute);
                            if (this.Mapping.ContainsKey(part.Name))
                            {
                                Logger.Messaging.WarnFormat(
                                    "Message type {0} has more than one message part named {1}.  Inherited members will be hidden.",
                                    this.MessageType.Name,
                                    part.Name
                                    );
                            }
                            else
                            {
                                this.mapping.Add(part.Name, part);
                            }
                        }
                    }
                }
                currentType = currentType.BaseType;
            } while (currentType != null);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            this.Constructors = this.MessageType.GetConstructors(flags);
        }
    }
}

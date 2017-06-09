using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CHY.OAuth2.Core.Common;

namespace CHY.OAuth2.Core.Messaging
{
    public class StandardMessageFactory : IMessageFactory
    {
        private readonly Dictionary<MessageDescription, ConstructorInfo> requestMessageTypes = new Dictionary<MessageDescription, ConstructorInfo>();
        private readonly Dictionary<MessageDescription, Dictionary<Type, ConstructorInfo>> responseMessageTypes = new Dictionary<MessageDescription, Dictionary<Type, ConstructorInfo>>();
        public StandardMessageFactory()
        {

        }

        public virtual void AddMessageTypes(IEnumerable<MessageDescription> messageTypes)
        {
            var unsupportedMessageTypes = new List<MessageDescription>(0);
            foreach(MessageDescription messageDescription in messageTypes)
            {
                bool supportedMessageType = false;

                if(typeof(IDirectedProtocolMessage).IsAssignableFrom(messageDescription.MessageType))
                {
                    foreach(ConstructorInfo ctor in messageDescription.Constructors)
                    {
                        ParameterInfo[] parameters = ctor.GetParameters();
                        if(parameters.Length == 2 && parameters[0].ParameterType == typeof(Uri) && parameters[1].ParameterType == typeof(Version))
                        {
                            supportedMessageType = true;
                            this.requestMessageTypes.Add(messageDescription, ctor);
                            break;
                        }
                    }
                }
                if (typeof(IDirectResponseProtocolMessage).IsAssignableFrom(messageDescription.MessageType))
                {
                    var responseCtors = new Dictionary<Type, ConstructorInfo>(messageDescription.Constructors.Length);
                    foreach(ConstructorInfo ctor in messageDescription.Constructors)
                    {
                        ParameterInfo[] parameters = ctor.GetParameters();
                        if(parameters.Length == 1 && typeof(IDirectedProtocolMessage).IsAssignableFrom(parameters[0].ParameterType))
                        {
                            responseCtors.Add(parameters[0].ParameterType, ctor);
                        }
                    }
                    if(responseCtors.Count > 0)
                    {
                        supportedMessageType = true;
                        this.responseMessageTypes.Add(messageDescription, responseCtors);
                    }
                }
                if(!supportedMessageType)
                {
                    unsupportedMessageTypes.Add(messageDescription);
                }
            }
            ErrorUtilities.VerifySupported(
                !unsupportedMessageTypes.Any(),
                MessagingStrings.StandardMessageFactoryUnsupportedMessageType,
                unsupportedMessageTypes.ToStringDeferred());
        }

        public virtual IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields)
        {
            MessageDescription matchingType = this.GetMessageDescription(recipient, fields);
            if(matchingType != null)
            {
                return this.InstantiateAsRequest(matchingType, recipient);
            }
            else
            {
                return null;
            }
        }

        public virtual IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string,string> fields)
        {
            MessageDescription matchingType = this.GetMessageDescription(request, fields);
            if(matchingType != null)
            {
                return this.InstantiateAsResponse(matchingType, request);
            }
            else
            {
                return null;
            }
        }

        protected virtual MessageDescription GetMessageDescription(MessageReceivingEndpoint recipient, IDictionary<string,string> fields)
        {
            var matches = this.requestMessageTypes.Keys
                .Where(message => message.CheckMessagePartsPassBasicValidation(fields))
                .OrderByDescending(message => CountInCommon(message.Mapping.Keys, fields.Keys))
                .ThenByDescending(message => message.Mapping.Count)
                .CacheGeneratedResults();
            var match = matches.FirstOrDefault();
            if(match != null)
            {
                if (Logger.Messaging.IsWarnEnabled() && matches.Count() > 1)
                {
                    Logger.Messaging.WarnFormat(
                        "Multiple message types seemed to fit the incoming data: {0}",
                        matches.ToStringDeferred());
                }

                return match;
            }
            else
            {
                return null;
            }
        }

        protected virtual MessageDescription GetMessageDescription(IDirectedProtocolMessage request, IDictionary<string, string> fields)
        {
            var matches = (from responseMessageType in this.responseMessageTypes
                           let messge = responseMessageType.Key
                           where messge.CheckMessagePartsPassBasicValidation(fields)
                           let ctors = this.FindMatchingResponseConstructors(messge, request.GetType())
                           where ctors.Any()
                           orderby GetDerivationDistance(ctors.First().GetParameters()[0].ParameterType, request.GetType()),
                       CountInCommon(messge.Mapping.Keys, fields.Keys) descending,
                       messge.Mapping.Count descending
                           select messge).CacheGeneratedResults();
            var match = matches.FirstOrDefault();
            if(match != null)
            {
                if (Logger.Messaging.IsWarnEnabled() && matches.Count() > 1)
                {
                    Logger.Messaging.WarnFormat(
                        "Multiple message types seemed to fit the incoming data: {0}",
                        matches.ToStringDeferred());
                }

                return match;
            }
            else
            {
                return null;
            }
        }

        protected virtual IDirectedProtocolMessage InstantiateAsRequest(MessageDescription messageDescripton, MessageReceivingEndpoint recipient)
        {
            ConstructorInfo ctor = this.requestMessageTypes[messageDescripton];
            return (IDirectedProtocolMessage)ctor.Invoke(new object[] { recipient.Location, messageDescripton.MessageVersion });
        }

        protected virtual IDirectResponseProtocolMessage InstantiateAsResponse(MessageDescription messageDescription, IDirectedProtocolMessage request)
        {
            Type requestType = request.GetType();
            var ctors = this.FindMatchingResponseConstructors(messageDescription, requestType);
            ConstructorInfo ctor = null;
            try
            {
                ctor = ctors.Single();
            }
            catch(InvalidOperationException)
            {
                if (ctors.Any())
                {
                    ErrorUtilities.ThrowInternal("More than one matching constructor for request type " + requestType.Name + " and response type " + messageDescription.MessageType.Name);
                }
                else
                {
                    ErrorUtilities.ThrowInternal("Unexpected request message type " + requestType.FullName + " for response type " + messageDescription.MessageType.Name);
                }
            }

            return (IDirectResponseProtocolMessage)ctor.Invoke(new object[] { request });
        }

        private static int GetDerivationDistance(Type assignableType, Type derivedType)
        {
            if(derivedType.IsAssignableFrom(assignableType))
            {
                return 0;
            }
            int steps;
            derivedType = derivedType.BaseType;
            for(steps = 1; assignableType.IsAssignableFrom(derivedType); steps++)
            {
                derivedType = derivedType.BaseType;
            }

            return steps;
        }

        private static int CountInCommon(ICollection<string> collection1, ICollection<string> collection2, StringComparison comparison = StringComparison.Ordinal)
        {
            return collection1.Count(value1 => collection2.Any(value2 => string.Equals(value1, value2, comparison)));
        }

        private IEnumerable<ConstructorInfo> FindMatchingResponseConstructors(MessageDescription messageDescription, Type requestType)
        {
            return this.responseMessageTypes[messageDescription].Where(pair => pair.Key.IsAssignableFrom(requestType)).Select(pair => pair.Value);
        }
    }
}

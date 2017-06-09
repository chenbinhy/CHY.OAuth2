using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements
{
    public class EndUserAuthorizationResponseTypeEncoder:IMessagePartEncoder
    {
        public EndUserAuthorizationResponseTypeEncoder()
        {

        }

        public string Encode(object value)
        {
            var responseType = (EndUserAuthorizationResponseType)value;
            switch(responseType)
            {
                case EndUserAuthorizationResponseType.AccessToken:
                    return Protocol.ResponseTypes.Token;
                case EndUserAuthorizationResponseType.AuthorizationCode:
                    return Protocol.ResponseTypes.Code;
                default:
                    throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.response_type, value);
            }
        }

        public object Decode(string value)
        {
            switch(value)
            {
                case Protocol.ResponseTypes.Token:
                    return EndUserAuthorizationResponseType.AccessToken;
                case Protocol.ResponseTypes.Code:
                    return EndUserAuthorizationResponseType.AuthorizationCode;
                default:
                    throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.response_type, value);
            }
        }
    }
}

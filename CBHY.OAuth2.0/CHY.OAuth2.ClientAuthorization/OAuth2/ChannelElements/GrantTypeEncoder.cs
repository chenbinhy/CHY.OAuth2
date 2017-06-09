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
    public class GrantTypeEncoder : IMessagePartEncoder
    {
        public GrantTypeEncoder() { }

        public string Encode(object value)
        {
            var responseType = (GrantType)value;
            switch(responseType)
            {
                case GrantType.ClientCredentials:
                    return Protocol.GrantTypes.ClientCredentials;
                case GrantType.AuthorizationCode:
                    return Protocol.GrantTypes.AuthorizationCode;
                case GrantType.RefreshToken:
                    return Protocol.GrantTypes.RefreshToken;
                case GrantType.Password:
                    return Protocol.GrantTypes.Password;
                case GrantType.Assertion:
                    return Protocol.GrantTypes.Assertion;
                default:
                    throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.grant_type, value);
            }
        }

        public object Decode(string value)
        {
            switch(value)
            {
                case Protocol.GrantTypes.ClientCredentials:
                    return GrantType.ClientCredentials;
                case Protocol.GrantTypes.Assertion:
                    return GrantType.Assertion;
                case Protocol.GrantTypes.Password:
                    return GrantType.Password;
                case Protocol.GrantTypes.RefreshToken:
                    return GrantType.RefreshToken;
                case Protocol.GrantTypes.AuthorizationCode:
                    return GrantType.AuthorizationCode;
                default:
                    throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.grant_type, value);
            }
        }
    }


}
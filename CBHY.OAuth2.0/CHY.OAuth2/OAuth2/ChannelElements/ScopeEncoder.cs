using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.ChannelElements
{
    public class ScopeEncoder:IMessagePartEncoder
    {
        public ScopeEncoder()
        {

        }

        public string Encode(object value)
        {
            var scopes = (IEnumerable<string>)value;
            ErrorUtilities.VerifyProtocol(!scopes.Any(scope => scope.Contains(" ")), OAuthStrings.ScopesMayNotContainSpaces);
            return (scopes != null && scopes.Any()) ? string.Join(" ", scopes.ToArray()) : null;
        }

        public object Decode(string value)
        {
            return OAuthUtilities.SplitScopes(value);
        }
    }
}

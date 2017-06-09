using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2
{
    public static class OAuthUtilities
    {
        public static readonly StringComparer ScopeStringComparer = StringComparer.Ordinal;
        private const string HttpBasicAuthScheme = "Basic";
        private static readonly char[] scopeDelimiter = new char[] { ' ' };
        private static readonly char[] ColonSeparator = new char[] { ';' };
        private static readonly Encoding HttpBasicEncoding = Encoding.UTF8;
        private static string accessTokenAuthorizationHeaderAllowedCharacters = MessagingUtilities.UppercaseLetters +
            MessagingUtilities.LowercaseLetters +
            MessagingUtilities.Digits +
            @"!#$%&'()*+-./:<=>?@[]^_`{|}~\,;";

        public static HashSet<string> SplitScopes(string scope)
        {
            if(string.IsNullOrEmpty(scope))
            {
                return new HashSet<string>();
            }
            var set = new HashSet<string>(scope.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries), ScopeStringComparer);
            VerifyValidScopeTokens(set);

            return set;
        }

        public static string JoinScopes(HashSet<string> scopes)
        {
            VerifyValidScopeTokens(scopes);
            return string.Join(" ", scopes.ToArray());
        }

        public static HashSet<string> ParseScopeSet(string scopes)
        {
            return ParseScopeSet(scopes.Split(scopeDelimiter, StringSplitOptions.RemoveEmptyEntries));
        }

        public static HashSet<string> ParseScopeSet(string[] scopes)
        {
            return new HashSet<string>(scopes, StringComparer.Ordinal);
        }

        public static void VerifyValidScopeTokens(IEnumerable<string> scopes)
        {
            foreach(string scope in scopes)
            {
                VerifyValidScopeToken(scope);
            }
        }

        public static void VerifyValidScopeToken(string scopeToken)
        {
            ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(scopeToken), OAuthStrings.InvalidScopeToken, scopeToken);
            for(int i = 0;i < scopeToken.Length; i++)
            {
                char ch = scopeToken[i];
                if(!(ch == '\x21' || (ch >= '\x23' && ch <= '\x5B') || (ch >= '\x5D' && ch <= '\x7E')))
                {
                    ErrorUtilities.ThrowProtocol(OAuthStrings.InvalidScopeToken, scopeToken);
                }
            }
        }

        public static void AuthorizeWithBearerToken(WebHeaderCollection requestHeaders, string accessToken)
        {
            ErrorUtilities.VerifyProtocol(accessToken.All(ch => accessTokenAuthorizationHeaderAllowedCharacters.IndexOf(ch) > 0), OAuthStrings.AccessTokenInvalidForHttpAuthorizationHeader);

            requestHeaders[HttpRequestHeader.Authorization] = string.Format(
                CultureInfo.InvariantCulture,
                Protocol.BearerHttpAuthorizationHeaderFormat,
                accessToken
                );
        }

        public static void ApplyHttpBasicAuth(System.Net.Http.Headers.HttpRequestHeaders headers, string userName, string password)
        {
            string concat = userName + ":" + password;
            byte[] bits = HttpBasicEncoding.GetBytes(concat);
            string base64 = Convert.ToBase64String(bits);
            headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(HttpBasicAuthScheme, base64);
        }

        public static NetworkCredential ParseHttpBasicAuth(System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            var authorizationHeader = headers.Authorization;
            if(authorizationHeader != null && string.Equals(authorizationHeader.Scheme, HttpBasicAuthScheme, StringComparison.Ordinal))
            {
                string base64 = authorizationHeader.Parameter;
                byte[] bits = Convert.FromBase64String(base64);
                string usernameColonPassword = HttpBasicEncoding.GetString(bits);
                string[] usernameAndPassword = usernameColonPassword.Split(ColonSeparator, 2);
                if(usernameAndPassword.Length == 2)
                {
                    return new NetworkCredential(usernameAndPassword[0], usernameAndPassword[1]);
                }
            }

            return null;
        }
    }
}

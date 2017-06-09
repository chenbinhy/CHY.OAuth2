using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2
{
    public enum ProtocolVersion
    { 
        V20
    }

    public class Protocol
    {
        public const string BearerHttpAuthorizationScheme = "Bearer";

        public const string BearerHttpAuthorizationSchemeWithTrailingSpace = BearerHttpAuthorizationScheme + " ";

        public const string BearerHttpAuthorizationHeaderFormat = BearerHttpAuthorizationSchemeWithTrailingSpace + "{0}";

        public const string BearerTokenEncodedUrlParameterName = "access_token";

        public const string state = "state";

        public const string redirect_uri_mismatch = "redirect_uri_mismatch";

        public const string redirect_uri = "redirect_uri";

        public const string client_id = "client_id";

        public const string scope = "scope";

        public const string client_secret = "client_secret";

        public const string code = "code";

        public const string error = "error";

        public const string access_token = "access_token";

        public const string token_type = "token_type";

        public const string refresh_token = "refresh_token";

        public const string expires_in = "expires_in";

        public const string username = "username";

        public const string password = "password";

        public static readonly Protocol V20 = new Protocol
        {
            Version = new Version(2, 0),
            ProtocolVersion = ProtocolVersion.V20
        };

        public static readonly List<Protocol> AllVersions = new List<Protocol>() { V20 };

        public static readonly Protocol Default = AllVersions[0];

        public const string error_uri = "error_uri";

        public const string error_description = "error_description";

        public const string response_type = "response_type";

        public const string grant_type = "grant_type";

        public Version Version { get; private set; }

        public ProtocolVersion ProtocolVersion { get; private set; }

        public static Protocol Lookup(ProtocolVersion version)
        {
            switch(version)
            {
                case ProtocolVersion.V20:
                    return Protocol.V20;
                default: throw new ArgumentOutOfRangeException("version");
            }
        }

        public static class EndUserAuthorizationRequestErrorCodes
        {
            public const string InvalidRequest = "invalid_request";

            public const string UnauthorizedClient = "unauthorized_client";

            public const string AccessDenied = "access_denied";

            public const string UnsupoortedResponseType = "unsupported_response_type";

            public const string InvalidScope = "invalid_scope";

            public const string ServerError = "server_error";

            public const string TemporarilyUnavailable = "temporarily_unavailable";
        }

        public static class ResponseTypes
        {
            public const string Code = "code";
            public const string Token = "token";
        }

        public static class GrantTypes
        {
            public const string AuthorizationCode = "authorization_code";
            public const string Password = "password";
            public const string Assertion = "assertion";
            public const string RefreshToken = "refresh_token";
            public const string ClientCredentials = "client_credentials";
        }

        public static class AccessTokenRequestErrorCodes
        {
            public const string InvalidRequest = "invalid_request";

            public const string InvalidClient = "invalid_client";

            public const string InvalidGrant = "invalid_grant";
            public const string UnauthorizedClient = "unauthorized_client";
            public const string UnsupportedGrantType = "unsupported_grant_type";
            public const string InvalidScope = "invalid_scope";
        }

        public static class AccessTokenTypes
        {
            public const string Bearer = "bearer";
        }

        public static class BearerTokenUnauthorizedResponseParameters
        {
            public const string Realm = "realm";
            public const string ErrorCode = "error";
            public const string ErrorDescription = "error_description";
            public const string ErrorUri = "error_uri";
            public const string Scope = "scope";
        }

        public static class BearerTokenErrorCodes
        {
            public const string InvalidRequest = "invalid_request";
            public const string InvalidToken = "invalid_token";
            public const string InsufficientScope = "insufficient_scope";
        }
    }
}

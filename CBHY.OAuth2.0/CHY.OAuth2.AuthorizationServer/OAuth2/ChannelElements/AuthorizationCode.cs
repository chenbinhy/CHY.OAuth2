using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using CHY.OAuth2.OAuth2;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    /// <summary>
    /// 授权码
    /// </summary>
    public class AuthorizationCode:AuthorizationDataBag
    {
        public const string AuthorizationCodeKeyBucket = "https://localhost/dnoa/oauth_authorization_code";

        public AuthorizationCode()
        {

        }

        public AuthorizationCode(string clientIdentifier, Uri callback, IEnumerable<string> scopes, string username)
        {
            this.ClientIdentifier = clientIdentifier;
            this.CallbackHash = CalculateCallbackHash(callback);
            this.Scope.ResetContents(scopes);
            this.User = username;
            this.UtcCreationDate = DateTime.UtcNow;
        }

        public static TimeSpan MaximumMessageAge
        {
            get { return CHY.OAuth2.Core.Configuration.DotNetOpenAuthSection.Messaging.MaximumMessageLifetimeNoSkew; }
        }

        [MessagePart("cb")]
        private byte[] CallbackHash { get; set; }

        public static IDataBagFormatter<AuthorizationCode> CreateFormatter(IAuthorizationServerHost authorizationServer)
        {
            var cryptoStore = authorizationServer.CryptoKeyStore;
            ErrorUtilities.VerifyHost(cryptoStore != null, OAuthStrings.ResultShouldNotBeNull, authorizationServer.GetType(), "CryptoKeyStore");

            return new UriStyleMessageFormatter<AuthorizationCode>(
                cryptoStore,
                AuthorizationCodeKeyBucket,
                signed:true,
                encrypted:true,
                compressed:false,
                maximumAge:MaximumMessageAge,
                decodeOnceOnly:authorizationServer.NonceStore
                );
        }

        public void VerifyCallback(Uri callback)
        {
            ErrorUtilities.VerifyProtocol(MessagingUtilities.AreEquivalentConstantTime(this.CallbackHash, CalculateCallbackHash(callback)), Protocol.redirect_uri_mismatch);
        }

        private static byte[] CalculateCallbackHash(Uri callback)
        {
            if(callback == null)
            {
                return null;
            }
            using(var hasher = SHA256.Create())
            {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(callback.AbsoluteUri));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CHY.OAuth2.Core.Common;
using System.Net.Http;
using System.Threading;
using System.Xml;
using CHY.OAuth2.Core.Messaging.Reflection;
using System.Net.Http.Headers;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Json;
using CHY.OAuth2.Core.Messaging.Bindings;

namespace CHY.OAuth2.Core.Messaging
{
    public static class MessagingUtilities
    {
        public static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

        public const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        public const string Digits = "0123456789";
        public const string AlphaNumeric = UppercaseLetters + LowercaseLetters + Digits;
        public const string Base64Characters = AlphaNumeric + "+" + "/";
        public const string Base64WebSafeCharacters = AlphaNumeric + "-" + "_";
        public const string AlphaNumericNoLookAlikes = "23456789abcdefghjkmnpqrstwxyzABCDEFGHJKMNPQRSTWXYZ";
        private const int SymmetricSecretHandleLength = 4;
        private static readonly Task CompletedTaskField = Task.FromResult<object>(null);
        private static readonly TimeSpan SymmetricSecretKeyLifespan = Configuration.DotNetOpenAuthSection.Messaging.PrivateSecretMaximumAge;
        private static readonly char[] EqualsArray = new char[] { '=' };
        private static readonly char[] CommaArray = new char[] { ',' };
        private static readonly char[] QuoteArray = new char[] { '"' };
        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")"};
        private static readonly Dictionary<string, string> javascriptStaticStringEscaping = new Dictionary<string, string>
        {
            { "\\", @"\\" }, // this WAS just above the & substitution but we moved it here to prevent double-escaping
			{ "\t", @"\t" },
			{ "\n", @"\n" },
			{ "\r", @"\r" },
			{ "\u0085", @"\u0085" },
			{ "\u2028", @"\u2028" },
			{ "\u2029", @"\u2029" },
			{ "'", @"\x27" },
			{ "\"", @"\x22" },
			{ "&", @"\x26" },
			{ "<", @"\x3c" },
			{ ">", @"\x3e" },
			{ "=", @"\x3d" },
        };

        public enum CompressionMethod
        {
            Deflate,
            Gzip,
        }

        public static Task CompletedTask
        {
            get { return CompletedTaskField; }
        }

        public static Random NonCryptoRandomDataGenerator
        {
            get { return ThreadSafeRandom.RandomNumberGenerator; }
        }

        public static Uri GetRequestUrlFromContext()
        {
            return new HttpRequestWrapper(HttpContext.Current.Request).GetPublicFacingUrl();
        }

        public static Uri StripQueryArgumentsWithPrefix(this Uri uri, string prefix)
        {
            NameValueCollection queryArgs = HttpUtility.ParseQueryString(uri.Query);
            var matchingKeys = queryArgs.Keys.OfType<string>().Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            if(matchingKeys.Count > 0)
            {
                UriBuilder builder = new UriBuilder(uri);
                foreach(string key in matchingKeys)
                {
                    queryArgs.Remove(key);
                }
                builder.Query = CreateQueryString(queryArgs.ToDictionary());
                return builder.Uri;
            }
            else
            {
                return uri;
            }
        }

        public static string ToStringDescriptive(this Exception exception)
        {
            if(exception == null)
            {
                Logger.Messaging.Error("MessagingUtilities.GetAllMessages called with null input.");
            }
            StringBuilder message = new StringBuilder();
            while(exception != null)
            {
                message.Append(exception.Message);
                exception = exception.InnerException;
                if(exception != null)
                {
                    message.Append(" ");
                }
            }
            return message.ToString();
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> sequence)
        {
            foreach (IEnumerable<T> subsequence in sequence)
            {
                foreach(T item in subsequence)
                {
                    yield return item;
                }
            }
        }

        public static DateTime CutToSecond(this DateTime value)
        {
            return value - TimeSpan.FromMilliseconds(value.Millisecond);
        }

        public static void AppendQueryArgument(this UriBuilder builder, string name, string value)
        {
            AppendQueryArgs(builder, new[] {new KeyValuePair<string,string>(name, value)});
        }

        public static void AddRange<T>(this ICollection<T> collection , IEnumerable<T> values)
        {
            foreach(var value in values)
            {
                collection.Add(value);
            }
        }

        public static bool Equals(this TimeSpan self, TimeSpan other, TimeSpan marginOfError)
        {
            return TimeSpan.FromMilliseconds(Math.Abs((self - other).TotalMilliseconds)) < marginOfError;
        }

        public static bool EqualsConstantTime(string value1, string value2)
        {
            if(value1 == null ^ value2 == null)
            {
                return false;
            }

            if(value1 == null)
            {
                return true;
            }

            if(value1.Length != value2.Length)
            {
                return false;
            }
            int result = 0;
            for(int i = 0; i < value1.Length; i++)
            {
                result |= value1[i] ^ value2[i];
            }
            return result == 0;
        }

        public static Uri GetPublicFacingUrl(this HttpRequestBase request, NameValueCollection serverVariables)
        {
            string httpHost;
            try
            {
                httpHost = serverVariables["HTTP_HOST"];
            }
            catch(NullReferenceException)
            {
                httpHost = null;
            }
            if(httpHost != null)
            {
                ErrorUtilities.VerifySupported(request.Url.Scheme == Uri.UriSchemeHttps || request.Url.Scheme == Uri.UriSchemeHttp, "Only HTTP and HTTPS are supported protocols");
                string scheme = serverVariables["HTTP_X_FORWARDED_PROTO"]??
                    (string.Equals(serverVariables["HTTP_FRONT_END_HTTPS"], "on", StringComparison.OrdinalIgnoreCase)?Uri.UriSchemeHttps:request.Url.Scheme);
                Uri hostAndPort = new Uri(scheme + Uri.SchemeDelimiter + serverVariables["HTTP_HOST"]);
                UriBuilder publicRequestUri = new UriBuilder(request.Url);
                publicRequestUri.Scheme = scheme;
                publicRequestUri.Host = hostAndPort.Host;
                publicRequestUri.Port = hostAndPort.Port;
                return publicRequestUri.Uri;
            }
            else
            {
                return new Uri(request.Url, request.RawUrl);
            }
        }

        public static Uri GetPublicFacingUrl(this HttpRequestBase request)
        {
            return GetPublicFacingUrl(request, request.ServerVariables);
        }

        public static Uri GetPublicFacingUrl()
        {
            ErrorUtilities.VerifyHttpContext();
            return GetPublicFacingUrl(new HttpRequestWrapper(HttpContext.Current.Request));
        }

        public static HttpRequestMessage AsHttpRequestMessage(this HttpRequestBase request)
        {
            Uri publicFacingUrl = request.GetPublicFacingUrl();
            var httpRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), publicFacingUrl);
            if(request.Form != null)
            {
                httpRequest.Content = new FormUrlEncodedContent(request.Form.AsKeyValuePairs());
            }
            else if(request.InputStream != null)
            {
                httpRequest.Content = new StreamContent(request.InputStream);
            }
            httpRequest.CopyHeadersFrom(request);
            return httpRequest;
        }

        public static async Task SendAsync(this HttpResponseMessage response, HttpContextBase context = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(context == null)
            {
                ErrorUtilities.VerifyHttpContext();
                context = new HttpContextWrapper(HttpContext.Current);
            }
            var responseContext = context.Response;
            responseContext.StatusCode = (int)response.StatusCode;
            responseContext.StatusDescription = response.ReasonPhrase;
            foreach(var header in response.Headers)
            {
                foreach(var value in header.Value)
                {
                    responseContext.AddHeader(header.Key, value);
                }
            }
            if(response.Content != null)
            {
                await response.Content.CopyToAsync(responseContext.OutputStream).ConfigureAwait(false);
            }
        }

        public static void DisposeIfNotNull(this IDisposable disposable)
        {
            if(disposable != null)
            {
                disposable.Dispose();
            }
        }

        public static HttpRequestMessage Clone(this HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);
            clone.Content = original.Content;
            foreach(var header in original.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            foreach(var property in original.Properties)
            {
                clone.Properties[property.Key] = property.Value;
            }
            clone.Version = original.Version;

            return clone;
        }

        public static Uri GetWebRoot()
        {
            HttpRequestBase requestInfo = new HttpRequestWrapper(HttpContext.Current.Request);
            UriBuilder realmUrl = new UriBuilder(requestInfo.GetPublicFacingUrl());
            realmUrl.Path = HttpContext.Current.Request.ApplicationPath;
            realmUrl.Query = null;
            realmUrl.Fragment = null;
            return realmUrl.Uri;
        }

        public static XmlReaderSettings CreateUntrustedXmlReaderSettings()
        {
            return new XmlReaderSettings
            {
                MaxCharactersFromEntities = 1024,
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
        }

        public static void ResetContents<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            collection.Clear();
            if(values != null)
            {
                AddRange(collection, values);
            }
        }

        public static Uri StripMessagePartsFromQueryString(this Uri uri, MessageDescription messageDescription)
        {
            NameValueCollection queryArgs = HttpUtility.ParseQueryString(uri.Query);
            var matchingKeys = queryArgs.Keys.OfType<string>().Where(key => messageDescription.Mapping.ContainsKey(key)).ToList();
            if(matchingKeys.Count > 0)
            {
                var builder = new UriBuilder(uri);
                foreach(string key in matchingKeys)
                {
                    queryArgs.Remove(key);
                }
                builder.Query = CreateQueryString(queryArgs.ToDictionary());
                return builder.Uri;
            }
            else
            {
                return uri;
            }
        }

        public static string AssembleAuthorizationHeader(IEnumerable<KeyValuePair<string,string>> fields)
        {
            var authorization = new StringBuilder();
            foreach(var pair in fields)
            {
                string key = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Key);
                string value = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Value);
                authorization.Append(key);
                authorization.Append("=\"");
                authorization.Append(value);
                authorization.Append("\",");
            }
            authorization.Length--;

            return authorization.ToString();
        }

        public static string AssembleAuthorizationHeader(string scheme, IEnumerable<KeyValuePair<string,string>> fields)
        {
            var authorization = new StringBuilder();
            authorization.Append(scheme);
            authorization.Append(" ");
            authorization.Append(AssembleAuthorizationHeader(fields));

            return authorization.ToString();
        }

        public static IEnumerable<KeyValuePair<string,string>> ParseAuthorizationHeader(string scheme, AuthenticationHeaderValue authorizationHeader)
        {
            if(authorizationHeader != null && authorizationHeader.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
            {
                return from element in authorizationHeader.Parameter.Split(CommaArray)
                       let parts = element.Trim().Split(EqualsArray, 2)
                       let key = Uri.UnescapeDataString(parts[0])
                       let value = Uri.UnescapeDataString(parts[1].Trim(QuoteArray))
                       select new KeyValuePair<string, string>(key, value);
            }

            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public static string CombinKeyHandleAndPayload(string handle, string payload)
        {
            return handle + "!" + payload;
        }

        public static void ExtractKeyHandleAndPayload(string messagePart, string keyHandleAndBlob, out string handle, out string dataBlob)
        {
            int privateHandleIndex = keyHandleAndBlob.IndexOf('!');
            ErrorUtilities.VerifyProtocol(privateHandleIndex > 0, MessagingStrings.UnexpectedMessagePartValue, messagePart ?? "<unknown>", keyHandleAndBlob);
            handle = keyHandleAndBlob.Substring(0, privateHandleIndex);
            dataBlob = keyHandleAndBlob.Substring(privateHandleIndex + 1);
        }

        /// <summary>
        /// 非加密随机数据
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetNonCryptoRandomData(int length)
        {
            byte[] buffer = new byte[length];
            NonCryptoRandomDataGenerator.NextBytes(buffer);
            return buffer;
        }

        public static byte[] GetCryptoRandomData(int length)
        {
            byte[] buffer = new byte[length];
            CryptoRandomDataGenerator.GetBytes(buffer);
            return buffer;
        }

        public static string GetCryptoRandomDataAsBase64(int binaryLength)
        {
            byte[] uniq_bytes = GetCryptoRandomData(binaryLength);
            string uniq = Convert.ToBase64String(uniq_bytes);
            return uniq;
        }

        public static string GetNonCryptoRandomDataAsBase64(int binaryLength, bool useWeb64 = false)
        {
            byte[] uniq_bytes = GetNonCryptoRandomData(binaryLength);
            string uniq = useWeb64 ? ConvertToBase64WebSafeString(uniq_bytes) : Convert.ToBase64String(uniq_bytes);
            return uniq;
        }

        public static string GetRandomString(int length, string allowableCharacters)
        {
            char[] randomString = new char[length];
            var random = NonCryptoRandomDataGenerator;
            for(int i = 0;i < length; i++)
            {
                randomString[i] = allowableCharacters[random.Next(allowableCharacters.Length)];
            }
            return new string(randomString);
        }

        public static string ComputeHash(this HashAlgorithm algorithm, string value, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytesToHash = encoding.GetBytes(value);
            byte[] hash = algorithm.ComputeHash(bytesToHash);
            string base64Hash = Convert.ToBase64String(hash);
            return base64Hash;
        }

        public static string ComputeHash(this HashAlgorithm algorithm, IDictionary<string,string>data, Encoding encoding = null)
        {
            var sortedData = new SortedDictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
            return ComputeHash(algorithm, (IEnumerable<KeyValuePair<string, string>>)sortedData, encoding);
        }

        public static string ComputeHash(this HashAlgorithm algorithm, IEnumerable<KeyValuePair<string,string>> sortedData, Encoding encoding = null)
        {
            return ComputeHash(algorithm, CreateQueryString(sortedData), encoding);
        }

        public static byte[] Encrypt(byte[] buffer, byte[] key)
        {
            using(SymmetricAlgorithm crypto = CreateSymmetricAlgorithm(key))
            {
                using(var ms = new MemoryStream())
                {
                    var binaryWrite = new BinaryWriter(ms);
                    binaryWrite.Write((byte)1);
                    binaryWrite.Write(crypto.IV);
                    binaryWrite.Flush();
                    var cryptoSteam = new CryptoStream(ms, crypto.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoSteam.Write(buffer, 0, buffer.Length);
                    cryptoSteam.FlushFinalBlock();

                    return ms.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] buffer, byte[] key)
        {
            using(SymmetricAlgorithm crypto = CreateSymmetricAlgorithm(key))
            {
                using(var ms = new MemoryStream(buffer))
                {
                    var binaryReader = new BinaryReader(ms);
                    int algorithmVersion = binaryReader.ReadByte();
                    ErrorUtilities.VerifyProtocol(algorithmVersion == 1, MessagingStrings.UnsupportedEncryptionAlgorithm);
                    crypto.IV = binaryReader.ReadBytes(crypto.IV.Length);

                    var decryptedBuffer = new byte[buffer.Length];
                    int actualDecryptedLength;
                    using(var cryptoStream = new CryptoStream(ms, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        actualDecryptedLength = cryptoStream.Read(decryptedBuffer, 0, decryptedBuffer.Length);
                    }
                    var finalDecryptedBuffer = new byte[actualDecryptedLength];
                    Array.Copy(decryptedBuffer, finalDecryptedBuffer, actualDecryptedLength);

                    return finalDecryptedBuffer;
                }
            }
        }

        public static string Encrypt(string plainText, byte[] key)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            byte[] cipher = Encrypt(buffer, key);

            return Convert.ToBase64String(cipher);
        }

        public static string Decrypt(string cipherText, byte[] key)
        {
            byte[] cipher = Convert.FromBase64String(cipherText);
            byte[] plainText = Decrypt(cipher, key);

            return Encoding.UTF8.GetString(plainText);
        }

        public static byte[] EncryptWithRandomSymmetricKey(this RSACryptoServiceProvider crypto, byte[] buffer)
        {
            using(var symmetricCrypto = SymmetricAlgorithm.Create())
            {
                symmetricCrypto.Mode = CipherMode.CBC;
                using(var encryptedStream = new MemoryStream())
                {
                    var encryptedStreamWriter = new BinaryWriter(encryptedStream);
                    byte[] prequel = new byte[symmetricCrypto.Key.Length + symmetricCrypto.IV.Length];
                    Array.Copy(symmetricCrypto.Key, prequel, symmetricCrypto.Key.Length);
                    Array.Copy(symmetricCrypto.IV, 0, prequel, symmetricCrypto.Key.Length, symmetricCrypto.IV.Length);
                    byte[] encryptePrequel = crypto.Encrypt(prequel, false);
                    encryptedStreamWriter.Write(encryptePrequel.Length);
                    encryptedStreamWriter.Write(encryptePrequel);
                    encryptedStreamWriter.Flush();

                    var cryptoStream = new CryptoStream(encryptedStream, symmetricCrypto.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(buffer, 0, buffer.Length);
                    cryptoStream.FlushFinalBlock();

                    return encryptedStream.ToArray();
                }
            }
        }

        public static byte[] DecryptWithRandomSymmetricKey(this RSACryptoServiceProvider crypto, byte[] buffer)
        {
            using(var encryptedStream = new MemoryStream(buffer))
            {
                var encryptedStreamReader = new BinaryReader(encryptedStream);
                byte[] encrptedPrequel = encryptedStreamReader.ReadBuffer(4096);
                byte[] prequel = crypto.Decrypt(encrptedPrequel, false);
                using(var symmetricCrypto = SymmetricAlgorithm.Create())
                {
                    symmetricCrypto.Mode = CipherMode.CBC;
                    byte[] symmetricKey = new byte[symmetricCrypto.Key.Length];
                    byte[] symmetricIV = new byte[symmetricCrypto.IV.Length];
                    Array.Copy(prequel, symmetricKey, symmetricKey.Length);
                    Array.Copy(prequel, symmetricKey.Length, symmetricIV, 0, symmetricIV.Length);
                    symmetricCrypto.Key = symmetricKey;
                    symmetricCrypto.IV = symmetricIV;
                    var decryptedBuffer = new byte[encryptedStream.Length - encryptedStream.Position];
                    int actualDecryptedLength;
                    using(var cryptoStream = new CryptoStream(encryptedStream, symmetricCrypto.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        actualDecryptedLength = cryptoStream.Read(decryptedBuffer, 0, decryptedBuffer.Length);
                    }
                    var finalDecryptedBuffer = new byte[actualDecryptedLength];
                    Array.Copy(decryptedBuffer, finalDecryptedBuffer, actualDecryptedLength);
                    return finalDecryptedBuffer;
                }
            }
        }

        public static KeyValuePair<string, CryptoKey> GetCurrentKey(this ICryptoKeyStore cryptoKeyStore, string bucket, TimeSpan minimumRemainingLife, int keySize = 256)
        {
            var cryptoKeyPair = cryptoKeyStore.GetKeys(bucket).FirstOrDefault(pair => pair.Value.Key.Length == keySize / 8);
            if(cryptoKeyPair.Value == null || cryptoKeyPair.Value.ExpiresUtc<DateTime.UtcNow + minimumRemainingLife)
            {
                if(cryptoKeyPair.Value == null)
                {
                    Logger.Messaging.InfoFormat("{0}.GetKeys returned no keys for bucket \"{1}\" with the required key length of {2} bits.  A new key will be created", typeof(ICryptoKeyStore), bucket, keySize);
                }else
                {
                    Logger.Messaging.InfoFormat("The first key returned by {0}.GetKeys for bucket \"{1}\" with the required key length of {2} bits was too near expiry to use.  A new key will be created", typeof(ICryptoKeyStore), bucket, keySize);
                }

                ErrorUtilities.VerifyHost(minimumRemainingLife <= SymmetricSecretKeyLifespan, "Unable to create a new symmetric key with the required lifespan of {0} because it is beyond the limit of {1}.", minimumRemainingLife, SymmetricSecretKeyLifespan);
                byte[] secret = GetCryptoRandomData(keySize / 8);
                DateTime expires = DateTime.UtcNow + SymmetricSecretKeyLifespan;
                var cryptoKey = new CryptoKey(secret, expires);
                int failedAttempts = 0;
            tryAgain:
                try
                {
                    string handle = GetRandomString(SymmetricSecretHandleLength, Base64WebSafeCharacters);
                    cryptoKeyPair = new KeyValuePair<string, CryptoKey>(handle, cryptoKey);
                    cryptoKeyStore.StoreKey(bucket, handle, cryptoKey);
                }catch(CryptoKeyCollisionException)
                {
                    ErrorUtilities.VerifyProtocol(++failedAttempts < 3, "Unable to derive a unique handle to a private symmetric key.");
                    Logger.Messaging.Warn("A randomly generated crypto key handle collided with an existing handle.  Another randomly generated handle will be attempted till the retry count is met.");
                    goto tryAgain;
                }
            }
            return cryptoKeyPair;
        }

        public static byte[] Compress(byte[] buffer, CompressionMethod method = CompressionMethod.Deflate)
        {
            using(var ms = new MemoryStream())
            {
                Stream compressingStream = null;
                try
                {
                    switch(method)
                    {
                        case CompressionMethod.Deflate:
                            compressingStream = new DeflateStream(ms, CompressionMode.Compress, true);
                            break;
                        case CompressionMethod.Gzip:
                            compressingStream = new GZipStream(ms, CompressionMode.Compress, true);
                            break;
                        default:
                            break;
                    }
                    compressingStream.Write(buffer, 0, buffer.Length);
                    return ms.ToArray();
                }
                finally
                {
                    if(compressingStream != null)
                    {
                        compressingStream.Dispose();
                    }
                }
            }
        }

        public static byte[] Decompress(byte[] buffer, CompressionMethod method = CompressionMethod.Deflate)
        {
            using(var compressedDataStream = new MemoryStream(buffer))
            {
                using(var decompressedDataStream = new MemoryStream())
                {
                    Stream decompressingStream = null;
                    try
                    {
                        switch(method)
                        {
                            case CompressionMethod.Deflate:
                                decompressingStream = new DeflateStream(compressedDataStream, CompressionMode.Decompress, true);
                                break;
                            case CompressionMethod.Gzip:
                                decompressingStream = new GZipStream(compressedDataStream, CompressionMode.Decompress, true);
                                break;
                            default:
                                break;
                        }
                        decompressingStream.CopyTo(decompressedDataStream);
                    }
                    finally
                    {
                        if(decompressingStream != null)
                        {
                            decompressingStream.Dispose();
                        }
                    }
                    return decompressedDataStream.ToArray();
                }
            }
        }

        public static string ConvertToBase64WebSafeString(byte[] data)
        {
            var builder = new StringBuilder(Convert.ToBase64String(data));
            builder.Replace('+', '-').Replace('/', '_');
            while(builder[builder.Length -1] == '=')
            {
                builder.Length -= 1;
            }

            return builder.ToString();
        }

        public static byte[] FromBase64WebSafeString(string base64WebSafe)
        {
            int missingPaddingCharacters;
            switch(base64WebSafe.Length % 4)
            {
                case 3:
                    missingPaddingCharacters = 1;
                    break;
                case 2:
                    missingPaddingCharacters = 2;
                    break;
                case 0:
                    missingPaddingCharacters = 0;
                    break;
                default:
                    throw new ProtocolException(MessagingStrings.DataCorruptionDetected, new ArgumentException("No more than two padding characters should be present for base64."));
            }
            var builder = new StringBuilder(base64WebSafe, base64WebSafe.Length + missingPaddingCharacters);
            builder.Replace('-', '+').Replace('_', '/');
            builder.Append('=', missingPaddingCharacters);

            return Convert.FromBase64String(builder.ToString());
        }

        public static void ApplyHeadersToResponse(WebHeaderCollection headers, HttpResponseBase response)
        {
            foreach(string headerName in headers)
            {
                switch(headerName)
                {
                    case "Content-Type":
                        response.ContentType = headers[HttpResponseHeader.ContentType];
                        break;
                    default:
                        response.AddHeader(headerName, headers[headerName]);
                        break;
                }
            }
        }

        public static int CopyUpTo(this Stream copyFrom, Stream copyTo, int maximumBytesToCopy)
        {
            byte[] buffer = new byte[1024];
            int readBytes;
            int totalCopiedBytes = 0;
            while((readBytes = copyFrom.Read(buffer, 0, Math.Min(1024, maximumBytesToCopy))) > 0)
            {
                int writeBytes = Math.Min(maximumBytesToCopy, readBytes);
                copyTo.Write(buffer, 0, writeBytes);
                totalCopiedBytes += writeBytes;
                maximumBytesToCopy -= writeBytes;
            }

            return totalCopiedBytes;
        }

        public static Stream CreateSnapshot(this Stream copyFrom)
        {
            MemoryStream copyTo = new MemoryStream(copyFrom.CanSeek ? (int)copyFrom.Length : 4 * 1024);
            try
            {
                copyFrom.CopyTo(copyTo);
                copyTo.Position = 0;
                return copyTo;
            }
            catch
            {
                copyTo.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 复制header内容
        /// </summary>
        /// <param name="message"></param>
        /// <param name="request"></param>
        public static void CopyHeadersFrom(this HttpRequestMessage message, HttpRequestBase request)
        {
            foreach(string headerName in request.Headers)
            {
                string[] headerValues = request.Headers.GetValues(headerName);
                if(headerName == "Authorization" && headerValues.Length > 1)
                {
                    message.Headers.Authorization = AuthenticationHeaderValue.Parse(string.Join(",", headerValues));
                }
                else
                {
                    if(!message.Headers.TryAddWithoutValidation(headerName, headerValues))
                    {
                        message.Content.Headers.TryAddWithoutValidation(headerName, headerValues);
                    }
                }
            }
        }

        public static bool AreEquivalent<T>(T[] first, T[] second)
        {
            if(first == null && second == null)
            {
                return true;
            }
            if(first == null || second == null)
            {
                return false;
            }
            if(first.Length != second.Length)
            {
                return false;
            }
            for(int i = 0; i < first.Length; i++)
            {
                if(!first[i].Equals(second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreEquivalentConstantTime(byte[] first, byte[] second)
        {
            if(first.Length != second.Length)
            {
                return false;
            }
            int result = 0;
            for(int i = 0; i < first.Length; i++)
            {
                result |= first[i] ^ second[i];
            }

            return result == 0;
        }

        public static bool AreEquivalent<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2)
        {
            if(sequence1 == null && sequence2 == null)
            {
                return true;
            }
            if((sequence1 == null) ^ (sequence2 == null))
            {
                return false;
            }
            IEnumerator<T> iterator1 = sequence1.GetEnumerator();
            IEnumerator<T> iterator2 = sequence2.GetEnumerator();
            bool movenext1, movenext2;
            while(true)
            {
                movenext1 = iterator1.MoveNext();
                movenext2 = iterator2.MoveNext();
                if(!movenext1 || !movenext2)
                {
                    break;
                }
                object obj1 = iterator1.Current;
                object obj2 = iterator2.Current;
                if(obj1 == null && obj2 == null)
                {
                    continue;
                }
                if(obj1 == null ^ obj2 == null)
                {
                    return false;
                }
                if(!obj1.Equals(obj2))
                {
                    return false;
                }
            }
            return movenext1 == movenext2;
        }

        public static bool AreEquivalentUnordered<T>(ICollection<T> first, ICollection<T> second)
        {
            if(first == null && second == null)
            {
                return true;
            }
            if((first == null) ^ (second == null))
            {
                return false;
            }
            if(first.Count != second.Count)
            {
                return false;
            }
            foreach(T value in first)
            {
                if(!second.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreEquivalent<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return AreEquivalent(first.ToArray(), second.ToArray());
        }

        public static string CreateQueryString(IEnumerable<KeyValuePair<string,string>> args)
        {
            if(!args.Any())
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder(args.Count() * 10);
            foreach(var p in args)
            {
                ErrorUtilities.VerifyArgument(!string.IsNullOrEmpty(p.Key), MessagingStrings.UnexpectedNullOrEmptyKey);
                ErrorUtilities.VerifyArgument(p.Value != null, MessagingStrings.UnexpectedNullValue, p.Key);
                sb.Append(EscapeUriDataStringRfc3986(p.Key));
                sb.Append('=');
                sb.Append(EscapeUriDataStringRfc3986(p.Value));
                sb.Append('&');
            }
            sb.Length--;
            return sb.ToString();
        }

        public static void AppendQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string,string>> args)
        {
            if(args != null && args.Count() > 0)
            {
                StringBuilder sb = new StringBuilder(50 + (args.Count() * 10));
                if(!string.IsNullOrEmpty(builder.Query))
                {
                    sb.Append(builder.Query.Substring(1));
                    sb.Append('&');
                }
                sb.Append(CreateQueryString(args));
                builder.Query = sb.ToString();
            }
        }

        public static void AppendFragmentArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string,string>> args)
        {
            if(args != null && args.Count() >0 )
            {
                StringBuilder sb = new StringBuilder(50 + (args.Count() * 10));
                if(!string.IsNullOrEmpty(builder.Fragment))
                {
                    sb.Append(builder.Fragment);
                    sb.Append('&');
                }
                sb.Append(CreateQueryString(args));
                builder.Fragment = sb.ToString();
            }
        }

        public static void AppendAndReplaceQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args)
        {
            if(args != null && args.Count() > 0)
            {
                NameValueCollection aggregatedArgs = HttpUtility.ParseQueryString(builder.Query);
                foreach(var pair in args)
                {
                    aggregatedArgs[pair.Key] = pair.Value;
                }

                builder.Query = CreateQueryString(aggregatedArgs.ToDictionary());
            }
        }

        public static MessageReceivingEndpoint GetRecipient(this HttpRequestMessage request)
        {
            return new MessageReceivingEndpoint(request.RequestUri, GetHttpDeliveryMethod(request.Method.Method));
        }

        public static HttpDeliveryMethods GetHttpDeliveryMethod(string httpVerb)
        {
            if(httpVerb == "GET")
            {
                return HttpDeliveryMethods.GetRequest;
            }
            else if(httpVerb == "POST")
            {
                return HttpDeliveryMethods.PostRequest;
            }
            else if(httpVerb == "PUT")
            {
                return HttpDeliveryMethods.PutRequest;
            }
            else if(httpVerb == "DELETE")
            {
                return HttpDeliveryMethods.DeleteRequest;
            }
            else if(httpVerb == "HEAD")
            {
                return HttpDeliveryMethods.HeadRequest;
            }
            else if(httpVerb == "PATCH")
            {
                return HttpDeliveryMethods.PatchRequest;
            }
            else if(httpVerb == "OPTIONS")
            {
                return HttpDeliveryMethods.OptionsRequest;
            }
            else
            {
                throw ErrorUtilities.ThrowArgumentNamed("httpVerb", MessagingStrings.UnsupportedHttpVerb, httpVerb);
            }
        }

        public static HttpMethod GetHttpVerb(HttpDeliveryMethods httpMethod)
        {
            if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.GetRequest)
            {
                return HttpMethod.Get;
            }
            else if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.PostRequest)
            {
                return HttpMethod.Post;
            }
            else if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.PutRequest)
            {
                return HttpMethod.Put;
            }
            else if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.DeleteRequest)
            {
                return HttpMethod.Delete;
            }
            else if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.HeadRequest)
            {
                return HttpMethod.Head;
            }
            else if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.PatchRequest)
            {
                return new HttpMethod("PATCH");
            }
            else if((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.OptionsRequest)
            {
                return HttpMethod.Options;
            }
            else if((httpMethod & HttpDeliveryMethods.AuthorizationHeaderRequest) != 0)
            {
                return HttpMethod.Get;
            }
            else
            {
                throw ErrorUtilities.ThrowArgumentNamed("httpMethod", MessagingStrings.UnsupportedHttpVerb, httpMethod);
            }
        }

        public static void AddExtraParameters(this MessageDictionary messageDictionary, IDictionary<string,string> extraParameters)
        {
            if(extraParameters != null)
            {
                foreach(var pair in extraParameters)
                {
                    try
                    {
                        messageDictionary.Add(pair);
                    }
                    catch(ArgumentException ex)
                    {
                        throw ErrorUtilities.Wrap(ex, MessagingStrings.ExtraParameterAddFailure, pair.Key, pair.Value);
                    }
                }
            }
        }

        public static Uri GetDirectUriRequest(this HttpResponseMessage response)
        {
            if(response.Headers.Location != null)
            {
                return response.Headers.Location;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> sequesce)
        {
            return sequesce.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static IEnumerable<KeyValuePair<string,string>> AsKeyValuePairs(this NameValueCollection nvc)
        {
            foreach(string key in nvc)
            {
                foreach(string value in nvc.GetValues(key))
                {
                    yield return new KeyValuePair<string, string>(key, value);
                }
            }
        }

        public static Dictionary<string ,string> ToDictionary(this NameValueCollection nvc)
        {
            return ToDictionary(nvc, false);
        }

        public static Dictionary<string,string> ToDictionary(this NameValueCollection nvc, bool throwOnNullKey)
        {
            if(nvc == null)
            {
                return null;
            }

            var dictionary = new Dictionary<string, string>();
            foreach(string key in nvc)
            {
                if(key == null)
                {
                    if(throwOnNullKey)
                    {
                        throw new ArgumentException(MessagingStrings.UnexpectedNullKey);
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(nvc[key]))
                        {
                            Logger.OpenId.WarnFormat("Null key with value {0} encountered while translating NameValueCollection to Dictionary.", nvc[key]);
                        }
                    }
                }
                else
                {
                    dictionary.Add(key, nvc[key]);
                }
            }

            return dictionary;
        }

        public static NameValueCollection ToNameValueCollection(this IDictionary<string,string> data)
        {
            var nvc = new NameValueCollection();
            foreach(var entry in data)
            {
                nvc.Add(entry.Key, entry.Value);
            }

            return nvc;
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Comparison<TKey> comparer)
        {
            return System.Linq.Enumerable.OrderBy<TSource, TKey>(source, keySelector, new ComparisonHelper<TKey>(comparer));
        }

        public static bool IsReuest(this IDirectedProtocolMessage message)
        {
            return message.Recipient != null;
        }

        public static bool IsDirectResponse(this IDirectResponseProtocolMessage message)
        {
            return message.OriginatingRequest != null;
        }

        public static void WriteBuffer(this BinaryWriter writer, byte[] buffer)
        {
            writer.Write(buffer.Length);
            writer.Write(buffer, 0, buffer.Length);
        }

        public static byte[] ReadBuffer(this BinaryReader reader, int maxBufferSize)
        {
            int length = reader.ReadInt32();
            ErrorUtilities.VerifyProtocol(length <= maxBufferSize, MessagingStrings.DataCorruptionDetected);
            byte[] buffer = new byte[length];
            ErrorUtilities.VerifyProtocol(reader.Read(buffer, 0, length) == length, MessagingStrings.UnexpectedBufferLength);
            return buffer;
        }

        public static string CreateJsonObject(IEnumerable<KeyValuePair<string,string>> namesAndValues, bool valuesPreEncoded)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            foreach(var pair in namesAndValues)
            {
                builder.Append(MessagingUtilities.GetSafeJavascriptValue(pair.Key));
                builder.Append(": ");
                builder.Append(valuesPreEncoded ? pair.Value : MessagingUtilities.GetSafeJavascriptValue(pair.Value));
                builder.Append(",");
            }
            if(builder[builder.Length - 1] == ',')
            {
                builder.Length -= 1;
            }
            builder.Append("}");

            return builder.ToString();
        }

        public static string SerializeAsJson(IMessage message, MessageDescriptionCollection messageDecriptions)
        {
            var encoding = Encoding.UTF8;
            var bytes = SerializeAsJsonBytes(message, messageDecriptions, encoding);
            string json = encoding.GetString(bytes);

            return json;
        }

        public static byte[] SerializeAsJsonBytes(IMessage message, MessageDescriptionCollection messageDescriptions, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            MessageDictionary messageDictionary = messageDescriptions.GetAccessor(message);
            using(var memoryStream = new MemoryStream())
            {
                using(var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, encoding))
                {
                    MessageSerializer.Serialize(messageDictionary, jsonWriter);
                    jsonWriter.Flush();
                }

                return memoryStream.ToArray();
            }
        }

        public static void DeserializeFromJson(byte[] jsonBytes, IMessage receivingMessage, MessageDescriptionCollection messageDescriptions, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            MessageDictionary messageDictionary = messageDescriptions.GetAccessor(receivingMessage);
            using(var jsonReader = JsonReaderWriterFactory.CreateJsonReader(jsonBytes, 0, jsonBytes.Length, encoding, Channel.DefaultUntrustedXmlDictionaryReaderQuotas, null))
            {
                MessageSerializer.Deserialize(messageDictionary, jsonReader);
            }
        }

        public static string GetSafeJavascriptValue(string value)
        {
            if(value == null)
            {
                return "null";
            }

            StringBuilder builder = new StringBuilder(value);
            foreach(var pair in javascriptStaticStringEscaping)
            {
                builder.Replace(pair.Key, pair.Value);
            }
            builder.Insert(0, '\'');
            builder.Append('\'');
            return builder.ToString();
        }

        public static string EscapeUriDataStringRfc3986(string value)
        {
            if(value.Length == 0)
            {
                return value;
            }

            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));
            for(int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }

            return escaped.ToString();
        }

        public static DateTime ToLocalTimeSafe(this DateTime value)
        {
            if(value.Kind == DateTimeKind.Unspecified)
            {
                return value;
            }

            return value.ToLocalTime();
        }

        public static DateTime ToUniversalTimeSafe(this DateTime value)
        {
            if(value.Kind == DateTimeKind.Unspecified)
            {
                return value;
            }

            return value.ToUniversalTime();
        }

        public static NameValueCollection GetQueryStringBeforeRewriting(this HttpRequestBase request)
        {
            Uri beforeRewriting = GetPublicFacingUrl(request);
            if(beforeRewriting == request.Url)
            {
                return request.QueryString;
            }
            else
            {
                ErrorUtilities.VerifyProtocol(beforeRewriting != null, "UrlBeforeRewriting is null, so the query string cannot be determined.");
                return HttpUtility.ParseQueryString(beforeRewriting.Query);
            }
        }

        public static bool GetIsUrlRewritten(this HttpRequestBase request)
        {
            return request.Url != GetPublicFacingUrl(request);
        }

        public static NameValueCollection GetQueryOrForm(this HttpRequestBase request)
        {
            return request.HttpMethod == "GET" ? GetQueryStringBeforeRewriting(request) : request.Form;
        }

        private static SymmetricAlgorithm CreateSymmetricAlgorithm(byte[] key)
        {
            SymmetricAlgorithm result = null;
            try
            {
                result = SymmetricAlgorithm.Create();
                result.Mode = CipherMode.CBC;
                result.Key = key;
                return result;
            }
            catch
            {
                IDisposable disposableResult = result;
                if(disposableResult != null)
                {
                    disposableResult.Dispose();
                }
                throw;
            }
        }

        private static class ThreadSafeRandom
        {
            private static readonly Random threadRandomInitializer = new Random();

            [ThreadStatic]
            private static Random threadRandom;

            public static Random RandomNumberGenerator
            {
                get
                {
                    if(threadRandom == null)
                    {
                        lock(threadRandomInitializer)
                        {
                            threadRandom = new Random(threadRandomInitializer.Next());
                        }
                    }

                    return threadRandom;
                }
            }
        }
        private class ComparisonHelper<T>:IComparer<T>
        {
            private Comparison<T> comparison;
            public ComparisonHelper(Comparison<T> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return this.comparison(x, y);
            }
        }
    }
}


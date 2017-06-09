using CHY.OAuth2.Core.Messaging.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CHY.OAuth2.Core.Messaging
{
    public class UriStyleMessageFormatter<T>:DataBagFormatterBase<T> where T:DataBag
    {
        public UriStyleMessageFormatter(RSACryptoServiceProvider signingKey = null, RSACryptoServiceProvider encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
            :base(signingKey, encryptingKey, compressed, maximumAge, decodeOnceOnly)
        {

        }

        public UriStyleMessageFormatter(ICryptoKeyStore cryptoKeyStore = null, string bucket = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? minimumAge = null, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
            :base(cryptoKeyStore, bucket, signed, encrypted, compressed, minimumAge, maximumAge, decodeOnceOnly)
        {

        }
        protected override byte[] SerializeCore(T message)
        {
            var fields = MessageSerializer.Get(message.GetType()).Serialize(MessageDescriptions.GetAccessor(message));
            string value = MessagingUtilities.CreateQueryString(fields);
            return Encoding.UTF8.GetBytes(value);
        }

        protected override void DeserializeCore(T message, byte[] data)
        {
            string value = Encoding.UTF8.GetString(data);
            var serializer = MessageSerializer.Get(message.GetType());
            var fields = MessageDescriptions.GetAccessor(message);
            serializer.Deserialize(HttpUtility.ParseQueryString(value).ToDictionary(), fields);
        }
    }
}

using CHY.OAuth2.Core.Messaging.Bindings;
using CHY.OAuth2.Core.Messaging.Reflection;
using CHY.OAuth2.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public abstract class DataBagFormatterBase<T>:IDataBagFormatter<T> where T: DataBag
    {
        protected static readonly MessageDescriptionCollection MessageDescriptions = new MessageDescriptionCollection();
        private const int NonceLenght = 6;
        private readonly TimeSpan minimumAge = TimeSpan.FromDays(1);
        private readonly ICryptoKeyStore cryptoKeyStore;
        private readonly string cryptoKeyBucket;
        private readonly RSACryptoServiceProvider asymmetricSigning; // 签名
        private readonly RSACryptoServiceProvider asymmetricEncrypting; // 加密
        private readonly bool signed;
        private readonly INonceStore decodeOnceOnly;
        private readonly TimeSpan? maximumAge;
        private readonly bool encrypted;
        private readonly bool compressed;

        protected DataBagFormatterBase(RSACryptoServiceProvider signingKey = null, RSACryptoServiceProvider encryptingKey = null, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
            :this(signingKey != null, encryptingKey != null, compressed, maximumAge, decodeOnceOnly)
        {
            this.asymmetricSigning = signingKey;
            this.asymmetricEncrypting = encryptingKey;
        }

        protected DataBagFormatterBase(ICryptoKeyStore cryptoKeyStore = null, string bucket = null, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? minimumAge = null, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
            :this(signed, encrypted, compressed, maximumAge, decodeOnceOnly)
        {
            this.cryptoKeyStore = cryptoKeyStore;
            this.cryptoKeyBucket = bucket;
            if(minimumAge.HasValue)
            {
                this.minimumAge = minimumAge.Value;
            }
        }

        private DataBagFormatterBase(bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
        {
            this.signed = signed;
            this.maximumAge = maximumAge;
            this.decodeOnceOnly = decodeOnceOnly;
            this.encrypted = encrypted;
            this.compressed = compressed;
        }

        /// <summary>
        /// 序列化消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Serialize(T message)
        {
            message.UtcCreationDate = DateTime.UtcNow;
            if(this.decodeOnceOnly != null)
            {
                message.Nonce = MessagingUtilities.GetNonCryptoRandomData(NonceLenght); // 获取随机字符
            }

            byte[] encoded = this.SerializeCore(message);
            if(this.compressed) // 压缩
            {
                encoded = MessagingUtilities.Compress(encoded);
            }

            string symmetricSecretHandle = null;
            if(this.encrypted) // 加密
            {
                encoded = this.Encrypt(encoded, out symmetricSecretHandle);
            }
            if(this.signed) // 签名
            {
                message.Signature = this.CalculateSignature(encoded, symmetricSecretHandle);
            }
            int capacity = this.signed ? 4 + message.Signature.Length + 4 + encoded.Length : encoded.Length;
            using(var finalStream = new MemoryStream(capacity))
            {
                var writer = new BinaryWriter(finalStream);
                if(this.signed)
                {
                    writer.WriteBuffer(message.Signature);
                }
                writer.WriteBuffer(encoded);
                writer.Flush();
                string payload = MessagingUtilities.ConvertToBase64WebSafeString(finalStream.ToArray());
                string result = payload;
                if(symmetricSecretHandle != null && (this.signed || this.encrypted))
                {
                    result = MessagingUtilities.CombinKeyHandleAndPayload(symmetricSecretHandle, payload);
                }

                return result;
            }
        }

        public void Deserialize(T message, string value, IProtocolMessage containingMessage, string messagePartName)
        {
            string symmetricSecretHandle = null;
            if(this.encrypted && this.cryptoKeyStore != null)
            {
                string valueWithoutHandle;
                MessagingUtilities.ExtractKeyHandleAndPayload(messagePartName, value, out symmetricSecretHandle, out valueWithoutHandle);
                value = valueWithoutHandle;
            }
            message.ContainingMessage = containingMessage;
            byte[] data = MessagingUtilities.FromBase64WebSafeString(value);
            byte []signature = null;
            if(this.signed)
            {
                using(var dataStream = new MemoryStream(data))
                {
                    var dataReader = new BinaryReader(dataStream);
                    signature = dataReader.ReadBuffer(1024);
                    data = dataReader.ReadBuffer(8 * 1024);
                }
                ErrorUtilities.VerifyProtocol(this.IsSignatureValid(data, signature, symmetricSecretHandle), MessagingStrings.SignatureInvalid);
            }
            if(this.encrypted)
            {
                data = this.Decrypt(data, symmetricSecretHandle);
            }
            if(this.compressed)
            {
                data = MessagingUtilities.Decompress(data);
            }
            this.DeserializeCore(message, data);
            message.Signature = signature;
            if(this.maximumAge.HasValue)
            {
                DateTime expirationDate = message.UtcCreationDate + this.maximumAge.Value;
                if(expirationDate < DateTime.UtcNow)
                {
                    throw new ExpiredMessageException(expirationDate, containingMessage);
                }
            }

            if(this.decodeOnceOnly != null)
            {
                ErrorUtilities.VerifyInternal(this.maximumAge.HasValue, "Oops!  How can we validate a nonce without a maximum message age?");
                string context = "{" + GetType().FullName + "}";
                if(!this.decodeOnceOnly.StoreNonce(context, Convert.ToBase64String(message.Nonce), message.UtcCreationDate))
                {
                    Logger.OpenId.ErrorFormat("Replayed nonce detected ({0} {1}).  Rejecting message.", message.Nonce, message.UtcCreationDate);
                    throw new ReplayedMessageException(containingMessage);
                }
            }
            ((IMessage)message).EnsureValidMessage();
        }

        protected abstract byte[] SerializeCore(T message);

        protected abstract void DeserializeCore(T message, byte[] data);

        private bool IsSignatureValid(byte[] signedData, byte[] signature, string symmetricSecretHandle)
        {
            if(this.asymmetricSigning != null)
            {
                using(var hasher = SHA1.Create())
                {
                    return this.asymmetricSigning.VerifyData(signedData, hasher, signature);
                }
            }
            else
            {
                return MessagingUtilities.AreEquivalentConstantTime(signature, this.CalculateSignature(signedData, symmetricSecretHandle));
            }
        }

        private byte[] CalculateSignature(byte[] bytesToSign, string symmetricSecretHandle)
        {
            if(this.asymmetricSigning != null)
            {
                using(var hasher = SHA1.Create())
                {
                    return this.asymmetricSigning.SignData(bytesToSign, hasher);
                }
            }
            else
            {
                var key = this.cryptoKeyStore.GetKey(this.cryptoKeyBucket, symmetricSecretHandle);
                ErrorUtilities.VerifyProtocol(key != null, MessagingStrings.MissingDecryptionKeyForHandle, this.cryptoKeyBucket, symmetricSecretHandle);
                using (var symmetricHasher = HmacAlgorithms.Create(HmacAlgorithms.HmacSha256, key.Key))
                {
                    return symmetricHasher.ComputeHash(bytesToSign);
                }
            }
        }

        private byte[] Encrypt(byte[] value, out string symmetricSecretHandle)
        {
            Assumes.True(this.asymmetricEncrypting != null || this.cryptoKeyStore != null);

            if(this.asymmetricEncrypting != null)
            {
                symmetricSecretHandle = null;
                return this.asymmetricEncrypting.EncryptWithRandomSymmetricKey(value);
            }
            else
            {
                var cryptoKey = this.cryptoKeyStore.GetCurrentKey(this.cryptoKeyBucket, this.minimumAge);
                symmetricSecretHandle = cryptoKey.Key;
                return MessagingUtilities.Encrypt(value, cryptoKey.Value.Key);
            }
        }

        private byte[] Decrypt(byte[] value, string symmetricSecretHandle)
        {
            if(this.asymmetricEncrypting != null)
            {
                return this.asymmetricEncrypting.DecryptWithRandomSymmetricKey(value);
            }
            else
            {
                var key = this.cryptoKeyStore.GetKey(this.cryptoKeyBucket, symmetricSecretHandle);
                ErrorUtilities.VerifyProtocol(key != null, MessagingStrings.MissingDecryptionKeyForHandle, this.cryptoKeyBucket, symmetricSecretHandle);
                return MessagingUtilities.Decrypt(value, key.Key);
            }
        }
    }
}


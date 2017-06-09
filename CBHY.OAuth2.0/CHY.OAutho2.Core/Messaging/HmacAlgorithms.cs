using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    internal static class HmacAlgorithms
    {
        public const string HmacSha1 = "HMACSHA1";
        public const string HmacSha256 = "HMACSHA256";
        public const string HmacSha384 = "HMACSHA384";
        public const string HmacSha512 = "HMACSHA512";
        public static HMAC Create(string algorithmName, byte[] key)
        {
            HMAC hmac = HMAC.Create(algorithmName);
            try
            {
                hmac.Key = key;
                return hmac;
            }
            catch{
                hmac.Dispose();
                throw;
            }
        }
    }
}

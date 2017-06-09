using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Bindings
{
    public class CryptoKey
    {
        private readonly byte[] key;
        private readonly DateTime expiresUtc;

        public CryptoKey(byte[] key, DateTime expiresUtc)
        {
            this.key = key;
            this.expiresUtc = expiresUtc;
        }

        public byte[] Key
        {
            get { return this.key; }
        }

        public DateTime ExpiresUtc
        {
            get { return this.expiresUtc; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as CryptoKey;
            if(other == null)
            {
                return false;
            }

            return this.expiresUtc == other.expiresUtc && MessagingUtilities.AreEquivalent(this.Key, other.Key);
        }

        public override int GetHashCode()
        {
            return this.ExpiresUtc.GetHashCode();
        }
    }
}

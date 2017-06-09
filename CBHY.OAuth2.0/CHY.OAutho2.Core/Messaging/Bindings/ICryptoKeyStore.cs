using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Bindings
{
    public interface ICryptoKeyStore
    {
        CryptoKey GetKey(string bucket, string handle);

        IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket);

        void StoreKey(string bucket, string handle, CryptoKey key);

        void RemoveKey(string bucket, string handle);

    }
}

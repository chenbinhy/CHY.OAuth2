using CHY.BaseFramework.DAL;
using DotNetOpenAuth.Messaging.Bindings;
using OAuth2.Models.CHYAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBHY.OAuth2.AuthorizetionServer.Code
{
    public class DatabaseKeyNonceStore:INonceStore, ICryptoKeyStore
    {
        private IRepository<Nonce> m_NonceRep;
        private IRepository<SymmetricCryptoKey> m_CryptoKeyRep;

        public DatabaseKeyNonceStore(IRepository<Nonce> nonceRep, IRepository<SymmetricCryptoKey> cryptoKeyRep)
        {
            m_NonceRep = nonceRep;
            m_CryptoKeyRep = cryptoKeyRep;
        }

        public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
        {
            m_NonceRep.Add(new Nonce() { Context = context, Code = nonce, Timestamp = timestampUtc });

            return true;
        }

        public CryptoKey GetKey(string bucket, string handle)
        {
            var info =  m_CryptoKeyRep.Get(s => s.Bucket == bucket && s.Handle == handle);
            return new CryptoKey(info.Secret, info.ExpiresUtc.AsUtc()); 
        }

        public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
        {
            var list = m_CryptoKeyRep.ToList(s => s.Bucket == bucket);
            return list.Select(s => new KeyValuePair<string,CryptoKey>(s.Handle, new CryptoKey(s.Secret, s.ExpiresUtc.AsUtc())));
        }

        public void StoreKey(string bucket, string handle, CryptoKey key)
        {
            var keyRow = new SymmetricCryptoKey()
            {
                Bucket = bucket,
                Handle = handle,
                Secret = key.Key,
                ExpiresUtc = key.ExpiresUtc
            };
            m_CryptoKeyRep.Add(keyRow);
        }

        public void RemoveKey(string bucket, string handle)
        {
            m_CryptoKeyRep.Delete(s => s.Bucket == bucket && s.Handle == handle);
        }
    }
}
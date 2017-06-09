using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Bindings
{
    public interface INonceStore
    {
        bool StoreNonce(string context, string nonce, DateTime timestampUtc);
    }
}

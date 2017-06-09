using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2
{
    public interface IClientDescription
    {
        Uri DefaultCallback { get; }

        ClientType ClientType { get; }

        bool HasNonEmptySecret { get; }

        bool IsCallbackAllowed(Uri callback);

        bool IsValidClientSecret(string secret);
    }
}

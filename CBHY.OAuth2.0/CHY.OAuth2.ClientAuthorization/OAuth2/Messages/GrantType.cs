using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public enum GrantType
    {
        AuthorizationCode,
        Password,
        Assertion,
        RefreshToken,
        ClientCredentials
    }
}

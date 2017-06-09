using CHY.OAuth2.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public interface IAccessTokenResult
    {
        bool AllowRefreshToken { get; set; }
        AccessToken AccessToken { get; }
    }
}

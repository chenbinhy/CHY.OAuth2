using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.ChannelElements
{
    public enum ClientAuthenticationResult
    {
        NoAuthenticationRecognized,
        ClientIdNotAuthenticated,
        ClientAuthenticated,
        ClientAuthenticationRejected
    }
}

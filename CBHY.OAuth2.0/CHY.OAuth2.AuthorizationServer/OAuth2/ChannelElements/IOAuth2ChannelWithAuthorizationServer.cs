using CHY.OAuth2.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public interface IOAuth2ChannelWithAuthorizationServer
    {
        IAuthorizationServerHost AuthorizationServer { get; }

        IScopeSatisfiedCheck ScopeSatisfiedCheck { get; set; }
    }
}

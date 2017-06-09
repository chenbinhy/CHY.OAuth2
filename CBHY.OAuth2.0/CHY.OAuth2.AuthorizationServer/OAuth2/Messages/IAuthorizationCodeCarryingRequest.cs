using CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.Messages
{
    public interface IAuthorizationCodeCarryingRequest:IAuthorizationCarryingRequest
    {
        string Code { get; set; }
        new AuthorizationCode AuthorizationDescription { get; set; }
    }
}

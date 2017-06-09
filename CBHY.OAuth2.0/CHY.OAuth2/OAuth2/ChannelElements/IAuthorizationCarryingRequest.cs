using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2.ChannelElements
{
    public interface IAuthorizationCarryingRequest:IDirectedProtocolMessage
    {
        IAuthorizationDescription AuthorizationDescription { get; }
    }
}

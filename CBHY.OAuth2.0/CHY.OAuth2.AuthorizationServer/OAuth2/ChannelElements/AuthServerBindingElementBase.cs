using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public abstract class AuthServerBindingElementBase:IChannelBindingElement
    {
        protected AuthServerBindingElementBase()
        {

        }

        public Channel Channel { get; set; }

        public abstract MessageProtections Protection { get; }
        public IOAuth2ChannelWithAuthorizationServer AuthServerChannel
        {
            get { return (IOAuth2ChannelWithAuthorizationServer)this.Channel; }
        }

        protected IAuthorizationServerHost AuthorizationServer
        {
            get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).AuthorizationServer; }
        }

        public abstract MessageProtections? ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken);

        public abstract MessageProtections? ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Client.OAuth2
{
    public interface IClientAuthorizationTracker
    {
        IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState);
    }
}

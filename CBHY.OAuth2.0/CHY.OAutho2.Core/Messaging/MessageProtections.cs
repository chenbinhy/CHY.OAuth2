using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public enum MessageProtections
    {
        None = 0x0,
        TamperProtection = 0x1,
        Expiration = 0x2,
        ReplayProtection = 0x4,
        All = TamperProtection | Expiration | ReplayProtection
    }
}

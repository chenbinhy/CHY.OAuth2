using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAutho2.Core.Messaging
{
    public static class MessageProtectionTasks
    {
        public static readonly Task<MessageProtections?> Null = Task.FromResult<MessageProtections?>(null);

        public static readonly Task<MessageProtections?> None =
            Task.FromResult<MessageProtections?>(MessageProtections.None);

        public static readonly Task<MessageProtections?> TamperProtection =
            Task.FromResult<MessageProtections?>(MessageProtections.TamperProtection);

        public static readonly Task<MessageProtections?> ReplayProtection =
            Task.FromResult<MessageProtections?>(MessageProtections.ReplayProtection);
    }
}

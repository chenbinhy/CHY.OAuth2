using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    [Serializable]
    public class UnprotectedMessageException : ProtocolException
    {
        public UnprotectedMessageException(IProtocolMessage faultedMessage, MessageProtections appliedProtection)
            : base(string.Format(CultureInfo.CurrentCulture, MessagingStrings.InsufficientMessageProtection, faultedMessage.GetType().Name, faultedMessage.RequiredProtection, appliedProtection), faultedMessage)
        {
        }

        protected UnprotectedMessageException(
            SerializationInfo info,
            StreamingContext context)
            :base(info, context)
        {

        }
    }
}


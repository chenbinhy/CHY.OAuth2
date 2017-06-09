using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Bindings
{
    public class ExpiredMessageException:ProtocolException
    {
        public ExpiredMessageException(DateTime utcExpirationDate, IProtocolMessage faultedMessage)
            :base(string.Format(CultureInfo.CurrentCulture, MessagingStrings.ExpiredMessage, utcExpirationDate.ToLocalTime(), DateTime.Now), faultedMessage)
        {

        }

        protected ExpiredMessageException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {

        }
    }
}

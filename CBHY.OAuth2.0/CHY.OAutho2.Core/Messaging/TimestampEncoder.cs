using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class TimestampEncoder : IMessagePartEncoder
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TimestampEncoder()
        {

        }

        public string Encode(object value)
        {
            if(value == null)
            {
                return null;
            }
            var timestamp = (DateTime)value;
            TimeSpan secondsSinceEpoch = timestamp - Epoch;
            return ((int)secondsSinceEpoch.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        public object Decode(string value)
        {
            if(value == null)
            {
                return null;
            }
            var secondsSinceEpoch = int.Parse(value, CultureInfo.InvariantCulture);
            return Epoch.AddSeconds(secondsSinceEpoch);
        }
    }
}

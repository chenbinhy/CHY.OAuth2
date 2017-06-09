using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public class TimespanSecondsEncoder: IMessagePartEncoder, IMessagePartFormattingEncoder
    {
        public TimespanSecondsEncoder() { }

        public Type FormattingType
        {
            get { return typeof(int); }
        }

        public string Encode(object value)
        {
            TimeSpan? timeSpan = value as TimeSpan?;
            if(timeSpan.HasValue)
            {
                return timeSpan.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return null;
            }
        }

        public object Decode(string value)
        {
            return TimeSpan.FromSeconds(double.Parse(value, CultureInfo.InvariantCulture));
        }
    }
}

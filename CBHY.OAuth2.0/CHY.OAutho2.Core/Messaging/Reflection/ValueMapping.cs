using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    public struct ValueMapping
    {
        public readonly Func<object, string> ValueToString;

        public readonly Func<object, string> ValueToOriginalString;

        public readonly Func<string, object> StringToValue;

        public ValueMapping(Func<object, string> toString, Func<object, string> toOriginalString, Func<string, object> toValue)
            :this()
        {
            this.ValueToString = toString;
            this.ValueToOriginalString = toOriginalString ?? toString;
            this.StringToValue = toValue;
        }

        public ValueMapping(IMessagePartEncoder encoder)
            : this()
        {
            this.Encoder = encoder;
            var nullEncoder = encoder as IMessagePartNullEncoder;
            string nullString = nullEncoder != null ? nullEncoder.EncodedNullValue : null;
            var originalStringEncoder = encoder as IMessagePartOriginalEncoder;
            Func<object, string> originalStringEncode = encoder.Encode;
            if(originalStringEncoder != null)
            {
                originalStringEncode = originalStringEncoder.EncodeAsOriginalString;
            }
            this.ValueToString = obj => (obj != null) ? encoder.Encode(obj) : nullString;
            this.StringToValue = str => (str != null) ? encoder.Decode(str) : null;
            this.ValueToOriginalString = obj => (obj != null) ? originalStringEncode(obj) : nullString;
        }

        public IMessagePartEncoder Encoder { get; private set; }
    }
}

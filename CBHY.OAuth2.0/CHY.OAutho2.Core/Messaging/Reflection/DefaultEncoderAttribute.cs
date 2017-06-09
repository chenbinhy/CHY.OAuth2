using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    public sealed class DefaultEncoderAttribute : Attribute
    {
        public DefaultEncoderAttribute(Type converterType)
        {
            this.Encoder = (IMessagePartEncoder)Activator.CreateInstance(converterType);
        }

        public IMessagePartEncoder Encoder { get; private set; }
    }
}

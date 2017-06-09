using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    public interface IMessagePartFormattingEncoder:IMessagePartEncoder
    {
        Type FormattingType { get; }
    }
}

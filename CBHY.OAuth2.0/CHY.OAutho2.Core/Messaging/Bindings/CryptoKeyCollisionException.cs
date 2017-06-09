using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Bindings
{
    public class CryptoKeyCollisionException:ArgumentException
    {
        public CryptoKeyCollisionException()
        {

        }

        public CryptoKeyCollisionException(Exception inner):base(null,inner)
        {

        }

        protected  CryptoKeyCollisionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
            ):base(info, context)
        {
            throw new NotImplementedException();
        }
    }
}

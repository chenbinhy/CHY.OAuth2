using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core
{
    public static class Assumes
    {
        public static void True(bool condition, string message = null)
        {
            if(!condition)
            {
                Fail(message);
            }
        }

        public static void True(bool condition, string unformattedMessage, params object[] args)
        {
            if(!condition)
            {
                Fail(string.Format(CultureInfo.CurrentCulture, unformattedMessage, args));
            }
        }

        public static void Fail(string message = null)
        {
            if(message != null)
            {
                throw new InternalErrorException(message);
            }
            else
            {
                throw new InternalErrorException();
            }
        }

        public static Exception NotReachable()
        {
            throw new InternalErrorException();
        }

        private class InternalErrorException:Exception
        {
            public InternalErrorException()
            {

            }

            public InternalErrorException(string message)
                : base(message)
            {

            }

            protected InternalErrorException(SerializationInfo info, StreamingContext context):base(info, context)
            {

            }
        }
    }
}

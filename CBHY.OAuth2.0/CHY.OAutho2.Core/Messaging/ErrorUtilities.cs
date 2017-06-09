using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHY.OAuth2.Core.Common;
using System.Web;

namespace CHY.OAuth2.Core.Messaging
{
    public static class ErrorUtilities
    {
        public static Exception Wrap(Exception inner, string errorMessage, params object[] args)
        {
            Assumes.True(errorMessage != null);

            return new ProtocolException(string.Format(CultureInfo.CurrentCulture, errorMessage, args), inner);
        }

        public static Exception ThrowInternal(string errorMessage)
        {
            if(Debugger.IsAttached)
            {
                Debugger.Break();
            }

            throw new InternalErrorException(errorMessage);
        }

        public static void VerifyInternal(bool condition, string errorMessage)
        {
            if (!condition)
            {
                ThrowInternal(errorMessage);
            }
        }

        public static void VerifyInternal(bool condition, string errorMessage, params object[] args)
        {
            Assumes.True(errorMessage != null);
            if (!condition)
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, args);
                throw new InternalErrorException(errorMessage);
            }
        }

        public static void VerifyOperation(bool condition, string errorMessage)
        {
            if (!condition)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        public static void VerifySupported(bool condition, string errorMessage)
        {
            if (!condition)
            {
                throw new NotSupportedException(errorMessage);
            }
        }

        public static void VerifySupported(bool condition, string errorMessage, params object[] args)
        {
            Assumes.True(errorMessage != null);
            if (!condition)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, errorMessage, args));
            }
        }

        public static void VerifyOperation(bool condition, string errorMessage, params object[] args)
        {
            Assumes.True(errorMessage != null);
            if (!condition)
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, args);
                throw new InvalidOperationException(errorMessage);
            }
        }

        public static void VerifyHost(bool condition, string errorMessage, params object[] args)
        {
            Assumes.True(errorMessage != null);
            if (!condition)
            {
                throw new HostErrorException(string.Format(CultureInfo.CurrentCulture, errorMessage, args));
            }
        }

        public static void VerifyProtocol(bool condition, IProtocolMessage faultedMessage, string errorMessage, params object[] args)
        {
            Assumes.True(errorMessage != null);
            if(!condition)
            {
                throw new ProtocolException(string.Format(CultureInfo.CurrentCulture, errorMessage, args), faultedMessage);
            }
        }

        public static void VerifyProtocol(bool condition, string unformattedMessage, params object[] args)
        {
            Assumes.True(unformattedMessage != null);
            if(!condition)
            {
                var exception = new ProtocolException(string.Format(CultureInfo.CurrentCulture, unformattedMessage, args));
                if(Logger.Messaging.IsErrorEnabled()){
                    Logger.Messaging.Error(string.Format(CultureInfo.CurrentCulture, "Protocol error:{0}{1}{2}", exception.Message, Environment.NewLine, new StackTrace()));
                }
                throw exception;
            }
        }

        public static Exception ThrowProtocol(string unformattedMessage, params object[] args)
        {
            Assumes.True(unformattedMessage != null);
            VerifyProtocol(false, unformattedMessage, args);

            return new InternalErrorException();
        }

        public static Exception ThrowFormat(string message, params object[] args)
        {
            Assumes.True(message != null);
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, message, args));
        }

        public static void VerifyFormat(bool condition, string message, params object[] args)
        {
            Assumes.True(message != null);
            if(!condition)
            {
                throw ThrowFormat(message, args);
            }
        }

        public static void VerifyArgument(bool condition, string message, params object[] args)
        {
            Assumes.True(message != null);
            if(!condition)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, message, args));
            }
        }

        public static Exception ThrowArgumentNamed(string parameterName, string message, params object[] args)
        {
            Assumes.True(message != null);
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, message, args), parameterName);
        }

        public static void VerifyArgumentNamed(bool condition, string parameterName, string message, params object[] args)
        {
            Assumes.True(message != null);
            if(!condition)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, message, args), parameterName);
            }
        }

        public static void VerifyArgumentNotNull(object value, string paramName)
        {
            if(value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void VerifyNonZeroLength(string value, string paramName)
        {
            VerifyArgumentNotNull(value, paramName);
            if(value.Length == 0)
            {
                throw new ArgumentException(MessagingStrings.UnexpectedEmptyString, paramName);
            }
        }

        public static void VerifyHttpContext()
        {
            ErrorUtilities.VerifyOperation(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
        }

        public static TValue GetValueOrThrow<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, IMessage message)
        {
            TValue value;
            VerifyProtocol(dictionary.TryGetValue(key, out value), MessagingStrings.ExpectedParameterWasMissing, key, message.GetType().Name);

            return value;
        }
    }
}

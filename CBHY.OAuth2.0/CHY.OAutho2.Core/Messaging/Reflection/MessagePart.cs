using CHY.OAuth2.Core.Configuration;
using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    public class MessagePart
    {
        private static readonly Dictionary<Type, ValueMapping> converters = new Dictionary<Type, ValueMapping>();
        private static readonly Dictionary<Type, IMessagePartEncoder> encoders = new Dictionary<Type, IMessagePartEncoder>();
        private ValueMapping converter;
        private PropertyInfo property;
        private FieldInfo field;
        private Type memberDeclaredType;
        private object defaultMemberValue;
        static MessagePart()
        {
            Func<string, Uri> safeUri = str =>
            {
                Assumes.True(str != null);
                return new Uri(str);
            };
            Func<string, bool> safeBool = str =>
            {
                Assumes.True(str != null);
                return bool.Parse(str);
            };
            Func<byte[], string> safeFromByteArray = bytes =>
            {
                Assumes.True(bytes != null);
                return Convert.ToBase64String(bytes);
            };
            Func<string,byte[]> safeToByteArray = str => {
                Assumes.True(str != null);
                return Convert.FromBase64String(str);
            };
            Map<Uri>(uri => uri.AbsoluteUri, uri=>uri.OriginalString,safeUri);
            Map<DateTime>(dt => XmlConvert.ToString(dt, XmlDateTimeSerializationMode.Utc), null, str => XmlConvert.ToDateTime(str, XmlDateTimeSerializationMode.Utc));
            Map<TimeSpan>(ts => ts.ToString(), null, str => TimeSpan.Parse(str));
            Map<byte[]>(safeFromByteArray, null, safeToByteArray);
            Map<bool>(value => value.ToString().ToLowerInvariant(),null, safeBool);
            Map<CultureInfo>(c => c.Name, null, str => new CultureInfo(str));
            Map<CultureInfo[]>(cs => string.Join(",", cs.Select(c => c.Name).ToArray()), null, str => str.Split(',').Select(s => new CultureInfo(s)).ToArray());
            Map<Type>(t => t.FullName, null, str =>Type.GetType(str));
        }

        public MessagePart(MemberInfo member, MessagePartAttribute attribute)
        {
            this.field = member as FieldInfo;
            this.property = member as PropertyInfo;
            this.Name = attribute.Name ?? member.Name;
            this.RequiredProtection = attribute.RequiredProtection;
            this.IsRequired = attribute.IsRequired;
            this.AllowEmpty = attribute.AllowEmpty;
            this.IsSecuritySensitive = attribute.IsSecuritySensitive;
            this.memberDeclaredType = (this.field != null) ? this.field.FieldType : this.property.PropertyType;
            this.defaultMemberValue = DeriveDefaultValue(this.memberDeclaredType);
            Assumes.True(this.memberDeclaredType != null);
            if(attribute.Encoder == null)
            {
                if(!converters.TryGetValue(this.memberDeclaredType, out this.converter))
                {
                    if(this.memberDeclaredType.IsGenericType&&
                        this.memberDeclaredType.GetGenericTypeDefinition() == typeof(Nullable))
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(this.memberDeclaredType);
                        ValueMapping underlyingMapping;
                        if (converters.TryGetValue(underlyingType, out underlyingMapping))
                        {
                            this.converter = new ValueMapping(
                                underlyingMapping.ValueToString,
                                null,
                                str => str != null?underlyingMapping.StringToValue(str):null
                                );
                        }
                        else
                        {
                            this.converter = GetDefaultEncoder(underlyingType);
                        }
                    }
                    else
                    {
                        this.converter = GetDefaultEncoder(this.memberDeclaredType);
                    }
                }
            }else
            {
                this.converter = new ValueMapping(GetEncoder(attribute.Encoder));
            }
            FieldAttributes constAttributes = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault;
            if(this.field != null &&(
                (this.field.Attributes & FieldAttributes.InitOnly) == FieldAttributes.InitOnly ||
                (this.field.Attributes & constAttributes) == constAttributes))
            {
                this.IsConstantValue = true;
                this.IsConstantValueAvailableStatically = this.field.IsStatic;
            }
            else if(this.property != null && !this.property.CanWrite)
            {
                this.IsConstantValue = true;
            }
            this.ValidateSettings();
        }

        public string Name { get; set; }
        public ProtectionLevel RequiredProtection { get; set; }
        public bool IsRequired { get; set; }
        public bool AllowEmpty { get; set; }
        public bool IsConstantValue { get; set; }
        public bool IsConstantValueAvailableStatically { get; set; }
        public bool IsSecuritySensitive { get; set; }
        public string StaticConstantValue
        {
            get
            {
                return this.ToString(this.field.GetValue(null), false);
            }
        }

        public Type MemberDeclaredType
        {
            get { return this.memberDeclaredType; }
        }

        public Type PreferredFormattingType
        {
            get
            {
                var formattingEncoder = this.converter.Encoder as IMessagePartFormattingEncoder;
                if(formattingEncoder != null)
                {
                    return formattingEncoder.FormattingType;
                }
                return this.MemberDeclaredType;
            }
        }

        public void SetValue(IMessage message, string value)
        {
            try
            {
                if(this.IsConstantValue)
                {
                    string constantValue = this.GetValue(message);
                    var caseSensitivity = DotNetOpenAuthSection.Messaging.Strict ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    if(!string.Equals(constantValue, value, caseSensitivity))
                    {
                        throw new ArgumentException(string.Format(
                            CultureInfo.CurrentCulture,
                            MessagingStrings.UnexpectedMessagePartValueForConstant,
                            message.GetType().Name,
                            this.Name,
                            constantValue,
                            value)
                            );
                    }
                }
                else
                {
                    this.SetValueAsObject(message, this.ToValue(value));
                }
            }catch (Exception ex)
            {
                throw ErrorUtilities.Wrap(ex, MessagingStrings.MessagePartReadFailure, message.GetType(), this.Name, value);
            }
        }

        public string GetValue(IMessage message)
        {
            try
            {
                object value = this.GetValueAsObject(message);
                return this.ToString(value, false);
            }catch(FormatException ex)
            {
                throw ErrorUtilities.Wrap(ex, MessagingStrings.MessagePartWriteFailure, message.GetType(), this.Name);
            }
        }

        public string GetValue(IMessage message, bool originalValue)
        {
            try
            {
                object value = this.GetValueAsObject(message);
                return this.ToString(value, originalValue);
            }
            catch(FormatException ex)
            {
                throw ErrorUtilities.Wrap(ex, MessagingStrings.MessagePartWriteFailure, message.GetType(), this.Name);
            }
        }

        public bool IsNondefaultValueSet(IMessage message)
        {
            if(this.memberDeclaredType.IsValueType)
            {
                return !this.GetValueAsObject(message).Equals(this.defaultMemberValue);
            }
            else
            {
                return this.defaultMemberValue != this.GetValueAsObject(message);
            }
        }

        private static void Map<T>(Func<T,string> toString, Func<T, string> toOriginalString, Func<string,T> toValue)
        {
            if(toOriginalString == null )
            {
                toOriginalString = toString;
            }
            Func<object, string> safeToString = obj => obj != null ? toString((T)obj) : null;
            Func<object, string> safeToOriginalString = obj => obj != null ? toOriginalString((T)obj) : null;
            Func<string, object> safeToT = str => str != null ? toValue(str) : default(T);
            converters.Add(typeof(T), new ValueMapping(safeToString, safeToOriginalString, safeToT));
        }

        private static ValueMapping CreateFallbackMapping(Type type)
        {
            return new ValueMapping(
                obj => obj != null ?obj.ToString():null,
                null,
                str => str != null?Convert.ChangeType(str, type, CultureInfo.InvariantCulture):null);
        }

        private static ValueMapping GetDefaultEncoder(Type type)
        {
            var converterAttributes = (DefaultEncoderAttribute[])type.GetCustomAttributes(typeof(DefaultEncoderAttribute), false);
            ErrorUtilities.VerifyInternal(converterAttributes.Length < 2, "Too Many attributes applied.");
            if(converterAttributes.Length == 1)
            {
                return new ValueMapping(converterAttributes[0].Encoder);
            }
            return CreateFallbackMapping(type);
        }

        private static object DeriveDefaultValue(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }

        private static bool IsNonNullableValueType(Type type)
        {
            if(!type.IsValueType)
            {
                return false;
            }
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return false;
            }

            return true;
        }

        private static IMessagePartEncoder GetEncoder(Type messagePartEncoder)
        {
            IMessagePartEncoder encoder;
            lock(encoders)
            {
                if(!encoders.TryGetValue(messagePartEncoder, out encoder))
                {
                    try
                    {
                        encoder = encoders[messagePartEncoder] = (IMessagePartEncoder)Activator.CreateInstance(messagePartEncoder);
                    }
                    catch(MissingMemberException ex)
                    {
                        throw ErrorUtilities.Wrap(ex, MessagingStrings.EncoderInstantiationFailed, messagePartEncoder.FullName);
                    }
                }
            }
            return encoder;
        }

        private object GetValueAsObject(IMessage message)
        {
            if(this.property != null)
            {
                return this.property.GetValue(message, null);
            }
            else
            {
                return this.field.GetValue(message);
            }
        }

        private void SetValueAsObject(IMessage message, object value)
        {
            if(this.property != null)
            {
                this.property.SetValue(message, value, null);
            }
            else
            {
                this.field.SetValue(message, value);
            }
        }

        private object ToValue(string value)
        {
            return this.converter.StringToValue(value);
        }

        private string ToString(object value, bool originalString)
        {
            return originalString ? this.converter.ValueToOriginalString(value) : this.converter.ValueToString(value);
        }

        private void ValidateSettings()
        {
            if(!this.IsRequired && IsNonNullableValueType(this.memberDeclaredType))
            {
                MemberInfo member = (MemberInfo)this.field ?? this.property;
                throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        "Invalid combination: {0} on message type {1} is a non-nullable value type but is marked as optional.",
                        member.Name,
                        member.DeclaringType)
                    );
            }
        }
    }
}

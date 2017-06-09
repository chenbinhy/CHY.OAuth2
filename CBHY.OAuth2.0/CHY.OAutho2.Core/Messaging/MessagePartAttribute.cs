using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited=true, AllowMultiple=true)]
    [DebuggerDisplay("MessagePartAttribute {Name}")]
    public sealed class MessagePartAttribute:Attribute
    {
        private string name;

        public MessagePartAttribute()
        {
            this.AllowEmpty = true;
            this.MinVersionValue = new Version(0, 0);
            this.MaxVersionValue = new Version(int.MaxValue, 0);
        }

        public MessagePartAttribute(string name)
            : this()
        {
            this.Name = name;
        }

        public string Name
        {
            get { return this.name; }
            private set { this.name = string.IsNullOrEmpty(value) ? null : value; }
        }

        public ProtectionLevel RequiredProtection { get; set; }

        public bool IsRequired { get; set; }

        public bool AllowEmpty { get; set; }

        public Type Encoder { get; set; }

        public string MinVersion
        {
            get { return this.MinVersionValue.ToString(); }
            set { this.MinVersionValue = new Version(value); }
        }

        public string MaxVersion
        {
            get { return this.MaxVersionValue.ToString(); }
            set { this.MaxVersionValue = new Version(value); }
        }

        public bool IsSecuritySensitive { get; set; }

        public Version MinVersionValue { get; set; }

        public Version MaxVersionValue { get; set; }
    }
}

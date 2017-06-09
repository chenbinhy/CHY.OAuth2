using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public abstract class DataBag:IMessage
    {
        private static readonly Version DefaultVersion = new Version(1, 0);
        private Version version;
        private Dictionary<string, string> extraData = new Dictionary<string, string>();
        protected DataBag():this(DefaultVersion)
        {

        }

        protected DataBag(Version version)
        {
            this.version = version;
        }

        Version IMessage.Version
        {
            get { return this.version; }
        }

        public IDictionary<string, string> ExtraData
        {
            get { return this.extraData; }
        }

        [MessagePart]
        public byte[] Nonce { get; set; }

        [MessagePart("ts", IsRequired=true, Encoder = typeof(TimestampEncoder))]
        public DateTime UtcCreationDate { get; set; }

        public byte[] Signature { get; set; }

        public IProtocolMessage ContainingMessage { get; set; }

        [MessagePart("t", IsRequired=true, AllowEmpty=false)]
        protected virtual Type BagType{
            get { return this.GetType();}
        }
        void IMessage.EnsureValidMessage(){
            this.EnsureValidMessage();
        }

        protected virtual void EnsureValidMessage(){

        }
    }
}

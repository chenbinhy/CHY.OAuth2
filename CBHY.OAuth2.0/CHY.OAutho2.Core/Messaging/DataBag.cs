using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    /// <summary>
    /// 数据袋
    /// </summary>
    public abstract class DataBag:IMessage
    {
        /// <summary>
        /// 默认版本1.0
        /// </summary>
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

        /// <summary>
        /// 版本
        /// </summary>
        Version IMessage.Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// 附件数据
        /// </summary>
        public IDictionary<string, string> ExtraData
        {
            get { return this.extraData; }
        }

        /// <summary>
        /// 随机字符
        /// </summary>
        [MessagePart]
        public byte[] Nonce { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [MessagePart("ts", IsRequired=true, Encoder = typeof(TimestampEncoder))]
        public DateTime UtcCreationDate { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public byte[] Signature { get; set; }

        public IProtocolMessage ContainingMessage { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [MessagePart("t", IsRequired=true, AllowEmpty=false)]
        protected virtual Type BagType{
            get { return this.GetType();}
        }

        /// <summary>
        /// 验证消息
        /// </summary>
        void IMessage.EnsureValidMessage(){
            this.EnsureValidMessage();
        }

        protected virtual void EnsureValidMessage(){

        }
    }
}

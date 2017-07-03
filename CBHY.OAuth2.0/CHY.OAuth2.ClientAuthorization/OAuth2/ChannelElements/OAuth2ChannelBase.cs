using CHY.OAuth2.Common;
using CHY.OAuth2.Core;
using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.ChannelElements
{
    public abstract class OAuth2ChannelBase:StandardMessageFactoryChannel
    {
        private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();
        public OAuth2ChannelBase(Type[] messageTypes, IChannelBindingElement[] channelBindingElements = null, IHostFactories hostFactories = null)
            :base(messageTypes, Versions, hostFactories??new DefaultOAuthHostFactories(), channelBindingElements??new IChannelBindingElement[0])
        {

        }

        /// <summary>
        /// 去除空值字典
        /// </summary>
        /// <param name="fields"></param>
        protected override void FilterReceivedFields(IDictionary<string, string> fields)
        {
            base.FilterReceivedFields(fields);

            var emptyKeys = from pair in fields
                            where string.IsNullOrEmpty(pair.Value)
                            select pair.Key;
            foreach(string emptyKey in emptyKeys.ToList())
            {
                fields.Remove(emptyKey);
            }
                            
        }
    }
}

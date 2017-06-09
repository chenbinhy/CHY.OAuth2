using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CHY.OAuth2.Client.OAuth2.ChannelElements
{
    public interface IOAuth2ChannelWithClient
    {
        string ClientIdentifier { get; set; }
        ClientCredentialApplicator ClientCredentialApplicator { get; set; }
        XmlDictionaryReaderQuotas JsonReaderQuotas { get; }
    }
}

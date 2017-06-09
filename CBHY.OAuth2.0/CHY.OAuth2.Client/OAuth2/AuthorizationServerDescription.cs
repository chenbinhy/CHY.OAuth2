using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Client.OAuth2
{
    public class AuthorizationServerDescription
    {
        public AuthorizationServerDescription()
        {
            this.ProtocolVersion = Protocol.Default.ProtocolVersion;
        }

        public Uri TokenEndpoint { get; set; }

        public Uri AuthorizationEndpoint { get; set; }

        public ProtocolVersion ProtocolVersion { get; set; }

        internal Version Version
        {
            get { return Protocol.Lookup(this.ProtocolVersion).Version; }
        }
    }
}

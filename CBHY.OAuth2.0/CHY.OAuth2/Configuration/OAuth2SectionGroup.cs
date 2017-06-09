using CHY.OAuth2.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Configuration
{
    public class OAuth2SectionGroup:ConfigurationSectionGroup
    {
        public const string SectionName = DotNetOpenAuthSection.SectionName + "/oauth2";
        public OAuth2SectionGroup()
        {

        }
    }
}

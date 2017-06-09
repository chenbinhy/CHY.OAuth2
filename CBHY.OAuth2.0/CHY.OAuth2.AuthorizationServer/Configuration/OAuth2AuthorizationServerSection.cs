using CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements;
using CHY.OAuth2.Configuration;
using CHY.OAuth2.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.Configuration
{
    public class OAuth2AuthorizationServerSection:ConfigurationSection
    {
        private const string SectionName = OAuth2SectionGroup.SectionName + "/authorizationServer";
        private const string ClientAuthenticationModulesElementName = "clientAuthenticationModules";

        private static readonly TypeConfigurationCollection<ClientAuthenticationModule> defaultClientAuthenticationModules =
            new TypeConfigurationCollection<ClientAuthenticationModule>(new Type[] { typeof(ClientCredentialHttpBasicReader), typeof(ClientCredentialMessagePartReader) });

        public OAuth2AuthorizationServerSection()
        {

        }

        public static OAuth2AuthorizationServerSection Configuration
        {
            get
            {
                return (OAuth2AuthorizationServerSection)ConfigurationManager.GetSection(SectionName) ?? new OAuth2AuthorizationServerSection();
            }
        }

        [ConfigurationProperty(ClientAuthenticationModulesElementName, IsDefaultCollection=false)]
        [ConfigurationCollection(typeof(TypeConfigurationCollection<ClientAuthenticationModule>))]
        public TypeConfigurationCollection<ClientAuthenticationModule> ClientAuthenticationModules
        {
            get
            {
                var configResult = (TypeConfigurationCollection<ClientAuthenticationModule>)this[ClientAuthenticationModulesElementName];
                return configResult != null && configResult.Count > 0 ? configResult : defaultClientAuthenticationModules;
            }
            set
            {
                this[ClientAuthenticationModulesElementName] = value;
            }
        }
    }
}

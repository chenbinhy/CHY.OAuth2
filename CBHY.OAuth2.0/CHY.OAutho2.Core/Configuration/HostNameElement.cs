using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Configuration
{
    public class HostNameElement:ConfigurationElement
    {
        /// <summary>
        /// Gets the name of the @name attribute.
        /// </summary>
        private const string NameConfigName = "name";

        /// <summary>
        /// Initializes a new instance of the <see cref="HostNameElement"/> class.
        /// </summary>
        public HostNameElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostNameElement"/> class.
        /// </summary>
        /// <param name="name">The default value of the <see cref="Name"/> property.</param>
        public HostNameElement(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the host on the white or black list.
        /// </summary>
        [ConfigurationProperty(NameConfigName, IsRequired = true, IsKey = true)]
        ////[StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (string)this[NameConfigName]; }
            set { this[NameConfigName] = value; }
        }
    }
}

using CHY.OAuth2.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Configuration
{
    public class TypeConfigurationCollection<T>:ConfigurationElementCollection where T:class
    {
        public TypeConfigurationCollection() { }

        public TypeConfigurationCollection(IEnumerable<Type> elements)
        {
            foreach(Type element in elements)
            {
                this.BaseAdd(new TypeConfigurationElement<T> { TypeName = element.AssemblyQualifiedName });
            }
        }

        public IEnumerable<T> CreateInstances(bool allowInternals, IHostFactories hostFactories)
        {
            return from element in this.Cast<TypeConfigurationElement<T>>()
                   where !element.IsEmpty
                   select element.CreateInstance(default(T), allowInternals, hostFactories);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TypeConfigurationElement<T>();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            TypeConfigurationElement<T> typedElement = (TypeConfigurationElement<T>)element;

            return (!string.IsNullOrEmpty(typedElement.TypeName)?typedElement.TypeName:typedElement.XamlSource)??string.Empty;
        }
    }
}

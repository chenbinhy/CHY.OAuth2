using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core
{
    public interface IEmbeddedResourceRetrieval
    {
        Uri GetWebResourceUrl(Type someTypeInResourceAssembly, string manifestResourceName);
    }
}

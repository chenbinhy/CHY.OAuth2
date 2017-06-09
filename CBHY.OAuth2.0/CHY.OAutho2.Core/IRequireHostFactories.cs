using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core
{
    public interface IRequireHostFactories
    {
        IHostFactories HostFactories { get; set; }
    }
}

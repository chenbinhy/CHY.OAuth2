
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2
{
    public interface IScopeSatisfiedCheck
    {
        bool IsScopeSatisfied(HashSet<string> requiredScope, HashSet<string> grantedScope);
    }
}

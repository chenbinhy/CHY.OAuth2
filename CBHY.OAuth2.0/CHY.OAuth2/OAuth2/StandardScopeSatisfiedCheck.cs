using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.OAuth2
{
    /// <summary>
    /// 范围检测
    /// </summary>
    public class StandardScopeSatisfiedCheck:IScopeSatisfiedCheck
    {
        public bool IsScopeSatisfied(HashSet<string> requiredScope, HashSet<string> grantedScope)
        {
            return grantedScope.IsSupersetOf(requiredScope);
        }
    }
}

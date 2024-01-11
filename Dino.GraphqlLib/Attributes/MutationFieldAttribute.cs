using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class MutationFieldAttribute : Attribute { }
}

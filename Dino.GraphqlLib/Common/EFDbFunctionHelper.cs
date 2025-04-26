using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Common
{
    public static class EFDbFunctionHelper
    {
        //[DbFunction("CHECKSUM", IsBuiltIn = true)]
        public static int Checksum(string id, string seed) => HashCode.Combine(id, seed);
    }
}

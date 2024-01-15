using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Utilities
{
    public static class SiteHelper
    {
        public const string Subfix = "graphql-site";
        public static string GetRoleSite(this string claimValue)
        {
            return $"{claimValue}-{Subfix}".ToLower();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Utilities
{
    public static class CollectionExtension
    {
        public static bool IsDbSet(this Type type, Type dbSetType)
        {
            return dbSetType.IsGenericType && dbSetType.GetGenericTypeDefinition() == type;
        }
        public static bool IsQueryabe(this Type type, Type queryable)
        {
            return queryable.IsAssignableFrom(type);
        }
    }
}

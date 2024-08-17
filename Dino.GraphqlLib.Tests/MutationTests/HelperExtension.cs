using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Tests.Mutations
{
    public static class HelperExtension
    {
        public static IEnumerable<Type> GetInnnerTypes(this Type type)
        {
            Type[] innerTypes = type.GetNestedTypes();
            return innerTypes;
        }
        public static IEnumerable<Type> GetInnnerTypes<TClass>(this TClass @class)
        {
            return typeof(TClass).GetInnnerTypes();
        }
    }
}

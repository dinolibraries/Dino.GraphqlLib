using Dino.GraphqlLib.Extensions.FilterWithRole;
using EntityGraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public static class FieldExtension
    {
        public static IField UseExpressionFilter(this IField field)
        {
            // register extension on field
            field.AddExtension(new ExpressionFilterFieldExtension());
            return field;
        }
    }
}

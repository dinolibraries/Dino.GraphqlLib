using Dino.GraphqlLib.Extensions;
using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.Extensions.RandomWithSeed;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
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
        public static IField UseRandomWithSeed(this IField field)
        {
            field.AddExtension(new RandomWithSeedExtension());
            return field;
        }
    }
}

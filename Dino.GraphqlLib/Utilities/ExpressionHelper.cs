using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Utilities
{
    internal static class ExpressionHelper
    {
        public static LambdaExpression CloneExpression(this LambdaExpression originalExpression)
        {
            return Expression.Lambda(originalExpression.Body, originalExpression.Parameters);
        }

    }
}

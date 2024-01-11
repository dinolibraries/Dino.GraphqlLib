using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Utilities
{
    public class ReplaceToContantExpression : ExpressionVisitor
    {
        [return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if(node.Method == null)
            {
                var value = Expression.Lambda(node).Compile().DynamicInvoke();
                return Expression.Constant(value);
            }
            return base.VisitBinary(node);
        }
    }
}

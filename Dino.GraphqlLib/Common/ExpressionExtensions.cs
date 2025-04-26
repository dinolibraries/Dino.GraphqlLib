using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Common
{
    public static class ExpressionExtensions
    {
        public static bool HasOrderBy(this Expression expression)
        {
            var visitor = new OrderByDetectorVisitor();
            visitor.Visit(expression);
            return visitor.HasOrderBy;
        }

        private class OrderByDetectorVisitor : ExpressionVisitor
        {
            public bool HasOrderBy { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(Queryable))
                {
                    if (node.Method.Name == "OrderBy" ||
                        node.Method.Name == "OrderByDescending" ||
                        node.Method.Name == "ThenBy" ||
                        node.Method.Name == "ThenByDescending")
                    {
                        HasOrderBy = true;
                    }
                }
                return base.VisitMethodCall(node);
            }
        }
    }
}

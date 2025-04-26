using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Common
{
    public class ReplaceOrderByVisitor : ExpressionVisitor
    {
        private readonly LambdaExpression _newOrderBySelector;
        private readonly string _orderByMethodName;

        public ReplaceOrderByVisitor(LambdaExpression newOrderBySelector, string orderByMethodName = "OrderBy")
        {
            _newOrderBySelector = newOrderBySelector;
            _orderByMethodName = orderByMethodName; // "OrderBy" hoặc "OrderByDescending" nếu cần
        }
        public static Expression Replace(Expression root, LambdaExpression lambdaExpression)
        {
            return new ReplaceOrderByVisitor(lambdaExpression).Visit(root);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == _orderByMethodName)
            {
                // Thay thế OrderBy
                var source = node.Arguments[0];
                var newMethod = typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == _orderByMethodName
                                && m.GetParameters().Length == 2)
                    .MakeGenericMethod(_newOrderBySelector.Parameters[0].Type, _newOrderBySelector.Body.Type);

                return Expression.Call(newMethod, source, _newOrderBySelector);
            }

            return base.VisitMethodCall(node);
        }
    }
}

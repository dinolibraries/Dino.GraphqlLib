using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Common
{
    using System;
    using System.Linq.Expressions;

    public class InsertOrderByVisitor : ExpressionVisitor
    {
        private readonly LambdaExpression _orderBySelector;
        private bool _inserted = false; // Flag to check if we have already inserted the first OrderBy

        // Constructor takes the OrderBy selector expression (e.g., checksum(x.Id, seedKey))
        public InsertOrderByVisitor(LambdaExpression orderBySelector)
        {
            _orderBySelector = orderBySelector;
        }
        public static Expression Insert(Expression root, LambdaExpression lambdaExpression)
        {
            return new InsertOrderByVisitor(lambdaExpression).Visit(root);
        }

        // Override the VisitMethodCall method to visit each method call in the expression tree
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Nếu gặp OrderBy hoặc OrderByDescending
            if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending")
            {
                var source = node.Arguments[0]; // Nguồn IQueryable

                // Tạo OrderBy với checksum(x.Id, seedKey)
                var orderByMethod = typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(_orderBySelector.Parameters[0].Type, _orderBySelector.Body.Type);

                // Tạo node mới: OrderBy(checksum(x.Id, seedKey))
                var newSource = Expression.Call(orderByMethod, source, _orderBySelector);

                // Tùy theo node hiện tại là OrderBy hay OrderByDescending
                var changeMethod = node.Method.Name == "OrderBy" ? "ThenBy" : "ThenByDescending";

                // Lấy ThenBy/ThenByDescending method
                var thenByMethod = typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == changeMethod && m.GetParameters().Length == 2)
                    .MakeGenericMethod(_orderBySelector.Parameters[0].Type, GetLambdaReturnType(node.Arguments[1]));

                // Tạo ThenBy/ThenByDescending giữ nguyên cái selector cũ (node.Arguments[1])
                var replaceExpress = Expression.Call(thenByMethod, newSource, node.Arguments[1]);

                return replaceExpress; // ⚡ Đây, trả ra cái mới đã chèn OrderBy rồi ThenBy
            }

            return base.VisitMethodCall(node); // Nếu không phải OrderBy thì đi tiếp như bình thường
        }

        private static Type GetLambdaReturnType(Expression expression)
        {
            if (expression is UnaryExpression unary && unary.Operand is LambdaExpression lambda)
            {
                return lambda.Body.Type;
            }
            if (expression is LambdaExpression lambdaExpr)
            {
                return lambdaExpr.Body.Type;
            }
            throw new InvalidOperationException("Cannot determine lambda return type.");
        }

    }
}

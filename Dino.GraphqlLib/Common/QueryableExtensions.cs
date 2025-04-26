using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByChecksum<T>(
    this IQueryable<T> source,
    Expression<Func<T, int>> idSelector,
    int seedKey)
        {
            // Get the parameter of the lambda expression (e.g., 't => ...')
            var param = idSelector.Parameters[0];

            // Extract the expression that accesses the 'Id' property (e.g., 't.Id')
            var idExpr = idSelector.Body;

            // Get the method reference for the custom EFDbFunctionHelper.Checksum method
            var method = typeof(EFDbFunctionHelper).GetMethod(nameof(EFDbFunctionHelper.Checksum), BindingFlags.Static | BindingFlags.Public);

            // Create an expression to call the Checksum method, passing the idExpr and seedKey as arguments
            var callExpr = Expression.Call(method, idExpr, Expression.Constant(seedKey));

            // Create a lambda expression 't => Checksum(t.Id, seedKey)'
            var lambda = Expression.Lambda<Func<T, int>>(callExpr, param);

            // Apply the 'OrderBy' using the lambda expression that performs the checksum calculation
            return source.OrderBy(lambda);
        }

        public static Expression<Func<T, int>> BuildChecksumExpression<T>(this string fieldName, string seedKey)
        {
            var paramType = typeof(T);
            // Ensure paramType matches the expected type T
            if (paramType != typeof(T))
            {
                throw new ArgumentException($"paramType must be of type {typeof(T).FullName}", nameof(paramType));
            }
            return (Expression<Func<T, int>>)BuildChecksumExpression(typeof(T), fieldName, seedKey);
        }
        public static Expression BuildChecksumExpression(this Type paramType, string fieldName, string seedKey)
        {

            // Create a parameter expression for the lambda (e.g., 'x => ...')
            var param = Expression.Parameter(paramType, "x");

            // Create an expression to access the property (e.g., 'x.Field')
            Expression propertyExpr = Expression.PropertyOrField(param, fieldName);
            if (propertyExpr.Type != typeof(string))
            {
                // Gọi ToString() nếu kiểu khác string
                propertyExpr = Expression.Call(propertyExpr, "ToString", Type.EmptyTypes);
            }
           
            // Create an expression that calculates the checksum using the property value (e.g., 'Checksum(x.Field, seedKey)')
            var checksumMethod = typeof(EFDbFunctionHelper).GetMethod(nameof(EFDbFunctionHelper.Checksum), BindingFlags.Static | BindingFlags.Public);
            var checksumExpr = Expression.Call(checksumMethod, propertyExpr, Expression.Constant(seedKey));

            // Return a lambda expression that computes 'OrderByChecksum(x.Field)'
            return Expression.Lambda(checksumExpr, param);
        }
    }
}

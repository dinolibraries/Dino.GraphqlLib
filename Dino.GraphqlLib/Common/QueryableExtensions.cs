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
        // 1. Hàm dùng cho LINQ tĩnh
        public static IQueryable<T> OrderByChecksum<T>(
            this IQueryable<T> source,
            Expression<Func<T, object>> idSelector,
            string seedKey)
        {
            var param = idSelector.Parameters;

            // Lấy biểu thức gốc (ví dụ: t.Id)
            var idExpr = idSelector.Body is UnaryExpression unary
                ? unary.Operand
                : idSelector.Body;

            // Bước 1: Thử tìm hàm Checksum khớp với kiểu dữ liệu gốc của cột
            var checksumMethod = typeof(EFDbFunctionHelper).GetMethod(
                nameof(EFDbFunctionHelper.Checksum),
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { idExpr.Type, typeof(string) },
                null
            );

            // Bước 2: NẾU KHÔNG THẤY -> Tự động gọi ToString() và lấy hàm Checksum(string)
            if (checksumMethod == null)
            {
                idExpr = Expression.Call(idExpr, "ToString", Type.EmptyTypes);
                checksumMethod = typeof(EFDbFunctionHelper).GetMethod(
                    nameof(EFDbFunctionHelper.Checksum),
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(string), typeof(string) },
                    null
                );
            }

            var callExpr = Expression.Call(checksumMethod, idExpr, Expression.Constant(seedKey));
            var lambda = Expression.Lambda<Func<T, int>>(callExpr, param);

            return source.OrderBy(lambda);
        }

        // 2. Hàm dùng cho Dynamic LINQ (Gọi qua String FieldName từ GraphQL)
        public static Expression<Func<T, int>> BuildChecksumExpression<T>(this string fieldName, string seedKey)
        {
            return (Expression<Func<T, int>>)BuildChecksumExpression(typeof(T), fieldName, seedKey);
        }

        public static Expression BuildChecksumExpression(this Type paramType, string fieldName, string seedKey)
        {
            var param = Expression.Parameter(paramType, "x");
            Expression propertyExpr = Expression.PropertyOrField(param, fieldName);

            // Bước 1: Thử tìm hàm Checksum khớp với kiểu dữ liệu gốc của cột
            var checksumMethod = typeof(EFDbFunctionHelper).GetMethod(
                nameof(EFDbFunctionHelper.Checksum),
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { propertyExpr.Type, typeof(string) },
                null
            );

            // Bước 2: NẾU KHÔNG THẤY -> Tự động gọi ToString() và lấy hàm Checksum(string)
            if (checksumMethod == null)
            {
                propertyExpr = Expression.Call(propertyExpr, "ToString", Type.EmptyTypes);
                checksumMethod = typeof(EFDbFunctionHelper).GetMethod(
                    nameof(EFDbFunctionHelper.Checksum),
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(string), typeof(string) },
                    null
                );
            }

            var checksumExpr = Expression.Call(checksumMethod, propertyExpr, Expression.Constant(seedKey));

            return Expression.Lambda(checksumExpr, param);
        }
    }
}

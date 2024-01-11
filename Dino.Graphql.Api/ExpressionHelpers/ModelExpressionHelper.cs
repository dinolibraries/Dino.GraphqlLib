using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Dino.Graphql.Api.ExpressionHelpers
{
    public static class ModelExpressionHelper
    {
        public static Expression<Func<TSource, bool>> BuildExpressionFindById<TSource>(this DbContext dbContext, TSource source)
        {
            Type typeFromHandle = typeof(TSource);
            IKey? key = dbContext.Model.FindEntityType(typeFromHandle).FindPrimaryKey();
            ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle, "x");
            PropertyInfo propertyInfo = key.Properties.First().PropertyInfo;
            Expression expression = Expression.Equal(Expression.Property(parameterExpression, propertyInfo), Expression.Constant(propertyInfo.GetValue(source)));
            foreach (IProperty item in key.Properties.Skip(1))
            {
                PropertyInfo propertyInfo2 = item.PropertyInfo;
                BinaryExpression right = Expression.Equal(Expression.Property(parameterExpression, propertyInfo2), Expression.Constant(propertyInfo2.GetValue(source)));
                expression = Expression.AndAlso(expression, right);
            }

            return Expression.Lambda<Func<TSource, bool>>(expression, new ParameterExpression[1] { parameterExpression });
        }

        public static Expression<Func<TSource, bool>> BuildExpressionFindByIdWith<TSource, Tkey>(this DbContext dbContext, Tkey valueObject)
        {
            Type typeFromHandle = typeof(TSource);
            IKey key = dbContext.Model.FindEntityType(typeFromHandle).FindPrimaryKey();
            ParameterExpression parameterExpression = Expression.Parameter(typeFromHandle, "x");
            Expression expression = null;
            Type type = valueObject.GetType();
            if (!type.IsClass || type == typeof(Guid) || type == typeof(string))
            {
                expression = Expression.Equal(Expression.Property(parameterExpression, key.Properties.First().PropertyInfo), Expression.Constant(valueObject));
            }
            else
            {
                PropertyInfo[] properties = valueObject.GetType().GetProperties();
                var source2 = key.Properties.Select((IProperty x) => x.PropertyInfo).Join(properties, (PropertyInfo x) => new
                {
                   x.Name,
                   x.PropertyType
                }, (PropertyInfo x) => new
                {
                    x.Name,
                    x.PropertyType
                }, (PropertyInfo source, PropertyInfo value) => new { source, value }).ToArray();
                var anon = source2.First();
                expression = Expression.Equal(Expression.Property(parameterExpression, anon.source), Expression.Constant(anon.value.GetValue(valueObject)));
                foreach (var item in source2.Skip(1))
                {
                    BinaryExpression right = Expression.Equal(Expression.Property(parameterExpression, item.source), Expression.Constant(item.value.GetValue(valueObject)));
                    expression = Expression.AndAlso(expression, right);
                }
            }

            return Expression.Lambda<Func<TSource, bool>>(expression, new ParameterExpression[1] { parameterExpression });
        }
    }
}

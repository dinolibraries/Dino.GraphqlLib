using Dino.GraphqlLib.Attributes;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static EntityQL.Grammer.EntityQLParser;

namespace Dino.GraphqlLib.ExpressionHelpers
{
    public static class ModelMapperExtension
    {
        public static TExpre ReplaceWhereCondition<TExpre, TModel>(this TExpre root, Expression<Func<TModel, bool>> whereCondition)
        where TExpre : Expression
        {
            var converter = new ChangeWhereClauseVisitor(whereCondition);
            return (TExpre)converter.Visit(root);
        }
        public static Expression<Func<TSource, TTarget>> ModelExtendExpression<TSource, TTarget>(Expression<Func<TSource, TTarget>> expression = null)
        {
            return (Expression<Func<TSource, TTarget>>)MapModelExpressionBase(typeof(TSource), typeof(TTarget), expression);
        }
        public static TTarget ModelExtendCustomSelect<TSource, TTarget>(this TSource source, Expression<Func<TSource, TTarget>> expression = null) where TSource : class
        {
            return ModelExtendExpression(expression).Compile()(source);
        }
        public static LambdaExpression MapModelExpressionBase(Type tS, Type tT, LambdaExpression expression = null)
        {
            var source = from s in tS.GetProperties()
                         join t in tT.GetProperties()
                         on new
                         {
                             s.Name,
                             s.PropertyType
                         }
                         equals new
                         {
                             t.Name,
                             t.PropertyType
                         }
                         select new { s, t };

            ParameterExpression parameter = (expression != null) ? expression.Parameters.First() : Expression.Parameter(tS);
            IEnumerable<MemberBinding> enumerable = source.Select(prop =>
            {
                MemberExpression expression2 = Expression.Property(parameter, prop.s);
                return (MemberBinding)Expression.Bind(prop.t, expression2);
            });
            if (expression?.Body is MemberInitExpression memberInitExpression)
            {
                enumerable = enumerable.Concat(memberInitExpression.Bindings);
            }
            return Expression.Lambda(Expression.MemberInit(Expression.New(tT), enumerable), parameter);
        }
        public static TTarget MapModelExpression<TSource, TTarget>(this TSource source, Expression<Func<TSource, TTarget>> expression = null) where TSource : class
        {
            return ModelExtendExpression(expression).Compile()(source);
        }
        public static TTarget MapModelExpression<TSource, TTarget>(this TSource source, TTarget target) where TSource : class
        {
            var tS = typeof(TSource);
            var tT = typeof(TTarget);

            var sources = from s in tS.GetProperties().Where(x => !Attribute.IsDefined(x.PropertyType, typeof(IgnorePropertyAttribute)))
                          join t in tT.GetProperties()
                          on new
                          {
                              s.Name,
                              s.PropertyType
                          }
                          equals new
                          {
                              t.Name,
                              t.PropertyType
                          }
                          select new { s, t };

            foreach (var item in sources)
            {
                item.t.SetValue(target, item.s.GetValue(source));
            }
            return target;
        }
        public static object MapModelExpressionBase(this object source, Type tS, Type tT, LambdaExpression expression = null)
        {
            return MapModelExpressionBase(tS, tT, expression).Compile().DynamicInvoke(source);
        }
    }
}

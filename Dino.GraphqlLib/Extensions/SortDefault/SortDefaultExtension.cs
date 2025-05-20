using Dino.GraphqlLib.Common;
using EntityGraphQL.Compiler.Util;
using EntityGraphQL.Compiler;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EntityGraphQL.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Dino.GraphqlLib.Extensions.SortDefault
{

    public class GraphqlSortDefaultAttribute<TModel> : Attribute
    {
        public GraphqlSortDefaultAttribute(Expression<Func<TModel, string>> KeySelector) : this((KeySelector.Body as MemberExpression).Member.Name)
        {

        }
        public GraphqlSortDefaultAttribute(string keyName)
        {

            KeyName = keyName;
        }
        public string KeyName { get; set; }
    }
    public class GraphqlSortDefaultAttribute : GraphqlSortDefaultAttribute<object>
    {
        public GraphqlSortDefaultAttribute(string keyName) : base(keyName)
        {
        }
    }
    public class SortDefaultExtension : BaseFieldExtension
    {
        private bool isQueryable;

        private Type listType;
        private Type orderType;
        private string primarykeyName = null;

        public SortDefaultExtension()
        {

        }
        public override void Configure(ISchemaProvider schema, IField field)
        {

            if (field.ResolveExpression == null)
            {
                throw new EntityGraphQLCompilerException("FilterExpressionExtension requires a Resolve function set on the field");
            }

            if (!field.ResolveExpression.Type.IsEnumerableOrArray())
            {
                throw new ArgumentException($"Expression for field {field.Name} must be a collection to use FilterExpressionExtension. Found type {field.ReturnType.TypeDotnet}");
            }

            listType = field.ReturnType.TypeDotnet.GetEnumerableOrArrayType();
            isQueryable = typeof(IQueryable).IsAssignableFrom(field.ResolveExpression.Type);
            if (field.ResolveExpression is MemberExpression propExpre)
            {
                var propInfo = listType.GetProperty("Id");
                orderType = propInfo?.PropertyType;
                primarykeyName = propInfo?.Name;
                if (Attribute.IsDefined(propExpre.Member, typeof(GraphqlSortDefaultAttribute)))
                {
                    primarykeyName = propExpre.Member.GetCustomAttribute<GraphqlSortDefaultAttribute>()?.KeyName;
                }
            }

        }
        public class Student
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
        public override Expression GetExpression(IField field, Expression expression, ParameterExpression argumentParam, dynamic arguments, Expression context, IGraphQLNode parentNode, bool servicesPass, ParameterReplacer parameterReplacer)
        {

            if (servicesPass || string.IsNullOrEmpty(primarykeyName))
            {
                return expression;
            }

            var param = Expression.Parameter(listType);
            var fieldExpression = Expression.Lambda(Expression.PropertyOrField(param, primarykeyName), param);

            if (expression.HasOrderBy())
            {
                expression = Expression.Call(isQueryable ? typeof(Queryable) : typeof(Enumerable), "ThenBy", new Type[] { listType, orderType },
                                            expression,
                                            fieldExpression
                                        );
            }
            else
            {
                expression = Expression.Call(isQueryable ? typeof(Queryable) : typeof(Enumerable), "OrderBy", new Type[] { listType, orderType },
                                            expression,
                                            fieldExpression
                                        );
            }
            return expression;
        }
    }
}

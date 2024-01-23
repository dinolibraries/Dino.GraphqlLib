using EntityGraphQL.Compiler.Util;
using EntityGraphQL.Compiler;
using EntityGraphQL.Schema.FieldExtensions;
using EntityGraphQL.Schema;
using System.Linq.Expressions;
using Dino.GraphqlLib.Utilities;
using System.Diagnostics;
using Dino.GraphqlLib.Authorizations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Dino.GraphqlLib.Extensions.FilterWithRole
{
    public class ExpressionFilterFieldExtension : BaseFieldExtension
    {
        public Type ModelType { get; private set; }
        private Type GetModelType(IField field)
        {
            var isCheck = field.ReturnType.TypeDotnet.IsQueryabe(typeof(IQueryable));
            if (isCheck)
            {
                return field.ReturnType.TypeDotnet.GenericTypeArguments.First();
            }
            else
            {
                return field.ReturnType.TypeDotnet;
            }
        }

        public override void Configure(ISchemaProvider schema, IField field)
        {
            ModelType = GetModelType(field);
        }
        public Expression GetExpression<TModel>(IServiceProvider serviceProvider, Expression expression)
            where TModel : class
        {
            var expressionFilter = serviceProvider.GetService<IExpressionFilter<TModel>>();
            return expressionFilter?.GetExpression(expression);
        }
        public Expression GetExpression(IServiceProvider serviceProvider, Type type, Expression expression)
        {
            var method = GetType().GetMethod(nameof(GetExpression), 1, new Type[] { typeof(IServiceProvider), typeof(Expression) }).MakeGenericMethod(type);

            return method.Invoke(this, new object[] { serviceProvider, expression }) as Expression;
        }
        public override Expression GetExpression(IField field, Expression expression, ParameterExpression argumentParam, dynamic arguments, Expression context, IGraphQLNode parentNode, bool servicesPass, ParameterReplacer parameterReplacer)
        {
            if (servicesPass)
            {
                return expression;
            }

            var authService = field.Schema.AuthorizationService as AuthorizationServiceBase;
            var express = GetExpression(authService.ServiceProvider, ModelType, expression);
            Debug.WriteLine("\n" + express + "\n");
            return express ?? expression;
        }

    }
}

using EntityGraphQL.Compiler.Util;
using EntityGraphQL.Compiler;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EntityGraphQL.Extensions;
using Dino.GraphqlLib.Common;

namespace Dino.GraphqlLib.Extensions
{
    public class RandomWithSeedArgs
    {
        public string SeedKey { get; set; }
        public string SeedField { get; set; }
    }
    public class RandomWithSeedExtension : BaseFieldExtension
    {
        private bool isQueryable;

        private Type? listType;

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
            object args = Activator.CreateInstance(typeof(RandomWithSeedArgs));
            field.AddArguments(args);
            isQueryable = typeof(IQueryable).IsAssignableFrom(field.ResolveExpression.Type);
        }

        public override Expression? GetExpression(IField field, Expression expression, ParameterExpression? argumentParam, dynamic? arguments, Expression context, IGraphQLNode? parentNode, bool servicesPass, ParameterReplacer parameterReplacer)
        {
            if (servicesPass)
            {
                return expression;
            }

            if (arguments != null && !string.IsNullOrEmpty(arguments?.SeedKey) && !string.IsNullOrEmpty(arguments?.Field))
            {

                var fieldExpression = listType.BuildChecksumExpression(arguments.Field as string, arguments.SeedKey as string);
                expression = Expression.Call(isQueryable ? typeof(Queryable) : typeof(Enumerable),"OrderBy",new Type[] { listType, typeof(int) },
                                            expression,
                                            fieldExpression
                                        );
            }

            return expression;
        }
    }
}

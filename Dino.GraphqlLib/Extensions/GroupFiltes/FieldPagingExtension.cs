using EntityGraphQL;
using EntityGraphQL.Compiler;
using EntityGraphQL.Compiler.Util;
using EntityGraphQL.Extensions;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Extensions.GroupFiltes
{
    public class FieldPagingExtension : BaseFieldExtension
    {
        private bool isQueryable;

        private Type? listType;

        private readonly int? defaultPageSize;

        private readonly int? maxPageSize;

        public List<IFieldExtension> Extensions { get; private set; } = new List<IFieldExtension>();

        public FieldPagingExtension(int defaultPageSize=50, int maxPageSize = 50)
        {
            this.defaultPageSize = defaultPageSize;
            this.maxPageSize = maxPageSize;
        }

        public override void Configure(ISchemaProvider schema, IField field)
        {
            if (field.ResolveExpression == null)
            {
                throw new EntityGraphQLCompilerException("GroupFilterPagingExtension requires a Resolve function set on the field");
            }

            if (!field.ResolveExpression.Type.IsEnumerableOrArray())
            {
                throw new ArgumentException($"Expression for field {field.Name} must be a collection to use GroupFilterPagingExtension. Found type {field.ReturnType.TypeDotnet}");
            }

            if (field.FieldType == GraphQLQueryFieldType.Mutation)
            {
                throw new EntityGraphQLCompilerException("GroupFilterPagingExtension cannot be used on a mutation field " + field.Name);
            }
            field.AddArguments(new FieldPagingOffsetArgs());
            if (defaultPageSize.HasValue)
            {
                field.Arguments["take"].DefaultValue = defaultPageSize.Value;
            }
            listType = field.ReturnType.TypeDotnet.GetEnumerableOrArrayType();
            isQueryable = typeof(IQueryable).IsAssignableFrom(field.ResolveExpression.Type);
        }

        public override Expression? GetExpression(IField field, Expression expression, ParameterExpression? argumentParam, dynamic? arguments, Expression context, IGraphQLNode? parentNode, bool servicesPass, ParameterReplacer parameterReplacer)
        {
            //if (servicesPass)
            //{
            //    return expression;
            //}

            if (argumentParam == null)
            {
                throw new EntityGraphQLCompilerException("GroupFilterPagingExtension requires argumentParams to be set");
            }

            if (maxPageSize.HasValue && arguments?.Take > maxPageSize.Value)
            {
                throw new EntityGraphQLArgumentException($"Argument take can not be greater than {maxPageSize}.");
            }

            // take skip
            Expression groupedExpression = expression;
            if (arguments?.Skip != null)
            {
                groupedExpression = Expression.Call(
                    isQueryable ? typeof(Queryable) : typeof(Enumerable),
                    "Skip",
                    new Type[] { listType! },
                    groupedExpression,
                    Expression.Convert(Expression.Property(argumentParam, "Skip"), typeof(int))
                );
            }
            else {
                groupedExpression = Expression.Call(
                    isQueryable ? typeof(Queryable) : typeof(Enumerable),
                    "Skip",
                    new Type[] { listType! },
                    groupedExpression,
                    Expression.Constant(0)
                );
            }
            if (argumentParam != null) {
                groupedExpression = Expression.Call(
                    isQueryable ? typeof(Queryable) : typeof(Enumerable),
                    "Take",
                    new Type[] { listType! },
                    groupedExpression,
                    Expression.Convert(Expression.Property(argumentParam, "Take"), typeof(int))
                );
            }

            return groupedExpression;
        }
    }
}

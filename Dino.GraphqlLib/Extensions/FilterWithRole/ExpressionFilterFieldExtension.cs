using EntityGraphQL.Compiler.Util;
using EntityGraphQL.Compiler;
using EntityGraphQL.Schema.FieldExtensions;
using EntityGraphQL.Schema;
using System.Linq.Expressions;
using Dino.GraphqlLib.Utilities;
using System.Diagnostics;

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

        public override Expression GetExpression(IField field, Expression expression, ParameterExpression argumentParam, dynamic arguments, Expression context, IGraphQLNode parentNode, bool servicesPass, ParameterReplacer parameterReplacer)
        {
            if (servicesPass)
            {
                return expression;
            }
            var express = ExpressionFilterCollection.Instance.GetExpression(ModelType, expression);
            Debug.WriteLine("\n" + express + "\n");
            return express ?? expression;
        }

    }
}

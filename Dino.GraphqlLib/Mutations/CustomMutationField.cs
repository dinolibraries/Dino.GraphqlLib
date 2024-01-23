using Dino.GraphqlLib.Utilities;
using EntityGraphQL.Compiler;
using EntityGraphQL.Schema;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public class CustomMutationField : MutationField
    {
        public CustomMutationField(ISchemaProvider schema, ISchemaType fromType, string methodName, GqlTypeInfo returnType, MethodInfo method, string description, RequiredAuthorization requiredAuth, bool isAsync, SchemaBuilderOptions options) : base(schema, fromType, methodName, returnType, method, description, requiredAuth, isAsync, options)
        {
        }

        private void CheckFieldAccess()
        {
            var userPrincial = this.GetService<IHttpContextAccessor>()?.HttpContext?.User;

            if (userPrincial != null && !Schema.AuthorizationService.IsAuthorized(userPrincial, RequiredAuthorization))
                throw new EntityGraphQLAccessException($"You are not authorized to access the '{Name}' field.");
        }
        public override Task<object> CallAsync(object context, IReadOnlyDictionary<string, object> gqlRequestArgs, IServiceProvider serviceProvider, ParameterExpression variableParameter, object docVariables)
        {
            CheckFieldAccess();
            return base.CallAsync(context, gqlRequestArgs, serviceProvider, variableParameter, docVariables);
        }
    }
}

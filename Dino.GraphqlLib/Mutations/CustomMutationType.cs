using Dino.GraphqlLib.Attributes;
using Dino.GraphqlLib.Extensions;
using EntityGraphQL.Schema;
using Nullability;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public class CustomMutationType : MutationType
    {
        public readonly ISchemaProvider schemaProvider;
        private readonly MethodInfo AddMethodAsFieldMethod;
        //private readonly SchemaType SchemaType;

        public CustomMutationType(ISchemaProvider schema, string name, string description, RequiredAuthorization requiredAuthorization) : base(schema, name, description, requiredAuthorization)
        {
            schemaProvider = schema;
            AddMethodAsFieldMethod = typeof(ControllerType).GetMethod("AddMethodAsField", BindingFlags.NonPublic | BindingFlags.Instance);
        }
  
        public RequiredAuthorization AuthorizationMutaionDefault { get; set; }

        public ControllerType AddFromModel(string rootName, Type type, SchemaBuilderOptions options = null)
        {
            if (options == null)
            {
                options = new SchemaBuilderOptions();
            }
            if (!type.IsAssignableTo(typeof(IMutationCRUD)))
            {
                throw new Exception("type must be extend from ICRUDMutation");
            }

            var item = type;
            RequiredAuthorization requiredAuthFromType = schemaProvider.AuthorizationService.GetRequiredAuthFromType(item);

            if (!requiredAuthFromType.Policies.Any() && !requiredAuthFromType.Roles.Any() && AuthorizationMutaionDefault != null)
            {
                requiredAuthFromType = AuthorizationMutaionDefault;
            }
            var methods = item.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .Where(x => Attribute.IsDefined(x, typeof(MutationFieldAttribute)));
            foreach (MethodInfo methodInfo in methods)
            {

                GraphQLMethodAttribute graphQLMethodAttribute = methodInfo.GetCustomAttribute(typeof(GraphQLMethodAttribute)) as GraphQLMethodAttribute;
                if (graphQLMethodAttribute != null || options!.AddNonAttributedMethodsInControllers)
                {
                    string name = schemaProvider.SchemaFieldNamer(Regex.Replace(methodInfo.Name, "[Aa][Ss][Yy][nN][Cc]", ""));
                    AddMethodAsFieldMedthod($"{rootName}_{name}", requiredAuthFromType, methodInfo, graphQLMethodAttribute?.Description ?? "", options);
                }
            }
            return schemaProvider.Mutation();
        }

        private BaseField AddMethodAsFieldMedthod(string name, RequiredAuthorization classLevelRequiredAuth, MethodInfo method, string description, SchemaBuilderOptions options)
        {
            Debug.WriteLine($"========= {nameof(AddMethodAsFieldMedthod)} - adding {name}.");
            // Use reflection to get the private method
            var temp = (BaseField)AddMethodAsFieldMethod.Invoke(schemaProvider.Mutation(), new object[] { name, classLevelRequiredAuth, method, description, options });
            return temp;
        }

        protected override BaseField MakeField(string name, MethodInfo method, string description, SchemaBuilderOptions options, bool isAsync, RequiredAuthorization requiredClaims, GqlTypeInfo returnType)
        {
            options ??= new SchemaBuilderOptions();
            return new CustomMutationField(SchemaType.Schema, SchemaType, name, returnType, method, description ?? string.Empty, requiredClaims, isAsync, options);
        }

    }
}

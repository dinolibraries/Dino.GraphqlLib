using Dino.GraphqlLib.Mutations;
using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public class GraphqlMutationBuilder<TSchemaContext>
       where TSchemaContext : ISchemaContext
    {
        private readonly SchemaProvider<TSchemaContext> _SchemaProvider;
        public GraphqlMutationBuilder(SchemaProvider<TSchemaContext> schemaProvider)
        {
            cutomMutation = schemaProvider.Mutation() as CustomMutationType;
            _SchemaProvider = schemaProvider;
        }
        private readonly CustomMutationType cutomMutation;
        public GraphqlMutationBuilder<TSchemaContext> AddRequiredAuthorizationDefault(RequiredAuthorization requiredAuthorization)
        {
            cutomMutation.AuthorizationMutaionDefault = requiredAuthorization;
            return this;
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutation<TModel, TCreate, TUpdate, TKey>()
        where TModel : class
        where TCreate : ModelBase<TModel>.CreateBase
        where TUpdate : ModelBase<TModel>.UpdateBase
        where TKey : ModelBase<TModel>.KeyBase
        {
            return AddMutation<ContextSelectorDefault<TSchemaContext>, TModel, TCreate, TUpdate, TKey>();
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutation<TDbSelector, TModel, TCreate, TUpdate, TKey>()
        where TDbSelector : class, IContextSelector<TSchemaContext>
        where TModel : class
        where TCreate : ModelBase<TModel>.CreateBase
        where TUpdate : ModelBase<TModel>.UpdateBase
        where TKey : ModelBase<TModel>.KeyBase
        {
            return AddMutation<MutationCRUDDedault<TSchemaContext, TModel, TDbSelector, TCreate, TUpdate, TKey>, TModel>();
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutation<TCRUDMutation, TModel>()
            where TCRUDMutation : IMutationCRUD<TModel>
            where TModel : class
        {
            return AddMutation<TCRUDMutation>(Regex.Replace(typeof(TModel).Name, "[^a-zA-Z0-9]", ""));
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutation<TCRUDMutation>()
            where TCRUDMutation : IMutationCRUD
        {
            var temp = typeof(TCRUDMutation);
            return AddMutation<TCRUDMutation>(temp.Name);
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutation<TCRUDMutation>(string mutationName)
            where TCRUDMutation : IMutationCRUD
        {
            var temp = typeof(TCRUDMutation);
            cutomMutation.AddFromModel(mutationName, temp, new SchemaBuilderOptions
            {
                AddNonAttributedMethodsInControllers = true,
                AutoCreateInputTypes = true,
                AutoCreateNewComplexTypes = true,
            });
            return this;
        }
    }
}

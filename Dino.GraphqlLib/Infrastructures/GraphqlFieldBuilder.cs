using Dino.GraphqlLib.Mutations;
using Dino.GraphqlLib.SchemaContexts;
using Dino.GraphqlLib.Tests.Mutations;
using EntityGraphQL.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public class GraphqlFieldBuilder<TSchemaContext>
        where TSchemaContext : ISchemaContext
    {
        private readonly SchemaProvider<TSchemaContext> SchemaProvider;
        private readonly IServiceCollection _services;
        public GraphqlFieldBuilder(IServiceCollection serviceDescriptors, SchemaProvider<TSchemaContext> schemaProvider)
        {
            SchemaProvider = schemaProvider;
            _services = serviceDescriptors;


        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutationBuilder<TDbContextService>()
        {
            _services.AddScoped(typeof(IDbContextService<,,>), typeof(TDbContextService));
            return new(SchemaProvider);
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutationBuilder(Type dbContextServiceType)
        {
            _services.AddScoped(typeof(IDbContextService<,,>), dbContextServiceType);
            return new(SchemaProvider);
        }
        public GraphqlFieldBuilder<TSchemaContext> RemoveField<TModel>()
        {
            var temps = SchemaProvider.Query()
              .GetFields()
              .Where(x => x.ReturnType.SchemaType.TypeDotnet == typeof(TModel));

            foreach (var item in temps)
            {
                SchemaProvider.Query().RemoveField(item.Name);
            }

            return this;
        }
        public FieldToResolve<TModel> ExtendField<TModel>(string newField)
        {
            var field = SchemaProvider.Type<TModel>();
            return field.AddField(newField, null);
        }
    }
    public class GraphqlMutationBuilder<TSchemaContext>
        where TSchemaContext : ISchemaContext
    {
        private readonly SchemaProvider<TSchemaContext> _SchemaProvider;
        public GraphqlMutationBuilder(SchemaProvider<TSchemaContext> schemaProvider)
        {
            cutomMutation = new CustomMutationType(schemaProvider);
            _SchemaProvider = schemaProvider;
        }
        private readonly CustomMutationType cutomMutation;
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
            return AddMutation< MutationCRUDDedault< TSchemaContext,TModel,TDbSelector,TCreate, TUpdate, TKey>,TModel>();
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
            });
            return this;
        }

        
    }
}

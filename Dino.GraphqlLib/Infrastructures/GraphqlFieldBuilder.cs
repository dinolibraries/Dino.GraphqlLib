using Dino.GraphqlLib.Mutations;
using Dino.GraphqlLib.SchemaContexts;
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
        private readonly SchemaProvider<TSchemaContext> _schemaProvider;
        private readonly IServiceCollection _services;
        public GraphqlFieldBuilder(IServiceCollection serviceDescriptors, SchemaProvider<TSchemaContext> schemaProvider)
        {
            _schemaProvider = schemaProvider;
            _services = serviceDescriptors;


        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutationBuilder<TDbContextService>()
        {
            _services.AddScoped(typeof(IDbContextService<,,>), typeof(TDbContextService));
            return new(_schemaProvider);
        }
        public GraphqlMutationBuilder<TSchemaContext> AddMutationBuilder(Type dbContextServiceType)
        {
            _services.AddScoped(typeof(IDbContextService<,,>), dbContextServiceType);
            return new(_schemaProvider);
        }
        public IEnumerable<IField> Fields<TModel>()
        {
            var temps = _schemaProvider.Query()
              .GetFields()
              .Where(x => x.ReturnType.SchemaType.TypeDotnet == typeof(TModel));
            return temps;
        }
            public GraphqlFieldBuilder<TSchemaContext> RemoveField<TModel>()
        {
            var temps = _schemaProvider.Query()
              .GetFields()
              .Where(x => x.ReturnType.SchemaType.TypeDotnet == typeof(TModel));

            foreach (var item in temps)
            {
                _schemaProvider.Query().RemoveField(item.Name);
            }

            return this;
        }

        private bool CheckFieldName(string fieldName)
        {
            return !Regex.IsMatch(fieldName, "^[A-Z]");
        }

        public FieldToResolve<TModel> ExtendField<TModel>(string newField)
        {
            if (!CheckFieldName(newField))
            {
                throw new Exception("newField must start with lower charater!");
            }
            var field = _schemaProvider.Type<TModel>();
            return field.AddField(newField, null);
        }

        public FieldToResolve<TModel> ReplaceField<TModel>(string nameField)
        {
            if (!CheckFieldName(nameField))
            {
                throw new Exception("newField must start with lower charater!");
            }
            var field = _schemaProvider.Type<TModel>();
            return field.ReplaceField(nameField, null);
        }

        public SchemaProvider<TSchemaContext> SchemaProvider { get=>_schemaProvider; }

    }
}

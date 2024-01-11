using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.Mutations;
using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public class GraphqlBuilder<TSchemaContext>
        where TSchemaContext : ISchemaContext
    {
        private readonly IServiceCollection _services;
        public GraphqlBuilder(IServiceCollection services)
        {
            _services = services;
            ExpressionFilterCollection.Instance.SetupService(_services);
        }

        public IExpressionFilterCollection AddFilterExpression<TDbContext>()
        {
            return AddFilterExpression(typeof(TDbContext));
        }
        public IExpressionFilterCollection AddFilterExpression(Type DbContextType)
        {
            AddDbContextType(DbContextType);
            if (DbContexType == null) throw new Exception(nameof(DbContexType) + " can not null!");
            return ExpressionFilterCollection.Instance;
        }
        public Action<GraphqlFieldBuilder<TSchemaContext>> FieldBuilder { get; set; }
        private Type DbContexType { get; set; }
        public GraphqlBuilder<TSchemaContext> AddDbContextType(Type type)
        {
            DbContexType = type;
            return this;
        }
        internal IEnumerable<Type> GetDbContexType()
        {
            if (DbContexType == null)
            {
                return Type.EmptyTypes;
            }
            var typeRoot = typeof(TSchemaContext);

            return typeRoot.GetProperties().Select(x => x.PropertyType).Union(new[] { typeRoot }).Where(x => x.IsAssignableTo(DbContexType));
        }
        public bool IsDbSet(Type type)
        {
            return type.IsGenericType && type.IsAssignableTo(typeof(IQueryable));
        }
        public bool IsClass(Type type)
        {
            return !type.IsGenericType && type.IsClass && type != typeof(string);
        }
        protected virtual void ConfigSchema(SchemaProvider<TSchemaContext> provider)
        {
            FieldBuilder?.Invoke(new(_services, provider));
            foreach (var field in GetDbContexType().SelectMany(x => provider.Type(x).GetFields()))
            {
                if (IsClass(field.ReturnType.TypeDotnet))
                {
                    field
                    .UseExpressionFilter();
                }
                else if (IsDbSet(field.ReturnType.TypeDotnet))
                {
                    field
                   .UseExpressionFilter()
                   .UseFilter()
                   .UseOffsetPaging();
                }
            }
        }
        internal void Build()
        {
            _services.AddGraphQLSchema<TSchemaContext>(provider =>
            {
                provider.ConfigureSchema = ConfigSchema;
            });
            _services.AddSingleton(p => p.GetService<SchemaProvider<TSchemaContext>>().AuthorizationService);
        }
    }
}

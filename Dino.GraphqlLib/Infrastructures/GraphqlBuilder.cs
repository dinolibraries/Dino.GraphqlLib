using Dino.GraphqlLib.Extensions;
using Dino.GraphqlLib.Extensions.SortDefault;
using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace Dino.GraphqlLib.Infrastructures
{
    public class GraphqlBuilder<TSchemaContext>
        where TSchemaContext : ISchemaContext
    {
        private readonly IServiceCollection _services;
        public GraphqlBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IExpressionFilterCollection AddFilterExpression<TDbContext>()
        {
            return AddFilterExpression(typeof(TDbContext));
        }
        public IExpressionFilterCollection AddFilterExpression(Type DbContextType)
        {
            AddDbContextType(DbContextType);
            if (DbContexType == null) throw new Exception(nameof(DbContexType) + " can not null!");
            return new GraphqlFilterBuilder(_services);
        }
        public Action<GraphqlFieldBuilder<TSchemaContext>> FieldBuilder { get; set; }
        public Action<GraphqlFieldBuilder<TSchemaContext>> AffterFieldBuilder { get; set; }
        public Action<GraphqlMutationBuilder<TSchemaContext>> MutationBuilder { get; set; }
     
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
        private IDictionary<Type, LambdaExpression> sortExpresses = new Dictionary<Type, LambdaExpression>();
        public GraphqlBuilder<TSchemaContext> ExtractSort<TElementType>(Expression<Func<TElementType, object>> expression)
        {
            sortExpresses.Add(typeof(TElementType), expression);
            return this;
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
                   .UseFilter()
                   .UseExpressionFilter();

                    var typeField = field.ReturnType.TypeDotnet.GenericTypeArguments.First();
                    if (sortExpresses.TryGetValue(typeField, out var expres))
                    {
                        field.AddExtension(new SortExtension(expres, false));
                    }
                    else
                    {
                        field.UseSort();
                    }

                    field.UseRandomWithSeed()
                    .AddExtension(new SortDefaultExtension())
                   .UseOffsetPaging();
                }
            }
            AffterFieldBuilder?.Invoke(new(_services, provider));

            var tt = provider.ToGraphQLSchemaString();
        }
        private Action<AddGraphQLOptions<TSchemaContext>> optionExtend;
        public GraphqlBuilder<TSchemaContext> AddOptionConfig(Action<AddGraphQLOptions<TSchemaContext>> configure)
        {
            optionExtend = configure;
            return this;
        }
        private void ValidateSchemaContext()
        {
            var typeProps = GetDbContexType().SelectMany(x => x.GetProperties().Where(x => IsDbSet(x.PropertyType)));

            var type = typeProps.GroupBy(x => x.PropertyType).FirstOrDefault(x => x.Count() > 1);

            if (type != null)
            {
                throw new InvalidDataException($"SchemaContext has {type.Key} is duplicate!");
            }

        }
        internal void Build()
        {
            ValidateSchemaContext();
            _services.AddGraphQLSchema<TSchemaContext>(provider =>
            {
                optionExtend?.Invoke(provider);
                provider.ConfigureSchema = ConfigSchema;
            });
        }
    }
}

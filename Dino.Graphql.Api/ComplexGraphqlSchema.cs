using Dino.Graphql.Api.DbContexts;
using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.Schema;

namespace Dino.Graphql.Api
{
    public class ComplexGraphqlSchema : ISchemaContext
    {
        public ComplexGraphqlSchema(Graph1DbContext graph1DbContext, Graph2DbContext graph2DbContext, IServiceProvider provider)
        {
            Graph1DbContext = graph1DbContext;
            Graph2DbContext = graph2DbContext;
            Provider = provider;
        }
        public Graph1DbContext Graph1DbContext { get; set; }
        public Graph2DbContext Graph2DbContext { get; set; }
        [GraphQLIgnore]
        public IServiceProvider Provider { get ; set; }
    }
}

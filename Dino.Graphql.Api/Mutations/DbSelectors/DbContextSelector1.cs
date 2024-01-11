using Dino.GraphqlLib.Mutations;

namespace Dino.Graphql.Api.Mutations.DbSelectors
{
    public class DbContextSelector1 : IContextSelector<ComplexGraphqlSchema>
    {
        public object GetDbContext(ComplexGraphqlSchema context)
        {
            return context.Graph1DbContext;
        }
    }
}

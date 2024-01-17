using Dino.Graphql.Api.DbContexts;
using Dino.GraphqlLib.Mutations;

namespace Dino.Graphql.Api.Mutations.ModelViews
{
    public class SubjectModels : ModelBase<Subject>
    {
        public class Create : CreateBase
        {
            public string Name { get; set; }
        }
        public class Update : UpdateBase
        {
            public string Name { get; set; }
        }
        public class Key : KeyBase
        {
            public Guid Id { get; set; }
        }
    }
}

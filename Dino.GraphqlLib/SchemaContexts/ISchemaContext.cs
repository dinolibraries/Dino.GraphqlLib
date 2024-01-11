using EntityGraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.SchemaContexts
{
    public interface ISchemaContext
    {
        [GraphQLIgnore]
        IServiceProvider Provider { get; set; }
    }
}

using Dino.GraphqlLib.SchemaContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public class ContextSelectorDefault<TContext> : IContextSelector<TContext>
        where TContext : ISchemaContext
    {
        public object GetDbContext(TContext context)
        {
            return context;
        }
    }
}

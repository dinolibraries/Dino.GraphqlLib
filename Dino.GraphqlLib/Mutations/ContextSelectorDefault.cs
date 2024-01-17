using Dino.GraphqlLib.SchemaContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public class ContextSelectorDefault<TContext> : IContextSelector<TContext>
        where TContext : ISchemaContext
    {
        public Expression<Func<TContext, TModel>> FirstExpression<TModel>() where TModel : class
        {
            throw new NotImplementedException();
        }

        public object GetDbContext(TContext context)
        {
            return context;
        }
    }
}

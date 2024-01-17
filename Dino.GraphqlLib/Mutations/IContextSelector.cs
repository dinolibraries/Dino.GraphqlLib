using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    public interface IContextSelector<TContext>
    {
        object GetDbContext(TContext context);
    }
}

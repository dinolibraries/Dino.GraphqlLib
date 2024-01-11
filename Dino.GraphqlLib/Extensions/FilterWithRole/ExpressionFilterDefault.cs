using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Extensions.FilterWithRole
{
    public class ExpressionFilterDefault<TModel> : IExpressionFilter<TModel> where TModel : class
    {
        public ExpressionFilterDefault(Expression<Func<TModel, bool>> expression)
        {
            Expression = expression;
        }

        public Expression<Func<TModel, bool>> Expression { get; set; }

        public Expression GetExpression(Expression expression)
        {
            var whereExpress = new WhereClauseExpression(Expression);
            return whereExpress.Visit(expression);
        }
    }
}

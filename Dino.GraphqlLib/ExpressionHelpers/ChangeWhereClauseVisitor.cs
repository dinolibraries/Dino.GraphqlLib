using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.ExpressionHelpers
{
    public class ChangeWhereClauseVisitor : ExpressionVisitor
    {
        public ChangeWhereClauseVisitor()
        { }
        public ChangeWhereClauseVisitor(Expression selectorExpres)
        {
            CustomerSelector = selectorExpres;
        }
        private Expression _SelectorExpres;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Where))
            {
                _SelectorExpres = node.Arguments[1];
            }
            return base.VisitMethodCall(node);
        }
        public Expression CustomerSelector { get; set; }

        [return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            return node == _SelectorExpres ? CustomerSelector : base.Visit(node);
        }
    }
}

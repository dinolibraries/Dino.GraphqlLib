using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Dino.GraphqlLib.Extensions.FilterWithRole
{
    public class WhereClauseExpression : ExpressionVisitor
    {
        public WhereClauseExpression()
        {
        }
        public WhereClauseExpression(Expression expression)
        {
            WhereClause = expression;
        }
        public bool IsQueryAble(Type type)
        {
            return type.IsGenericType && type.IsAssignableTo(typeof(IQueryable));
        }

        public Expression WhereClause { get; set; }
        protected override Expression VisitMember(MemberExpression node)
        {
            if (WhereClause != null && WhereClause is LambdaExpression lamdaWhere && lamdaWhere.Parameters.FirstOrDefault()?.Type == node.Type.GenericTypeArguments.FirstOrDefault() && IsQueryAble(node.Type))
            {
                var expres = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { node.Type.GenericTypeArguments[0] }, node, WhereClause);
                return expres;
            }
            return base.VisitMember(node);
        }
        [return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            if (WhereClause == null) return node;
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }
    }
}

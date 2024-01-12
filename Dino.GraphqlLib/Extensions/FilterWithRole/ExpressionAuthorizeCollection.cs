using Dino.GraphqlLib.Mutations;
using EntityGraphQL.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Extensions.FilterWithRole
{
    public interface IWhereClauseAuthorizeCollection<TModel>
        where TModel : class
    {
        /// <summary>
        ///Any roles
        /// </summary>
        /// <param name="Roles"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        IWhereClauseAuthorizeCollection<TModel> AddRoles(IEnumerable<List<string>> Roles, Expression<Func<TModel, bool>> expression);
        /// <summary>
        /// Any roles
        /// </summary>
        /// <param name="Roles"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        IWhereClauseAuthorizeCollection<TModel> AddRoles(IEnumerable<string> Roles, Expression<Func<TModel, bool>> expression);
        IWhereClauseAuthorizeCollection<TModel> AddRoles(Action<RequiredAuthorization> action, Expression<Func<TModel, bool>> expression);
        IWhereClauseAuthorizeCollection<TModel> AddRoles(RequiredAuthorization requiredAuthorization, Expression<Func<TModel, bool>> expression);
    }
    public class ExpressionAuthorizeCollection<TModel> : IWhereClauseAuthorizeCollection<TModel>
        where TModel : class
    {
        public ExpressionAuthorizeCollection()
        {
            MapExpression = new MapExpression<TModel>();
        }

        private readonly MapExpression<TModel> MapExpression;

        public IWhereClauseAuthorizeCollection<TModel> AddRoles(RequiredAuthorization requiredAuthorization, Expression<Func<TModel, bool>> expression)
        {
            MapExpression.Add(requiredAuthorization, expression);
            return this;
        }
        public WhereClauseAuthorized<TModel> GetService(IServiceProvider serviceProvider)
        {
            var instance = ActivatorUtilities.CreateInstance<WhereClauseAuthorized<TModel>>(serviceProvider, MapExpression);
            return instance;
        }
        public IWhereClauseAuthorizeCollection<TModel> AddRoles(Action<RequiredAuthorization> action, Expression<Func<TModel, bool>> expression)
        {
            var requirdAuth = new RequiredAuthorization();
            action(requirdAuth);
            MapExpression.Add(requirdAuth, expression);
            return this;
        }
        public IWhereClauseAuthorizeCollection<TModel> AddRoles(IEnumerable<List<string>> roles, Expression<Func<TModel, bool>> expression)
        {
            MapExpression.Add(new RequiredAuthorization(roles, null), expression);
            return this;
        }
        public IWhereClauseAuthorizeCollection<TModel> AddRoles(IEnumerable<string> Roles, Expression<Func<TModel, bool>> expression)
        {
            return AddRoles(new[] { Roles.ToList() }, expression);
        }

    }
}

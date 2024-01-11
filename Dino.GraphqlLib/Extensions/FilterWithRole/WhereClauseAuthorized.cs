using EntityGraphQL.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Extensions.FilterWithRole
{
    public class MapExpression<TModel> : Dictionary<RequiredAuthorization, Expression<Func<TModel, bool>>>
    {

    }
    public class WhereClauseAuthorized<TModel> : IExpressionFilter<TModel>
        where TModel : class
    {
        private readonly IGqlAuthorizationService _authorizationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _contextAccessor;
        private ILogger<WhereClauseAuthorized<TModel>> _logger;
        private readonly MapExpression<TModel> MapExpression;
        public WhereClauseAuthorized(
            IGqlAuthorizationService gqlAuthorizationService,
            IServiceProvider serviceProvider,
            MapExpression<TModel> keyValuePairs,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _authorizationService = gqlAuthorizationService;
            _serviceProvider = serviceProvider;
            _contextAccessor = httpContextAccessor;
            _logger = _serviceProvider.GetService<ILogger<WhereClauseAuthorized<TModel>>>();
            MapExpression = keyValuePairs;
        }

        public Expression GetExpression(Expression expression)
        {
            var visitExpres = new WhereClauseExpression(GetWhere());
            return visitExpres.Visit(expression);
        }

        public Expression<Func<TModel, bool>> GetWhere()
        {
            if (_contextAccessor.HttpContext == null)
            {
                _logger?.LogWarning("HttpContext is null!");
                return x => false;
            }

            var data = MapExpression.FirstOrDefault(x => _authorizationService.IsAuthorized(_contextAccessor.HttpContext.User, x.Key));

            if (data.Key == null)
            {
                _logger?.LogWarning("No match role in maprole!");
                return x => false;
            }

            return data.Value;
        }
    }
}

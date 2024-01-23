using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.Mutations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public interface IExpressionFilterCollection
    {
        IExpressionFilterCollection AddWhereClause<TWhereClause, TModel>() where TModel : class where TWhereClause : class, IExpressionFilter<TModel>;
        IExpressionFilterCollection AddWhereClause<TModel>(Func<IServiceProvider, Expression<Func<TModel, bool>>> action) where TModel : class;
        IExpressionFilterCollection AddAuthorizeWhereClauseDefault();
        IExpressionFilterCollection AddAuthorizeWhereClause<TModel>(Action<IWhereClauseAuthorizeCollection<TModel>> config) where TModel : class;
        IExpressionFilterCollection AddSiteClaimTransformation<T>() where T : CallbackAttachSite;
        IExpressionFilterCollection AddSiteRoleTransformation(Func<HttpContext, IEnumerable<string>> Action);
    }

    public class GraphqlFilterBuilder : IExpressionFilterCollection
    {
        public GraphqlFilterBuilder(IServiceCollection serviceDescriptors)
        {
            Services = serviceDescriptors;
        }
        //Service
        public IServiceCollection Services { get; set; }

        public IExpressionFilterCollection AddWhereClause<TWhereClause, TModel>()
           where TModel : class
           where TWhereClause : class, IExpressionFilter<TModel>
        {
            Services.AddScoped<IExpressionFilter<TModel>, TWhereClause>();
            return this;
        }
        public IExpressionFilterCollection AddWhereClause<TModel>(Expression<Func<TModel, bool>> expression)
           where TModel : class
        {
            Services.AddScoped<IExpressionFilter<TModel>>(x => ActivatorUtilities.CreateInstance<ExpressionFilterDefault<TModel>>(x, expression));
            return this;
        }
        public IExpressionFilterCollection AddWhereClause<TModel>(Func<IServiceProvider, Expression<Func<TModel, bool>>> action)
           where TModel : class
        {
            Services.AddScoped<IExpressionFilter<TModel>>(x => ActivatorUtilities.CreateInstance<ExpressionFilterDefault<TModel>>(x, action(x)));
            return this;
        }
        public IExpressionFilterCollection AddAuthorizeWhereClause<TModel>(Action<IWhereClauseAuthorizeCollection<TModel>> config) where TModel : class
        {
            var builder = new ExpressionAuthorizeCollection<TModel>();

            Services.AddScoped<IExpressionFilter<TModel>>(p =>
            {
                config(builder);
                return builder.GetService(p);
            });
            return this;
        }
        /// <summary>
        /// All fields without defined roles (AddRoles) will not be denied access.
        /// </summary>
        /// <returns></returns>
        public IExpressionFilterCollection AddAuthorizeWhereClauseDefault()
        {
            Services.AddScoped(typeof(IExpressionFilter<>), typeof(WhereClauseAuthorized<>));
            return this;
        }
        public IExpressionFilterCollection AddSiteClaimTransformation<T>() where T : CallbackAttachSite
        {
            Services.AddTransient<IClaimsTransformation, AttachSiteToClaimsTransformation>();
            Services.AddTransient<CallbackAttachSite, T>();
            return this;
        }

        public IExpressionFilterCollection AddSiteRoleTransformation(Func<HttpContext, IEnumerable<string>> Action)
        {
            Services.AddTransient<IClaimsTransformation, AttachSiteToClaimsTransformation>();
            Services.AddTransient(p => new CallbackAttachSite() { GetSite = Action });
            return this;
        }
    }

}

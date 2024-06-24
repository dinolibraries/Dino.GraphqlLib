using Dino.Graphql.Api;
using Dino.Graphql.Api.DbContexts;
using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.Infrastructures;
using Dino.GraphqlLib.Mutations;
using Dino.GraphqlLib.Utilities;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Tests
{
    internal static class ServiceHelper
    {
        public static bool IsDbSet(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DbSet<>);
        }
        public static bool IsClass(this Type type)
        {
            return !type.IsGenericType && type.IsClass && type != typeof(string);
        }

        public static IServiceCollection GetServiceCollectionBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.AddScoped<ComplexGraphqlSchema>();
            services.AddDbContext<Graph1DbContext>(opt => opt.UseInMemoryDatabase("Demo1"));
            services.AddDbContext<Graph2DbContext>(opt => opt.UseInMemoryDatabase("Demo2"));
            return services;
        }
        public const string GraphqlSiteHeaderKey = "graphql-site";
        public static string GetGraphqlSite(this HttpContext httpContext)
        {
            return httpContext.Request.Headers.TryGetValue(GraphqlSiteHeaderKey, out var value) ? value.ToString() : null;
        }
        public static IServiceCollection GetServiceCollection(Action<GraphqlBuilder<ComplexGraphqlSchema>> action)
        {
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            services.AddLogging();
            services.AddScoped<ComplexGraphqlSchema>();
            services.AddDbContext<Graph1DbContext>(opt => opt.UseInMemoryDatabase("Demo1"));
            services.AddDbContext<Graph2DbContext>(opt => opt.UseInMemoryDatabase("Demo2"));
            services.AddGraphql<ComplexGraphqlSchema>(builder =>
            {
                action(builder);
            });
            return services;
        }
        public class HttpContextOption
        {
            public string[] Roles { get; set; }
            public string Site { get; set; }
        }
        public static HttpContext GetHttpContext(IServiceProvider serviceProvider, HttpContextOption httpContextOption)
        {
            var httpcontext = new DefaultHttpContext() { RequestServices = serviceProvider };

            // User is logged in
            httpcontext.User = new GenericPrincipal(
               new GenericIdentity("username"),
              httpContextOption.Roles
            );

            if (!string.IsNullOrEmpty(httpContextOption.Site))
            {
                var identity = httpcontext.User.Identity as ClaimsIdentity;
                httpcontext.User.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(identity.RoleClaimType, httpContextOption.Site.GetRoleSite()) }));
            }

            //// User is logged out
            //httpcontext.User = new GenericPrincipal(
            //    new GenericIdentity(String.Empty),
            //    );
            return httpcontext;
        }
    }
}

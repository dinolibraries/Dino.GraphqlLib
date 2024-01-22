using Dino.Graphql.Api;
using Dino.Graphql.Api.DbContexts;
using Dino.GraphqlLib.Extensions.FilterWithRole;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema;
using EntityGraphQL.Schema.FieldExtensions;
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
        public static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();

            services.AddScoped<ComplexGraphqlSchema>();
            services.AddDbContext<Graph1DbContext>(opt => opt.UseInMemoryDatabase("Demo1"));
            services.AddDbContext<Graph2DbContext>(opt => opt.UseInMemoryDatabase("Demo2"));
            services.AddGraphQLSchema<ComplexGraphqlSchema>(provider =>
            {
                provider.ConfigureSchema = (option) =>
                {
                    var fields = option.Type<Graph1DbContext>()
                    .GetFields()
                    .Union(option.Type<Graph2DbContext>().GetFields()).ToList();

                    var filedDbset = fields.Where(x => IsDbSet(x.ReturnType.TypeDotnet)).ToList();
                    foreach (var field in filedDbset)
                    {
                        field
                        .AddExtension(new ExpressionFilterFieldExtension())
                        .UseFilter()
                        .UseOffsetPaging();
                    }

                    var fieldClass = fields.Where(x => IsClass(x.ReturnType.TypeDotnet)).ToList();
                    foreach (var field in fieldClass)
                    {
                        field
                        .AddExtension(new ExpressionFilterFieldExtension())
                        ;
                    }

                    var test = option.ToGraphQLSchemaString();
                };
            });
            services.AddSingleton(p => p.GetService<SchemaProvider<ComplexGraphqlSchema>>().AuthorizationService);
            return services;
        }
        public class HttpContextOption
        {
            public string[] Roles { get; set; }
            public string Site { get; set; }
        }
        public static HttpContext GetHttpContext(IServiceProvider serviceProvider,HttpContextOption httpContextOption)
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
                httpcontext.User.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(identity.RoleClaimType, httpContextOption.Site) }));
            }

            //// User is logged out
            //httpcontext.User = new GenericPrincipal(
            //    new GenericIdentity(String.Empty),
            //    );
            return httpcontext;
        }
    }
}

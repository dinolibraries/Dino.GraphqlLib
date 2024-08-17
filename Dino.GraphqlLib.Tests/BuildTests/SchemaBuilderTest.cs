using Dino.Graphql.Api;
using Dino.Graphql.Api.DbContexts;
using Dino.GraphqlLib.Extensions.FilterWithRole;
using EntityGraphQL;
using EntityGraphQL.Schema;
using HotChocolate.Language;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Dino.GraphqlLib.Infrastructures;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data;
using Dino.GraphqlLib.Utilities;
using System.Linq.Expressions;
using Dino.Graphql.Api.ExpressionHelpers;
using static Dino.GraphqlLib.Tests.Builds.SchemaBuilderTest;
using Dino.GraphqlLib.Tests.BuildTests;
namespace Dino.GraphqlLib.Tests.Builds
{
    public class SchemaBuilderTest : BuildBase
    {
        [Theory]
        [InlineData(Queryhelper.SubjectQuery)]
        [InlineData(Queryhelper.SubjectPageQuery)]
        public void FieldExtension(string query)
        {
            var provider = SetupSercvice();
            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption { });

            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = query;
            var result = schemaProvider.ExecuteRequest(graphqlRequest, provider, null);
            Assert.False(result.Errors?.Any() ?? false);
        }

        [Theory]
        [InlineData(Queryhelper.SubjectQuery)]
        [InlineData(Queryhelper.SubjectPageQuery)]
        public void HttpContextAuthorized(string query)
        {
            var provider = SetupSercvice();
            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption { });

            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = query;
            var result = schemaProvider.ExecuteRequest(graphqlRequest, provider, httpAccessor.HttpContext.User);
            Assert.False(result.Errors?.Any() ?? false);
        }

        [Theory]
        //[InlineData(Queryhelper.SubjectPageQuery, new[] { RoleHelper.Admin, RoleHelper.User }, "", new[] { RoleHelper.Admin, RoleHelper.User }, null)]
        [InlineData(Queryhelper.SubjectPageQuery, new[] { RoleHelper.Admin }, "hello4", new[] { RoleHelper.Admin, RoleHelper.User }, null)]
        [InlineData(Queryhelper.SubjectPageQuery, new[] { RoleHelper.Admin, RoleHelper.User }, "hello4", new[] { RoleHelper.Admin, RoleHelper.User }, "hello4")]
        [InlineData(Queryhelper.SubjectPageQuery, new[] { RoleHelper.Admin, RoleHelper.User }, "hello4", new string[] { }, "hello4")]
        [InlineData(Queryhelper.SubjectPageQuery, new string[] { }, "hello4", new[] { RoleHelper.Admin, RoleHelper.User }, null)]
        [InlineData(Queryhelper.SubjectPageQuery, new[] { RoleHelper.Admin }, "hello4", new[] { RoleHelper.Admin }, "hello4")]
        [InlineData(Queryhelper.SubjectPageQuery, new[] { RoleHelper.Manage }, "hello4", new[] { RoleHelper.Manage }, "hello4")]
        [InlineData(Queryhelper.SubjectPageQuery, new string[] { }, "hello4", new[] { RoleHelper.Manage }, null)]
        public void AuthorizedExpression(string query, string[] roles, string name, string[] requiredRoles, string resultValue)
        {

            var provider = SetupSercvice(new ServiceOption
            {
                Name = name,
                Roles = requiredRoles
            });
            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption
            {
                Roles = roles
            });
            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = query;
            var result = schemaProvider.ExecuteRequest(graphqlRequest, provider, httpAccessor.HttpContext.User);

            if (resultValue == null)
            {
                var value = result.Errors[0].FirstOrDefault() as dynamic;
                var temp = value?.Value as string;
                Assert.True(temp?.Contains("Resource access denied!"));
            }
            else
            {
                var value = result.Data.Values.FirstOrDefault() as dynamic;
                var temp = Enumerable.FirstOrDefault(value?.subjects?.items)?.name as string;
                Assert.True(temp == resultValue);
            }
        }
        public string Gettest(string query)
        {
            return query;
        }
        [Theory]
        [InlineData(new[] { RoleHelper.Admin }, new[] { RoleHelper.User }, new[] { "hello1" }, null)]
        [InlineData(new[] { RoleHelper.Admin }, new[] { RoleHelper.User, RoleHelper.Admin }, new[] { "hello1", "hello2" }, "hello2")]
        [InlineData(new[] { RoleHelper.User }, new[] { RoleHelper.User, RoleHelper.Admin }, new[] { "hello1", "hello2" }, "hello1")]
        [InlineData(new[] { RoleHelper.User, RoleHelper.Admin }, new[] { RoleHelper.User, RoleHelper.Admin }, new[] { "hello1", "hello2" }, "hello1")]
        [InlineData(new[] { RoleHelper.User, RoleHelper.Admin }, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { "hello1", "hello2" }, "hello1")]
        [InlineData(new[] { RoleHelper.Manage }, new[] { RoleHelper.User, RoleHelper.Admin }, new[] { "hello1", "hello2" }, null)]
        public void GraphqlBuilder(string[] roles, string[] releRequireds, string[] condition, string resultValue)
        {
            var services = ServiceHelper.GetServiceCollectionBase();

            services.AddGraphql<ComplexGraphqlSchema>(builder =>
            {
                builder.AddFilterExpression<DbContext>()
                .AddAuthorizeWhereClause<Subject>((opt) =>
                {
                    for (int i = 0; i < releRequireds.Length; i++)
                    {
                        var value = condition[i];
                        opt.AddRoles(new string[] { releRequireds[i] }, p => x => x.Name == value);
                    }
                });
            });

            var provider = services.BuildServiceProvider();

            InitialDbContext(provider);

            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption
            {
                Roles = roles
            });

            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.SubjectPageQuery;
            var result = schemaProvider.ExecuteRequest(graphqlRequest, provider, httpAccessor.HttpContext.User);
            if (resultValue == null)
            {
                var value = result.Errors[0].FirstOrDefault() as dynamic;
                var temp = value?.Value as string;
                Assert.True(temp?.Contains("Resource access denied!"));
            }
            else
            {
                var value = result.Data.Values.FirstOrDefault() as dynamic;
                var temp = Enumerable.FirstOrDefault(value?.subjects?.items)?.name as string;
                Assert.True(temp == resultValue);
            }
        }

        [Theory]
        [InlineData(null, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.User }, "hello1", "hello1")]
        [InlineData(null, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.Admin }, "hello1", "hello1")]
        [InlineData(RoleHelper.AdminSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.Admin, RoleHelper.UserSite + "-graphql-site" }, "hello1", null)]
        [InlineData(RoleHelper.UserSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.Admin, RoleHelper.UserSite + "-graphql-site" }, "hello1", "hello1")]
        [InlineData(RoleHelper.UserSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.User, RoleHelper.UserSite + "-graphql-site" }, "hello1", "hello1")]
        public void AttachSite(string Site, string[] roles, string[] releRequireds, string condition, string resultValue)
        {
            var services = ServiceHelper.GetServiceCollection(builder =>
            {
                builder.AddFilterExpression<DbContext>()
                .AddSiteRoleTransformation(x =>
                {
                    if (string.IsNullOrEmpty(Site))
                    {
                        return new string[] { };

                    }
                    return new[] { Site };
                })
                .AddAuthorizeWhereClause<Subject>((opt) =>
                {
                    var value = condition;
                    opt.AddRoles(roles => roles.RequiresAllRoles(releRequireds.Select(x => x.ToLower()).ToArray()), p => x => x.Name == value);
                });

                builder.AddOptionConfig(option =>
                {
                    option.PreBuildSchemaFromContext = (context) =>
                    {
                        context.AddScalarType<TimeSpan>("TimeSpan");
                    };
                });
            });

            var provider = services.BuildServiceProvider();

            InitialDbContext(provider);

            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption
            {
                Roles = roles,
                Site = Site,
            });

            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.SubjectPageQuery;

            var result = schemaProvider.ExecuteRequest(graphqlRequest, provider, httpAccessor.HttpContext.User);

            if (resultValue == null)
            {
                var value = result.Errors[0].FirstOrDefault() as dynamic;
                var temp = value?.Value as string;
                Assert.True(temp?.Contains("Resource access denied!"));
            }
            else
            {
                var value = result.Data.Values.FirstOrDefault() as dynamic;
                var temp = Enumerable.FirstOrDefault(value?.subjects?.items)?.name as string;
                Assert.True(temp == resultValue);
            }
        }

        private string[][] RequiedRoles = new[] {
            new[] { "hello3", RoleHelper.Admin, RoleHelper.UserSite.GetRoleSite() },
            new[] { "hello4", RoleHelper.Admin, RoleHelper.AdminSite.GetRoleSite() },
            new[] { "hello5", RoleHelper.User, RoleHelper.UserSite.GetRoleSite() },
            new[] { "hello1", RoleHelper.User } ,
            new[] { "hello2", RoleHelper.Admin },
        };

        [Theory]
        [InlineData(RoleHelper.AdminSite, new[] { RoleHelper.Admin, RoleHelper.User }, "hello4")]
        [InlineData(RoleHelper.TestSite, new[] { RoleHelper.Admin }, "hello2")]
        [InlineData(RoleHelper.UserSite, new[] { RoleHelper.Admin }, "hello3")]
        [InlineData(RoleHelper.UserSite, new[] { RoleHelper.User }, "hello5")]
        [InlineData(RoleHelper.TestSite, new[] { RoleHelper.Manage }, null)]
        public void AttachSite2(string Site, string[] roles, string resultValue)
        {
            var services = ServiceHelper.GetServiceCollectionBase();

            services.AddGraphql<ComplexGraphqlSchema>(builder =>
            {
                builder.AddFilterExpression<DbContext>()
                .AddSiteRoleTransformation(x =>
                {
                    return new[] { Site };
                })
                .AddAuthorizeWhereClause<Subject>((opt) =>
                {
                    foreach (var role in RequiedRoles)
                    {
                        var value = role[0];
                        opt.AddRoles(roles => roles.RequiresAllRoles(role.Skip(1).ToArray()), p => x => x.Name == value);
                    }
                });
            });

            var provider = services.BuildServiceProvider();

            InitialDbContext(provider);

            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption
            {
                Roles = roles,
                Site = Site,
            });

            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.SubjectPageQuery;
            var result = schemaProvider.ExecuteRequest(graphqlRequest, provider, httpAccessor.HttpContext.User);

            if (resultValue == null)
            {
                var value = result.Errors[0].FirstOrDefault() as dynamic;
                var temp = value?.Value as string;
                Assert.True(temp?.Contains("Resource access denied!"));
            }
            else
            {
                var value = result.Data.Values.FirstOrDefault() as dynamic;
                var temp = Enumerable.FirstOrDefault(value?.subjects?.items)?.name as string;
                Assert.True(temp == resultValue);
            }
        }
    }
}

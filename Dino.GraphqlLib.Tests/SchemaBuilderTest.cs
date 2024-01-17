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
namespace Dino.GraphqlLib.Tests
{
    public class SchemaBuilderTest
    {
        public SchemaBuilderTest()
        {
        }

        public class ServiceOption
        {
            public string[] Roles { get; set; } = new string[0];
            public string Name { get; set; }
        }
        private IServiceProvider SetupSercvice(ServiceOption serviceOption)
        {
            ExpressionFilterCollection.Clear();
            var services = ServiceHelper.GetServiceCollection();
            ExpressionFilterCollection.Instance.SetupService(services);

            ExpressionFilterCollection.Instance
              .AddAuthorizeWhereClause<Subject>((provider, option) =>
              {
                  option.AddRoles(requird => requird.RequiresAllRoles(serviceOption.Roles), x => x.Name == serviceOption.Name);
                  option.AddRoles(requird => requird.RequiresAllRoles(RoleHelper.Manage), x => x.Name == serviceOption.Name);
              });

            services.AddHttpContextAccessor();

            var provider = services.BuildServiceProvider();
            ExpressionFilterCollection.Instance.SetupProvider(provider);
            InitialDbContext(provider);
            return provider;
        }
        private void InitialDbContext(IServiceProvider provider)
        {
            var _context1 = provider.GetService<Graph1DbContext>();
            var _context2 = provider.GetService<Graph2DbContext>();
            if (!_context1.Subjects.Any())
            {
                _context1.Subjects.AddRange(new[] {
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13375"),
                        Name = "hello1"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("1ac14418-0775-4d44-b888-70f32be73c79"),
                        Name = "hello2"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13376"),
                        Name = "hello3"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13377"),
                        Name = "hello4"
                    },
                    new Subject
                    {
                        Id = Guid.Parse("8889ba81-c75b-4c07-91ca-513760f13378"),
                        Name = "hello5"
                    }
                });
                _context1.SaveChanges();

            }
        }

        [Theory]
        [InlineData(Queryhelper.SubjectQuery)]
        [InlineData(Queryhelper.SubjectPageQuery)]
        public void FieldExtension(string query)
        {
            var provider = SetupSercvice(new ServiceOption { });
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
            var provider = SetupSercvice(new ServiceOption { });
            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(new ServiceHelper.HttpContextOption { });

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
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(new ServiceHelper.HttpContextOption
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
                .AddAuthorizeWhereClause<Subject>((p, opt) =>
                {
                    for (int i = 0; i < releRequireds.Length; i++)
                    {
                        var value = condition[i];
                        opt.AddRoles(new string[] { releRequireds[i] }, x => x.Name == value);
                    }
                });
            });

            var provider = services.BuildServiceProvider();
            provider.UseGraphql();

            InitialDbContext(provider);

            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(new ServiceHelper.HttpContextOption
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
        [InlineData(RoleHelper.AdminSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.User }, "hello1", "hello1")]
        [InlineData(RoleHelper.AdminSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.Admin }, "hello1", "hello1")]
        [InlineData(RoleHelper.AdminSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.Admin, RoleHelper.UserSite }, "hello1", null)]
        [InlineData(RoleHelper.UserSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.Admin, RoleHelper.UserSite }, "hello1", "hello1")]
        [InlineData(RoleHelper.UserSite, new[] { RoleHelper.Admin, RoleHelper.User }, new[] { RoleHelper.User, RoleHelper.UserSite }, "hello1", "hello1")]
        public void AttachSite(string Site, string[] roles, string[] releRequireds, string condition, string resultValue)
        {
            var services = ServiceHelper.GetServiceCollectionBase();

            services.AddGraphql<ComplexGraphqlSchema>(builder =>
            {
                builder.AddFilterExpression<DbContext>()
                .AddSiteRoleTransformation(x =>
                {
                    return new[] { Site };
                })
                .AddAuthorizeWhereClause<Subject>((p, opt) =>
                {
                    var value = condition;
                    opt.AddRoles(roles => roles.RequiresAllRoles(releRequireds), x => x.Name == value);
                });
            });

            var provider = services.BuildServiceProvider();
            provider.UseGraphql();

            InitialDbContext(provider);

            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(new ServiceHelper.HttpContextOption
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
            new[] { "hello3", RoleHelper.Admin, RoleHelper.UserSite },
            new[] { "hello4", RoleHelper.Admin, RoleHelper.AdminSite },
            new[] { "hello5", RoleHelper.User, RoleHelper.UserSite },
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
                .AddAuthorizeWhereClause<Subject>((p, opt) =>
                {
                    foreach (var role in RequiedRoles)
                    {
                        var value = role[0];
                        opt.AddRoles(roles => roles.RequiresAllRoles(role.Skip(1).ToArray()), x => x.Name == value);
                    }
                });
            });

            var provider = services.BuildServiceProvider();
            provider.UseGraphql();

            InitialDbContext(provider);

            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(new ServiceHelper.HttpContextOption
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

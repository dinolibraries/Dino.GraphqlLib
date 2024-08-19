using Dino.Graphql.Api;
using EntityGraphQL.Schema;
using EntityGraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Dino.GraphqlLib.Tests.BuildTests
{
    public class QueryTest : BuildBase
    {
        [Fact]
        public async Task DoubleTest()
        {
            var provider = SetupSercvice();
            //var httpAccessor = provider.GetService<IHttpContextAccessor>();
            //httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption { });
            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.DoubleQuery;
            var result = await schemaProvider.ExecuteRequestAsync(graphqlRequest, provider, null);
            Assert.False(result.Errors?.Any() ?? false);
        }
        [Fact]
        public async Task GuidTest()
        {
            var provider = SetupSercvice();
            //var httpAccessor = provider.GetService<IHttpContextAccessor>();
            //httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption { });
            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.GuidQuery;
            var result = await schemaProvider.ExecuteRequestAsync(graphqlRequest, provider, null);
            Assert.False(result.Errors?.Any() ?? false);
        }
        [Fact]
        public async Task CacheTest()
        {
            var provider = SetupSercvice(new ServiceOption
            {
                Name = "hello1",
                Roles = new string[] { RoleHelper.User }
            });
            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();

            graphqlRequest.Query = Queryhelper.SubjectListQuery;

            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption
            {
                Roles = new string[] { RoleHelper.User }
            });
            var result = await schemaProvider.ExecuteRequestAsync(graphqlRequest, provider, httpAccessor.HttpContext.User);

            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption
            {
                Roles = new string[] { RoleHelper.Manage }
            });

            var result2 = await schemaProvider.ExecuteRequestAsync(graphqlRequest, provider, httpAccessor.HttpContext.User);

            var value1 = result.Data.Values.FirstOrDefault() as dynamic;
            var temp1 = value1?.subjects?.totalItems as int?;

            var value2 = result2.Data.Values.FirstOrDefault() as dynamic;
            var temp2 = value2?.subjects?.totalItems as int?;

            Assert.True(temp1 == 1 && temp2 == 0);
        }

        [Fact]
        public async Task ComplexFilterTest()
        {
            var provider = SetupSercvice();
            //var httpAccessor = provider.GetService<IHttpContextAccessor>();
            //httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption { });
            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.ComplexFilterQuery;
            var result = await schemaProvider.ExecuteRequestAsync(graphqlRequest, provider, null);
            Assert.False(result.Errors?.Any() ?? false);
        }
    }
}

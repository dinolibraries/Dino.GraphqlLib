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
            var httpAccessor = provider.GetService<IHttpContextAccessor>();
            httpAccessor.HttpContext = ServiceHelper.GetHttpContext(provider, new ServiceHelper.HttpContextOption { });
            var schemaProvider = provider.GetService<SchemaProvider<ComplexGraphqlSchema>>();
            var graphqlRequest = new QueryRequest();
            graphqlRequest.Query = Queryhelper.DoubleQuery;
            var result = await schemaProvider.ExecuteRequestAsync(graphqlRequest, provider, null);
        }
    }
}

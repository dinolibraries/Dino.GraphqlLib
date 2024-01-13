using EntityGraphQL.AspNet;
using EntityGraphQL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dino.GraphqlLib.Controllers
{
    public class GraphqlContentResult : IActionResult
    {
        private readonly QueryResult _results;
        public GraphqlContentResult(QueryResult keyValuePairs)
        {
            _results = keyValuePairs;
        }
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await context.HttpContext.RequestServices
               .GetService<IGraphQLResponseSerializer>().SerializeAsync(context.HttpContext.Response.Body, _results);
        }
    }
}

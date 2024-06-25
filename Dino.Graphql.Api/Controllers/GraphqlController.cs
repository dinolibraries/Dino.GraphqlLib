using Dino.Graphql.Api;
using EntityGraphQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Controllers
{
    public class GraphqlController:GrapqlControllerBase
    {
        public GraphqlController() { }

        [HttpPost]
        [Route("grql/graphql")]
        public async Task<IActionResult> SchemaAsync([FromBody] QueryRequest queryRequest)
        {
            return await TryOk(() => RequestGraphql<ComplexGraphqlSchema>(queryRequest));
        }
    }
}

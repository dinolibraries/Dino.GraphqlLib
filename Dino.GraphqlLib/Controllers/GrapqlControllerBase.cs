using EntityGraphQL.Schema;
using EntityGraphQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dino.GraphqlLib.Controllers
{
    public abstract class GrapqlControllerBase : ControllerBase
    {
        protected async Task<IActionResult> TryOk(Func<Task<IActionResult>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return BadRequest(ModelState);
            }
        }
        protected virtual Task<IActionResult> RequestGraphql<TContext>(QueryRequest query)
        {
            return RequestGraphql<TContext>(query, null);
        }
        protected virtual async Task<IActionResult> RequestGraphql<TContext>(QueryRequest query, ExecutionOptions executionOptions)
        {
            var context = HttpContext.RequestServices.GetService<TContext>();
            var schemeProvider = HttpContext.RequestServices.GetService<SchemaProvider<TContext>>();
            var results = await schemeProvider.ExecuteRequestWithContextAsync(query, context, HttpContext.RequestServices, HttpContext.User, executionOptions);
            var errors = results.Errors;
            if (errors != null && errors!.Count > 0)
            {
                return BadRequest(errors);
            }
            else
            {
                return new GraphqlContentResult(results);
            }
        }
    }
}

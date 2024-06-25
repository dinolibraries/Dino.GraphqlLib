using EntityGraphQL.Schema;
using EntityGraphQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Dino.GraphqlLib.Loggers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dino.GraphqlLib.Controllers
{
    public abstract class GrapqlControllerBase : ControllerBase
    {
        public GrapqlControllerBase(ILogger logger = null)
        {
            _logger = logger;
        }
        protected ILogger _logger;
        protected ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = HttpContext.RequestServices.GetService<ILogger<GrapqlControllerBase>>();
                }
                return _logger;
            }
        }
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
                var collectionEx = HttpContext.RequestServices.GetService<ExceptionCollection>();
                if (collectionEx != null && collectionEx.Exceptions.Any())
                {
                    foreach (var item in collectionEx.Exceptions)
                    {
                        ErrorHandling(item);
                    }
                }
                foreach (var item in errors)
                {
                    var content = string.Join("-", item.Message?.Split("-").Skip(1))?.Trim();

                    if (!ModelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage)).Any(u => u == content))
                    {
                        ModelState.AddModelError("All", item.Message);
                    }
                }

                return BadRequest(ModelState);
            }
            else
            {
                return new GraphqlContentResult(results);
            }
        }
        protected virtual void ErrorHandling(Exception ex)
        {
            var handling = HttpContext.RequestServices.GetService<IApiControllerErrorHandling>();
            if (handling == null)
            {
                handling = ActivatorUtilities.CreateInstance<ApiControllerErrorHandlingDefault>(HttpContext.RequestServices, ModelState, Logger);
            }
            handling?.ErrorHandling(ex);
        }
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Loggers
{
    public class ApiControllerErrorHandlingDefault : IApiControllerErrorHandling
    {
        private readonly ILogger _logger;
        public ApiControllerErrorHandlingDefault(ModelStateDictionary modelState, ILogger logger)
        {
            ModelState = modelState;
            _logger = logger;
        }
        public ModelStateDictionary ModelState { get; }
        private IEnumerable<Exception> ErrorTravels(Exception ex)
        {
            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }

        public void ErrorHandling(Exception ex)
        {
            var error = false;
            foreach (var item in ErrorTravels(ex))
            {
                if (item is GraphqlLoggerException model)
                {
                    ModelState?.AddModelError(model.KeyName, model.Message);
                    error = true;
                }

                if (item is GraphqlLoggerExceptions models)
                {
                    foreach (var it in models.Messages)
                    {
                        ModelState?.AddModelError(it.Item1, it.Item2);
                    }
                    error = true;
                }
                _logger?.LogError(ex, ex.Message);
            }
            if (!error)
            {
                ModelState?.AddModelError(string.Empty, "Error from server!");
            }
        }
    }
}

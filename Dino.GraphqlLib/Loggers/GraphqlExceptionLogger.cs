using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Loggers
{
    public class ExceptionCollection
    {
        public ExceptionCollection()
        {
            Exceptions = new List<Exception>();
        }
        public IList<Exception> Exceptions { get; set; }
    }
    internal class GraphqlExceptionLogger : ILogger
    {
        private readonly string _name;
        private readonly IServiceProvider _serviceProvider;

        public GraphqlExceptionLogger(IServiceProvider serviceProvider)
        {
            _name = typeof(GraphqlExceptionLogger).Name;
            _serviceProvider = serviceProvider;
        }
        public HttpContext HttpContext { get => _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext; }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return HttpContext != null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || exception == null)
            {
                return;
            }

            var collection = HttpContext.RequestServices.GetService<ExceptionCollection>();
            collection.Exceptions.Add(exception);
        }
    }
}

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
    internal class GraphqlExceptionProvider : ILoggerProvider
    {
        private readonly IServiceProvider _provider;

        public GraphqlExceptionProvider(IServiceProvider serviceProvider)
        {
            _provider = serviceProvider;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new GraphqlExceptionLogger(_provider);
        }
        public void Dispose()
        {
            // Dispose resources if any
        }
    }
}

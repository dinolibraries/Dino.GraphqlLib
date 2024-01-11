using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.SchemaContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public static class ConfigurationExtension
    {
        public static IServiceCollection AddGraphql<TSchemaContext>(this IServiceCollection services, Action<GraphqlBuilder<TSchemaContext>> action)
            where TSchemaContext : class, ISchemaContext
        {
            services.AddScoped<TSchemaContext>();
            var builder = new GraphqlBuilder<TSchemaContext>(services);
            action(builder);
            builder.Build();
            return services;
        }
        public static IApplicationBuilder UseGraphql(this IApplicationBuilder services)
        {
            ExpressionFilterCollection.Instance.SetupProvider(services.ApplicationServices.CreateScope().ServiceProvider);
            return services;
        }

        public static IServiceProvider UseGraphql(this IServiceProvider provider)
        {
            ExpressionFilterCollection.Instance.SetupProvider(provider);
            return provider;
        }
    }
}

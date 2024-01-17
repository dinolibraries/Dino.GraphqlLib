using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.SchemaContexts;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Infrastructures
{
    public static class ConfigurationExtension
    {
        internal static IServiceCollection AddGraphQLSchema<TSchemaContext>(this IServiceCollection serviceCollection, Action<AddGraphQLOptions<TSchemaContext>> configure)
        {
            serviceCollection.TryAddSingleton((IGraphQLRequestDeserializer)new DefaultGraphQLRequestDeserializer());
            serviceCollection.TryAddSingleton((IGraphQLResponseSerializer)new DefaultGraphQLResponseSerializer());
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            IAuthorizationService service = provider.GetService<IAuthorizationService>();
            IWebHostEnvironment service2 = provider.GetService<IWebHostEnvironment>();
            ILogger<SchemaProvider<TSchemaContext>> logger = provider.GetService<ILogger<SchemaProvider<TSchemaContext>>>();
            AddGraphQLOptions<TSchemaContext> addGraphQLOptions = new AddGraphQLOptions<TSchemaContext>();
            configure(addGraphQLOptions);
            SchemaProvider<TSchemaContext> schemaProvider = new SchemaProvider<TSchemaContext>(new PolicyOrRoleBasedAuthorization(service), addGraphQLOptions.FieldNamer, logger, introspectionEnabled: true, service2?.IsEnvironment("Development") ?? true);
            addGraphQLOptions.PreBuildSchemaFromContext?.Invoke(schemaProvider);
            if (addGraphQLOptions.AutoBuildSchemaFromContext)
            {
                schemaProvider.PopulateFromContext(addGraphQLOptions);
            }

            addGraphQLOptions.ConfigureSchema?.Invoke(schemaProvider);
            serviceCollection.AddSingleton(schemaProvider);
            return serviceCollection;
        }
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

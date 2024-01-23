using Dino.GraphqlLib.Authorizations;
using Dino.GraphqlLib.Extensions.FilterWithRole;
using Dino.GraphqlLib.SchemaContexts;
using Dino.GraphqlLib.SchemaProviders;
using EntityGraphQL.AspNet;
using EntityGraphQL.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            serviceCollection.TryAddSingleton<PolicyOrRoleBasedAuthorization>();
            serviceCollection.TryAddSingleton<AuthorizationServiceBase>();

            var provider = serviceCollection.BuildServiceProvider();

            AddGraphQLOptions<TSchemaContext> addGraphQLOptions = new AddGraphQLOptions<TSchemaContext>();

            configure(addGraphQLOptions);

            var schema = ActivatorUtilities.CreateInstance<CustomSchemaProvider<TSchemaContext>>(provider, addGraphQLOptions.FieldNamer, provider.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? true);
            addGraphQLOptions.PreBuildSchemaFromContext?.Invoke(schema);

            if (addGraphQLOptions.AutoBuildSchemaFromContext)
            {
                schema.PopulateFromContext(addGraphQLOptions);
            }

            addGraphQLOptions.ConfigureSchema?.Invoke(schema);

            serviceCollection.TryAddSingleton<SchemaProvider<TSchemaContext>>(schema);

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
    }
}

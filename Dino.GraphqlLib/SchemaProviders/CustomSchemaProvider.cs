using Dino.GraphqlLib.Authorizations;
using Dino.GraphqlLib.Mutations;
using EntityGraphQL.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.SchemaProviders
{
    internal class CustomSchemaProvider<TContext> : SchemaProvider<TContext>
    {
        public CustomSchemaProvider(AuthorizationServiceBase authorizationService, Func<string, string> fieldNamer, ILogger<SchemaProvider<TContext>> logger, bool isDevelopment) : base(authorizationService, fieldNamer, logger, true, isDevelopment)
        {
            OvverideMutationType();
        }

        private void OvverideMutationType()
        {
            var prop =  typeof(SchemaProvider<TContext>).GetField("mutationType", BindingFlags.NonPublic | BindingFlags.Instance);
            prop.SetValue(this, new CustomMutationType(this, "Mutation", null, null));
        }

    }
}

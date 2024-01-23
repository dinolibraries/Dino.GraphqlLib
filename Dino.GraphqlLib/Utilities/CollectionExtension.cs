using Dino.GraphqlLib.Authorizations;
using EntityGraphQL.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Utilities
{
    public static class CollectionExtension
    {
        public static bool IsDbSet(this Type type, Type dbSetType)
        {
            return dbSetType.IsGenericType && dbSetType.GetGenericTypeDefinition() == type;
        }
        public static bool IsQueryabe(this Type type, Type queryable)
        {
            return queryable.IsAssignableFrom(type);
        }
        public static TService GetService<TService>(this IField field)
        {
            var provider = (field.Schema.AuthorizationService as AuthorizationServiceBase).ServiceProvider;
            return provider.GetService<TService>();
        }
    }
}

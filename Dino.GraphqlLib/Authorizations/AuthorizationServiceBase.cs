using EntityGraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Authorizations
{
    public class AuthorizationServiceBase : RoleBasedAuthorization
    {
        public IServiceProvider ServiceProvider { get;  }
        public AuthorizationServiceBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}

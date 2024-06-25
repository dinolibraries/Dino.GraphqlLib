using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Loggers
{
    /// <summary>
    /// addition    services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    /// </summary>
    public interface IApiControllerErrorHandling
    {
        void ErrorHandling(Exception ex);
    }
}

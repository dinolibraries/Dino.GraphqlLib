using Dino.GraphqlLib.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Mutations
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/claims?view=aspnetcore-7.0#extend-or-add-custom-claims-using-iclaimstransformation
    /// </summary>
    internal class AttachSiteToClaimsTransformation : IClaimsTransformation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        public AttachSiteToClaimsTransformation(IServiceProvider serviceProvider, IHttpContextAccessor httpContext)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContext;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {

            var _callbackAttachSite = _serviceProvider.GetService<CallbackAttachSite>();
            var value = _callbackAttachSite?.GetSite(_httpContextAccessor.HttpContext);
            if (value != null && value.Any())
            {
                if (principal.Identity is ClaimsIdentity claimsIdentity1)
                {
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claimsIdentity1);
                    foreach (var claim in value)
                    {
                        claimsIdentity.AddClaim(new Claim(claimsIdentity1.RoleClaimType, SiteHelper.GetRoleSite(claim)));
                    }
                    principal.AddIdentity(claimsIdentity);
                }
            }
            return Task.FromResult(principal);
        }
    }
}

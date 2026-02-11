using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace Manager
{
    public class MultiTenantManagerMiddleware(
        ITenantSetter setter) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string? tenantId = null;

            if (context.Request.Headers.TryGetValue(ConstantTenants.SelectedTenantId, out var headerTenant) &&
                !string.IsNullOrEmpty(headerTenant.FirstOrDefault()))
            {
                tenantId = headerTenant.FirstOrDefault();
            }
            else
            {
                tenantId = ConstantTenants.Demo;
            }

            setter.SetTenant(tenantId);

            await next(context);
        }
    }
}

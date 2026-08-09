using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;
using SchoolManagement.DAL.TenantContext;

namespace SchoolManagement.API.Filters;

/// <summary>
/// Ensures the request resolved a tenant schema via X-Tenant-ID (per-school data isolation).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireTenantAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenant = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        if (string.IsNullOrEmpty(tenant.SchemaName))
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Fail(
                    $"{AppConstants.TenantHeaderName} header is required (school slug). Each school has separate data."));
            return;
        }

        await next();
    }
}

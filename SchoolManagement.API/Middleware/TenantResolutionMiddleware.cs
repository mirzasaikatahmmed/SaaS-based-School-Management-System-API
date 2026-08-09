using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;

namespace SchoolManagement.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    private static readonly HashSet<string> BypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/favicon.ico",
        "/iclock",
        "/cron_api"
    };

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantRepository tenantRepository)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (BypassPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // JWT may already indicate super admin — still allow optional tenant header
        if (context.Request.Headers.TryGetValue(AppConstants.TenantHeaderName, out var tenantHeader))
        {
            var slug = tenantHeader.ToString().Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(slug))
            {
                var tenant = await tenantRepository.GetBySlugAsync(slug, context.RequestAborted);
                if (tenant is null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = $"Tenant '{slug}' not found.",
                        data = (object?)null,
                        errors = (object?)null,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (!tenant.IsActive)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = $"Tenant '{slug}' is inactive.",
                        data = (object?)null,
                        errors = (object?)null,
                        timestamp = DateTime.UtcNow
                    });
                    return;
                }

                tenantContext.SetTenant(tenant.Id, tenant.Slug, tenant.SchemaName, tenant.Name);
                _logger.LogDebug("Resolved tenant {Slug} -> schema {Schema}", tenant.Slug, tenant.SchemaName);
            }
        }

        await _next(context);
    }
}

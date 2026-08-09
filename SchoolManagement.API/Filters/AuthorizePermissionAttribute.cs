using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using System.Security.Claims;

namespace SchoolManagement.API.Filters;

/// <summary>
/// Checks the caller's role permission matrix for the given feature + action
/// (View / Add / Edit / Delete). SuperAdmin always passes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string FeatureKey { get; }
    public string Action { get; }

    public AuthorizePermissionAttribute(string featureKey, string action)
    {
        FeatureKey = featureKey;
        Action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roles = user.FindAll("role").Concat(user.FindAll(ClaimTypes.Role))
            .Select(c => c.Value).ToList();

        if (roles.Any(r => r.Equals(AppConstants.Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)))
            return;

        var userIdRaw = user.FindFirst(AppConstants.Claims.UserId)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var allowed = await permissionService.HasPermissionAsync(
            userId, roles, FeatureKey, Action, context.HttpContext.RequestAborted);

        if (!allowed)
        {
            context.Result = new ObjectResult(new
            {
                success = false,
                message = $"Permission denied for {FeatureKey}:{Action}.",
                data = (object?)null,
                errors = (object?)null,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

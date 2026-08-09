using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Services;
using SchoolManagement.Common.Constants;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Controllers;

[ApiController]
[Route("api/settings/login-log")]
[Authorize]
public class UserLoginLogController(IUserLoginLogService service) : ControllerBase
{
    [HttpGet]
    [AuthorizePermission("Settings.LoginLog", AppConstants.PermissionActions.View)]
    public async Task<IActionResult> Get(
        [FromQuery] string? type,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? export = null,
        CancellationToken ct = default)
    {
        var result = await service.GetAsync(type, search, page, pageSize, export, ct);
        if (string.Equals(export, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = UserLoginLogService.ToCsv(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "login-log.csv");
        }

        return Ok(ApiResponse<LoginLogListDto>.Ok(result, "Login log retrieved"));
    }

    [HttpDelete("clear")]
    [AuthorizePermission("Settings.LoginLog", AppConstants.PermissionActions.Delete)]
    public async Task<IActionResult> Clear(CancellationToken ct = default)
    {
        await service.ClearAsync(ct);
        return Ok(ApiResponse.Ok("Login log cleared"));
    }
}

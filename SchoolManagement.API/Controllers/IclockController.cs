using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.BLL.Interfaces;

namespace SchoolManagement.API.Controllers;

/// <summary>
/// ZKTeco ADMS (push protocol) endpoint for K40-H devices. The route MUST stay exactly
/// "/iclock/..." — it is hardcoded in device firmware and cannot be changed on the device side.
/// Anonymous, plain-text only: the device firmware cannot send JWTs or parse JSON.
/// </summary>
[ApiController]
[Route("iclock")]
[AllowAnonymous]
public class IclockController(IZkTecoAdmsService adms) : ControllerBase
{
    [HttpGet("cdata")]
    public async Task<IActionResult> CdataGet(
        [FromQuery(Name = "SN")] string? sn,
        [FromQuery] string? options,
        CancellationToken ct = default)
    {
        var response = await adms.HandleCdataGetAsync(sn ?? string.Empty, options, ct);
        return Content(response, "text/plain");
    }

    [HttpPost("cdata")]
    public async Task<IActionResult> CdataPost(
        [FromQuery(Name = "SN")] string? sn,
        [FromQuery] string? table,
        [FromQuery] string? stamp,
        CancellationToken ct = default)
    {
        var body = await ReadBodyAsync(ct);
        var response = await adms.HandleCdataPostAsync(sn ?? string.Empty, table, stamp, body, ct);
        return Content(response, "text/plain");
    }

    [HttpGet("getrequest")]
    public async Task<IActionResult> GetRequest([FromQuery(Name = "SN")] string? sn, CancellationToken ct = default)
    {
        var response = await adms.HandleGetRequestAsync(sn ?? string.Empty, ct);
        return Content(response, "text/plain");
    }

    [HttpPost("devicecmd")]
    public async Task<IActionResult> DeviceCmd([FromQuery(Name = "SN")] string? sn, CancellationToken ct = default)
    {
        var body = await ReadBodyAsync(ct);
        var response = await adms.HandleDeviceCmdAsync(sn ?? string.Empty, body, ct);
        return Content(response, "text/plain");
    }

    [HttpGet("registry")]
    [HttpPost("registry")]
    public async Task<IActionResult> Registry([FromQuery(Name = "SN")] string? sn, CancellationToken ct = default)
    {
        var response = await adms.HandleRegistryAsync(sn ?? string.Empty, ct);
        return Content(response, "text/plain");
    }

    private async Task<string> ReadBodyAsync(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        return await reader.ReadToEndAsync(ct);
    }
}

namespace SchoolManagement.BLL.Interfaces;

/// <summary>
/// Implements the ZKTeco ADMS (push protocol) surface used by K40-H devices under /iclock/*.
/// All methods are best-effort: unknown/unregistered devices never fail the request — the
/// device must always get a response it understands ("OK" or an option block) so it keeps
/// pushing data instead of retrying indefinitely.
/// </summary>
public interface IZkTecoAdmsService
{
    Task<string> HandleCdataGetAsync(string serialNumber, string? options, CancellationToken cancellationToken = default);

    Task<string> HandleCdataPostAsync(
        string serialNumber, string? table, string? stamp, string body, CancellationToken cancellationToken = default);

    Task<string> HandleGetRequestAsync(string serialNumber, CancellationToken cancellationToken = default);

    Task<string> HandleDeviceCmdAsync(string serialNumber, string body, CancellationToken cancellationToken = default);

    Task<string> HandleRegistryAsync(string serialNumber, CancellationToken cancellationToken = default);
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolManagement.API.Filters;

/// <summary>
/// Appends a live endpoint count (and per-method breakdown) to the Swagger document title/description.
/// </summary>
public sealed class EndpointCountDocumentFilter : IDocumentFilter
{
    private const string BaseDescription =
        "SaaS School Management System — Authentication & Multi-Tenancy";

    private static readonly string[] MethodOrder =
        ["GET", "POST", "PATCH", "DELETE", "PUT", "HEAD", "OPTIONS"];

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var total = 0;

        foreach (var pathItem in swaggerDoc.Paths.Values)
        {
            foreach (var operation in pathItem.Operations)
            {
                total++;
                var method = operation.Key.ToString().ToUpperInvariant();
                counts[method] = counts.GetValueOrDefault(method) + 1;
            }
        }

        var ordered = MethodOrder
            .Where(counts.ContainsKey)
            .Select(m => $"{m}: {counts[m]}")
            .Concat(
                counts.Keys
                    .Except(MethodOrder, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(m => m, StringComparer.OrdinalIgnoreCase)
                    .Select(m => $"{m}: {counts[m]}"));

        var breakdown = string.Join(" · ", ordered);

        swaggerDoc.Info.Title = $"School Management System API — {total} endpoints";
        swaggerDoc.Info.Description =
            $"{BaseDescription}\n\n**Total endpoints: {total}**  \n{breakdown}";
    }
}

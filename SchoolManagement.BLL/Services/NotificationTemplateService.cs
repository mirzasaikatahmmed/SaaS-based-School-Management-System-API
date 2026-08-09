using System.Text.RegularExpressions;
using SchoolManagement.BLL.Interfaces;

namespace SchoolManagement.BLL.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private static readonly Regex Placeholder = new(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

    public string Render(string template, IReadOnlyDictionary<string, string?> data)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        return Placeholder.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            if (data.TryGetValue(key, out var value) && value is not null)
                return value;
            // also allow case-insensitive lookup
            foreach (var kv in data)
            {
                if (kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && kv.Value is not null)
                    return kv.Value;
            }
            return m.Value;
        });
    }
}

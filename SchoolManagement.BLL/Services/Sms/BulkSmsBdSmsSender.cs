using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Services.Sms;

public record SmsSendResult(bool Success, string RawResponse, string Message, string? Code = null);

public record SmsBalanceResult(bool Success, string RawResponse, string Message, decimal? Balance = null);

/// <summary>
/// BulkSMSBD.net gateway — single SMS via <c>/api/smsapi</c>, batch via <c>/api/smsapimany</c>,
/// balance via <c>/api/getBalanceApi</c>.
/// Credentials JSON keys: <c>api_key</c> (or <c>ApiKey</c>), <c>senderid</c> (or <c>SenderId</c>).
/// </summary>
public class BulkSmsBdSmsSender : ISmsSender
{
    public const string HttpClientName = "BulkSmsBd";
    private const string BaseHost = "http://bulksmsbd.net";

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _senderId;
    private readonly ILogger _logger;

    public BulkSmsBdSmsSender(HttpClient http, string apiKey, string senderId, ILogger logger)
    {
        _http = http;
        _apiKey = apiKey;
        _senderId = senderId;
        _logger = logger;
    }

    public async Task SendAsync(string to, string body, CancellationToken cancellationToken = default)
    {
        var result = await SendOneAsync(to, body, cancellationToken);
        if (!result.Success)
            throw new AppException($"BulkSMSBD send failed: {result.Message}", 400);
    }

    public async Task<SmsSendResult> SendOneAsync(string to, string body, CancellationToken cancellationToken = default)
    {
        var number = NormalizeNumber(to);
        if (string.IsNullOrWhiteSpace(number))
            return new SmsSendResult(false, string.Empty, "Recipient number is required.", "1001");
        if (string.IsNullOrWhiteSpace(body))
            return new SmsSendResult(false, string.Empty, "Message body is required.", "1003");
        if (body.Contains('\''))
            body = body.Replace('\'', '’'); // single quotes often cause forbidden results

        // Prefer GET as shown in BulkSMSBD docs; message is URL-encoded.
        var url =
            $"{BaseHost}/api/smsapi?api_key={Uri.EscapeDataString(_apiKey)}" +
            $"&type=text" +
            $"&number={Uri.EscapeDataString(number)}" +
            $"&senderid={Uri.EscapeDataString(_senderId)}" +
            $"&message={Uri.EscapeDataString(body)}";

        try
        {
            using var response = await _http.GetAsync(url, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            return InterpretSubmitResponse(raw, response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BulkSMSBD single-send failed to {Number}", number);
            return new SmsSendResult(false, ex.Message, $"BulkSMSBD request failed: {ex.Message}");
        }
    }

    public async Task<SmsSendResult> SendManyAsync(
        IReadOnlyList<(string To, string Body)> messages,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
            return new SmsSendResult(false, string.Empty, "At least one message is required.", "1003");

        var payload = new BulkSmsBdManyRequest
        {
            ApiKey = _apiKey,
            SenderId = _senderId,
            Messages = messages
                .Select(m => new BulkSmsBdManyItem
                {
                    To = NormalizeNumber(m.To),
                    Message = m.Body?.Replace('\'', '’') ?? string.Empty
                })
                .Where(m => !string.IsNullOrWhiteSpace(m.To) && !string.IsNullOrWhiteSpace(m.Message))
                .ToList()
        };

        if (payload.Messages.Count == 0)
            return new SmsSendResult(false, string.Empty, "No valid recipients/messages after normalization.", "1001");

        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync($"{BaseHost}/api/smsapimany", content, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            return InterpretSubmitResponse(raw, response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BulkSMSBD multi-send failed ({Count} messages)", payload.Messages.Count);
            return new SmsSendResult(false, ex.Message, $"BulkSMSBD multi-send failed: {ex.Message}");
        }
    }

    public async Task<SmsBalanceResult> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{BaseHost}/api/getBalanceApi?api_key={Uri.EscapeDataString(_apiKey)}";
        try
        {
            using var response = await _http.GetAsync(url, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new SmsBalanceResult(false, raw, $"HTTP {(int)response.StatusCode}: {raw}");

            // Response is typically a numeric balance string or JSON with balance/code.
            if (decimal.TryParse(raw.Trim().Trim('"'), out var bal))
                return new SmsBalanceResult(true, raw, "Balance retrieved.", bal);

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("balance", out var b) && b.TryGetDecimal(out var jb))
                    return new SmsBalanceResult(true, raw, "Balance retrieved.", jb);
                if (doc.RootElement.TryGetProperty("Balance", out var b2) && b2.TryGetDecimal(out var jb2))
                    return new SmsBalanceResult(true, raw, "Balance retrieved.", jb2);
                if (doc.RootElement.TryGetProperty("response_code", out var codeEl))
                {
                    var code = codeEl.ToString();
                    if (code != "202" && code != "0")
                        return new SmsBalanceResult(false, raw, ExplainCode(code));
                }
            }
            catch
            {
                // fall through — treat non-empty body as success payload
            }

            return new SmsBalanceResult(true, raw, raw);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BulkSMSBD balance check failed");
            return new SmsBalanceResult(false, ex.Message, $"Balance check failed: {ex.Message}");
        }
    }

    public static (string ApiKey, string SenderId) ParseCredentials(string credentialsJson)
    {
        Dictionary<string, string?> map = new(StringComparer.OrdinalIgnoreCase);
        try
        {
            map = JsonSerializer.Deserialize<Dictionary<string, string?>>(
                      string.IsNullOrWhiteSpace(credentialsJson) ? "{}" : credentialsJson)
                  ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            // ignore malformed JSON
        }

        string? Pick(params string[] keys)
        {
            foreach (var k in keys)
            {
                if (map.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return null;
        }

        var apiKey = Pick("api_key", "ApiKey", "apikey")
            ?? throw new AppException("BulkSMSBD credentials require 'api_key'.", 400);
        var senderId = Pick("senderid", "SenderId", "sender_id")
            ?? throw new AppException("BulkSMSBD credentials require 'senderid'.", 400);
        return (apiKey, senderId);
    }

    /// <summary>Normalizes BD numbers to 8801XXXXXXXXX when possible.</summary>
    public static string NormalizeNumber(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return string.Empty;
        var digits = new string(number.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("880") && digits.Length >= 13) return digits;
        if (digits.StartsWith('0') && digits.Length == 11) return "88" + digits;
        if (digits.Length == 10 && digits.StartsWith('1')) return "880" + digits;
        return digits;
    }

    private static SmsSendResult InterpretSubmitResponse(string raw, bool httpOk)
    {
        var code = TryExtractCode(raw);
        if (code == "202")
            return new SmsSendResult(true, raw, ExplainCode("202"), "202");

        if (!string.IsNullOrEmpty(code) && code != "202")
            return new SmsSendResult(false, raw, ExplainCode(code), code);

        // Some responses are plain "SMS Submitted Successfully"
        if (httpOk && raw.Contains("success", StringComparison.OrdinalIgnoreCase))
            return new SmsSendResult(true, raw, raw.Trim(), "202");

        if (!httpOk)
            return new SmsSendResult(false, raw, string.IsNullOrWhiteSpace(raw) ? "HTTP request failed." : raw.Trim());

        // Ambiguous but HTTP 200 — treat as success with raw body
        return new SmsSendResult(true, raw, string.IsNullOrWhiteSpace(raw) ? "Submitted." : raw.Trim(), code);
    }

    private static string? TryExtractCode(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var trimmed = raw.Trim();
        if (trimmed.All(char.IsDigit)) return trimmed;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            foreach (var name in new[] { "response_code", "responseCode", "code", "error_code" })
            {
                if (doc.RootElement.TryGetProperty(name, out var el))
                    return el.ToString();
            }
        }
        catch { /* not JSON */ }

        return null;
    }

    public static string ExplainCode(string code) => code switch
    {
        "202" => "SMS Submitted Successfully",
        "1001" => "Invalid Number",
        "1002" => "Sender ID not correct / sender ID is disabled",
        "1003" => "Please required all fields / contact your system administrator",
        "1005" => "Internal Error",
        "1006" => "Balance Validity Not Available",
        "1007" => "Balance Insufficient",
        "1011" => "User Id not found",
        "1012" => "Masking SMS must be sent in Bengali",
        "1013" => "Sender Id has not found Gateway by api key",
        "1014" => "Sender Type Name not found using this sender by api key",
        "1015" => "Sender Id has not found Any Valid Gateway by api key",
        "1016" => "Sender Type Name Active Price Info not found by this sender id",
        "1017" => "Sender Type Name Price Info not found by this sender id",
        "1018" => "The Owner of this Account is disabled",
        "1019" => "The Sender Type Name Price of this Account is disabled",
        "1020" => "The parent of this account is not found",
        "1021" => "The parent active Sender Type Name price of this account is not found",
        "1031" => "Your Account Not Verified, Please Contact Administrator",
        "1032" => "IP not whitelisted",
        _ => $"BulkSMSBD response code {code}"
    };

    private sealed class BulkSmsBdManyRequest
    {
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("senderid")]
        public string SenderId { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<BulkSmsBdManyItem> Messages { get; set; } = [];
    }

    private sealed class BulkSmsBdManyItem
    {
        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}

public class LoggingSmsSender(ILogger<LoggingSmsSender> logger) : ISmsSender
{
    public Task SendAsync(string to, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("SMS (stub) → {To}: {Body}", to, body);
        return Task.CompletedTask;
    }
}

public class SmsSenderFactory(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory) : ISmsSenderFactory
{
    public ISmsSender Resolve(string gateway, string credentialsJson)
    {
        var key = gateway?.Trim().ToLowerInvariant() ?? string.Empty;
        if (key is SmsGateways.BulksmsbdNet or "bulksmsbd.net" or "bulksmsbdnet")
        {
            var (apiKey, senderId) = BulkSmsBdSmsSender.ParseCredentials(credentialsJson);
            var client = httpClientFactory.CreateClient(BulkSmsBdSmsSender.HttpClientName);
            return new BulkSmsBdSmsSender(
                client,
                apiKey,
                senderId,
                loggerFactory.CreateLogger<BulkSmsBdSmsSender>());
        }

        return new LoggingSmsSender(loggerFactory.CreateLogger<LoggingSmsSender>());
    }

    public BulkSmsBdSmsSender ResolveBulkSmsBd(string credentialsJson)
    {
        var (apiKey, senderId) = BulkSmsBdSmsSender.ParseCredentials(credentialsJson);
        var client = httpClientFactory.CreateClient(BulkSmsBdSmsSender.HttpClientName);
        return new BulkSmsBdSmsSender(
            client,
            apiKey,
            senderId,
            loggerFactory.CreateLogger<BulkSmsBdSmsSender>());
    }
}

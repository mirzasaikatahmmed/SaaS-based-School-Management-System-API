using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Website;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;

namespace SchoolManagement.BLL.Services;

public partial class SscBoardResultService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<SscBoardResultService> logger) : ISscBoardResultService
{
    public const string HttpClientName = "SscBoardResults";

    private static readonly Dictionary<string, int> FallbackBoards = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DHAKA"] = 10,
        ["CUMILLA"] = 11,
        ["COMILLA"] = 11,
        ["RAJSHAHI"] = 12,
        ["JASHORE"] = 13,
        ["JESSORE"] = 13,
        ["CHATTOGRAM"] = 14,
        ["CHITTAGONG"] = 14,
        ["BARISAL"] = 15,
        ["BARISHAL"] = 15,
        ["SYLHET"] = 16,
        ["DINAJPUR"] = 17,
        ["MADRASAH"] = 18,
        ["TECHNICAL"] = 19,
        ["MYMENSINGH"] = 21,
    };

    private HttpClient Client => httpClientFactory.CreateClient(HttpClientName);

    public async Task<IReadOnlyList<SscBoardOptionDto>> GetBoardsAsync(CancellationToken ct = default)
    {
        var map = await LoadBoardMapAsync(ct);
        return map
            .GroupBy(kv => kv.Value)
            .Select(g => new SscBoardOptionDto
            {
                BoardId = g.Key,
                BoardName = g.Select(x => x.Key).OrderBy(n => n.Length).First(),
            })
            .OrderBy(b => b.BoardName)
            .ToList();
    }

    public async Task<SscBoardCaptchaDto> GetCaptchaAsync(bool tryAutoSolve = false, CancellationToken ct = default)
    {
        var captcha = await FetchCaptchaAsync(ct);
        if (tryAutoSolve)
        {
            captcha.CaptchaText = await TrySolveCaptchaAsync(captcha.ImageBase64, ct);
        }

        return captcha;
    }

    public async Task<SscBoardResultDto> SearchAsync(SscBoardSearchRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RollNo))
            throw new AppException("Roll number is required.");
        if (string.IsNullOrWhiteSpace(request.RegNo))
            throw new AppException("Registration number is required.");
        if (string.IsNullOrWhiteSpace(request.Board))
            throw new AppException("Board is required.");
        if (request.PassYear < 1990 || request.PassYear > DateTime.UtcNow.Year + 1)
            throw new AppException("Pass year is invalid.");

        var boards = await LoadBoardMapAsync(ct);
        var boardId = ResolveBoardId(request.Board, boards);
        var boardName = boards.FirstOrDefault(kv => kv.Value == boardId).Key
                        ?? request.Board.Trim().ToUpperInvariant();

        var hasManualCaptcha = !string.IsNullOrWhiteSpace(request.CaptchaText)
                               && !string.IsNullOrWhiteSpace(request.Hash)
                               && request.Timestamp.HasValue;

        if (!request.AutoSolve && !hasManualCaptcha)
            throw new AppException(
                "Captcha is required. Call /results/ssc-board/captcha first, or set autoSolve=true.");

        var maxAttempts = request.AutoSolve
            ? Math.Clamp(configuration.GetValue("SscBoardResults:AutoSolveRetries", 5), 1, 10)
            : 1;

        string? lastMessage = null;
        SscBoardCaptchaDto? lastCaptcha = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            string captchaText;
            string hash;
            long timestamp;

            if (hasManualCaptcha && attempt == 1 && !request.AutoSolve)
            {
                captchaText = request.CaptchaText!.Trim();
                hash = request.Hash!;
                timestamp = request.Timestamp!.Value;
            }
            else
            {
                lastCaptcha = await FetchCaptchaAsync(ct);
                hash = lastCaptcha.Hash;
                timestamp = lastCaptcha.Timestamp;

                captchaText = (await TrySolveCaptchaAsync(lastCaptcha.ImageBase64, ct))?.Trim() ?? "";
                if (string.IsNullOrEmpty(captchaText))
                {
                    lastMessage = "Could not auto-solve captcha. Enter the captcha manually.";
                    continue;
                }
            }

            var api = await PostResultAsync(
                request.RollNo.Trim(),
                request.RegNo.Trim(),
                boardId,
                request.PassYear,
                captchaText,
                hash,
                timestamp,
                ct);

            if (IsCaptchaError(api))
            {
                lastCaptcha = await FetchCaptchaAsync(ct);
                lastMessage = api.Message ?? "Incorrect or expired captcha. Try again.";
                if (!request.AutoSolve)
                {
                    return new SscBoardResultDto
                    {
                        Roll = request.RollNo.Trim(),
                        Registration = request.RegNo.Trim(),
                        Board = boardName,
                        BoardId = boardId,
                        Year = request.PassYear,
                        ApiStatus = api.Status,
                        ApiMessage = lastMessage,
                        Captcha = lastCaptcha,
                    };
                }

                continue;
            }

            var flattened = Flatten(request, boardName, boardId, api);
            if (flattened.ApiStatus == 200 && string.IsNullOrWhiteSpace(flattened.Name)
                && string.IsNullOrWhiteSpace(flattened.ResultStatus))
            {
                throw new NotFoundException(flattened.ApiMessage ?? "Result not found for the given details.");
            }

            if (flattened.ApiStatus is 210
                || (flattened.ApiMessage?.Contains("not published", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                throw new AppException(flattened.ApiMessage ?? "Result is not published yet.", 404);
            }

            if (flattened.ApiStatus is not null and not 200)
            {
                throw new AppException(
                    flattened.ApiMessage ?? "Could not retrieve SSC result from the board.",
                    flattened.ApiStatus is >= 400 and < 600 ? flattened.ApiStatus.Value : 400);
            }

            return flattened;
        }

        return new SscBoardResultDto
        {
            Roll = request.RollNo.Trim(),
            Registration = request.RegNo.Trim(),
            Board = boardName,
            BoardId = boardId,
            Year = request.PassYear,
            ApiMessage = lastMessage ?? "Captcha verification failed.",
            Captcha = lastCaptcha ?? await FetchCaptchaAsync(ct),
        };
    }

    private async Task<Dictionary<string, int>> LoadBoardMapAsync(CancellationToken ct)
    {
        var boards = new Dictionary<string, int>(FallbackBoards, StringComparer.OrdinalIgnoreCase);
        try
        {
            using var resp = await Client.GetAsync("/api/rescrutiny/getEduBoard", ct);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (doc.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in result.EnumerateArray())
                {
                    var name = item.TryGetProperty("boardName", out var n) ? n.GetString()?.Trim() : null;
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (!item.TryGetProperty("boardId", out var idEl)) continue;
                    var id = idEl.ValueKind == JsonValueKind.Number
                        ? idEl.GetInt32()
                        : int.TryParse(idEl.GetString(), out var parsed) ? parsed : 0;
                    if (id > 0)
                        boards[name.ToUpperInvariant()] = id;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not refresh SSC board list; using built-in map.");
        }

        return boards;
    }

    private static int ResolveBoardId(string board, Dictionary<string, int> boards)
    {
        var value = board.Trim();
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            return id;

        var key = value.ToUpperInvariant();
        if (boards.TryGetValue(key, out var found))
            return found;

        foreach (var (name, boardId) in boards)
        {
            if (key.Contains(name, StringComparison.OrdinalIgnoreCase)
                || name.Contains(key, StringComparison.OrdinalIgnoreCase))
                return boardId;
        }

        throw new AppException(
            $"Unknown board: {board}. Known: {string.Join(", ", boards.Keys.OrderBy(x => x).Distinct())}.");
    }

    private async Task<SscBoardCaptchaDto> FetchCaptchaAsync(CancellationToken ct)
    {
        using var resp = await Client.GetAsync("/v1/captcha/generate-string-image-captcha", ct);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;
        var image = root.TryGetProperty("imageBase64", out var img) ? img.GetString() : null;
        var hash = root.TryGetProperty("hash", out var h) ? h.GetString() : null;
        long? timestamp = null;
        if (root.TryGetProperty("timestamp", out var ts))
        {
            timestamp = ts.ValueKind switch
            {
                JsonValueKind.Number => ts.TryGetInt64(out var n) ? n : null,
                JsonValueKind.String => long.TryParse(ts.GetString(), out var s) ? s : null,
                _ => null,
            };
        }

        if (string.IsNullOrWhiteSpace(image) || string.IsNullOrWhiteSpace(hash) || timestamp is null)
            throw new AppException("Invalid captcha response from education board.", 502);

        return new SscBoardCaptchaDto
        {
            ImageBase64 = image,
            Hash = hash,
            Timestamp = timestamp.Value,
        };
    }

    private async Task<BoardApiResponse> PostResultAsync(
        string roll,
        string reg,
        int boardId,
        int year,
        string captchaText,
        string hash,
        long timestamp,
        CancellationToken ct)
    {
        var payload = new
        {
            rollNo = roll,
            regNo = reg,
            boardId,
            passYear = year,
            captchaText,
            hash,
            timestamp,
        };

        using var resp = await Client.PostAsJsonAsync("/api/rescrutiny/getStudentResult", payload, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode && string.IsNullOrWhiteSpace(body))
            throw new AppException($"Board API error ({(int)resp.StatusCode}).", 502);

        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = doc.RootElement;
            return new BoardApiResponse
            {
                Status = root.TryGetProperty("status", out var st)
                    ? st.ValueKind == JsonValueKind.Number ? st.GetInt32()
                    : int.TryParse(st.GetString(), out var psi) ? psi : null
                    : (int?)resp.StatusCode,
                Message = root.TryGetProperty("message", out var msg) ? msg.GetString()
                    : root.TryGetProperty("error", out var err) ? err.GetString() : null,
                Result = root.TryGetProperty("result", out var result) ? result.Clone() : default,
            };
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse board result JSON");
            throw new AppException("Unexpected response from education board.", 502);
        }
    }

    private static bool IsCaptchaError(BoardApiResponse api)
    {
        var message = api.Message ?? "";
        if (api.Status is 400 or 401 or 403 or 422
            && CaptchaWordRegex().IsMatch(message))
            return true;

        return CaptchaWordRegex().IsMatch(message)
               && CaptchaFailRegex().IsMatch(message);
    }

    private static SscBoardResultDto Flatten(
        SscBoardSearchRequestDto request,
        string boardName,
        int boardId,
        BoardApiResponse api)
    {
        string? name = null, father = null, mother = null, gpa = null, resultStatus = null;
        string? group = null, session = null, studentType = null, institution = null, rawResult = null;
        IReadOnlyList<SscBoardSubjectDto> subjects = [];

        if (api.Result.ValueKind == JsonValueKind.Object)
        {
            var result = api.Result;
            if (result.TryGetProperty("info", out var info) && info.ValueKind == JsonValueKind.Object)
            {
                name = GetString(info, "name");
                father = GetString(info, "fatherName");
                mother = GetString(info, "motherName");
                gpa = GetStringOrNumber(info, "gpa");
                resultStatus = GetString(info, "result");
                group = GetString(info, "groupName");
                session = GetString(info, "session");
                studentType = GetString(info, "type");
                institution = ResolveInstitution(info);
            }

            if (result.TryGetProperty("result", out var resultField))
            {
                rawResult = resultField.ValueKind == JsonValueKind.String
                    ? resultField.GetString()
                    : resultField.GetRawText();
                subjects = ParseSubjects(resultField);
            }
        }

        return new SscBoardResultDto
        {
            Roll = request.RollNo.Trim(),
            Registration = request.RegNo.Trim(),
            Board = boardName,
            BoardId = boardId,
            Year = request.PassYear,
            ApiStatus = api.Status,
            ApiMessage = api.Message,
            Name = name,
            FatherName = father,
            MotherName = mother,
            Gpa = gpa,
            ResultStatus = resultStatus,
            GroupName = group,
            Session = session,
            StudentType = studentType,
            Institution = institution,
            Subjects = subjects,
            RawResult = rawResult,
        };
    }

    private static string? ResolveInstitution(JsonElement info)
    {
        foreach (var key in new[] { "institutionName", "instituteName" })
        {
            var v = GetString(info, key);
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        if (!info.TryGetProperty("institution", out var institution)) return null;
        if (institution.ValueKind == JsonValueKind.String)
            return institution.GetString()?.Trim();
        if (institution.ValueKind == JsonValueKind.Object)
        {
            return GetString(institution, "name") ?? GetString(institution, "institutionName");
        }

        return null;
    }

    private static IReadOnlyList<SscBoardSubjectDto> ParseSubjects(JsonElement resultField)
    {
        if (resultField.ValueKind == JsonValueKind.Array)
        {
            var list = new List<SscBoardSubjectDto>();
            foreach (var item in resultField.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                list.Add(new SscBoardSubjectDto
                {
                    SubjectCode = GetString(item, "subjectCode") ?? GetString(item, "code") ?? "",
                    SubjectName = GetString(item, "subjectName") ?? GetString(item, "name") ?? "",
                    Grade = (GetString(item, "grade") ?? "").ToUpperInvariant(),
                    Mark = GetStringOrNumber(item, "mark") ?? GetStringOrNumber(item, "marks"),
                });
            }

            return list;
        }

        if (resultField.ValueKind != JsonValueKind.String)
            return [];

        var text = resultField.GetString();
        if (string.IsNullOrWhiteSpace(text)) return [];

        const string gradePat = @"A\+|A-|A|B|C|D|F|X";
        var splitter = new Regex($@",\s*(?=\d+(?:-\d+)*\s+(?:{gradePat})\s+)", RegexOptions.IgnoreCase);
        var rowRe = new Regex($@"^(\d+(?:-\d+)*)\s+({gradePat})\s+(.+)$", RegexOptions.IgnoreCase);
        var markRe = new Regex(@"^(.*?)\s+(-?\d+(?:\.\d+)?)$");

        var subjects = new List<SscBoardSubjectDto>();
        foreach (var part in splitter.Split(text.Trim()))
        {
            var match = rowRe.Match(part.Trim());
            if (!match.Success) continue;
            var code = match.Groups[1].Value;
            var grade = match.Groups[2].Value.ToUpperInvariant();
            var rest = match.Groups[3].Value.Trim();
            var markMatch = markRe.Match(rest);
            subjects.Add(new SscBoardSubjectDto
            {
                SubjectCode = code,
                Grade = grade,
                SubjectName = markMatch.Success ? markMatch.Groups[1].Value.Trim() : rest,
                Mark = markMatch.Success ? markMatch.Groups[2].Value : null,
            });
        }

        return subjects;
    }

    private async Task<string?> TrySolveCaptchaAsync(string imageBase64, CancellationToken ct)
    {
        var enabled = configuration.GetValue("SscBoardResults:EnableAutoCaptcha", true);
        if (!enabled) return null;

        var python = configuration.GetValue<string>("SscBoardResults:PythonPath") ?? "python3";
        var script = """
import base64,sys
try:
    import ddddocr
except ImportError:
    sys.exit(2)
raw=base64.b64decode(sys.stdin.read().strip())
ocr=ddddocr.DdddOcr(show_ad=False)
print(ocr.classification(raw).strip(), end="")
""";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = python,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add(script);
            using var proc = Process.Start(psi);
            if (proc is null) return null;

            await proc.StandardInput.WriteAsync(imageBase64.AsMemory(), ct);
            await proc.StandardInput.FlushAsync(ct);
            proc.StandardInput.Close();

            var stdout = await proc.StandardOutput.ReadToEndAsync(ct);
            var stderr = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
            {
                logger.LogDebug("Auto captcha solver exit {Code}: {Err}", proc.ExitCode, stderr.Trim());
                return null;
            }

            var text = stdout.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Auto captcha solver unavailable");
            return null;
        }
    }

    private static string? GetString(JsonElement el, string name)
        => el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString()?.Trim()
            : null;

    private static string? GetStringOrNumber(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString()?.Trim(),
            JsonValueKind.Number => p.ToString(),
            _ => null,
        };
    }

    [GeneratedRegex("captcha", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CaptchaWordRegex();

    [GeneratedRegex(
        "incorrect|invalid|mismatch|does not match|did not match|wrong|expired|verification failed",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CaptchaFailRegex();

    private sealed class BoardApiResponse
    {
        public int? Status { get; init; }
        public string? Message { get; init; }
        public JsonElement Result { get; init; }
    }
}

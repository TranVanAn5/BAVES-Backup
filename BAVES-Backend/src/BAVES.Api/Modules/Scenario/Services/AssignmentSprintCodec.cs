using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BAVES.Api.Modules.Scenario.Models.DTOs;

namespace BAVES.Api.Modules.Scenario.Services;

/// <summary>
/// Lưu sprint trong cột Assignment (JSON). Hạn sprint = duration days/weeks tương đối
/// (sprint 1 từ lúc bắt đầu mô phỏng; sprint sau = sau khi sprint trước kết thúc).
/// </summary>
public static class AssignmentSprintCodec
{
    public const string SchemaV1 = "baves.sprints.v1";
    public const int DefaultDurationValue = 1;
    public const string DefaultDurationUnit = "weeks";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private static readonly Regex HeaderRegex = new(
        @"\bSprint\s*(\d+)\s*" +
        @"(?:\|\s*(?:Ends:\s*(\d{4}-\d{2}-\d{2}(?:[T ]\d{2}:\d{2}(?::\d{2})?)?)|Duration:\s*(\d+)\s*([dw]|days?|weeks?))?)?\s*" +
        @":\s*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex DeliverablesRegex = new(
        @"\s*\[Deliverables:\s*([^\]]+)\]\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static IReadOnlyList<ScenarioSprintDto> Parse(string? assignment)
    {
        if (string.IsNullOrWhiteSpace(assignment))
        {
            return [EmptySprint(1)];
        }

        var text = assignment.Trim();
        if (TryParseJson(text, out var fromJson) && fromJson.Count > 0)
        {
            return Renumber(SplitIfEmbeddedBlob(fromJson));
        }

        return Renumber(SplitIfEmbeddedBlob(ParseLegacyText(text)));
    }

    public static string Serialize(IEnumerable<ScenarioSprintDto> sprints)
    {
        var list = Renumber(sprints).Select(s => new SprintJsonItem
        {
            SprintNo = s.SprintNo,
            Focus = s.Focus?.Trim() ?? string.Empty,
            DurationValue = NormalizeDurationValue(s.DurationValue),
            DurationUnit = NormalizeDurationUnit(s.DurationUnit),
            TemplateCodes = (s.TemplateCodes ?? [])
                .Select(c => c.Trim().ToUpperInvariant())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        }).ToList();

        return JsonSerializer.Serialize(new SprintJsonEnvelope
        {
            Schema = SchemaV1,
            Sprints = list
        }, JsonOptions);
    }

    public static ScenarioSprintDto Normalize(ScenarioSprintDto sprint, int sprintNo) =>
        new(
            sprintNo,
            sprint.Focus?.Trim() ?? string.Empty,
            NormalizeDurationValue(sprint.DurationValue),
            NormalizeDurationUnit(sprint.DurationUnit),
            (sprint.TemplateCodes ?? [])
                .Select(c => c.Trim().ToUpperInvariant())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());

    public static int ToDays(int durationValue, string? durationUnit)
    {
        var value = NormalizeDurationValue(durationValue);
        return NormalizeDurationUnit(durationUnit) == "days" ? value : value * 7;
    }

    private static ScenarioSprintDto EmptySprint(int sprintNo) =>
        new(sprintNo, "", DefaultDurationValue, DefaultDurationUnit, []);

    private static bool TryParseJson(string text, out List<ScenarioSprintDto> sprints)
    {
        sprints = [];
        if (text[0] is not ('{' or '['))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                sprints = ReadSprintArray(doc.RootElement);
                return sprints.Count > 0;
            }

            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("sprints", out var arr) &&
                arr.ValueKind == JsonValueKind.Array)
            {
                sprints = ReadSprintArray(arr);
                return sprints.Count > 0;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    private static List<ScenarioSprintDto> ReadSprintArray(JsonElement arr)
    {
        var list = new List<ScenarioSprintDto>();
        foreach (var el in arr.EnumerateArray())
        {
            var sprintNo = el.TryGetProperty("sprintNo", out var noEl) && noEl.TryGetInt32(out var n)
                ? n
                : list.Count + 1;
            var focus = el.TryGetProperty("focus", out var focusEl) ? focusEl.GetString() ?? "" : "";

            var durationValue = DefaultDurationValue;
            if (el.TryGetProperty("durationValue", out var dv) && dv.TryGetInt32(out var dvInt) && dvInt > 0)
            {
                durationValue = dvInt;
            }
            else if (el.TryGetProperty("duration", out var d) && d.TryGetInt32(out var dInt) && dInt > 0)
            {
                durationValue = dInt;
            }

            var durationUnit = DefaultDurationUnit;
            if (el.TryGetProperty("durationUnit", out var du) && du.ValueKind == JsonValueKind.String)
            {
                durationUnit = NormalizeDurationUnit(du.GetString());
            }

            var codes = new List<string>();
            if (el.TryGetProperty("templateCodes", out var codesEl) && codesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in codesEl.EnumerateArray())
                {
                    var code = c.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(code))
                        codes.Add(code.ToUpperInvariant());
                }
            }

            list.Add(new ScenarioSprintDto(
                sprintNo,
                focus.Trim(),
                NormalizeDurationValue(durationValue),
                durationUnit,
                codes.ToArray()));
        }

        return list;
    }

    private static List<ScenarioSprintDto> ParseLegacyText(string text)
    {
        var headers = HeaderRegex.Matches(text);
        if (headers.Count == 0)
        {
            return [new ScenarioSprintDto(1, text.Trim(), DefaultDurationValue, DefaultDurationUnit, [])];
        }

        var byNo = new Dictionary<int, ScenarioSprintDto>();
        for (var i = 0; i < headers.Count; i++)
        {
            var match = headers[i];
            if (!int.TryParse(match.Groups[1].Value, out var sprintNo) || sprintNo < 1)
            {
                continue;
            }

            var contentStart = match.Index + match.Length;
            var contentEnd = i + 1 < headers.Count ? headers[i + 1].Index : text.Length;
            var body = text[contentStart..contentEnd].Trim().TrimEnd('-', '•', ' ', '\t', '\r', '\n');
            var codes = Array.Empty<string>();
            var del = DeliverablesRegex.Match(body);
            if (del.Success)
            {
                codes = del.Groups[1].Value
                    .Split([',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(c => c.ToUpperInvariant())
                    .Where(c => c.Length > 0)
                    .ToArray();
                body = DeliverablesRegex.Replace(body, "").Trim();
            }

            var durationValue = DefaultDurationValue;
            var durationUnit = DefaultDurationUnit;
            if (match.Groups[3].Success && int.TryParse(match.Groups[3].Value, out var parsedDuration) && parsedDuration > 0)
            {
                durationValue = parsedDuration;
                durationUnit = NormalizeDurationUnit(match.Groups[4].Value);
            }

            byNo[sprintNo] = new ScenarioSprintDto(sprintNo, body, durationValue, durationUnit, codes);
        }

        return byNo.Count > 0
            ? byNo.Values.OrderBy(s => s.SprintNo).ToList()
            : [new ScenarioSprintDto(1, text.Trim(), DefaultDurationValue, DefaultDurationUnit, [])];
    }

    private static List<ScenarioSprintDto> SplitIfEmbeddedBlob(List<ScenarioSprintDto> sprints)
    {
        if (sprints.Count != 1)
        {
            return sprints;
        }

        var focus = sprints[0].Focus ?? string.Empty;
        if (!HeaderRegex.IsMatch(focus))
        {
            return sprints;
        }

        var text = Regex.IsMatch(focus, @"^\s*Sprint\s*\d+\s*:", RegexOptions.IgnoreCase)
            ? focus
            : $"Sprint 1: {focus}";
        var split = ParseLegacyText(text);
        if (split.Count <= 1)
        {
            return sprints;
        }

        var fallbackDuration = sprints[0].DurationValue;
        var fallbackUnit = sprints[0].DurationUnit;
        var codes = sprints[0].TemplateCodes;
        return split
            .Select((s, idx) => new ScenarioSprintDto(
                idx + 1,
                s.Focus,
                s.DurationValue > 0 ? s.DurationValue : fallbackDuration,
                string.IsNullOrWhiteSpace(s.DurationUnit) ? fallbackUnit : s.DurationUnit,
                s.TemplateCodes.Count > 0 ? s.TemplateCodes : codes))
            .ToList();
    }

    private static List<ScenarioSprintDto> Renumber(IEnumerable<ScenarioSprintDto> sprints) =>
        sprints.Select((s, idx) => Normalize(s, idx + 1)).ToList();

    private static int NormalizeDurationValue(int value) =>
        value > 0 ? value : DefaultDurationValue;

    private static string NormalizeDurationUnit(string? unit)
    {
        var raw = (unit ?? "").Trim().ToLowerInvariant();
        return raw.StartsWith('d') ? "days" : "weeks";
    }

    private sealed class SprintJsonEnvelope
    {
        public string Schema { get; set; } = SchemaV1;
        public List<SprintJsonItem> Sprints { get; set; } = [];
    }

    private sealed class SprintJsonItem
    {
        public int SprintNo { get; set; }
        public string Focus { get; set; } = "";
        public int DurationValue { get; set; } = DefaultDurationValue;
        public string DurationUnit { get; set; } = DefaultDurationUnit;
        public List<string> TemplateCodes { get; set; } = [];
    }
}

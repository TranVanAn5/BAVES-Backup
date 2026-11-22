using System.Text.Json;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Entities;
using BAVES.Api.Modules.Scenario.Models.Requests;

namespace BAVES.Api.Modules.Scenario.Services;

internal static class ScenarioSprintMapper
{
    public static IReadOnlyList<string> ParseTemplateCodes(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split([',', '|', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => c.ToUpperInvariant())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

    public static string? JoinTemplateCodes(IEnumerable<string>? codes)
    {
        var list = (codes ?? [])
            .Select(c => c.Trim().ToUpperInvariant())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return list.Count == 0 ? null : string.Join(",", list);
    }

    public static ScenarioSprintDto ToDto(ScenarioSprint entity) =>
        new(
            entity.SprintNo,
            entity.Focus ?? string.Empty,
            entity.DurationValue <= 0 ? 1 : entity.DurationValue,
            NormalizeUnit(entity.DurationUnit),
            ParseTemplateCodes(entity.TemplateCodes));

    public static List<ScenarioSprintDto> ToDtoList(IEnumerable<ScenarioSprint>? entities) =>
        (entities ?? [])
            .OrderBy(s => s.SprintNo)
            .Select(ToDto)
            .ToList();

    /// <summary>
    /// Đọc sprint từ bảng; nếu trống thì fallback parse Assignment (JSON/legacy) để tương thích dữ liệu cũ.
    /// </summary>
    public static List<ScenarioSprintDto> ResolveSprints(ScenarioVersion version)
    {
        if (version.Sprints is { Count: > 0 })
        {
            return ToDtoList(version.Sprints);
        }

        return AssignmentSprintCodec.Parse(version.Assignment).ToList();
    }

    public static ScenarioSprint ToEntity(int scenarioVersionId, int sprintNo, UpsertScenarioSprintRequest request) =>
        new()
        {
            ScenarioVersionId = scenarioVersionId,
            SprintNo = sprintNo,
            Focus = request.Focus?.Trim() ?? string.Empty,
            DurationValue = request.DurationValue > 0 ? request.DurationValue : 1,
            DurationUnit = NormalizeUnit(request.DurationUnit),
            TemplateCodes = JoinTemplateCodes(request.TemplateCodes)
        };

    public static ScenarioSprint ToEntity(int scenarioVersionId, ScenarioSprintDto dto) =>
        new()
        {
            ScenarioVersionId = scenarioVersionId,
            SprintNo = dto.SprintNo,
            Focus = dto.Focus?.Trim() ?? string.Empty,
            DurationValue = dto.DurationValue > 0 ? dto.DurationValue : 1,
            DurationUnit = NormalizeUnit(dto.DurationUnit),
            TemplateCodes = JoinTemplateCodes(dto.TemplateCodes)
        };

    public static string NormalizeUnit(string? unit)
    {
        var raw = (unit ?? "").Trim().ToLowerInvariant();
        return raw.StartsWith('d') ? "days" : "weeks";
    }

    public static bool LooksLikeSprintJson(string? assignment)
    {
        if (string.IsNullOrWhiteSpace(assignment))
        {
            return false;
        }

        var t = assignment.Trim();
        if (!t.StartsWith('{') && !t.StartsWith('['))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(t);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return true;
            }

            return doc.RootElement.ValueKind == JsonValueKind.Object &&
                   doc.RootElement.TryGetProperty("sprints", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Assignment bị đổ danh sách sprint (legacy migrate) thay vì brief free-text.
    /// </summary>
    public static bool LooksLikeSprintDump(string? assignment)
    {
        if (string.IsNullOrWhiteSpace(assignment))
        {
            return false;
        }

        if (LooksLikeSprintJson(assignment))
        {
            return true;
        }

        // "Sprint 1 (1 tuần): ... Sprint 2 ..." hoặc "Sprint 1: ..."
        var matches = System.Text.RegularExpressions.Regex.Matches(
            assignment,
            @"\bSprint\s*\d+\s*(?:\([^)]*\))?\s*:",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return matches.Count >= 1;
    }

    /// <summary>Chỉ giữ Assignment nếu là brief thật — xóa JSON/dump sprint.</summary>
    public static string SanitizeAssignmentBrief(string? assignment) =>
        LooksLikeSprintDump(assignment) ? string.Empty : (assignment?.Trim() ?? string.Empty);
}

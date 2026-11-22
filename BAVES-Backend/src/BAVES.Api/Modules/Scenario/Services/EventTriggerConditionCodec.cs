using System.Text.Json;

namespace BAVES.Api.Modules.Scenario.Services;

/// <summary>
/// Chuẩn TriggerConditionJson cho ScenarioEvent.
/// AfterSprintEnds: { "afterSprintNo": 2 }
/// </summary>
public static class EventTriggerConditionCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string BuildAfterSprintEnds(int sprintNo) =>
        JsonSerializer.Serialize(new { afterSprintNo = Math.Max(1, sprintNo) }, JsonOptions);

    public static int? TryGetAfterSprintNo(string? triggerConditionJson)
    {
        if (string.IsNullOrWhiteSpace(triggerConditionJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(triggerConditionJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (doc.RootElement.TryGetProperty("afterSprintNo", out var prop) &&
                prop.TryGetInt32(out var n) &&
                n > 0)
            {
                return n;
            }

            // Alias legacy
            if (doc.RootElement.TryGetProperty("sprintNo", out var sprintProp) &&
                sprintProp.TryGetInt32(out var sn) &&
                sn > 0)
            {
                return sn;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}

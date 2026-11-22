namespace BAVES.Api.Modules.Scenario.Models.Enums;

public enum ScenarioEventTriggerType
{
    Manual = 0,
    MessageCount = 1,
    TimeElapsed = 2,
    Condition = 3,
    /// <summary>Tự kích hoạt khi learner hoàn tất (finalize) Sprint N — N nằm trong TriggerConditionJson.</summary>
    AfterSprintEnds = 4
}

using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.Entities;

public class SessionEvent
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ScenarioEventId { get; set; }

    public EventTriggerSource TriggerSource { get; set; }

    public DateTimeOffset TriggeredAt { get; set; }

    public int SequenceNo { get; set; }

    public string EventContentSnapshot { get; set; } = null!;

    public SimulationSession Session { get; set; } = null!;

}

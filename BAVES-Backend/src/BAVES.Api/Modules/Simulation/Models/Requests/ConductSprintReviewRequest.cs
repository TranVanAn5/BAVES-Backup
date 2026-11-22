using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Simulation.Models.Requests;

public sealed class ConductSprintReviewRequest
{
    [MaxLength(4000)]
    public string? LearnerReply { get; set; }

    public bool FinalizeSprint { get; set; }
}

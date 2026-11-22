using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.Entities;

public class CohortMember
{
    public int Id { get; set; }

    public int CohortId { get; set; }

    public int LearnerId { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public CohortMemberStatus Status { get; set; } = CohortMemberStatus.Active;

    public Cohort Cohort { get; set; } = null!;

}

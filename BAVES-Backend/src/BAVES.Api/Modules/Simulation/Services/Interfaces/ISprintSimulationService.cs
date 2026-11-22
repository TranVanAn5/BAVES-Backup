using BAVES.Api.Modules.Simulation.Models.DTOs;

namespace BAVES.Api.Modules.Simulation.Services.Interfaces;

public interface ISprintSimulationService
{
    Task<SprintBoardDto> GetBoardAsync(int learnerId, int sessionId, CancellationToken cancellationToken = default);

    Task<SprintReviewTurnDto> ConductReviewAsync(
        int learnerId,
        int sessionId,
        int sprintNo,
        string? learnerReply,
        bool finalizeSprint,
        AgentAiConnection aiConnection,
        CancellationToken cancellationToken = default);
}

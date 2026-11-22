using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Simulation.Models.Requests;

/// <summary>
/// Body của POST /api/sessions/{sessionId}/agents/{agentId}/messages.
/// Chỉ có nội dung tin nhắn; phần định tuyến đã xác định session và agent.
/// </summary>
public sealed class SendAgentMessageRequest
{
    /// <summary>
    /// Nội dung tin nhắn của learner gửi tới stakeholder.
    /// Không được rỗng và tối đa 4000 ký tự.
    /// </summary>
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}

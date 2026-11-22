namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Thông tin tối thiểu về một StakeholderPersona (stakeholder trong kịch bản),
/// do module Scenario sở hữu và cung cấp cho các module khác.
/// Chỉ chứa dữ liệu "public" của persona — không kèm hidden information
/// (KnowledgeItem, quyền truy cập kiến thức, business rule) vì những phần đó
/// được quản lý riêng bởi AI runtime khi phỏng vấn.
/// </summary>
public sealed record ScenarioPersonaInfoDto(
    int Id,
    string Name,
    string StakeholderRole,
    int DisplayOrder
);

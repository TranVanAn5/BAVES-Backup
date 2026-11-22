using BAVES.Api.Modules.Planning.Models.DTOs;
using BAVES.Api.Modules.Planning.Models.Enums;
using BAVES.Api.Modules.Planning.Repositories.Interfaces;
using BAVES.Api.Modules.Planning.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAPlanEntity = BAVES.Api.Modules.Planning.Models.Entities.BAPlan;
using PlannedQuestionEntity = BAVES.Api.Modules.Planning.Models.Entities.PlannedQuestion;

namespace BAVES.Api.Modules.Planning.Services;

/// <summary>
/// Triển khai nghiệp vụ Planning: lấy BA Plan cho session của Learner.
/// IPlanningRepository: truy vấn bảng planning.BAPlans, planning.PlannedQuestions
/// ISimulationSessionService: kiểm tra session thuộc về learner (ownership)
/// </summary>
public sealed class PlanningService(
    IPlanningRepository planningRepository,
    ISimulationSessionService simulationSessionService
    ) : IPlanningService
{
    public async Task<BAPlanDto?> GetPlanBySessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // B1: Kiểm tra session tồn tại VÀ thuộc về learner.
        // Gọi qua service của module Simulation (cross-module read),
        // không truy vấn trực tiếp bảng simulation.SimulationSessions.
        var sessionExistAndOwned = await simulationSessionService.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);

        if (!sessionExistAndOwned)
        {
            return null;
        }

        // B2: Tìm BAPlan theo SessionId (1 session chỉ có 1 plan - UNIQUE index).
        // Repository đã map sẵn sang DTO, Service không cần map lại.
        var plan = await planningRepository.GetPlanBySessionAsync(sessionId, cancellationToken);

        // B3: Chưa có plan → null → Controller trả 404.
        if (plan == null)
        {
            return null;
        }

        return plan;
    }

    public async Task<BAPlanDto?> UpsertPlanAsync(
        int learnerId,
        int sessionId,
        UpsertPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        // ─────────── VALIDATE INPUT ───────────
        // Mục đích: chặn sớm request không hợp lệ, tránh để EF/DB ném exception → trả 500.
        // IsNullOrWhiteSpace: bắt cả null, "", "   " (toàn khoảng trắng).
        if (string.IsNullOrWhiteSpace(request.Objective))
        {
            throw new ArgumentException(
                "Objective không được để trống hoặc chỉ chứa khoảng trắng.",
                nameof(request));
        }

        // Session phải tồn tại và thuộc về learner đang gọi.
        var sessionExistAndOwned = await simulationSessionService.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);

        if (!sessionExistAndOwned)
        {
            return null;
        }

        //TÌM PLAN CŨ  
        // 1 session chỉ có tối đa 1 plan 
        var plan = await planningRepository.GetPlanEntityForUpdateAsync(sessionId, cancellationToken);

        // NẾU CHƯA CÓ → TẠO MỚI 
        if (plan == null)
        {
            var now = DateTimeOffset.UtcNow;

            plan = new BAPlanEntity
            {
                SessionId = sessionId,
                Objective = request.Objective,
                Approach = request.Approach,
                Status = BAPlanStatus.Draft,    // bắt đầu là nháp
                CreatedAt = now,
                UpdatedAt = null               // chưa cập nhật lần nào
            };

            // Đánh dấu cần INSERT (chưa gọi DB).
            planningRepository.AddPlan(plan);
        }
        // NẾU CÓ RỒI → CẬP NHẬT
        else
        {
            plan.Objective = request.Objective;
            plan.Approach = request.Approach;
            plan.UpdatedAt = DateTimeOffset.UtcNow;
            // Status KHÔNG đổi (giữ nguyên trạng thái hiện tại).
        }

        // XỬ LÝ DANH SÁCH CÂU HỎI
        //  - Questions == null  → KHÔNG đụng (giữ nguyên câu hỏi cũ).
        //  - Questions.Count==0 → XÓA HẾT câu hỏi.
        //  - Questions.Count>0  → REPLACE toàn bộ.
        if (request.Questions != null)
        {
            // Xóa hết câu hỏi cỏ (nếu có).
            if (plan.PlannedQuestions.Count > 0)
            {
                planningRepository.RemoveQuestions(plan.PlannedQuestions);
            }

            // Tạo câu hỏi mới từ danh sách client gửi lên.
            foreach (var input in request.Questions)
            {
                plan.PlannedQuestions.Add(new PlannedQuestionEntity
                {
                    // BAPlanId: EF sẽ tự điền khi SaveChanges (vì plan.Id = 0 chưa có).
                    StakeholderPersonaId = input.StakeholderPersonaId,
                    Topic = input.Topic,
                    InformationNeed = input.InformationNeed,
                    Question = input.Question,
                    Priority = input.Priority,
                    DisplayOrder = input.DisplayOrder,
                    Status = PlannedQuestionStatus.Planned,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        // LƯU XUỐNG DB
        // SaveChangesAsync: gom Insert + Update + Delete thành 1 transaction.
        // Nếu 1 chỗ lỗi → tất cả rollback (không lưu nửa vời).
        await planningRepository.SaveChangesAsync(cancellationToken);

        // LẤY LẠI DTO ĐỂ TRẢ VỀ 
        // Sau SaveChanges, plan.Id đã có (do DB tự sinh), UpdatedAt cũng đúng.
        // Đọc lại từ DB để lấy đúng dữ liệu đã persist + Id thật của câu hỏi.
        var result = await planningRepository.GetPlanBySessionAsync(sessionId, cancellationToken);
        return result;
    }
}

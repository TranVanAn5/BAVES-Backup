-- ====================================================
-- BA-VES SEED: BeanCounter Café — Đặt nguyên liệu tuần (BEG-CAFE-01)
-- Beginner · IsFreeAccess = 1 · Personas đủ field · Knowledge/Rules/Event/Rubric
-- ====================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @InstructorUserId int;
SELECT TOP 1 @InstructorUserId = [Id] FROM [identity].[Users] ORDER BY [Id];
IF @InstructorUserId IS NULL
BEGIN
    INSERT INTO [identity].[Users] (
        [FullName], [Status], [CreatedAt], [UserName], [NormalizedUserName],
        [Email], [NormalizedEmail], [EmailConfirmed], [PhoneNumberConfirmed],
        [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount]
    ) VALUES (
        N'System Instructor', N'Active', SYSDATETIMEOFFSET(), N'instructor@baves.local', N'INSTRUCTOR@BAVES.LOCAL',
        N'instructor@baves.local', N'INSTRUCTOR@BAVES.LOCAL', 1, 0, 0, 1, 0
    );
    SET @InstructorUserId = SCOPE_IDENTITY();
END;

DECLARE @IndustryId int;
SELECT @IndustryId = [Id] FROM [content].[Industries] WHERE [Code] = N'RETAIL';
IF @IndustryId IS NULL
BEGIN
    INSERT INTO [content].[Industries] ([Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])
    VALUES (N'RETAIL', N'Bán lẻ & Thời trang', N'Lĩnh vực Bán lẻ & F&B / cửa hàng', N'Active', 2, SYSDATETIMEOFFSET());
    SET @IndustryId = SCOPE_IDENTITY();
END;

DECLARE @ApplicationId int;
SELECT @ApplicationId = [Id] FROM [content].[Applications] WHERE [IndustryId] = @IndustryId AND [Code] = N'CAFE_OPS';
IF @ApplicationId IS NULL
BEGIN
    INSERT INTO [content].[Applications] ([IndustryId], [Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])
    VALUES (@IndustryId, N'CAFE_OPS', N'Cafe Operations & Ordering', N'Vận hành quán cà phê độc lập — đặt nguyên liệu tuần, checklist nhu cầu, trách nhiệm đề xuất/duyệt/gửi NCC.', N'Active', 5, SYSDATETIMEOFFSET());
    SET @ApplicationId = SCOPE_IDENTITY();
END;

DECLARE @ScenarioId int;
SELECT @ScenarioId = [Id] FROM [scenario].[Scenarios] WHERE [Code] = N'BEG-CAFE-01';
IF @ScenarioId IS NULL
BEGIN
    INSERT INTO [scenario].[Scenarios] ([ApplicationId], [CreatedByUserId], [Code], [Title], [Description], [Status], [CreatedAt])
    VALUES (
        @ApplicationId, @InstructorUserId, N'BEG-CAFE-01',
        N'BeanCounter Café — Chuẩn hóa quy trình đặt nguyên liệu tuần',
        N'Beginner: Junior BA giúp quán cà phê độc lập thống nhất cách đặt sữa/đậu/đồ dùng mỗi tuần giữa quán trưởng, barista và kế toán — giảm thiếu hàng và đặt trùng.',
        N'Published', SYSDATETIMEOFFSET()
    );
    SET @ScenarioId = SCOPE_IDENTITY();
END;
ELSE
BEGIN
    UPDATE [scenario].[Scenarios]
    SET [Title] = N'BeanCounter Café — Chuẩn hóa quy trình đặt nguyên liệu tuần',
        [Description] = N'Beginner: Junior BA giúp quán cà phê độc lập thống nhất cách đặt sữa/đậu/đồ dùng mỗi tuần giữa quán trưởng, barista và kế toán — giảm thiếu hàng và đặt trùng.',
        [ApplicationId] = @ApplicationId,
        [Status] = N'Published'
    WHERE [Id] = @ScenarioId;
END;

DECLARE @ScenarioVersionId int;
SELECT @ScenarioVersionId = [Id] FROM [scenario].[ScenarioVersions] WHERE [ScenarioId] = @ScenarioId AND [VersionNo] = 1;

DECLARE @Summary nvarchar(max) = N'Giúp BeanCounter Café thống nhất quy trình đặt nguyên liệu tuần (sữa, đậu, ly) giữa quán trưởng, barista và kế toán — giảm thiếu hàng giữa tuần và đặt trùng.';
DECLARE @BusinessContext nvarchar(max) = N'BeanCounter Café là quán cà phê độc lập 1 cơ sở tại TP.HCM (~40–60 khách/ngày, 2 ca). Mỗi tuần quán trưởng đặt sữa, đậu rang và ly giấy qua Zalo với 2 nhà cung cấp (NCC A: sữa/ly; NCC B: đậu). Barista ca sáng ghi “hết sữa / hết đậu” trên giấy dán tủ lạnh; kế toán part-time theo dõi chi phí nguyên liệu trên file Excel riêng (Chi_phi_NL.xlsx). Không có danh mục nhu cầu tuần chung nên đôi khi thiếu đậu giữa tuần, đôi khi đặt trùng sữa vì quán trưởng và kế toán không cùng một danh sách.';
DECLARE @BusinessProblem nvarchar(max) = N'1) Thiếu hàng giữa tuần vì không tổng hợp nhu cầu từ ca trước khi đặt. 2) Đặt trùng / đặt thừa vì kế toán và quán trưởng không dùng chung một checklist. 3) Chưa có mẫu checklist đặt hàng tuần và trách nhiệm đề xuất–duyệt–gửi NCC rõ ràng.';
DECLARE @BusinessObjectives nvarchar(max) = N'1) Mô tả quy trình đặt nguyên liệu tuần as-is. 2) Đề xuất danh mục nhu cầu tối thiểu + ai đề xuất / ai duyệt / ai gửi NCC. 3) Hoàn thành deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be checklist, 3 user stories + AC.';
DECLARE @BARole nvarchar(max) = N'Bạn là Junior Business Analyst hỗ trợ BeanCounter Café làm rõ quy trình đặt nguyên liệu tuần. Không thiết kế app đặt hàng, không đàm phán lại hợp đồng NCC, không mở thêm chi nhánh.';
DECLARE @Assignment nvarchar(max) = N'Sprint 1: Đọc context; lập BAP ngắn (mục tiêu, phạm vi, kế hoạch phỏng vấn) và stakeholder map 3 bên (Quán trưởng, Barista, Kế toán). [Deliverables: STK-REGISTER]
Sprint 2: Phỏng vấn Quán trưởng, Barista ca sáng, Kế toán; ghi elicitation notes (facts + assumptions) về cách đặt hiện tại và điểm lệch thông tin.
Sprint 3: Lập checklist đặt hàng tuần as-is / to-be và bảng trách nhiệm đề xuất–duyệt–gửi NCC. [Deliverables: BRC-CATALOG]
Sprint 4: Viết Problem Statement ngắn + 3 user stories kèm Acceptance Criteria cho checklist / nhắc thiếu hàng; phản ánh nhẹ sự kiện E1 nếu được kích hoạt. [Deliverables: US-AGILE]';
DECLARE @InScope nvarchar(max) = N'- Nhu cầu tuần: sữa (kể cả oat), đậu, ly/ống hút.
- Ai đề xuất / ai duyệt / ai gửi tin Zalo NCC.
- Checklist đặt hàng tuần + 3 user stories cơ bản + AC.
- Deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be.';
DECLARE @OutOfScope nvarchar(max) = N'- App mobile đặt hàng / inventory real-time.
- Đổi giá hoặc đổi NCC; mở thêm chi nhánh.
- BRD/SRS/RTM đầy đủ, RACI Intermediate, crisis mất điện kéo dài.';
DECLARE @KnownConstraints nvarchar(max) = N'- Ngân sách đào tạo; số liệu giả lập phục vụ học BA.
- Giữ 2 NCC hiện tại (A: sữa/ly; B: đậu).
- Không thu thập dữ liệu cá nhân thật của nhân viên.';

IF @ScenarioVersionId IS NULL
BEGIN
    INSERT INTO [scenario].[ScenarioVersions] (
        [ScenarioId], [VersionNo], [Summary], [BusinessContext], [BusinessProblem], [BusinessObjectives],
        [BARole], [Assignment], [InScope], [OutOfScope], [KnownConstraints], [Difficulty], [IsFreeAccess],
        [Status], [IsPublished], [PublishedAt], [CreatedAt]
    ) VALUES (
        @ScenarioId, 1, @Summary, @BusinessContext, @BusinessProblem, @BusinessObjectives,
        @BARole, @Assignment, @InScope, @OutOfScope, @KnownConstraints, N'Beginner', 1,
        N'Published', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    );
    SET @ScenarioVersionId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE [scenario].[ScenarioVersions]
    SET [Summary] = @Summary,
        [BusinessContext] = @BusinessContext,
        [BusinessProblem] = @BusinessProblem,
        [BusinessObjectives] = @BusinessObjectives,
        [BARole] = @BARole,
        [Assignment] = @Assignment,
        [InScope] = @InScope,
        [OutOfScope] = @OutOfScope,
        [KnownConstraints] = @KnownConstraints,
        [Difficulty] = N'Beginner',
        [IsFreeAccess] = 1,
        [Status] = N'Published',
        [IsPublished] = 1,
        [PublishedAt] = COALESCE([PublishedAt], SYSDATETIMEOFFSET())
    WHERE [Id] = @ScenarioVersionId;
END;

-- Personas (đủ field nội dung)
IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Nguyễn Thị Mai')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas] (
        [ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities],
        [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive]
    ) VALUES (
        @ScenarioVersionId, N'Nguyễn Thị Mai', N'Owner / Store Manager', N'Vận hành',
        N'Sở hữu quán; duyệt và gửi đơn đặt tuần qua Zalo NCC A/B; quyết định số lượng cuối cùng thứ Hai sáng; theo dõi trải nghiệm khách và lãng phí.',
        N'Hết tuần không thiếu hàng, không lãng phí; giữ quán chạy mượt với 2 ca.',
        N'Thứ Hai sáng thường duyệt “theo cảm giác” vì chưa kịp đọc Excel kế toán; hay bị barista nhắc thiếu giữa tuần.',
        N'Thường duyệt theo cảm giác thứ Hai sáng, không đọc Chi_phi_NL.xlsx trước khi nhắn NCC. Lo mất mặt với khách nếu hết sữa oat. Lập trường xung đột: ưu tiên trải nghiệm khách hơn tiết kiệm 1–2 thùng sữa.',
        N'Thẳng, nhanh, thích ví dụ ca thật; ít kiên nhẫn với bảng dài.',
        N'Thực dụng, quyết đoán, đôi khi bỏ qua số liệu kế toán khi vội.',
        1, 1
    );
END
ELSE
BEGIN
    UPDATE [scenario].[StakeholderPersonas]
    SET [StakeholderRole] = N'Owner / Store Manager',
        [Department] = N'Vận hành',
        [Responsibilities] = N'Sở hữu quán; duyệt và gửi đơn đặt tuần qua Zalo NCC A/B; quyết định số lượng cuối cùng thứ Hai sáng; theo dõi trải nghiệm khách và lãng phí.',
        [Goals] = N'Hết tuần không thiếu hàng, không lãng phí; giữ quán chạy mượt với 2 ca.',
        [PainPoints] = N'Thứ Hai sáng thường duyệt “theo cảm giác” vì chưa kịp đọc Excel kế toán; hay bị barista nhắc thiếu giữa tuần.',
        [BusinessConcerns] = N'Thường duyệt theo cảm giác thứ Hai sáng, không đọc Chi_phi_NL.xlsx trước khi nhắn NCC. Lo mất mặt với khách nếu hết sữa oat. Lập trường xung đột: ưu tiên trải nghiệm khách hơn tiết kiệm 1–2 thùng sữa.',
        [CommunicationStyle] = N'Thẳng, nhanh, thích ví dụ ca thật; ít kiên nhẫn với bảng dài.',
        [Personality] = N'Thực dụng, quyết đoán, đôi khi bỏ qua số liệu kế toán khi vội.',
        [DisplayOrder] = 1,
        [IsActive] = 1
    WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Nguyễn Thị Mai';
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Trần Minh Tuấn')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas] (
        [ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities],
        [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive]
    ) VALUES (
        @ScenarioVersionId, N'Trần Minh Tuấn', N'Barista (ca sáng)', N'Pha chế',
        N'Pha chế ca sáng; kiểm tra tủ lạnh/đậu đầu ca; ghi thiếu lên giấy dán; đề xuất số lượng cần cho quán trưởng (không gửi đơn NCC).',
        N'Luôn đủ sữa oat và đậu đậm trong ca; không phải giải thích với khách khi hết topping/sữa.',
        N'Chỉ ghi giấy dán; không báo nếu hết vào cuối ca vì vội đóng cửa; không thấy Excel kế toán.',
        N'Chỉ ghi giấy dán tủ lạnh; không chủ động báo nếu hết vào cuối ca. Không được gửi Zalo NCC. Lập trường xung đột: muốn đặt dư một chút để khỏi thiếu — đối nghịch với kế toán.',
        N'Nói ngắn, dùng tiếng quán; hợp tác nếu hỏi đúng ca làm việc.',
        N'Thân thiện với khách, hơi né trách nhiệm giấy tờ, trung thành với thói quen giấy dán.',
        2, 1
    );
END
ELSE
BEGIN
    UPDATE [scenario].[StakeholderPersonas]
    SET [StakeholderRole] = N'Barista (ca sáng)',
        [Department] = N'Pha chế',
        [Responsibilities] = N'Pha chế ca sáng; kiểm tra tủ lạnh/đậu đầu ca; ghi thiếu lên giấy dán; đề xuất số lượng cần cho quán trưởng (không gửi đơn NCC).',
        [Goals] = N'Luôn đủ sữa oat và đậu đậm trong ca; không phải giải thích với khách khi hết topping/sữa.',
        [PainPoints] = N'Chỉ ghi giấy dán; không báo nếu hết vào cuối ca vì vội đóng cửa; không thấy Excel kế toán.',
        [BusinessConcerns] = N'Chỉ ghi giấy dán tủ lạnh; không chủ động báo nếu hết vào cuối ca. Không được gửi Zalo NCC. Lập trường xung đột: muốn đặt dư một chút để khỏi thiếu — đối nghịch với kế toán.',
        [CommunicationStyle] = N'Nói ngắn, dùng tiếng quán; hợp tác nếu hỏi đúng ca làm việc.',
        [Personality] = N'Thân thiện với khách, hơi né trách nhiệm giấy tờ, trung thành với thói quen giấy dán.',
        [DisplayOrder] = 2,
        [IsActive] = 1
    WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Trần Minh Tuấn';
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Lê Thu Hà')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas] (
        [ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities],
        [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive]
    ) VALUES (
        @ScenarioVersionId, N'Lê Thu Hà', N'Accountant (part-time)', N'Tài chính',
        N'Đối chiếu hóa đơn NCC cuối tháng trên Excel Chi_phi_NL.xlsx; theo dõi chi phí nguyên liệu/tuần; đôi khi đặt bù khi thấy số liệu lệch (không phải lúc nào cũng báo quán trưởng).',
        N'Kiểm soát chi phí nguyên liệu theo tuần; tránh đặt thừa và trùng đơn.',
        N'Có file Excel NCC nhưng không chia sẻ cho barista; khó thuyết phục quán trưởng đọc số trước khi duyệt.',
        N'Có Chi_phi_NL.xlsx và lịch sử đặt; tuần trước đã đặt bù 2 thùng sữa mà quán trưởng không biết. Lập trường xung đột: ưu tiên tiết kiệm / chống trùng hơn trải nghiệm — đối nghịch Mai & Tuấn.',
        N'Cẩn thận, thích bảng số; hỏi lại khi số liệu mơ hồ.',
        N'Kỷ luật, hơi bảo thủ với chi phí, hợp tác nếu learner tôn trọng số liệu.',
        3, 1
    );
END
ELSE
BEGIN
    UPDATE [scenario].[StakeholderPersonas]
    SET [StakeholderRole] = N'Accountant (part-time)',
        [Department] = N'Tài chính',
        [Responsibilities] = N'Đối chiếu hóa đơn NCC cuối tháng trên Excel Chi_phi_NL.xlsx; theo dõi chi phí nguyên liệu/tuần; đôi khi đặt bù khi thấy số liệu lệch (không phải lúc nào cũng báo quán trưởng).',
        [Goals] = N'Kiểm soát chi phí nguyên liệu theo tuần; tránh đặt thừa và trùng đơn.',
        [PainPoints] = N'Có file Excel NCC nhưng không chia sẻ cho barista; khó thuyết phục quán trưởng đọc số trước khi duyệt.',
        [BusinessConcerns] = N'Có Chi_phi_NL.xlsx và lịch sử đặt; tuần trước đã đặt bù 2 thùng sữa mà quán trưởng không biết. Lập trường xung đột: ưu tiên tiết kiệm / chống trùng hơn trải nghiệm — đối nghịch Mai & Tuấn.',
        [CommunicationStyle] = N'Cẩn thận, thích bảng số; hỏi lại khi số liệu mơ hồ.',
        [Personality] = N'Kỷ luật, hơi bảo thủ với chi phí, hợp tác nếu learner tôn trọng số liệu.',
        [DisplayOrder] = 3,
        [IsActive] = 1
    WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Lê Thu Hà';
END;

DECLARE @PMai int = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Nguyễn Thị Mai');
DECLARE @PTuan int = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Trần Minh Tuấn');
DECLARE @PHa int = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Lê Thu Hà');

-- Knowledge
IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-01')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES (@ScenarioVersionId, N'K-CAFE-01', N'Cách đặt nguyên liệu tuần hiện tại (as-is)', N'Mỗi **thứ Hai sáng**:
1. Quán trưởng (Mai) xem nhanh giấy dán tủ lạnh + “cảm giác” tồn.
2. Nhắn Zalo **NCC A** (sữa tươi, sữa oat, ly giấy) và **NCC B** (đậu).
3. Barista (Tuấn) chỉ ghi thiếu lên giấy dán trong tuần; **không** gửi đơn NCC.
4. Kế toán (Hà) đối chiếu hóa đơn **cuối tháng** trên Excel `Chi_phi_NL.xlsx`.

**Thiếu bước:** không có “tổng hợp nhu cầu tuần” trước khi nhắn NCC. Không có checklist chung Mai–Tuấn–Hà.', N'Process', N'High', SYSDATETIMEOFFSET());
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-02')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES (@ScenarioVersionId, N'K-CAFE-02', N'Danh mục nhu cầu tuần tối thiểu (gợi ý phạm vi)', N'Phạm vi Beginner chỉ gồm:
| Nhóm | Ví dụ | NCC |
| --- | --- | --- |
| Sữa | Sữa tươi, sữa oat | A |
| Đậu | Đậu Arabica rang sẵn (túi 1kg) | B |
| Đồ dùng | Ly giấy 12/16oz, ống hút | A |

Ngoài phạm vi: siro đặc biệt, máy móc, vệ sinh công nghiệp, đổi giá NCC.', N'BusinessFact', N'Medium', SYSDATETIMEOFFSET());
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-03')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES (@ScenarioVersionId, N'K-CAFE-03', N'Sự cố đặt trùng sữa tuần trước (cần elicit)', N'Fact ẩn — nên khơi gợi qua phỏng vấn Kế toán / Quán trưởng:

Tuần trước kế toán (Hà) thấy tồn sữa thấp trên Excel và **đặt bù 2 thùng sữa** với NCC A vào Chủ nhật tối. Thứ Hai sáng quán trưởng (Mai) vẫn nhắn đặt theo cảm giác → **đặt trùng**.

Barista không biết có đơn bù. Đây là bằng chứng thiếu checklist và thiếu bước “ai đã đặt gì tuần này”.', N'BusinessFact', N'High', SYSDATETIMEOFFSET());
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-04')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES (@ScenarioVersionId, N'K-CAFE-04', N'Quy ước trách nhiệm đề xuất–duyệt–gửi (chưa thành policy)', N'Quy ước miệng hiện tại (chưa ghi chính thức):
- Barista: **đề xuất** thiếu hàng (giấy dán).
- Quán trưởng: **duyệt số lượng** và **gửi Zalo NCC**.
- Kế toán: **đối chiếu chi phí** sau; đôi khi đặt bù ngoài quy trình.

Mục tiêu to-be Beginner: mọi đơn tuần phải có xác nhận quán trưởng trước khi gửi NCC; barista không gửi đơn.', N'Policy', N'Medium', SYSDATETIMEOFFSET());
END;

DECLARE @K1 int = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-01');
DECLARE @K2 int = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-02');
DECLARE @K3 int = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-03');
DECLARE @K4 int = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'K-CAFE-04');

-- Access maps
IF @PMai IS NOT NULL AND @K1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PMai AND [KnowledgeItemId] = @K1)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PMai, @K1, N'Knows', NULL, 0);
IF @PTuan IS NOT NULL AND @K1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PTuan AND [KnowledgeItemId] = @K1)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PTuan, @K1, N'Partial', NULL, 0);
IF @PHa IS NOT NULL AND @K1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PHa AND [KnowledgeItemId] = @K1)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PHa, @K1, N'Partial', NULL, 0);

IF @PMai IS NOT NULL AND @K2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PMai AND [KnowledgeItemId] = @K2)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PMai, @K2, N'Knows', NULL, 0);
IF @PTuan IS NOT NULL AND @K2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PTuan AND [KnowledgeItemId] = @K2)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PTuan, @K2, N'Knows', NULL, 0);
IF @PHa IS NOT NULL AND @K2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PHa AND [KnowledgeItemId] = @K2)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PHa, @K2, N'Knows', NULL, 0);

IF @PHa IS NOT NULL AND @K3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PHa AND [KnowledgeItemId] = @K3)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PHa, @K3, N'Knows', N'Chỉ tiết lộ khi learner hỏi về đặt trùng / đặt bù / Excel tuần trước.', 1);
IF @PMai IS NOT NULL AND @K3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PMai AND [KnowledgeItemId] = @K3)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PMai, @K3, N'Partial', N'Tiết lộ một phần nếu learner hỏi vì sao tủ lạnh đầy sữa bất thường.', 1);

IF @PMai IS NOT NULL AND @K4 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PMai AND [KnowledgeItemId] = @K4)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PMai, @K4, N'Knows', NULL, 0);
IF @PTuan IS NOT NULL AND @K4 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PTuan AND [KnowledgeItemId] = @K4)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PTuan, @K4, N'Partial', NULL, 0);
IF @PHa IS NOT NULL AND @K4 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @PHa AND [KnowledgeItemId] = @K4)
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@PHa, @K4, N'Knows', NULL, 0);

-- Business rules
IF NOT EXISTS (SELECT 1 FROM [scenario].[BusinessRules] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'BR-CAFE-01')
BEGIN
    INSERT INTO [scenario].[BusinessRules] ([ScenarioVersionId], [Code], [Title], [Description], [Importance], [IsHidden])
    VALUES (@ScenarioVersionId, N'BR-CAFE-01', N'Quán trưởng duyệt trước khi gửi NCC', N'Mọi đơn đặt nguyên liệu tuần phải có xác nhận của quán trưởng trước khi gửi NCC; barista chỉ được đề xuất, không gửi đơn.', N'High', 1);
END;
IF NOT EXISTS (SELECT 1 FROM [scenario].[BusinessRules] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'BR-CAFE-02')
BEGIN
    INSERT INTO [scenario].[BusinessRules] ([ScenarioVersionId], [Code], [Title], [Description], [Importance], [IsHidden])
    VALUES (@ScenarioVersionId, N'BR-CAFE-02', N'Một checklist nhu cầu tuần dùng chung', N'Trước khi nhắn NCC, phải có một checklist nhu cầu tuần (sữa/đậu/ly) được cập nhật từ ca và đối chiếu với ghi chú kế toán nếu có.', N'High', 0);
END;
IF NOT EXISTS (SELECT 1 FROM [scenario].[BusinessRules] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Code] = N'BR-CAFE-03')
BEGIN
    INSERT INTO [scenario].[BusinessRules] ([ScenarioVersionId], [Code], [Title], [Description], [Importance], [IsHidden])
    VALUES (@ScenarioVersionId, N'BR-CAFE-03', N'Đặt bù ngoài lịch phải thông báo quán trưởng', N'Mọi đơn đặt bù ngoài lịch thứ Hai phải được thông báo cho quán trưởng trong ngày; không đặt âm thầm chỉ trên Excel.', N'Medium', 0);
END;

-- Event
IF NOT EXISTS (SELECT 1 FROM [scenario].[ScenarioEvents] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Title] = N'NCC A báo trễ sữa 2 ngày')
BEGIN
    INSERT INTO [scenario].[ScenarioEvents] ([ScenarioVersionId], [Title], [Description], [TriggerType], [TriggerConditionJson], [EventContent], [DisplayOrder], [IsActive])
    VALUES (
        @ScenarioVersionId,
        N'NCC A báo trễ sữa 2 ngày',
        N'NCC A xác nhận sữa tươi giao muộn 2 ngày so với lịch thứ Ba — learner phản ánh nhẹ vào notes / user story.',
        N'Manual',
        NULL,
        N'NCC A nhắn Zalo: sữa tươi giao **muộn 2 ngày** so với lịch thứ Ba hàng tuần.

Yêu cầu learner (Beginner): nêu **tác động** tới ca pha chế và trải nghiệm khách; liệt kê **2–3 câu hỏi** cần hỏi thêm stakeholder; đề xuất **1 chỉnh sửa ngắn** trên checklist / user story (ví dụ nhắc tồn an toàn hoặc kế hoạch dự phòng nhẹ) — không yêu cầu redesign toàn bộ chuỗi cung ứng.',
        1, 1
    );
END;

-- Rubric
DECLARE @BabokTaskId int;
SELECT TOP 1 @BabokTaskId = [Id] FROM [assessment].[BabokTasks] WHERE [Code] = N'RADD-7.1';
IF @BabokTaskId IS NULL
BEGIN
    INSERT INTO [assessment].[BabokTasks] ([KnowledgeAreaCode], [KnowledgeAreaName], [Code], [Name], [Purpose], [Description], [BabokVersion], [DisplayOrder])
    VALUES (N'RADD', N'Requirements Analysis and Design Definition', N'RADD-7.1', N'Specify and Model Requirements', N'Đặc tả và mô hình hóa yêu cầu nghiệp vụ', N'Phân tích, tổng hợp và đặc tả yêu cầu từ kết quả khảo sát Stakeholder.', N'v3', 1);
    SET @BabokTaskId = SCOPE_IDENTITY();
END;

DECLARE @RubricId int;
SELECT @RubricId = [Id] FROM [assessment].[Rubrics] WHERE [ScenarioVersionId] = @ScenarioVersionId AND [Name] = N'Rubric Beginner — BeanCounter Café (BEG-CAFE-01)';
IF @RubricId IS NULL
BEGIN
    INSERT INTO [assessment].[Rubrics] ([ScenarioVersionId], [CreatedByUserId], [Name], [Version], [Description], [Status], [CreatedAt])
    VALUES (@ScenarioVersionId, @InstructorUserId, N'Rubric Beginner — BeanCounter Café (BEG-CAFE-01)', 1,
        N'Tiêu chí Beginner: Completeness · Stakeholder clarity · Elicitation quality · As-is/to-be checklist · User stories + AC cơ bản.',
        N'Published', SYSDATETIMEOFFSET());
    SET @RubricId = SCOPE_IDENTITY();
END;

IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = @RubricId AND [Code] = N'COMPLETENESS')
    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [DisplayOrder], [IsRequired])
    VALUES (@RubricId, @BabokTaskId, N'COMPLETENESS', N'Tính đầy đủ deliverable Beginner', N'Có BAP ngắn, stakeholder map 3 bên, interview notes, checklist as-is/to-be, 3 user stories + AC.', 1.0, 100.0, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = @RubricId AND [Code] = N'STAKEHOLDER')
    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [DisplayOrder], [IsRequired])
    VALUES (@RubricId, @BabokTaskId, N'STAKEHOLDER', N'Rõ ràng stakeholder (Stakeholder clarity)', N'Phân biệt đúng vai trò Mai (duyệt/gửi), Tuấn (đề xuất), Hà (chi phí/đối chiếu) và xung đột ưu tiên.', 1.0, 100.0, 2, 1);
IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = @RubricId AND [Code] = N'ELICITATION')
    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [DisplayOrder], [IsRequired])
    VALUES (@RubricId, @BabokTaskId, N'ELICITATION', N'Chất lượng elicitation', N'Notes tách facts/assumptions; khơi gợi được đặt trùng / giấy dán / Excel nếu đã hỏi đúng.', 1.0, 100.0, 3, 1);
IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = @RubricId AND [Code] = N'CHECKLIST')
    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [DisplayOrder], [IsRequired])
    VALUES (@RubricId, @BabokTaskId, N'CHECKLIST', N'As-is / to-be checklist', N'Checklist nhu cầu tuần và bảng đề xuất–duyệt–gửi khớp BR-CAFE-*; khả thi trong phạm vi Beginner.', 1.0, 100.0, 4, 1);
IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = @RubricId AND [Code] = N'USERSTORIES')
    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [DisplayOrder], [IsRequired])
    VALUES (@RubricId, @BabokTaskId, N'USERSTORIES', N'User stories + AC cơ bản', N'3 US có AC kiểm thử được; phản ánh nhẹ E1 nếu được kích hoạt; không vượt out-of-scope.', 1.0, 100.0, 5, 1);

COMMIT TRANSACTION;
PRINT N'>> BeanCounter Café (BEG-CAFE-01) seeded successfully.';

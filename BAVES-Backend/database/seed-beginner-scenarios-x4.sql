-- ==============================================================================================
-- DEPRECATED: dùng OKF markdown + import_okf.py thay cho script này.
-- Nguồn: scenarios/*-starter/v1/  →  scripts/create_beginner_okf_bundles.py
-- Import: database/run-okf-sql.ps1
-- ==============================================================================================
-- (Giữ file để tham chiếu lịch sử — không khuyến nghị chạy lại)
/*
-- 4 kịch bản Beginner theo Application hiện có trong hệ thống
-- 2/4 gắn IsFreeAccess = 1 (miễn phí learner)
--
-- Mapping:
--   BEG-OMS-01   → App OMS          (SUPPLY_CHAIN)  · FREE
--   BEG-WMS-01   → App WMS_COLD     (SUPPLY_CHAIN)  · FREE
--   BEG-LOS-01   → App DIGITAL_LOS  (FINANCE)       · trả phí
--   BEG-PROMO-01 → App LMS_PROMO    (RETAIL)        · trả phí
--
-- Yêu cầu: đã chạy seed OKF (industry/application tồn tại) + ideally seed-free-beginner-scenarios.sql
-- ==============================================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF COL_LENGTH(N'scenario.ScenarioVersions', N'IsFreeAccess') IS NULL
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions]
        ADD [IsFreeAccess] bit NOT NULL CONSTRAINT [DF_ScenarioVersions_IsFreeAccess_Beg] DEFAULT (0);
END
GO

DECLARE @InstructorId int =
    (SELECT TOP (1) [Id] FROM [identity].[Users] ORDER BY [Id]);

IF @InstructorId IS NULL
BEGIN
    RAISERROR(N'Không tìm thấy user trong [identity].[Users] để gán CreatedByUserId.', 16, 1);
    RETURN;
END;

DECLARE @AppOms int = (SELECT TOP 1 [Id] FROM [content].[Applications] WHERE [Code] = N'OMS');
DECLARE @AppWms int = (SELECT TOP 1 [Id] FROM [content].[Applications] WHERE [Code] = N'WMS_COLD');
DECLARE @AppLos int = (SELECT TOP 1 [Id] FROM [content].[Applications] WHERE [Code] = N'DIGITAL_LOS');
DECLARE @AppPromo int = (SELECT TOP 1 [Id] FROM [content].[Applications] WHERE [Code] = N'LMS_PROMO');

IF @AppOms IS NULL OR @AppWms IS NULL OR @AppLos IS NULL OR @AppPromo IS NULL
BEGIN
    RAISERROR(N'Thiếu Application (OMS / WMS_COLD / DIGITAL_LOS / LMS_PROMO). Hãy chạy seed-all-scenarios-from-okf.sql trước.', 16, 1);
    RETURN;
END;

/* -------------------------------------------------------------------------- */
/* Helper inline: upsert 1 beginner scenario                                  */
/* -------------------------------------------------------------------------- */

-- ========== 1) BEG-OMS-01 · FREE ==========
DECLARE @Code1 nvarchar(40) = N'BEG-OMS-01';
DECLARE @Scen1 int, @Ver1 int, @P1 int, @K1 int;

SELECT @Scen1 = [Id] FROM [scenario].[Scenarios] WHERE [Code] = @Code1;
IF @Scen1 IS NULL
BEGIN
    INSERT INTO [scenario].[Scenarios]
        ([ApplicationId], [CreatedByUserId], [Code], [Title], [Description], [Status], [CreatedAt])
    VALUES
        (@AppOms, @InstructorId, @Code1,
         N'OmniMart Starter — Theo dõi trạng thái đơn hàng cơ bản',
         N'Beginner: học viên làm quen BA qua bài toán trạng thái đơn hàng online vs cửa hàng, phỏng vấn 1 stakeholder và lập BAP ngắn.',
         N'Published', SYSDATETIMEOFFSET());
    SET @Scen1 = SCOPE_IDENTITY();
END
ELSE
    UPDATE [scenario].[Scenarios] SET [Status] = N'Published', [ApplicationId] = @AppOms, [UpdatedAt] = SYSDATETIMEOFFSET() WHERE [Id] = @Scen1;

SELECT @Ver1 = [Id] FROM [scenario].[ScenarioVersions] WHERE [ScenarioId] = @Scen1 AND [VersionNo] = 1;
IF @Ver1 IS NULL
BEGIN
    INSERT INTO [scenario].[ScenarioVersions] (
        [ScenarioId], [VersionNo], [Summary], [BusinessContext], [BusinessProblem], [BusinessObjectives],
        [BARole], [Assignment], [InScope], [OutOfScope], [KnownConstraints],
        [Difficulty], [IsFreeAccess], [Status], [IsPublished], [PublishedAt], [CreatedAt]
    ) VALUES (
        @Scen1, 1,
        N'Beginner OMS: làm rõ trạng thái đơn hàng giữa website và cửa hàng.',
        N'OmniMart Starter là phiên bản rút gọn cho trainee BA. Chuỗi có website và vài cửa hàng; đội CSKH thường bị hỏi “đơn của tôi đang ở đâu?” nhưng mỗi kênh dùng tên trạng thái khác nhau.',
        N'1) Khách không hiểu trạng thái đơn trên app. 2) CSKH phải gọi kho thủ công. 3) Chưa có danh mục trạng thái chuẩn dùng chung.',
        N'1) Liệt kê danh mục trạng thái đơn chuẩn. 2) Xác định stakeholder chịu trách nhiệm cập nhật từng trạng thái. 3) Soạn BAP ngắn và 5 câu hỏi phỏng vấn.',
        N'Bạn là Junior Business Analyst mới vào dự án OmniMart Starter, phụ trách khảo sát hiện trạng trạng thái đơn hàng.',
        N'Sprint 1: Đọc context, lập BAP ngắn, stakeholder map (1–2 người).
Sprint 2: Phỏng vấn Operations Coordinator, ghi elicitation notes.
Sprint 3: Chuẩn hóa danh mục trạng thái đơn (as-is / to-be).
Sprint 4: Viết Problem Statement ngắn + 3 user stories cơ bản.',
        N'- Phạm vi: vòng đời trạng thái đơn online cơ bản (đặt → xác nhận → đóng gói → giao → hoàn tất/hủy).',
        N'- Ngoài phạm vi: thanh toán cổng, loyalty, thay POS.',
        N'- Thời gian khảo sát ≤ 1 tuần học. Không yêu cầu thay hệ thống lõi.',
        N'Beginner', 1, N'Published', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    );
    SET @Ver1 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE [scenario].[ScenarioVersions]
    SET [Difficulty] = N'Beginner', [IsFreeAccess] = 1, [Status] = N'Published',
        [IsPublished] = 1, [PublishedAt] = ISNULL([PublishedAt], SYSDATETIMEOFFSET())
    WHERE [Id] = @Ver1;
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver1 AND [Name] = N'Phạm Thu Hà')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas]
        ([ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities], [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive])
    VALUES
        (@Ver1, N'Phạm Thu Hà', N'Operations Coordinator', N'Vận hành',
         N'Điều phối đơn giữa website và cửa hàng, cập nhật trạng thái thủ công trên Excel khi hệ thống lệch.',
         N'Muốn một danh mục trạng thái đơn thống nhất để CSKH đỡ hỏi lại.',
         N'Phải trả lời chat nội bộ mỗi khi khách hỏi “đơn đang ở đâu”.',
         N'Khách đánh giá 1 sao vì thiếu minh bạch trạng thái.',
         N'Ngắn gọn, thích ví dụ thực tế, hay đưa số liệu ca làm việc.',
         N'Thực tế, hợp tác nếu learner hỏi cụ thể.', 1, 1);
END
SET @P1 = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver1 AND [Name] = N'Phạm Thu Hà');

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver1 AND [Code] = N'K-BEG-OMS-01')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems]
        ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES
        (@Ver1, N'K-BEG-OMS-01', N'Danh mục trạng thái đơn hiện tại (as-is)',
         N'Website dùng: New / Paid / Shipping / Done.
Cửa hàng dùng: Chờ lấy / Đang gói / Đã giao.
CSKH ghi chú tự do trên ticket (không chuẩn hóa).
Tỷ lệ ticket “hỏi trạng thái đơn” ≈ 35% tổng ticket tuần.',
         N'BusinessFact', N'Medium', SYSDATETIMEOFFSET());
END
SET @K1 = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver1 AND [Code] = N'K-BEG-OMS-01');

IF @P1 IS NOT NULL AND @K1 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @P1 AND [KnowledgeItemId] = @K1)
BEGIN
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@P1, @K1, N'Knows', N'Tiết lộ khi learner hỏi về trạng thái đơn hoặc ticket CSKH.', 0);
END;

PRINT N'>> Upserted ' + @Code1 + N' (FREE)';

-- ========== 2) BEG-WMS-01 · FREE ==========
DECLARE @Code2 nvarchar(40) = N'BEG-WMS-01';
DECLARE @Scen2 int, @Ver2 int, @P2 int, @K2 int;

SELECT @Scen2 = [Id] FROM [scenario].[Scenarios] WHERE [Code] = @Code2;
IF @Scen2 IS NULL
BEGIN
    INSERT INTO [scenario].[Scenarios]
        ([ApplicationId], [CreatedByUserId], [Code], [Title], [Description], [Status], [CreatedAt])
    VALUES
        (@AppWms, @InstructorId, @Code2,
         N'AquaCold Starter — Theo dõi nhiệt độ kho lạnh cơ bản',
         N'Beginner: làm quen BA trong cold-chain WMS, phỏng vấn giám sát kho về cảnh báo nhiệt độ và nhật ký sự cố đơn giản.',
         N'Published', SYSDATETIMEOFFSET());
    SET @Scen2 = SCOPE_IDENTITY();
END
ELSE
    UPDATE [scenario].[Scenarios] SET [Status] = N'Published', [ApplicationId] = @AppWms, [UpdatedAt] = SYSDATETIMEOFFSET() WHERE [Id] = @Scen2;

SELECT @Ver2 = [Id] FROM [scenario].[ScenarioVersions] WHERE [ScenarioId] = @Scen2 AND [VersionNo] = 1;
IF @Ver2 IS NULL
BEGIN
    INSERT INTO [scenario].[ScenarioVersions] (
        [ScenarioId], [VersionNo], [Summary], [BusinessContext], [BusinessProblem], [BusinessObjectives],
        [BARole], [Assignment], [InScope], [OutOfScope], [KnownConstraints],
        [Difficulty], [IsFreeAccess], [Status], [IsPublished], [PublishedAt], [CreatedAt]
    ) VALUES (
        @Scen2, 1,
        N'Beginner WMS lạnh: hiểu cảnh báo nhiệt độ và quy trình ghi nhận sự cố.',
        N'AquaCold Starter vận hành 1 kho lạnh nhỏ cho thực phẩm đông lạnh. Cảm biến IoT ghi nhiệt độ nhưng cảnh báo đang gửi chung một nhóm Zalo, dễ bỏ sót ca đêm.',
        N'1) Cảnh báo nhiệt độ không phân mức nghiêm trọng. 2) Không có form ghi nhận sự cố chuẩn. 3) BA trainee cần làm rõ as-is trước khi đề xuất cải tiến.',
        N'1) Mô tả quy trình nhận cảnh báo hiện tại. 2) Phân loại mức cảnh báo cơ bản. 3) Đề xuất trường dữ liệu tối thiểu cho nhật ký sự cố.',
        N'Bạn là Junior BA hỗ trợ khảo sát vận hành kho lạnh AquaCold Starter.',
        N'Sprint 1: Đọc context + BAP ngắn.
Sprint 2: Phỏng vấn Warehouse Supervisor.
Sprint 3: Lập bảng mức cảnh báo (Info / Warning / Critical).
Sprint 4: Viết 3 yêu cầu chức năng nhật ký sự cố nhiệt độ.',
        N'- Phạm vi: cảnh báo nhiệt độ 1 kho, nhật ký sự cố thủ công/bán tự động.',
        N'- Ngoài phạm vi: tối ưu routing xe lạnh, thay toàn bộ WMS.',
        N'- Ngân sách giai đoạn học tập; không mua cảm biến mới.',
        N'Beginner', 1, N'Published', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    );
    SET @Ver2 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE [scenario].[ScenarioVersions]
    SET [Difficulty] = N'Beginner', [IsFreeAccess] = 1, [Status] = N'Published',
        [IsPublished] = 1, [PublishedAt] = ISNULL([PublishedAt], SYSDATETIMEOFFSET())
    WHERE [Id] = @Ver2;
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver2 AND [Name] = N'Đỗ Văn Khoa')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas]
        ([ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities], [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive])
    VALUES
        (@Ver2, N'Đỗ Văn Khoa', N'Warehouse Supervisor', N'Kho lạnh',
         N'Giám sát ca kho, phản ứng khi nhiệt độ vượt ngưỡng, báo cáo sự cố cho quản lý.',
         N'Muốn cảnh báo rõ mức độ và template ghi sự cố trong 2 phút.',
         N'Zalo nhóm quá ồn; ca đêm bỏ lỡ tin nhắn.',
         N'Hư hàng nếu phản ứng chậm — mất hàng và uy tín khách 3PL.',
         N'Thẳng, dùng số liệu ca, ít kiên nhẫn với câu hỏi mơ hồ.',
         N'Thực dụng, coi trọng an toàn hàng hóa.', 1, 1);
END
SET @P2 = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver2 AND [Name] = N'Đỗ Văn Khoa');

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver2 AND [Code] = N'K-BEG-WMS-01')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems]
        ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES
        (@Ver2, N'K-BEG-WMS-01', N'Ngưỡng nhiệt độ kho đông hiện hành',
         N'Kho đông mục tiêu: -18°C ± 2°C.
Warning: vượt -15°C quá 10 phút.
Critical: vượt -12°C hoặc mất điện máy lạnh.
Hiện mọi mức đều gửi cùng một tin Zalo “Nhiệt độ bất thường”.',
         N'BusinessFact', N'High', SYSDATETIMEOFFSET());
END
SET @K2 = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver2 AND [Code] = N'K-BEG-WMS-01');

IF @P2 IS NOT NULL AND @K2 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @P2 AND [KnowledgeItemId] = @K2)
BEGIN
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@P2, @K2, N'Knows', N'Tiết lộ khi hỏi ngưỡng nhiệt độ hoặc quy trình cảnh báo.', 0);
END;

PRINT N'>> Upserted ' + @Code2 + N' (FREE)';

-- ========== 3) BEG-LOS-01 · PAID ==========
DECLARE @Code3 nvarchar(40) = N'BEG-LOS-01';
DECLARE @Scen3 int, @Ver3 int, @P3 int, @K3 int;

SELECT @Scen3 = [Id] FROM [scenario].[Scenarios] WHERE [Code] = @Code3;
IF @Scen3 IS NULL
BEGIN
    INSERT INTO [scenario].[Scenarios]
        ([ApplicationId], [CreatedByUserId], [Code], [Title], [Description], [Status], [CreatedAt])
    VALUES
        (@AppLos, @InstructorId, @Code3,
         N'FinNova Starter — Làm rõ bước eKYC khoản vay tiêu dùng',
         N'Beginner: khảo sát luồng eKYC cơ bản trong Digital LOS, phỏng vấn Credit Ops về điểm nghẽn hồ sơ bị treo.',
         N'Published', SYSDATETIMEOFFSET());
    SET @Scen3 = SCOPE_IDENTITY();
END
ELSE
    UPDATE [scenario].[Scenarios] SET [Status] = N'Published', [ApplicationId] = @AppLos, [UpdatedAt] = SYSDATETIMEOFFSET() WHERE [Id] = @Scen3;

SELECT @Ver3 = [Id] FROM [scenario].[ScenarioVersions] WHERE [ScenarioId] = @Scen3 AND [VersionNo] = 1;
IF @Ver3 IS NULL
BEGIN
    INSERT INTO [scenario].[ScenarioVersions] (
        [ScenarioId], [VersionNo], [Summary], [BusinessContext], [BusinessProblem], [BusinessObjectives],
        [BARole], [Assignment], [InScope], [OutOfScope], [KnownConstraints],
        [Difficulty], [IsFreeAccess], [Status], [IsPublished], [PublishedAt], [CreatedAt]
    ) VALUES (
        @Scen3, 1,
        N'Beginner LOS: hiểu các bước eKYC và lý do hồ sơ bị treo.',
        N'FinNova Starter cung cấp khoản vay tiêu dùng online. Khách chụp CCCD và selfie; một phần hồ sơ bị treo ở bước “chờ đối soát thủ công” mà không có lý do rõ trên UI nội bộ.',
        N'1) Ops không biết vì sao hồ sơ treo. 2) Khách bị gọi đi gọi lại. 3) Chưa có taxonomy lý do reject/hold eKYC cho BA trainee luyện tập.',
        N'1) Vẽ as-is 5 bước eKYC. 2) Liệt kê lý do hold phổ biến. 3) Đề xuất trường “hold reason” tối thiểu cho màn hình Ops.',
        N'Bạn là Junior BA hỗ trợ đội Credit Operations của FinNova Starter.',
        N'Sprint 1: BAP ngắn + phạm vi eKYC.
Sprint 2: Phỏng vấn Credit Ops Officer.
Sprint 3: As-is eKYC steps + hold reasons.
Sprint 4: 3 user stories cho màn hình hold reason.',
        N'- Phạm vi: bước eKYC CCCD + selfie + hold/reject lý do cơ bản.',
        N'- Ngoài phạm vi: mô hình credit scoring, deepfake detection nâng cao.',
        N'- Tuân thủ lưu trữ ảnh KYC theo chính sách nội bộ (không đưa PII ra ngoài bài nộp).',
        N'Beginner', 0, N'Published', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    );
    SET @Ver3 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE [scenario].[ScenarioVersions]
    SET [Difficulty] = N'Beginner', [IsFreeAccess] = 0, [Status] = N'Published',
        [IsPublished] = 1, [PublishedAt] = ISNULL([PublishedAt], SYSDATETIMEOFFSET())
    WHERE [Id] = @Ver3;
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver3 AND [Name] = N'Ngô Minh Châu')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas]
        ([ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities], [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive])
    VALUES
        (@Ver3, N'Ngô Minh Châu', N'Credit Ops Officer', N'Vận hành tín dụng',
         N'Xử lý hàng đợi hồ sơ eKYC bị treo, gọi khách bổ sung giấy tờ.',
         N'Muốn thấy lý do hold ngay trên danh sách hồ sơ.',
         N'Phải mở 3 màn hình và đọc log thô để đoán nguyên nhân.',
         N'Thời gian xử lý SLA 4 giờ bị trễ → khách bỏ cuộc.',
         N'Cẩn thận, thích checklist, hay hỏi “bước nào cụ thể?”.',
         N'Kỷ luật quy trình, ít nói lan man.', 1, 1);
END
SET @P3 = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver3 AND [Name] = N'Ngô Minh Châu');

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver3 AND [Code] = N'K-BEG-LOS-01')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems]
        ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES
        (@Ver3, N'K-BEG-LOS-01', N'Top lý do hold eKYC tuần gần nhất',
         N'Top hold (ước lượng Ops):
1) Ảnh CCCD mờ / lóa (42%)
2) Selfie không khớp góc mặt (27%)
3) CCCD hết hạn (15%)
4) Khác / chưa phân loại (16%) — đang khiến Ops mất thời gian nhất.',
         N'BusinessFact', N'High', SYSDATETIMEOFFSET());
END
SET @K3 = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver3 AND [Code] = N'K-BEG-LOS-01');

IF @P3 IS NOT NULL AND @K3 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @P3 AND [KnowledgeItemId] = @K3)
BEGIN
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@P3, @K3, N'Knows', N'Tiết lộ khi hỏi về lý do hold hoặc thống kê eKYC.', 0);
END;

PRINT N'>> Upserted ' + @Code3 + N' (PAID)';

-- ========== 4) BEG-PROMO-01 · PAID ==========
DECLARE @Code4 nvarchar(40) = N'BEG-PROMO-01';
DECLARE @Scen4 int, @Ver4 int, @P4 int, @K4 int;

SELECT @Scen4 = [Id] FROM [scenario].[Scenarios] WHERE [Code] = @Code4;
IF @Scen4 IS NULL
BEGIN
    INSERT INTO [scenario].[Scenarios]
        ([ApplicationId], [CreatedByUserId], [Code], [Title], [Description], [Status], [CreatedAt])
    VALUES
        (@AppPromo, @InstructorId, @Code4,
         N'MegaFashion Starter — Làm rõ điều kiện voucher cơ bản',
         N'Beginner: khảo sát điều kiện áp dụng voucher đơn giản trong Promotion Engine, phỏng vấn Marketing Ops.',
         N'Published', SYSDATETIMEOFFSET());
    SET @Scen4 = SCOPE_IDENTITY();
END
ELSE
    UPDATE [scenario].[Scenarios] SET [Status] = N'Published', [ApplicationId] = @AppPromo, [UpdatedAt] = SYSDATETIMEOFFSET() WHERE [Id] = @Scen4;

SELECT @Ver4 = [Id] FROM [scenario].[ScenarioVersions] WHERE [ScenarioId] = @Scen4 AND [VersionNo] = 1;
IF @Ver4 IS NULL
BEGIN
    INSERT INTO [scenario].[ScenarioVersions] (
        [ScenarioId], [VersionNo], [Summary], [BusinessContext], [BusinessProblem], [BusinessObjectives],
        [BARole], [Assignment], [InScope], [OutOfScope], [KnownConstraints],
        [Difficulty], [IsFreeAccess], [Status], [IsPublished], [PublishedAt], [CreatedAt]
    ) VALUES (
        @Scen4, 1,
        N'Beginner Promotion: hiểu điều kiện voucher và xung đột mã giảm giá.',
        N'MegaFashion Starter chạy chiến dịch voucher “GIẢM50K” trên app. Khách báo mã không áp dụng được dù đơn đủ điều kiện; Marketing và POS hiểu điều kiện khác nhau.',
        N'1) Điều kiện voucher viết trên banner khác với cấu hình hệ thống. 2) Không có danh mục rule đơn giản cho BA trainee. 3) Xung đột “không cộng dồn” chưa được mô tả rõ.',
        N'1) Liệt kê điều kiện voucher as-is. 2) Chỉ ra chỗ lệch banner vs hệ thống. 3) Viết 3 acceptance criteria cho 1 voucher mẫu.',
        N'Bạn là Junior BA hỗ trợ Marketing Ops của MegaFashion Starter.',
        N'Sprint 1: BAP ngắn + stakeholder map.
Sprint 2: Phỏng vấn Marketing Ops.
Sprint 3: Bảng điều kiện voucher as-is / to-be.
Sprint 4: 3 AC cho voucher GIẢM50K.',
        N'- Phạm vi: 1 voucher tiền mặt cố định, điều kiện đơn tối thiểu, không cộng dồn.',
        N'- Ngoài phạm vi: engine điểm thưởng phức tạp, cá nhân hóa AI.',
        N'- Không đổi chính sách giá gốc sản phẩm trong bài tập.',
        N'Beginner', 0, N'Published', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    );
    SET @Ver4 = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE [scenario].[ScenarioVersions]
    SET [Difficulty] = N'Beginner', [IsFreeAccess] = 0, [Status] = N'Published',
        [IsPublished] = 1, [PublishedAt] = ISNULL([PublishedAt], SYSDATETIMEOFFSET())
    WHERE [Id] = @Ver4;
END;

IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver4 AND [Name] = N'Vũ Thanh Tâm')
BEGIN
    INSERT INTO [scenario].[StakeholderPersonas]
        ([ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities], [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive])
    VALUES
        (@Ver4, N'Vũ Thanh Tâm', N'Marketing Ops', N'Marketing',
         N'Cấu hình chiến dịch voucher, trả lời khiếu nại mã không dùng được.',
         N'Muốn điều kiện voucher rõ ràng giữa banner, app và POS.',
         N'Khách inbox liên tục; Ops phải giải thích thủ công từng case.',
         N'Chiến dịch bị dừng sớm vì khiếu nại — lãng phí ngân sách ads.',
         N'Nhanh, dùng ví dụ đơn hàng thật, thích bảng điều kiện.',
         N'Hướng kết quả, đôi khi bỏ qua chi tiết kỹ thuật.', 1, 1);
END
SET @P4 = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = @Ver4 AND [Name] = N'Vũ Thanh Tâm');

IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver4 AND [Code] = N'K-BEG-PROMO-01')
BEGIN
    INSERT INTO [scenario].[KnowledgeItems]
        ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])
    VALUES
        (@Ver4, N'K-BEG-PROMO-01', N'Cấu hình voucher GIẢM50K (as-is)',
         N'Banner: “Giảm 50K cho đơn từ 499K, mọi sản phẩm”.
Hệ thống thực tế:
- Min order 499.000đ
- Chỉ áp dụng danh mục Áo (không phải mọi sản phẩm)
- Không cộng dồn với mã FREESHIP
- 1 mã / 1 tài khoản / ngày
Đây là nguồn lệch kỳ vọng khách hàng.',
         N'BusinessFact', N'High', SYSDATETIMEOFFSET());
END
SET @K4 = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = @Ver4 AND [Code] = N'K-BEG-PROMO-01');

IF @P4 IS NOT NULL AND @K4 IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = @P4 AND [KnowledgeItemId] = @K4)
BEGIN
    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])
    VALUES (@P4, @K4, N'Knows', N'Tiết lộ khi hỏi điều kiện voucher hoặc xung đột mã.', 0);
END;

PRINT N'>> Upserted ' + @Code4 + N' (PAID)';

PRINT N'>> Done: 4 Beginner scenarios (BEG-OMS-01, BEG-WMS-01 FREE; BEG-LOS-01, BEG-PROMO-01 paid).';
GO
*/

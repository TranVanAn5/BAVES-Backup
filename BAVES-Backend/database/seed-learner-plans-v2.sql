-- ==============================================================================================
-- BA-VES Learner Plans v2 (Gói 1–4 + add-on Expert)
-- Idempotent: archive gói cũ, upsert gói mới theo Code
-- ==============================================================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'commerce')
BEGIN
    EXEC(N'CREATE SCHEMA [commerce]');
END
GO

-- Archive gói cũ (giữ lịch sử đơn hàng)
UPDATE [commerce].[Plans]
SET [Status] = N'Archived'
WHERE [Code] IN (
    N'PLAN-SCENARIO-SINGLE',
    N'PLAN-APP-RETAIL',
    N'PLAN-ALL-SUBSCRIPTION',
    N'PLAN-REVIEW-ADDON',
    N'PLAN-REVIEW-CONSULTATION'
);
GO

-- Không còn gói Trial: trải nghiệm miễn phí = scenario Beginner có IsFreeAccess
-- (xem seed-free-beginner-scenarios.sql)
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-FREE')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Status] = N'Archived',
        [Description] = N'Đã thay bằng scenario Beginner miễn phí (IsFreeAccess).'
    WHERE [Code] = N'PLAN-FREE';
END
GO

-- Gói 1: mua lẻ 1 Scenario — giá theo độ khó (Beginner 59k / Intermediate 99k / Advanced 149k)
-- Archive flat PLAN-SCENARIO cũ nếu còn
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-SCENARIO')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Status] = N'Archived',
        [Description] = N'Thay bằng PLAN-SCENARIO-BEGINNER / INTERMEDIATE / ADVANCED.'
    WHERE [Code] = N'PLAN-SCENARIO';
END
GO

-- Helper pattern for 3 difficulty-priced scenario SKUs
DECLARE @ScenarioSkus TABLE (
    Code nvarchar(40),
    Name nvarchar(200),
    Description nvarchar(500),
    Price decimal(12,2),
    DisplayOrder int
);
INSERT INTO @ScenarioSkus VALUES
(N'PLAN-SCENARIO-BEGINNER', N'Gói 1 — 1 kịch bản Beginner',
 N'Mở khóa 1 kịch bản Beginner. AI chấm. Không kèm Expert (mua add-on nếu cần).', 59000, 1),
(N'PLAN-SCENARIO-INTERMEDIATE', N'Gói 1 — 1 kịch bản Intermediate',
 N'Mở khóa 1 kịch bản Intermediate. AI chấm. Không kèm Expert (mua add-on nếu cần).', 99000, 2),
(N'PLAN-SCENARIO-ADVANCED', N'Gói 1 — 1 kịch bản Advanced',
 N'Mở khóa 1 kịch bản Advanced. AI chấm. Không kèm Expert (mua add-on nếu cần).', 149000, 3);
-- DisplayOrder 1–3 = Gói 1 theo độ khó; 4–6 = Application / Industry / All (xem bên dưới)

DECLARE @Code nvarchar(40), @Name nvarchar(200), @Desc nvarchar(500), @Price decimal(12,2), @Ord int;
DECLARE sku_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT Code, Name, Description, Price, DisplayOrder FROM @ScenarioSkus;
OPEN sku_cursor;
FETCH NEXT FROM sku_cursor INTO @Code, @Name, @Desc, @Price, @Ord;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = @Code)
        UPDATE [commerce].[Plans]
        SET [Name]=@Name, [Description]=@Desc, [PlanType]=N'ScenarioBundle', [Price]=@Price,
            [Currency]=N'VND', [DurationDays]=60, [SessionQuota]=10, [ReviewCredits]=0,
            [ConsultationCredits]=0, [Status]=N'Published', [DisplayOrder]=@Ord
        WHERE [Code]=@Code;
    ELSE
        INSERT INTO [commerce].[Plans]
            ([Code],[Name],[Description],[PlanType],[Price],[Currency],[DurationDays],[SessionQuota],[ReviewCredits],[ConsultationCredits],[Status],[DisplayOrder])
        VALUES (@Code,@Name,@Desc,N'ScenarioBundle',@Price,N'VND',60,10,0,0,N'Published',@Ord);

    DECLARE @Pid int = (SELECT [Id] FROM [commerce].[Plans] WHERE [Code]=@Code);
    DELETE FROM [commerce].[PlanScopes] WHERE [PlanId]=@Pid;
    INSERT INTO [commerce].[PlanScopes] ([PlanId],[ScopeType],[ScopeId]) VALUES (@Pid, N'Scenario', NULL);

    FETCH NEXT FROM sku_cursor INTO @Code, @Name, @Desc, @Price, @Ord;
END
CLOSE sku_cursor; DEALLOCATE sku_cursor;
GO

-- Gói 2: Application — AI + 2 Expert
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-APPLICATION')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Name] = N'Gói 2 — Trọn Application',
        [Description] = N'Mở mọi kịch bản trong 1 Application. AI chấm toàn bộ + tặng 2 lượt Expert (chọn scenario thật sự muốn chuyên gia chấm).',
        [PlanType] = N'ScenarioBundle',
        [Price] = 449000,
        [Currency] = N'VND',
        [DurationDays] = 90,
        [SessionQuota] = 40,
        [ReviewCredits] = 2,
        [ConsultationCredits] = 0,
        [Status] = N'Published',
        [DisplayOrder] = 4
    WHERE [Code] = N'PLAN-APPLICATION';
END
ELSE
BEGIN
    INSERT INTO [commerce].[Plans]
        ([Code], [Name], [Description], [PlanType], [Price], [Currency], [DurationDays], [SessionQuota], [ReviewCredits], [ConsultationCredits], [Status], [DisplayOrder])
    VALUES
        (N'PLAN-APPLICATION', N'Gói 2 — Trọn Application',
         N'Mở mọi kịch bản trong 1 Application. AI chấm toàn bộ + tặng 2 lượt Expert (chọn scenario thật sự muốn chuyên gia chấm).',
         N'ScenarioBundle', 449000, N'VND', 90, 40, 2, 0, N'Published', 4);
END

DECLARE @P2 int = (SELECT [Id] FROM [commerce].[Plans] WHERE [Code] = N'PLAN-APPLICATION');
DELETE FROM [commerce].[PlanScopes] WHERE [PlanId] = @P2;
INSERT INTO [commerce].[PlanScopes] ([PlanId], [ScopeType], [ScopeId])
VALUES (@P2, N'Application', NULL);
GO

-- Gói 3: Industry — AI + 4 Expert
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-INDUSTRY')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Name] = N'Gói 3 — Trọn Industry',
        [Description] = N'Mở mọi Application/scenario trong 1 ngành. AI chấm không giới hạn phiên (trong hạn gói) + tặng 4 lượt Expert.',
        [PlanType] = N'ScenarioBundle',
        [Price] = 799000,
        [Currency] = N'VND',
        [DurationDays] = 120,
        [SessionQuota] = NULL,
        [ReviewCredits] = 4,
        [ConsultationCredits] = 0,
        [Status] = N'Published',
        [DisplayOrder] = 5
    WHERE [Code] = N'PLAN-INDUSTRY';
END
ELSE
BEGIN
    INSERT INTO [commerce].[Plans]
        ([Code], [Name], [Description], [PlanType], [Price], [Currency], [DurationDays], [SessionQuota], [ReviewCredits], [ConsultationCredits], [Status], [DisplayOrder])
    VALUES
        (N'PLAN-INDUSTRY', N'Gói 3 — Trọn Industry',
         N'Mở mọi Application/scenario trong 1 ngành. AI chấm không giới hạn phiên (trong hạn gói) + tặng 4 lượt Expert.',
         N'ScenarioBundle', 799000, N'VND', 120, NULL, 4, 0, N'Published', 5);
END

DECLARE @P3 int = (SELECT [Id] FROM [commerce].[Plans] WHERE [Code] = N'PLAN-INDUSTRY');
DELETE FROM [commerce].[PlanScopes] WHERE [PlanId] = @P3;
INSERT INTO [commerce].[PlanScopes] ([PlanId], [ScopeType], [ScopeId])
VALUES (@P3, N'Industry', NULL);
GO

-- Gói 4: All-access — AI + 8 Expert + 1 consultation
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-ALL-ACCESS')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Name] = N'Gói 4 — Toàn hệ thống',
        [Description] = N'Truy cập mọi kịch bản published. AI chấm không giới hạn phiên + 8 lượt Expert + 1 buổi cố vấn 1-on-1.',
        [PlanType] = N'Subscription',
        [Price] = 1490000,
        [Currency] = N'VND',
        [DurationDays] = 180,
        [SessionQuota] = NULL,
        [ReviewCredits] = 8,
        [ConsultationCredits] = 1,
        [Status] = N'Published',
        [DisplayOrder] = 6
    WHERE [Code] = N'PLAN-ALL-ACCESS';
END
ELSE
BEGIN
    INSERT INTO [commerce].[Plans]
        ([Code], [Name], [Description], [PlanType], [Price], [Currency], [DurationDays], [SessionQuota], [ReviewCredits], [ConsultationCredits], [Status], [DisplayOrder])
    VALUES
        (N'PLAN-ALL-ACCESS', N'Gói 4 — Toàn hệ thống',
         N'Truy cập mọi kịch bản published. AI chấm không giới hạn phiên + 8 lượt Expert + 1 buổi cố vấn 1-on-1.',
         N'Subscription', 1490000, N'VND', 180, NULL, 8, 1, N'Published', 6);
END

DECLARE @P4 int = (SELECT [Id] FROM [commerce].[Plans] WHERE [Code] = N'PLAN-ALL-ACCESS');
DELETE FROM [commerce].[PlanScopes] WHERE [PlanId] = @P4;
INSERT INTO [commerce].[PlanScopes] ([PlanId], [ScopeType], [ScopeId])
VALUES (@P4, N'All', NULL);
GO

-- Add-on: 1 lượt Expert (cho Gói 1 hoặc khi hết credit)
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-EXPERT-1')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Name] = N'Add-on — 1 lượt Expert chấm',
        [Description] = N'Mua thêm 1 lượt chuyên gia chấm hồ sơ theo rubric BABOK. Không mở thêm kịch bản.',
        [PlanType] = N'ReviewCredit',
        [Price] = 89000,
        [Currency] = N'VND',
        [DurationDays] = NULL,
        [SessionQuota] = NULL,
        [ReviewCredits] = 1,
        [ConsultationCredits] = 0,
        [Status] = N'Published',
        [DisplayOrder] = 10
    WHERE [Code] = N'PLAN-EXPERT-1';
END
ELSE
BEGIN
    INSERT INTO [commerce].[Plans]
        ([Code], [Name], [Description], [PlanType], [Price], [Currency], [DurationDays], [SessionQuota], [ReviewCredits], [ConsultationCredits], [Status], [DisplayOrder])
    VALUES
        (N'PLAN-EXPERT-1', N'Add-on — 1 lượt Expert chấm',
         N'Mua thêm 1 lượt chuyên gia chấm hồ sơ theo rubric BABOK. Không mở thêm kịch bản.',
         N'ReviewCredit', 89000, N'VND', NULL, NULL, 1, 0, N'Published', 10);
END
GO

-- Add-on: 3 lượt Expert (tiết kiệm ~15%)
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-EXPERT-3')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Name] = N'Add-on — 3 lượt Expert chấm',
        [Description] = N'Gói 3 lượt Expert (~15% rẻ hơn mua lẻ). Dùng cho bất kỳ scenario đã mở khóa.',
        [PlanType] = N'ReviewCredit',
        [Price] = 229000,
        [Currency] = N'VND',
        [DurationDays] = NULL,
        [SessionQuota] = NULL,
        [ReviewCredits] = 3,
        [ConsultationCredits] = 0,
        [Status] = N'Published',
        [DisplayOrder] = 11
    WHERE [Code] = N'PLAN-EXPERT-3';
END
ELSE
BEGIN
    INSERT INTO [commerce].[Plans]
        ([Code], [Name], [Description], [PlanType], [Price], [Currency], [DurationDays], [SessionQuota], [ReviewCredits], [ConsultationCredits], [Status], [DisplayOrder])
    VALUES
        (N'PLAN-EXPERT-3', N'Add-on — 3 lượt Expert chấm',
         N'Gói 3 lượt Expert (~15% rẻ hơn mua lẻ). Dùng cho bất kỳ scenario đã mở khóa.',
         N'ReviewCredit', 229000, N'VND', NULL, NULL, 3, 0, N'Published', 11);
END
GO

PRINT N'>> Learner plans v2 seeded (PLAN-SCENARIO / APPLICATION / INDUSTRY / ALL-ACCESS + Expert add-ons).';
GO

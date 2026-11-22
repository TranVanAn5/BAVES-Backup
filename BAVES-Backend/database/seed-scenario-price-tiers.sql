-- Giá mua lẻ 1 kịch bản theo độ khó
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-SCENARIO')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Status] = N'Archived',
        [Description] = N'Thay bằng PLAN-SCENARIO-BEGINNER / INTERMEDIATE / ADVANCED.'
    WHERE [Code] = N'PLAN-SCENARIO';
END

DECLARE @ScenarioSkus TABLE (
    Code nvarchar(40),
    Name nvarchar(200),
    Description nvarchar(500),
    Price decimal(12,2),
    DisplayOrder int
);
INSERT INTO @ScenarioSkus VALUES
(N'PLAN-SCENARIO-BEGINNER', N'Gói 1 — 1 kịch bản Beginner',
 N'Mở khóa 1 kịch bản Beginner. AI chấm. Không kèm Expert.', 59000, 1),
(N'PLAN-SCENARIO-INTERMEDIATE', N'Gói 1 — 1 kịch bản Intermediate',
 N'Mở khóa 1 kịch bản Intermediate. AI chấm. Không kèm Expert.', 99000, 2),
(N'PLAN-SCENARIO-ADVANCED', N'Gói 1 — 1 kịch bản Advanced',
 N'Mở khóa 1 kịch bản Advanced. AI chấm. Không kèm Expert.', 149000, 3);

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

SELECT [Code], [Price], [Status] FROM [commerce].[Plans]
WHERE [Code] LIKE N'PLAN-SCENARIO%'
ORDER BY [DisplayOrder], [Code];
GO

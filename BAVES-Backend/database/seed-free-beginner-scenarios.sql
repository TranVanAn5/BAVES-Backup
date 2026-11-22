-- ==============================================================================================
-- Bỏ gói Trial + gắn miễn phí cho một số scenario Beginner
-- ==============================================================================================
SET NOCOUNT ON;

-- 1) Archive gói miễn phí (nếu còn)
IF EXISTS (SELECT 1 FROM [commerce].[Plans] WHERE [Code] = N'PLAN-FREE')
BEGIN
    UPDATE [commerce].[Plans]
    SET [Status] = N'Archived',
        [Name] = N'(Archived) Gói trải nghiệm (Trial)',
        [Description] = N'Đã thay bằng scenario Beginner miễn phí (IsFreeAccess).'
    WHERE [Code] = N'PLAN-FREE';
END
GO

-- 2) Cột IsFreeAccess trên ScenarioVersions
IF COL_LENGTH(N'scenario.ScenarioVersions', N'IsFreeAccess') IS NULL
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions]
        ADD [IsFreeAccess] bit NOT NULL CONSTRAINT [DF_ScenarioVersions_IsFreeAccess] DEFAULT (0);
END
GO

-- 3) Gắn miễn phí cho một số scenario Beginner đã publish
--    Mặc định: OmniMart (OMNI-001) nếu có; nếu không lấy tối đa 2 scenario published đầu tiên.
DECLARE @FreeCodes TABLE (Code nvarchar(50));
INSERT INTO @FreeCodes (Code) VALUES (N'OMNI-001');
INSERT INTO @FreeCodes (Code) VALUES (N'BEG-CAFE-01');
INSERT INTO @FreeCodes (Code) VALUES (N'BEG-OMS-01');

-- Nếu chưa có OMNI-001, chọn tối đa 2 scenario published bất kỳ
IF NOT EXISTS (
    SELECT 1
    FROM [scenario].[Scenarios] s
    INNER JOIN @FreeCodes f ON f.Code = s.Code
)
BEGIN
    INSERT INTO @FreeCodes (Code)
    SELECT TOP (2) s.Code
    FROM [scenario].[Scenarios] s
    WHERE s.Status = N'Published'
    ORDER BY s.Id;
END

UPDATE v
SET
    v.[Difficulty] = N'Beginner',
    v.[IsFreeAccess] = 1
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.Id = v.ScenarioId
INNER JOIN @FreeCodes f ON f.Code = s.Code
WHERE v.[IsPublished] = 1;

PRINT N'>> Free beginner scenarios seeded; PLAN-FREE archived.';
GO

-- ====================================================
-- Migrate Assignment JSON → scenario.ScenarioSprints
-- Chạy sau EF migration AddScenarioSprints.
-- ====================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF OBJECT_ID(N'scenario.ScenarioSprints', N'U') IS NULL
BEGIN
    RAISERROR(N'Table scenario.ScenarioSprints chưa tồn tại. Chạy EF migration AddScenarioSprints trước.', 16, 1);
    RETURN;
END;

DECLARE @Id int, @Assignment nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT v.[Id], v.[Assignment]
    FROM [scenario].[ScenarioVersions] v
    WHERE NOT EXISTS (
        SELECT 1 FROM [scenario].[ScenarioSprints] s WHERE s.[ScenarioVersionId] = v.[Id]
    )
      AND v.[Assignment] IS NOT NULL
      AND LTRIM(RTRIM(v.[Assignment])) LIKE N'{%';

OPEN cur;
FETCH NEXT FROM cur INTO @Id, @Assignment;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        IF ISJSON(@Assignment) = 1 AND JSON_QUERY(@Assignment, '$.sprints') IS NOT NULL
        BEGIN
            INSERT INTO [scenario].[ScenarioSprints]
                ([ScenarioVersionId], [SprintNo], [Focus], [DurationValue], [DurationUnit], [TemplateCodes])
            SELECT
                @Id,
                ROW_NUMBER() OVER (ORDER BY ISNULL(TRY_CAST(JSON_VALUE(j.[value], '$.sprintNo') AS int), 9999), TRY_CAST(j.[key] AS int)),
                ISNULL(LTRIM(RTRIM(JSON_VALUE(j.[value], '$.focus'))), N''),
                CASE
                    WHEN TRY_CAST(JSON_VALUE(j.[value], '$.durationValue') AS int) > 0
                    THEN TRY_CAST(JSON_VALUE(j.[value], '$.durationValue') AS int)
                    ELSE 1
                END,
                CASE
                    WHEN LOWER(ISNULL(JSON_VALUE(j.[value], '$.durationUnit'), N'weeks')) LIKE N'd%'
                    THEN N'days'
                    ELSE N'weeks'
                END,
                NULLIF(
                    REPLACE(REPLACE(REPLACE(
                        ISNULL(JSON_QUERY(j.[value], '$.templateCodes'), N''),
                        N'[', N''), N']', N''), N'"', N''),
                    N'')
            FROM OPENJSON(@Assignment, '$.sprints') AS j;

            DECLARE @brief nvarchar(max);
            SELECT @brief = STRING_AGG(
                CONCAT(
                    N'Sprint ', s.[SprintNo], N' (', s.[DurationValue], N' ',
                    CASE WHEN s.[DurationUnit] = N'days' THEN N'ngày' ELSE N'tuần' END,
                    N'): ',
                    CASE WHEN LEN(s.[Focus]) > 80 THEN LEFT(s.[Focus], 80) + N'…' ELSE s.[Focus] END
                ),
                N' '
            ) WITHIN GROUP (ORDER BY s.[SprintNo])
            FROM [scenario].[ScenarioSprints] s
            WHERE s.[ScenarioVersionId] = @Id;

            IF @brief IS NOT NULL AND LEN(@brief) > 0
                UPDATE [scenario].[ScenarioVersions]
                SET [Assignment] = @brief
                WHERE [Id] = @Id;
        END
    END TRY
    BEGIN CATCH
        PRINT CONCAT(N'Skip version ', @Id, N': ', ERROR_MESSAGE());
    END CATCH;

    FETCH NEXT FROM cur INTO @Id, @Assignment;
END;

CLOSE cur;
DEALLOCATE cur;

SELECT COUNT(DISTINCT [ScenarioVersionId]) AS VersionsWithSprints
FROM [scenario].[ScenarioSprints];

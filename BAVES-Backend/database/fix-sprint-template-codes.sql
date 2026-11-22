/*
  Gán TemplateCodes (DEL-01..DEL-08) cho mọi ScenarioSprints theo đúng template hệ thống
  và hợp lý với focus từng sprint.

  Catalog:
    DEL-01 Problem Statement
    DEL-02 Business Analysis Plan (BAP)
    DEL-03 Stakeholder Analysis
    DEL-04 Elicitation Results
    DEL-05 Business Requirements (BRD)
    DEL-06 Process Model
    DEL-07 Requirements Specification (SRS)
    DEL-08 Traceability Matrix (RTM)

  Quy ước: mỗi sprint gắn deliverable *mới* của sprint đó.
*/

SET NOCOUNT ON;

-- 1) Mặc định theo số sprint (kịch bản chuẩn)
UPDATE sp
SET sp.[TemplateCodes] = CASE sp.[SprintNo]
    WHEN 1 THEN N'DEL-02,DEL-03'   -- BAP + Stakeholder Analysis
    WHEN 2 THEN N'DEL-04,DEL-05'   -- Elicitation Results + BRD
    WHEN 3 THEN N'DEL-07'          -- SRS
    WHEN 4 THEN N'DEL-08'          -- RTM
    ELSE sp.[TemplateCodes]
END
FROM [scenario].[ScenarioSprints] sp;

-- 2) Sprint 3 thiên về quy trình / as-is / flow → Process Model + SRS
UPDATE sp
SET sp.[TemplateCodes] = N'DEL-06,DEL-07'
FROM [scenario].[ScenarioSprints] sp
WHERE sp.[SprintNo] = 3
  AND (
        sp.[Focus] LIKE N'%quy trình%'
     OR sp.[Focus] LIKE N'%as-is%'
     OR sp.[Focus] LIKE N'%to-be%'
     OR sp.[Focus] LIKE N'%State catalog%'
     OR sp.[Focus] LIKE N'%taxonomy%'
     OR sp.[Focus] LIKE N'%acknowledge flow%'
     OR sp.[Focus] LIKE N'%Severity matrix%'
     OR sp.[Focus] LIKE N'%kho lạnh%'
     OR sp.[Focus] LIKE N'%WMS%'
  );

-- 3) Sprint 3 Rule table / business rules (không nhấn SRS) → BRD
UPDATE sp
SET sp.[TemplateCodes] = N'DEL-05'
FROM [scenario].[ScenarioSprints] sp
WHERE sp.[SprintNo] = 3
  AND (
        sp.[Focus] LIKE N'%Rule table%'
     OR (sp.[Focus] LIKE N'%Business Rules%' AND sp.[Focus] NOT LIKE N'%SRS%')
  );

-- 4) Beginner sprint 4: User Stories + AC → SRS (chưa RTM)
UPDATE sp
SET sp.[TemplateCodes] = N'DEL-07'
FROM [scenario].[ScenarioSprints] sp
WHERE sp.[SprintNo] = 4
  AND (
        sp.[Focus] LIKE N'%User stor%'
     OR sp.[Focus] LIKE N'%user stor%'
     OR sp.[Focus] LIKE N'%US + AC%'
     OR sp.[Focus] LIKE N'%3 US%'
  );

-- 5) Beginner (BEG-*): Sprint 2 chỉ elicitation
UPDATE sp
SET sp.[TemplateCodes] = N'DEL-04'
FROM [scenario].[ScenarioSprints] sp
JOIN [scenario].[ScenarioVersions] v ON v.[Id] = sp.[ScenarioVersionId]
JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE sp.[SprintNo] = 2
  AND s.[Code] LIKE N'BEG-%';

-- 6) Beginner promo Sprint 3: Rule table → BRD
UPDATE sp
SET sp.[TemplateCodes] = N'DEL-05'
FROM [scenario].[ScenarioSprints] sp
JOIN [scenario].[ScenarioVersions] v ON v.[Id] = sp.[ScenarioVersionId]
JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE sp.[SprintNo] = 3
  AND s.[Code] = N'BEG-PROMO-01';

-- 7) Phiên bản 2 sprint (DEMO): S1 planning, S2 elicitation + BRD
UPDATE sp
SET sp.[TemplateCodes] = CASE sp.[SprintNo]
    WHEN 1 THEN N'DEL-02,DEL-03'
    WHEN 2 THEN N'DEL-04,DEL-05'
    ELSE sp.[TemplateCodes]
END
FROM [scenario].[ScenarioSprints] sp
WHERE sp.[ScenarioVersionId] IN (
    SELECT v.[Id]
    FROM [scenario].[ScenarioVersions] v
    WHERE (SELECT COUNT(*) FROM [scenario].[ScenarioSprints] x WHERE x.[ScenarioVersionId] = v.[Id]) = 2
);

SELECT
    sc.[Code],
    sp.[SprintNo],
    sp.[TemplateCodes],
    LEFT(sp.[Focus], 72) AS Focus
FROM [scenario].[ScenarioSprints] sp
JOIN [scenario].[ScenarioVersions] v ON v.[Id] = sp.[ScenarioVersionId]
JOIN [scenario].[Scenarios] sc ON sc.[Id] = v.[ScenarioId]
ORDER BY sc.[Code], sp.[SprintNo];

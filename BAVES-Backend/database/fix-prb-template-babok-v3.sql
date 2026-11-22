-- DEL-01 Problem Statement sections aligned to BABOK v3 Strategy Analysis (English)
SET NOCOUNT ON;
BEGIN TRANSACTION;

UPDATE [artifact].[ArtifactTemplates]
SET
    [Name] = N'Problem Statement & Opportunity Assessment - BABOK v3',
    [Description] = N'BABOK Guide v3 Strategy Analysis: business need, current state, root cause, future-state outcomes, gaps, risks/constraints, and evidence.'
WHERE [Id] = 1 OR [Code] = N'DEL-01';

DELETE FROM [artifact].[ArtifactTemplateSections]
WHERE [ArtifactTemplateId] IN (SELECT [Id] FROM [artifact].[ArtifactTemplates] WHERE [Id] = 1 OR [Code] = N'DEL-01');

DECLARE @PrbId int;
SELECT @PrbId = [Id] FROM [artifact].[ArtifactTemplates] WHERE [Code] = N'DEL-01';
IF @PrbId IS NULL SET @PrbId = 1;

INSERT INTO [artifact].[ArtifactTemplateSections]
    ([ArtifactTemplateId], [SectionKey], [Title], [Instructions], [DisplayOrder], [IsRequired])
VALUES
(@PrbId, N'document_governance', N'0. Document Governance', N'Record standard (BABOK v3 Strategy Analysis), project, Lead BA, Sponsor, status, and DEL-01.', 1, 1),
(@PrbId, N'business_need', N'1. Business Need & Problem Statement', N'Describe What, Who, Where & When, and quantified Impact.', 2, 1),
(@PrbId, N'current_state', N'2. Current State Summary', N'Summarize as-is capabilities, systems/information, pain points, and what already works.', 3, 1),
(@PrbId, N'root_cause', N'3. Root Cause Analysis (5 Whys)', N'Run 5 Whys to a systemic process/policy/architecture root cause.', 4, 1),
(@PrbId, N'future_state', N'4. Desired Future State & Outcomes', N'State future-state vision and measurable outcomes (baseline/target/measure).', 5, 1),
(@PrbId, N'gaps', N'5. Gaps (Current vs Future)', N'List capability/process, information/data, and technology gaps that block the future state.', 6, 1),
(@PrbId, N'risks_constraints', N'6. Risks, Constraints & Assumptions', N'Document risks to addressing the need, constraints, and assumptions with responses.', 7, 1),
(@PrbId, N'evidence', N'7. Evidence', N'Cite at least one stakeholder quote or system log supporting impact or root cause.', 8, 1);

COMMIT TRANSACTION;
PRINT N'SUCCESSFULLY_UPDATED_PRB_TEMPLATE_BABOK_V3';

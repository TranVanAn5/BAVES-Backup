-- ====================================================
-- BAP (DEL-02) aligned to BABOK v3 Planning & Monitoring
-- Content: English 100%
-- ====================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;

UPDATE [artifact].[ArtifactTemplates]
SET
    [Name] = N'Business Analysis Plan (BAP) - BABOK v3',
    [Description] = N'BABOK Guide v3 Business Analysis Planning and Monitoring: BA Approach, Stakeholder Engagement, BA Governance, Information Management, Deliverables & Schedule, Risks & Assumptions, and BA Performance Metrics.'
WHERE [Id] = 2 OR [Code] = N'DEL-02';

DELETE FROM [artifact].[ArtifactTemplateSections]
WHERE [ArtifactTemplateId] IN (SELECT [Id] FROM [artifact].[ArtifactTemplates] WHERE [Id] = 2 OR [Code] = N'DEL-02');

DECLARE @BapId int;
SELECT @BapId = [Id] FROM [artifact].[ArtifactTemplates] WHERE [Code] = N'DEL-02';
IF @BapId IS NULL SET @BapId = 2;

INSERT INTO [artifact].[ArtifactTemplateSections]
    ([ArtifactTemplateId], [SectionKey], [Title], [Instructions], [DisplayOrder], [IsRequired])
VALUES
(
    @BapId,
    N'document_governance',
    N'0. Document Governance Header',
    N'Fill in English: Standard = BABOK Guide v3 (Business Analysis Planning and Monitoring); Project name & code; Author = [Learner full name] (Lead Business Analyst); Version; Template code DEL-02. State purpose of this BAP in 2-3 sentences.',
    1,
    1
),
(
    @BapId,
    N'ba_approach',
    N'1. Business Analysis Approach',
    N'Per BABOK task Plan Business Analysis Approach: select Predictive / Adaptive / Hybrid and justify for this initiative. Describe planning formality, level of detail, prioritization approach, and how BA work adapts across early definition vs later delivery sprints. Include when predictive artifacts vs adaptive backlog items will be used.',
    2,
    1
),
(
    @BapId,
    N'stakeholder_engagement',
    N'2. Stakeholder Engagement Plan',
    N'Per BABOK task Plan Stakeholder Engagement: for each stakeholder group list (1) engagement objective, (2) collaboration needs, (3) elicitation/collaboration techniques (e.g. Interview, Observation, Workshop, Interface Analysis), (4) cadence and channel. Cover authority, attitude toward change, and decision influence where known. Detailed Power/Interest profiles may reference DEL-03.',
    3,
    1
),
(
    @BapId,
    N'ba_governance',
    N'3. Business Analysis Governance',
    N'Per BABOK task Plan Business Analysis Governance: define decision rights (who approves requirements, prioritization, and scope changes); Change Request process; Impact Analysis expectations; Change Control Board (CCB) or equivalent; escalation path; and definition of “approved” vs “deferred” requirements.',
    4,
    1
),
(
    @BapId,
    N'information_management',
    N'4. Business Analysis Information Management',
    N'Per BABOK task Plan Business Analysis Information Management: define how BA information is organized, stored, accessed, and traced (tools, folders/repositories, naming, versioning). Specify traceability approach from need/problem → evidence → requirements → designs/stories → tests. State reuse and single source of truth rules.',
    5,
    1
),
(
    @BapId,
    N'deliverables_schedule',
    N'5. BA Activities, Deliverables & Schedule',
    N'List planned BA activities mapped to deliverables (e.g. DEL-01 to DEL-08 as applicable - select only those required for this scenario). For each row: Activity, Deliverable/code, Owner, Sprint/milestone, Acceptance check. Do not assume every DEL code is mandatory.',
    6,
    1
),
(
    @BapId,
    N'risks_assumptions',
    N'6. Risks & Assumptions (BA Work)',
    N'Document risks that affect BA approach or engagement (e.g. unavailable stakeholders, conflicting priorities, unstable scope) with likelihood/impact and mitigation. List assumptions and constraints that the BA plan depends on.',
    7,
    1
),
(
    @BapId,
    N'ba_performance_metrics',
    N'7. Business Analysis Performance Metrics',
    N'Per BABOK task Identify Business Analysis Performance Improvements: define how BA effectiveness will be measured during the initiative (e.g. % requirements approved first-pass, elicitation coverage of planned stakeholders, open conflict count, traceability completeness, on-time deliverable rate). State review cadence for improving the BA approach.',
    8,
    1
);

COMMIT TRANSACTION;
PRINT N'SUCCESSFULLY_UPDATED_BAP_TEMPLATE_BABOK_V3';

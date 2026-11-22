-- ====================================================
-- ARTIFACT TEMPLATES: English-only content (name, description, sections)
-- UI chrome may stay Vietnamese; document templates must be 100% English.
-- ====================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;

-- 1. ArtifactTemplates
UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Problem Statement & Opportunity Assessment',
    [Description] = N'Define the core problem statement, root-cause analysis (5 Whys), current-state impact metrics, and OKR targets.'
WHERE [Id] = 1;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Business Analysis Plan (BAP) - BABOK v3',
    [Description] = N'BABOK Guide v3 Business Analysis Planning and Monitoring: BA Approach, Stakeholder Engagement, BA Governance, Information Management, Deliverables & Schedule, Risks & Assumptions, and BA Performance Metrics.'
WHERE [Id] = 2;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Stakeholder Analysis & RACI Matrix',
    [Description] = N'Identify stakeholder profiles, Power/Interest classification, and RACI responsibilities for BA activities.'
WHERE [Id] = 3;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Elicitation Notes (Interview Record)',
    [Description] = N'Structured elicitation meeting notes: questions, responses, evidence references, and conflicts to resolve.'
WHERE [Id] = 4;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Business Requirements Document (BRD)',
    [Description] = N'End-to-end business requirements: In-Scope/Out-of-Scope, functional requirements (FR), non-functional requirements (NFR), and business rules (BR).'
WHERE [Id] = 5;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Business Process Model (BPMN As-Is / To-Be)',
    [Description] = N'Document current-state (As-Is) and future-state (To-Be) processes using BPMN 2.0 swimlanes.'
WHERE [Id] = 6;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Software Requirements & User Stories Backlog (SRS)',
    [Description] = N'Software requirements specification with Agile user stories and measurable acceptance criteria (Given/When/Then).'
WHERE [Id] = 7;

UPDATE [artifact].[ArtifactTemplates]
SET [Name] = N'Requirements Traceability Matrix (RTM)',
    [Description] = N'Two-way traceability from business problem to evidence, BRD items, user stories, and test cases.'
WHERE [Id] = 8;

-- 2. ArtifactTemplateSections
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. Core Problem Statement', [Instructions] = N'Describe What, Who, Where, When, and Impact in clear English.' WHERE [Id] = 1;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. Root Cause Analysis (5 Whys)', [Instructions] = N'Run a 5 Whys chain to identify the root cause.' WHERE [Id] = 2;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'3. Business Opportunity & OKR Framework', [Instructions] = N'Define Baseline, Target, and measurement method for each objective.' WHERE [Id] = 3;
-- BAP sections (DEL-02): prefer running fix-bap-template-babok-v3.sql for full replace.
-- Kept as no-ops if old Ids were deleted; full BAP structure lives in fix-bap-template-babok-v3.sql.
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. Stakeholder Profiles & Pain Points', [Instructions] = N'Capture role, representative, pain points, and expected outcomes.' WHERE [Id] = 8;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. Power/Interest Grid', [Instructions] = N'Classify stakeholders: Manage Closely, Keep Satisfied, Keep Informed, Monitor.' WHERE [Id] = 9;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'3. RACI Matrix', [Instructions] = N'Assign R, A, C, I for each BA activity.' WHERE [Id] = 10;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. Interview Session Context', [Instructions] = N'Record time, participants, and session objectives.' WHERE [Id] = 11;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. Interview Log & Fact Capture', [Instructions] = N'Document Q&A details with each stakeholder.' WHERE [Id] = 12;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'3. Evidence & Conflicts to Resolve', [Instructions] = N'Link evidence IDs and list conflicts that need alignment.' WHERE [Id] = 13;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. Business Scope', [Instructions] = N'Define In-Scope, Out-of-Scope, and Assumptions.' WHERE [Id] = 14;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. Functional Requirements (FR)', [Instructions] = N'List functional requirements prioritized with MoSCoW.' WHERE [Id] = 15;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'3. Business Rules (BR)', [Instructions] = N'Document business rules (e.g., inventory hold, refund, cancellation).' WHERE [Id] = 16;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'4. Non-Functional Requirements (NFR)', [Instructions] = N'Cover performance, security, availability, and resilience.' WHERE [Id] = 17;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. As-Is Process Flow', [Instructions] = N'Describe current steps and bottlenecks.' WHERE [Id] = 18;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. To-Be BPMN Flow', [Instructions] = N'Provide swimlane diagram: Customer, POS, OMS, Warehouse, Delivery.' WHERE [Id] = 19;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'3. Step-level Process Specification', [Instructions] = N'For each step: actor, action, supporting system, and branch conditions.' WHERE [Id] = 20;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. Epics Overview & Feature Breakdown', [Instructions] = N'List Epics and allocate Story Points / priority.' WHERE [Id] = 21;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. User Stories Backlog & Acceptance Criteria', [Instructions] = N'Use As a / I want / So that and Given/When/Then.' WHERE [Id] = 22;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'3. API Contract & Data Model', [Instructions] = N'Specify endpoints, request/response payloads, and HTTP status codes.' WHERE [Id] = 23;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'1. End-to-End Traceability Matrix', [Instructions] = N'Map Problem ID -> Evidence ID -> FR ID -> User Story ID -> Test Case ID.' WHERE [Id] = 24;
UPDATE [artifact].[ArtifactTemplateSections] SET [Title] = N'2. Coverage Metrics & Risk', [Instructions] = N'Report tested-requirement coverage and orphan requirements.' WHERE [Id] = 25;

COMMIT TRANSACTION;
PRINT N'SUCCESSFULLY_UPDATED_ARTIFACT_TEMPLATES_TO_ENGLISH';

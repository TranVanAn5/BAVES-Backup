# BA-VES MVP Models

Implementation source: BA-VES_MVP_Database_and_CSharp_Model_Design.docx. This specification supersedes the earlier full 67-table design and the proposed v2 design for the current implementation.

The MVP maps exactly 40 physical tables across nine schemas. There are 36 custom entity classes; Identity supplies the four claim/login/token entity types. All domain and Identity keys use int. Timestamp fields use DateTimeOffset and DateTimeOffset?, date fields use DateOnly?, and 29 enums are stored as bounded strings.

| Module / schema | Tables | Context |
| --- | --- | --- |
| Identity / identity | Users, Roles, UserRoles, UserClaims, UserLogins, UserTokens, RoleClaims | IdentityDbContext |
| Content / content | Industries, Applications | ContentDbContext |
| Training / training | Cohorts, CohortMembers, Assignments | TrainingDbContext |
| Scenario / scenario | Scenarios, ScenarioVersions, StakeholderPersonas, KnowledgeItems, StakeholderKnowledgeAccess, BusinessRules, ScenarioEvents | ScenarioDbContext |
| Simulation / simulation | SimulationSessions, SessionAgents, Messages, SessionEvents, SessionAgentKnowledgeStates | SimulationDbContext |
| Planning / planning | BAPlans, PlannedQuestions | PlanningDbContext |
| Analysis / analysis | AnalysisItems, MessageEvidenceLinks, AnalysisItemRelations | AnalysisDbContext |
| Artifact / artifact | ArtifactTemplates, ArtifactTemplateSections, Artifacts, ArtifactVersions, Submissions | ArtifactDbContext |
| Assessment / assessment | BabokKnowledgeAreas, BabokTasks, Rubrics, RubricCriteria, AIPracticeFeedbacks, HumanReviews | AssessmentDbContext |

## Structure

Each module in BAVES.Api/Modules owns Models/Entities, Models/Enums, and Data (DbContext, Configurations and Migrations). Controllers, Services/Interfaces, Repositories/Interfaces, Models/DTOs and Models/Requests are scaffolded for future feature implementations. PriorityLevel belongs to Planning and is reused by Analysis, as specified in the document. DbContext configuration discovery is filtered by the owning module namespace to avoid registering foreign tables.

```text
BAVES.Api/
  Modules/
    Scenario/
      Controllers/
      Services/Interfaces/
      Repositories/Interfaces/
      Models/
        Entities/
        DTOs/
        Requests/
        Enums/
      Data/
        ScenarioDbContext.cs
        Configurations/
        Migrations/
  Shared/
    Responses/
    Exceptions/
    Common/
```

The same layout is used for all nine MVP modules, including Training. The solution contains BAVES.Api and the offline model validation project. The former Infrastructure and Shared projects have been consolidated into the API module structure. This folder/namespace change requires no database migration because the generated SQL schema is unchanged.

All contexts use the configured DefaultConnection, with a separate __EFMigrationsHistory table in their own schema. AddBavesModules registers the contexts and the standard Identity stores for ApplicationUser / ApplicationRole / ApplicationUserRole.

## Relationships and Constraints

Within-module relationships have explicit navigation properties and foreign keys with NO ACTION deletion. Cross-module references remain scalar IDs with indexes. This follows the document's permitted MVP approach: service contracts must validate cross-module existence, ownership, scenario consistency and rubric membership before writes. Those services and APIs are outside this model change.

The configuration includes documented uniqueness rules, unique stakeholder/knowledge pairs, one BAPlan per session, and practical uniqueness for cohort membership, runtime personas, version/attempt numbers and typed evidence relationships.

Assignments require exactly one of CohortId or LearnerId. Optional MaxAttempts must be positive. Agent trust/patience/cooperation values are bounded to 0..100; feedback Confidence is bounded to 0..1. JSON fields have ISJSON check constraints. Analysis relations cannot link an item to itself.

The document does not specify string lengths or decimal precision for every field. This implementation uses bounded codes/names/titles, nvarchar(max) for long text and JSON, datetimeoffset(3) for timestamps, decimal(9,4) for scores/weights and decimal(5,4) for Confidence. Required timestamps must be set by the creating service; they are not implicitly derived from a client clock.

AIPracticeFeedback uses FeedbackItemsJson and SuggestedScore. HumanReview uses CriterionResultsJson and FinalScore. A Submission directly references one frozen ArtifactVersion. Structured draft content lives on Artifact; frozen editor content lives on ArtifactVersion.

Published scenario/template/rubric immutability, submission ownership and AI evidence validation require write rules in future services. Models and FK deletion settings alone do not enforce those workflows. The document's Phase 2 tables are intentionally absent from the MVP mapping.

## SQL and Validation

The generated database/BA-VES-MVP.Initial.sql initializes a new, empty database. It creates schemas, tables, indexes and local constraints. It does not migrate existing data or record EF migration history. Do not apply it to an existing full-design database as a conversion script.

Database:EnsureCreated is unsupported with multiple module-owned contexts, because calling it on one context can leave the other schemas uncreated. Use reviewed per-module migrations for an existing deployment, or the initial SQL script for a fresh database.

Run verification from the repository root:

```powershell
dotnet build BAVES.sln --no-restore -m:1 -nr:false -p:UseSharedCompilation=false
dotnet run --project tests/BAVES.ModelValidation/BAVES.ModelValidation.csproj --no-restore
```

The validation fixture was extracted from the supplied document. It checks all 40 table names, every scalar field/type/nullability, Identity key generation, all 29 enum dictionaries/string conversions, module isolation, migration history configuration, and SQL Server DDL generation without opening a database connection.

Regenerate the SQL artifact after changes:

```powershell
dotnet run --project tests/BAVES.ModelValidation/BAVES.ModelValidation.csproj --no-restore -- --export-sql database/BA-VES-MVP.Initial.sql
```

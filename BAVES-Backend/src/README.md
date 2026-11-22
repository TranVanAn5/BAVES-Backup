# BA-VES

Current database implementation follows `BA-VES_MVP_Database_and_CSharp_Model_Design.docx`: 40 tables, nine module-owned schemas/DbContexts, and ASP.NET Core Identity<int>. See [MVP model documentation](BAVES.Api/Modules/README.md). The broader product roadmap below includes capabilities outside this database baseline.

> **AI-Powered Multi-Domain Virtual Enterprise Simulation for Business Analysis Training**

BA-VES is a **desktop-first Business Analysis training platform** that allows learners to practice end-to-end BA activities inside realistic enterprise scenarios. Learners plan BA activities, interact with multiple AI Stakeholders, capture and analyze elicited information, create BA artifacts, receive AI Practice Feedback, and optionally receive a final rubric-based assessment from an Instructor or BA Expert.

The system is designed around **BABOK-aligned learning objectives**, structured BA evidence, traceability, scenario-based simulation, and human-in-the-loop assessment.

---

## 1. Project Overview

Traditional BA learning often focuses heavily on theory, templates, and isolated exercises. BA-VES is designed to provide a more realistic practice environment where learners must:

- understand a business problem;
- plan elicitation activities;
- identify and engage relevant stakeholders;
- interview AI-powered stakeholder agents;
- discover incomplete, hidden, conflicting, or role-specific information;
- convert elicitation results into findings, requirements, business rules, constraints, and open questions;
- maintain traceability between evidence and BA outputs;
- create structured BA artifacts;
- receive formative AI feedback;
- receive final human assessment when required.

BA-VES is **not primarily a chatbot**. Stakeholder chat is only one component inside a broader Business Analysis simulation workflow.

---

## 2. Core Learning Flow

```text
Industry / Application
        ↓
Scenario
        ↓
Scenario Context
        ↓
BA Planning
        ↓
Stakeholder Simulation
        ↓
Capture & Analyze Information
        ↕
Validation / Re-Elicitation
        ↓
BA Artifact Creation
        ↓
Final Submission
        ↓
AI Practice Feedback
        ↓
Optional Human Assessment
        ↓
Result & Progress
```

### Key principle

The learner is responsible for performing the BA work.

AI Stakeholder Agents provide information according to their assigned role, persona, knowledge boundary, goals, and business context. They do **not** automatically produce the learner's final requirements or artifacts.

---

## 3. Main User Roles

### Learner

Learners can:

- browse available Industries, Applications, and Scenarios;
- access purchased or assigned Scenarios;
- review Scenario context and BA assignment;
- create BA plans;
- interact with multiple AI Stakeholders;
- capture Findings and Open Questions;
- create and refine Requirements;
- identify Business Rules, Constraints, Risks, and Conflicts;
- create structured BA Artifacts;
- submit final work;
- receive AI Practice Feedback;
- request Expert Review when available;
- view Human Assessment results and progress.

### Instructor / BA Expert

Instructors and Experts can:

- create or manage private Scenarios;
- configure Stakeholders and knowledge;
- map Scenario objectives to BABOK tasks;
- define required BA artifacts;
- configure assessment rubrics;
- create Cohorts and Assignments;
- review learner submissions;
- inspect supporting evidence and traceability;
- score learners using BABOK-aligned criteria;
- provide professional feedback.

### Administrator

Administrators manage:

- users and roles;
- organizations;
- Industries and Applications;
- official Scenarios;
- artifact templates;
- product packages and access policies;
- supported AI providers;
- system configuration;
- review operations;
- audit and platform monitoring.

---

## 4. Main System Flows

### Flow 1 — Simulation Main Flow

```text
Select Scenario
    ↓
Read Context
    ↓
Create BA Plan
    ↓
Interview AI Stakeholders
    ↓
Capture Findings
    ↓
Analyze Requirements / Rules / Constraints
    ↕
Re-Elicit / Confirm Information
    ↓
Create BA Artifacts
    ↓
Submit
```

### Flow 2 — Scenario Creation Main Flow

```text
Create Scenario
    ↓
Select Industry & Application
    ↓
Define Context & Assignment
    ↓
Configure Stakeholders
    ↓
Set Rules / Constraints / Events
    ↓
Select Required Artifacts
    ↓
Select BABOK Tasks & Rubric
    ↓
Configure AI Feedback
    ↓
Test
    ↓
Publish
    ↓
Assign to Cohort
```

### Flow 3 — Assessment & Feedback Main Flow

```text
Submission
    ↓
AI Review
    ↓
AI Practice Feedback
    ↓
Learner Revision
    ↓
Final Submission
    ↓
Instructor / Expert Review
    ↓
BABOK Rubric Assessment
    ↓
Final Score + Human Feedback
```

### Flow 4 — Purchase & Access Main Flow

```text
Browse Content
    ↓
Select Scenario / Application / Industry / All Access
    ↓
Choose Access Option
    ↓
Purchase
    ↓
Grant Access
    ↓
Unlock Content
    ↓
Start Practice
```

---

## 5. Content Hierarchy

BA-VES organizes practice content using the following hierarchy:

```text
Industry
   ↓
Application
   ↓
Scenario
```

Target full content model:

```text
4 Industries
×
3 Applications per Industry
×
3 Scenarios per Application
=
36 Scenarios
```

The MVP may begin with only one Industry, one Application, and one fully implemented Scenario.

---

## 6. BA Working Elements

BA-VES separates **working analysis data** from **final artifacts**.

### Working BA Elements

Examples:

- BA Plan
- Information Need
- Planned Question
- Finding
- Requirement
- Business Rule
- Constraint
- Risk
- Open Question
- Conflict
- Evidence Link

These are structured data managed throughout the simulation.

### Final BA Artifacts

Potential supported artifacts include:

- Business Analysis Plan (BAP)
- Stakeholder Analysis
- Business Requirements Document (BRD)
- Software Requirements Specification (SRS)
- Requirement Backlog
- User Stories & Acceptance Criteria
- Business Rules Catalog
- Process Model
- Gap Analysis
- Change Impact Analysis
- Requirements Traceability Matrix (RTM)
- Business Case

A Scenario only requires the artifacts relevant to its learning objectives.

---

## 7. Traceability Model

Traceability is a core design principle of BA-VES.

Example:

```text
Stakeholder Message
        ↓
Finding
        ↓
Requirement
        ↓
Artifact Section
```

This allows the system to answer questions such as:

- Where did this requirement come from?
- Which stakeholder provided the supporting evidence?
- Which artifact section contains the requirement?
- Which learner decision was based on which elicited information?

Traceability is used by both AI Practice Feedback and Human Assessment.

---

## 8. AI in BA-VES

BA-VES uses AI in two primary areas.

### 8.1 AI Stakeholder Simulation

Each Stakeholder Agent may contain:

- role;
- responsibilities;
- goals;
- pain points;
- communication style;
- allowed knowledge;
- hidden information;
- business concerns;
- scenario-specific context.

The agent should remain within its assigned knowledge boundary and role.

### 8.2 AI Practice Feedback

AI analyzes learner submissions using:

- Scenario context;
- applicable BABOK tasks;
- learner artifacts;
- simulation evidence;
- findings;
- requirements;
- business rules;
- constraints;
- traceability links;
- scenario reference evaluation data.

AI Practice Feedback may contain:

- Strengths
- Potential Gaps
- Missing Evidence
- Inconsistencies
- Areas to Revisit
- Suggestions

AI Practice Feedback is **formative** and should not be treated as the official final score.

---

## 9. Human Assessment

Official scoring is performed by an Instructor or BA Expert.

Human reviewers can inspect:

- Scenario context;
- BA plan;
- interview history;
- Findings;
- Requirements;
- Business Rules;
- Constraints;
- traceability;
- final BA Artifacts;
- AI Practice Feedback;
- BABOK-aligned Rubric.

The reviewer then provides:

- criterion-level score;
- professional comments;
- final score;
- final human feedback.

---

## 10. BABOK Alignment

BA-VES uses a **BABOK-aligned assessment model**.

The system stores BABOK Knowledge Areas and Tasks as assessment reference data. Each Scenario selects only the tasks relevant to its learning objectives.

Example Scenario task mapping:

```text
3.2 Plan Stakeholder Engagement
4.1 Prepare for Elicitation
4.2 Conduct Elicitation
4.3 Confirm Elicitation Results
5.1 Trace Requirements and Designs
7.1 Specify and Model Requirements and Designs
7.2 Verify Requirements and Designs
7.3 Validate Requirements and Designs
```

Artifacts are treated as **evidence**, not as the assessment framework itself.

> BA-VES does not claim that BABOK provides an official 100-point student scoring rubric. Weighting and scoring rules are implementation decisions made by BA-VES while the assessment criteria remain aligned with applicable BABOK tasks.

---

# 11. System Architecture

BA-VES uses a **Desktop-First + Modular Monolith Backend + Agent Runtime** architecture.

```text
                    BA-VES Desktop App
                            │
                            │ HTTPS
                            ▼
                 ASP.NET Core Backend
                            │
              ┌─────────────┴─────────────┐
              │     Modular Monolith      │
              │                           │
              │ Identity                  │
              │ Content                   │
              │ Scenario                  │
              │ Simulation                │
              │ Planning / Analysis       │
              │ Artifact / Submission     │
              │ Assessment                │
              │ Cohort                    │
              │ Access                    │
              │ AI Integration            │
              └─────────────┬─────────────┘
                            │
             ┌──────────────┼───────────────┐
             ▼              ▼               ▼
         SQL Server    Object Storage   Agent Runtime
                                           │
                                           ▼
                                      LangChain
                                           │
                                           ▼
                              OpenAI / Gemini / Other LLM
                                     via BYOK
```

---

## 12. Backend Architecture

The core backend is implemented using **C# ASP.NET Core** following a **Modular Monolith** architecture.

Each business capability is implemented as an independent module while remaining inside the same deployable backend application.

Example modules:

```text
Modules/
├── Identity
├── Content
├── Scenario
├── Simulation
├── Planning
├── Analysis
├── Artifact
├── Assessment
├── Cohort
├── Access
└── AIIntegration
```

---

## 13. Internal Module Structure

Each module follows a simplified **3-Layer architecture**:

```text
Module
│
├── Controllers
│
├── Services
│   └── Interfaces
│
├── Repositories
│   └── Interfaces
│
├── DTOs
├── Requests
├── Responses
├── Entities
└── Mapping
```

Request flow:

```text
Controller
    ↓
Service Interface
    ↓
Service
    ↓
Repository Interface
    ↓
Repository
    ↓
Entity Framework Core
    ↓
SQL Server
```

### Responsibility boundaries

**Controller**
- receives HTTP requests;
- validates request boundary;
- calls application services;
- returns standardized responses.

**Service**
- contains business rules and orchestration;
- coordinates repositories and other module services;
- manages domain-level workflow.

**Repository**
- handles persistence;
- performs Entity Framework Core queries;
- isolates database access from business logic.

**Interfaces**
- reduce coupling;
- support dependency injection;
- simplify testing and replacement of implementations.

---

## 14. Proposed Repository Structure

```text
BA-VES/
│
├── README.md
│
├── docs/
│
│   ├── architecture/
│   ├── api/
│   ├── database/
│   └── scenarios/
│
├── desktop/
│   └── baves-desktop/
│
├── backend/
│   ├── BAVES.sln
│   │
│   ├── src/
│   │   └── BAVES.Api/
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       ├── Shared/
│   │       │   ├── Responses/
│   │       │   ├── Exceptions/
│   │       │   └── Common/
│   │       └── Modules/
│   │           ├── Identity/
│   │           ├── Content/
│   │           ├── Training/
│   │           ├── Scenario/
│   │           ├── Simulation/
│   │           ├── Planning/
│   │           ├── Analysis/
│   │           ├── Artifact/
│   │           └── Assessment/
│   │
│   └── tests/
│       └── BAVES.ModelValidation/
│
└── agent-runtime/
    ├── app/
    │   ├── agents/
    │   ├── chains/
    │   ├── prompts/
    │   ├── context/
    │   ├── providers/
    │   ├── schemas/
    │   └── api/
    │
    ├── tests/
    └── requirements.txt
```

The final folder structure may evolve during development.

---

## 15. LangChain Agent Runtime

The ASP.NET Core backend remains the **system of record and business authority**.

LangChain is used as an **AI orchestration layer**, not as the main business backend.

Responsibilities of the Agent Runtime may include:

- stakeholder agent orchestration;
- prompt composition;
- LLM provider abstraction;
- context assembly;
- memory summarization;
- structured output handling;
- criterion-by-criterion feedback chains;
- future agent workflows.

### Important boundary

LangChain should **not directly decide**:

- whether a learner owns a Scenario;
- whether a user can access a Submission;
- whether hidden stakeholder knowledge may be exposed;
- final human score;
- database authorization rules.

Those decisions remain inside the ASP.NET Core backend.

---

## 16. Agent Request Flow

Example Stakeholder Agent flow:

```text
Desktop App
    ↓
ASP.NET Core Simulation API
    ↓
Validate User / Session / Access
    ↓
Load ScenarioVersion
    ↓
Load Stakeholder Persona
    ↓
Select Eligible Knowledge
    ↓
Build Agent Request
    ↓
LangChain Agent Runtime
    ↓
LLM Provider via BYOK
    ↓
Structured Agent Response
    ↓
ASP.NET Core validates result
    ↓
Save Message
    ↓
Return to Desktop App
```

This design prevents the LLM from independently accessing unrestricted scenario data.

---

## 17. BYOK — Bring Your Own Key

BA-VES is designed to support user-owned AI provider credentials.

Potential providers:

- OpenAI
- Google Gemini
- Anthropic
- other supported providers

Recommended flow:

```text
User selects provider
        ↓
Enter API key
        ↓
Validate key
        ↓
Select model
        ↓
Store credential securely on the user's device
        ↓
Use provider for AI requests
```

Raw API keys must never be written to application logs or stored as plain text in the application database.

---

## 18. Core Backend Modules

### Identity Module

Responsibilities:

- authentication;
- authorization;
- role management;
- user profile.

### Content Module

Responsibilities:

- Industry;
- Application;
- content hierarchy.

### Scenario Module

Responsibilities:

- Scenario;
- ScenarioVersion;
- Business Context;
- Stakeholder Persona;
- Stakeholder Knowledge;
- Scenario Rules and Constraints;
- required artifacts;
- BABOK task mapping.

### Simulation Module

Responsibilities:

- SimulationSession;
- SessionStakeholder;
- Messages;
- conversation state;
- stakeholder interaction.

### Planning Module

Responsibilities:

- BA Plan;
- Information Needs;
- Planned Questions;
- stakeholder planning.

### Analysis Module

Responsibilities:

- Findings;
- Requirements;
- Business Rules;
- Constraints;
- Open Questions;
- Conflicts;
- traceability.

### Artifact Module

Responsibilities:

- Artifact Templates;
- Artifact Sections;
- Artifact Versions;
- Artifact Element Links;
- Attachments;
- Submission snapshots.

### Assessment Module

Responsibilities:

- BABOK master data;
- Rubrics;
- AI Practice Feedback;
- Human Reviews;
- criterion results;
- final score.

### Cohort Module

Responsibilities:

- Cohort;
- Cohort Members;
- Assignments;
- Instructor-to-Learner learning flow.

### Access Module

Responsibilities:

- content entitlement;
- purchase access;
- cohort assignment access;
- organization access.

### AI Integration Module

Responsibilities:

- communication with Agent Runtime;
- agent request/response contracts;
- provider metadata;
- AI timeout and error handling;
- feedback orchestration.

---

## 19. Example Core Entities

```text
Identity
├── User
├── Role
└── UserRole

Content
├── Industry
└── Application

Scenario
├── Scenario
├── ScenarioVersion
├── StakeholderPersona
└── AgentKnowledgeItem

Simulation
├── SimulationSession
├── SessionStakeholder
└── Message

Planning
├── BAPlan
├── InformationNeed
└── PlannedQuestion

Analysis
├── Finding
├── Requirement
├── BusinessRule
├── Constraint
├── OpenQuestion
├── Conflict
└── TraceabilityLink

Artifact
├── ArtifactTemplate
├── ArtifactTemplateSection
├── Artifact
├── ArtifactSection
├── ArtifactVersion
├── ArtifactElementLink
└── Submission

Assessment
├── BabokKnowledgeArea
├── BabokTask
├── Rubric
├── RubricCriterion
├── AIPracticeFeedback
├── HumanReview
└── ReviewCriterionResult
```

---

## 20. Artifact Data Strategy

Structured artifact content should be stored as the system's source of truth.

Example:

```text
Artifact
    ↓
Artifact Sections
    ↓
Structured JSON + Plain Text
    ↓
Traceability Links
```

Files such as DOCX or PDF should be treated primarily as export or attachment formats.

This approach enables:

- better AI analysis;
- reliable section identification;
- requirement-to-artifact traceability;
- version control;
- structured submission snapshots.

---

## 21. Submission Versioning

Submissions must be immutable.

Recommended flow:

```text
Artifact Draft
    ↓
Create ArtifactVersion V3
    ↓
Freeze V3
    ↓
Create Submission
    ↓
Submission references V3
```

AI and Human Review must evaluate the same frozen version.

---

## 22. API Design Principles

Recommended conventions:

- RESTful endpoints;
- consistent response model;
- asynchronous I/O;
- role and resource-level authorization;
- request validation;
- versioned API contracts when necessary;
- Swagger / OpenAPI documentation.

Example endpoints:

```text
POST /api/auth/register
POST /api/auth/login

GET  /api/applications/{id}/scenarios
GET  /api/scenarios/{id}

POST /api/scenarios/{id}/sessions
POST /api/sessions/{id}/stakeholders/{stakeholderId}/messages

PUT  /api/sessions/{id}/plan

POST /api/sessions/{id}/findings
POST /api/sessions/{id}/requirements

POST /api/sessions/{id}/artifacts
PUT  /api/artifacts/{id}/sections/{sectionId}
POST /api/artifacts/{id}/versions

POST /api/sessions/{id}/submissions

POST /api/submissions/{id}/ai-feedback

POST /api/submissions/{id}/reviews
PUT  /api/reviews/{id}/criteria/{criterionId}
POST /api/reviews/{id}/complete
```

---

## 23. Standard API Response

A common response wrapper is recommended.

Example:

```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": {}
}
```

Example error:

```json
{
  "success": false,
  "code": "SCENARIO_ACCESS_DENIED",
  "message": "You do not have access to this scenario.",
  "data": null
}
```

---

## 24. Access Model

Users should not be represented by a simple global `IsPremium` flag.

Access may come from:

```text
FREE
PURCHASE
SUBSCRIPTION
COHORT_ASSIGNMENT
ORGANIZATION_LICENSE
```

Conceptually:

```text
User
    ↓
Entitlement / Access Grant
    ↓
Scenario / Application / Industry
```

---

## 25. Desktop-First Design

BA-VES uses a Desktop App as the main client.

The Desktop App is responsible for:

- learner workspace;
- stakeholder simulation UI;
- artifact authoring;
- instructor workspace;
- local BYOK credential handling;
- API communication with the backend.

The Desktop App does not replace the centralized backend. Shared data, authorization, simulation state, submissions, and assessment remain server-side.

---

## 26. Security Principles

The system should enforce:

- authenticated API access;
- role-based authorization;
- resource-level authorization;
- secure password hashing;
- safe JWT handling;
- no raw API key logging;
- input validation;
- upload validation;
- request rate limiting;
- immutable submission versions;
- auditability of human review changes;
- backend-controlled stakeholder knowledge boundaries.

Learners must not be able to:

- access private Scenarios without permission;
- read hidden Stakeholder knowledge directly;
- modify another learner's work;
- alter a submitted immutable version;
- change Human Assessment scores.

---

## 27. Performance & Scalability

Desktop-first does not mean single-user.

Each Desktop App instance connects to the centralized backend.

Initial MVP performance targets should be validated through load testing rather than treated as guaranteed limits.

Suggested test categories:

- normal API workload;
- artifact autosave;
- concurrent simulation sessions;
- concurrent stakeholder AI generations;
- AI feedback background jobs.

The backend should use:

- `async/await`;
- efficient EF Core queries;
- proper database indexes;
- caching for stable scenario metadata;
- throttling for AI generation;
- background queues for non-real-time AI processing.

---

## 28. Testing Strategy

### Unit Tests

Focus on:

- business services;
- knowledge selection;
- context building;
- traceability logic;
- submission validation;
- score calculation;
- access control.

### Integration Tests

Cover:

- Authentication;
- Scenario retrieval;
- Session creation;
- Stakeholder messaging;
- Finding / Requirement creation;
- Artifact versioning;
- Submission;
- AI Feedback;
- Human Review.

### End-to-End Scenario Test

```text
Create / Seed Scenario
    ↓
Learner Starts Session
    ↓
BA Plan
    ↓
Stakeholder Interviews
    ↓
Findings
    ↓
Requirements
    ↓
Artifact
    ↓
Submission
    ↓
AI Feedback
    ↓
Human Review
    ↓
Final Result
```

---

## 29. Initial MVP Roadmap

The first development milestone focuses on **one complete Scenario**, not the full 36-Scenario content library.

### Week 1 — Scenario Foundation

- Authentication
- Scenario / ScenarioVersion
- Stakeholder Persona
- Knowledge Items
- BYOK Provider Setup

### Week 2 — Simulation

- Simulation Session
- Stakeholder AI
- BA Plan
- Findings
- Requirements
- Traceability

### Week 3 — Artifact & Submission

- BRD Template
- Artifact Sections
- Artifact Versioning
- Submission Snapshot

### Week 4 — Feedback & Assessment

- BABOK task mapping
- AI Practice Feedback
- Human Rubric Assessment
- Final Score & Human Feedback

Target demonstration:

```text
Scenario
→ Planning
→ Stakeholder Simulation
→ Analysis
→ BRD
→ Submission
→ AI Feedback
→ Human Assessment
```

---

## 30. Environment Configuration

Example environment variables for the ASP.NET Core backend:

```env
ASPNETCORE_ENVIRONMENT=Development

ConnectionStrings__DefaultConnection=YOUR_SQL_SERVER_CONNECTION_STRING

AgentRuntime__BaseUrl=http://localhost:8000

Jwt__Issuer=BA-VES
Jwt__Audience=BA-VES-Desktop
Jwt__SigningKey=SET_A_RANDOM_VALUE_OF_AT_LEAST_32_CHARACTERS
```

Do not commit secrets to Git.

Use:

- `.env.example`;
- `appsettings.Development.json` with safe local values;
- environment variables;
- user secrets or a secret manager.

---

## 31. Local Development

### Prerequisites

Install the following:

- .NET SDK compatible with the project;
- SQL Server or a compatible development database;
- Python for the LangChain Agent Runtime;
- Git;
- your selected Desktop App runtime/toolchain;
- an API key for at least one supported AI provider.

### Clone Repository

```bash
git clone <repository-url>
cd BA-VES
```

### Backend

```bash
cd backend
dotnet restore
dotnet build
```

Apply database migrations according to the project's EF Core configuration.

Then start the API:

```bash
dotnet run --project src/BAVES.Api
```

### Agent Runtime

```bash
cd agent-runtime

python -m venv .venv
```

Activate the virtual environment, install the project dependencies, and start the Agent Runtime according to its configured API entry point.

> Exact commands may change as the Agent Runtime implementation is finalized.

### Desktop App

Start the Desktop App using the toolchain defined in the `desktop/` project.

> The Desktop framework and exact command should be documented once the client stack is frozen.

---

## 32. Development Rules

- Build features as vertical slices.
- Keep Controllers thin.
- Keep business rules in Services.
- Keep persistence in Repositories.
- Avoid direct database access from Controllers.
- Do not let the LangChain runtime bypass backend authorization.
- Use ScenarioVersion for reproducible simulations.
- Use immutable ArtifactVersion for assessment.
- Keep AI output structured whenever it affects system data.
- Validate AI-generated evidence references.
- Update Swagger whenever an API contract changes.
- Add migration scripts for schema changes.
- Review pull requests before merging into the main integration branch.

---

## 33. Git Workflow

Suggested workflow:

```text
main
  ↑
develop
  ↑
feature/*
```

Examples:

```text
feature/auth
feature/scenario
feature/stakeholder-agent
feature/simulation
feature/analysis
feature/artifact
feature/assessment
```

Recommended flow:

```text
Feature Branch
    ↓
Pull Request
    ↓
Code Review
    ↓
Integration Test
    ↓
develop
    ↓
Release Validation
    ↓
main
```

---

## 34. Project Status

Current target:

> **Build one complete end-to-end BA Scenario before expanding the scenario library.**

Priority chain:

```text
Scenario
→ Simulation
→ Analysis
→ Artifact
→ Submission
→ AI Feedback
→ Human Assessment
```

Features such as advanced dynamic events, time-based simulation chunks, voice interaction, production payment, advanced agent personality state, and large-scale analytics are planned as later extensions.

---

## 35. Documentation

Recommended documentation structure:

```text
docs/
├── architecture/
│   ├── system-architecture.md
│   ├── backend-modules.md
│   └── agent-runtime.md
│
├── api/
│   └── api-overview.md
│
├── database/
│   └── data-model.md
│
├── scenarios/
│   └── scenario-authoring.md
│
└── assessment/
    └── babok-rubric.md
```

---

## 36. Contributors

BA-VES is developed as a capstone project by a four-member team consisting of:

- Backend Developers
- Frontend / Desktop Developers
- AI integration responsibilities distributed across the project team

Detailed ownership should be documented in the project management plan.

---

## 37. Disclaimer

BA-VES is an educational simulation platform.

AI-generated stakeholder responses and AI Practice Feedback may contain errors or incomplete interpretations. AI output is intended to support learner practice and should not be treated as authoritative professional advice or an official competency score.

Official assessment, where provided, is performed by an Instructor or BA Expert according to the configured assessment rubric.

BABOK® is a registered trademark of the International Institute of Business Analysis™ (IIBA®). BA-VES is an independent educational project and is not presented as an official IIBA product unless separately authorized.

---

## 38. License

The project license has not yet been finalized.

Add the appropriate license file before public distribution.

---

## 39. Summary

BA-VES combines:

```text
Structured BA Practice
+
Multi-Stakeholder AI Simulation
+
Traceable BA Analysis
+
Guided Artifact Creation
+
AI Practice Feedback
+
BABOK-Aligned Human Assessment
```

The project follows a **Desktop-First**, **ASP.NET Core Modular Monolith**, and **LangChain-based Agent Orchestration** architecture with clear boundaries between business logic, AI orchestration, persistence, and assessment.

The MVP strategy is simple:

> **Complete one realistic Scenario end-to-end first, then scale the platform, content library, and simulation complexity.**

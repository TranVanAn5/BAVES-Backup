-- =============================================================================
-- BA-VES Physical Database Schema & Report Queries (Chuẩn hóa theo Quy trình ERD 5 bước)
-- DBMS: Microsoft SQL Server
-- Tổng số bảng: 43 bảng / 10 schemas (42 bảng Logical + 1 bảng kỹ thuật UserTokens)
-- Bổ sung:
--   1. Đầy đủ FOREIGN KEY xuyên phân hệ & ON DELETE CASCADE cho quan hệ Composition
--   2. Khóa ngoại ghép (Composite FK - Mục 8.4) chặn dữ liệu lệch ngữ cảnh kịch bản / phiên
--   3. Ràng buộc CHECK cho toàn bộ cột Status / Enum theo State Diagram (Mục 9.1)
--   4. Bốn VIEW và câu truy vấn Báo cáo đầu ra theo chuẩn BABOK (Mục 9.4)
-- =============================================================================

IF SCHEMA_ID(N'identity')   IS NULL EXEC(N'CREATE SCHEMA [identity];');
IF SCHEMA_ID(N'content')    IS NULL EXEC(N'CREATE SCHEMA [content];');
IF SCHEMA_ID(N'scenario')   IS NULL EXEC(N'CREATE SCHEMA [scenario];');
IF SCHEMA_ID(N'training')   IS NULL EXEC(N'CREATE SCHEMA [training];');
IF SCHEMA_ID(N'simulation') IS NULL EXEC(N'CREATE SCHEMA [simulation];');
IF SCHEMA_ID(N'planning')   IS NULL EXEC(N'CREATE SCHEMA [planning];');
IF SCHEMA_ID(N'analysis')   IS NULL EXEC(N'CREATE SCHEMA [analysis];');
IF SCHEMA_ID(N'artifact')   IS NULL EXEC(N'CREATE SCHEMA [artifact];');
IF SCHEMA_ID(N'assessment') IS NULL EXEC(N'CREATE SCHEMA [assessment];');
IF SCHEMA_ID(N'commerce')   IS NULL EXEC(N'CREATE SCHEMA [commerce];');
GO

-- =============================================================================
-- 1. SCHEMA: [identity] — Quản lý người dùng và phân quyền
-- =============================================================================
CREATE TABLE [identity].[Roles] (
    [Id]               int                NOT NULL IDENTITY,
    [Name]             nvarchar(256)      NULL,
    [NormalizedName]   nvarchar(256)      NULL,
    [Description]      nvarchar(max)      NULL,
    [IsActive]         bit                NOT NULL DEFAULT 1,
    [ConcurrencyStamp] nvarchar(max)      NULL,
    [CreatedAt]        datetimeoffset(3)  NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [identity].[Users] (
    [Id]                   int                NOT NULL IDENTITY,
    [FullName]             nvarchar(200)      NOT NULL,
    [AvatarUrl]            nvarchar(1000)     NULL,
    [Status]               nvarchar(40)       NOT NULL DEFAULT N'Active',
    [CreatedAt]            datetimeoffset(3)  NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]            datetimeoffset(3)  NULL,
    [UserName]             nvarchar(256)      NULL,
    [NormalizedUserName]   nvarchar(256)      NULL,
    [Email]                nvarchar(320)      NULL,
    [NormalizedEmail]      nvarchar(320)      NULL,
    [EmailConfirmed]       bit                NOT NULL DEFAULT 0,
    [PasswordHash]         nvarchar(max)      NULL,
    [SecurityStamp]        nvarchar(max)      NULL,
    [ConcurrencyStamp]     nvarchar(max)      NULL,
    [PhoneNumber]          nvarchar(max)      NULL,
    [PhoneNumberConfirmed] bit                NOT NULL DEFAULT 0,
    [TwoFactorEnabled]     bit                NOT NULL DEFAULT 0,
    [LockoutEnd]           datetimeoffset(3)  NULL,
    [LockoutEnabled]       bit                NOT NULL DEFAULT 1,
    [AccessFailedCount]    int                NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Users_Status] CHECK ([Status] IN (N'Active', N'Suspended', N'Archived'))
);
GO

CREATE TABLE [identity].[UserRoles] (
    [UserId]           int               NOT NULL,
    [RoleId]           int               NOT NULL,
    [AssignedAt]       datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [AssignedByUserId] int               NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId])
        REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId])
        REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_AssignedByUserId] FOREIGN KEY ([AssignedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [identity].[UserTokens] (
    [UserId]        int           NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name]          nvarchar(450) NOT NULL,
    [Value]         nvarchar(max) NULL,
    CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId])
        REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [identity].[Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [IX_UserRoles_AssignedByUserId] ON [identity].[UserRoles] ([AssignedByUserId]);
CREATE INDEX [IX_UserRoles_RoleId] ON [identity].[UserRoles] ([RoleId]);
CREATE INDEX [EmailIndex] ON [identity].[Users] ([NormalizedEmail]);
CREATE UNIQUE INDEX [IX_Users_Email] ON [identity].[Users] ([Email]) WHERE [Email] IS NOT NULL;
CREATE INDEX [IX_Users_Status] ON [identity].[Users] ([Status]);
CREATE UNIQUE INDEX [UserNameIndex] ON [identity].[Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

-- =============================================================================
-- 2. SCHEMA: [content] — Danh mục lĩnh vực và hệ thống doanh nghiệp
-- =============================================================================
CREATE TABLE [content].[Industries] (
    [Id]           int               NOT NULL IDENTITY,
    [Code]         nvarchar(80)      NOT NULL,
    [Name]         nvarchar(200)     NOT NULL,
    [Description]  nvarchar(max)     NULL,
    [Status]       nvarchar(40)      NOT NULL DEFAULT N'Active',
    [DisplayOrder] int               NOT NULL DEFAULT 1,
    [CreatedAt]    datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]    datetimeoffset(3) NULL,
    CONSTRAINT [PK_Industries] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Industries_Status] CHECK ([Status] IN (N'Active', N'Inactive', N'Archived'))
);
GO

CREATE TABLE [content].[Applications] (
    [Id]           int               NOT NULL IDENTITY,
    [IndustryId]   int               NOT NULL,
    [Code]         nvarchar(80)      NOT NULL,
    [Name]         nvarchar(200)     NOT NULL,
    [Description]  nvarchar(max)     NULL,
    [Status]       nvarchar(40)      NOT NULL DEFAULT N'Active',
    [DisplayOrder] int               NOT NULL DEFAULT 1,
    [CreatedAt]    datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]    datetimeoffset(3) NULL,
    CONSTRAINT [PK_Applications] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Applications_Status] CHECK ([Status] IN (N'Active', N'Inactive', N'Archived')),
    CONSTRAINT [FK_Applications_Industries_IndustryId] FOREIGN KEY ([IndustryId])
        REFERENCES [content].[Industries] ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Industries_Code] ON [content].[Industries] ([Code]);
CREATE UNIQUE INDEX [IX_Applications_IndustryId_Code] ON [content].[Applications] ([IndustryId], [Code]);
GO

-- =============================================================================
-- 3. SCHEMA: [scenario] — Kịch bản mô phỏng, Stakeholder Persona, Tri thức & Sự kiện
-- =============================================================================
CREATE TABLE [scenario].[Scenarios] (
    [Id]              int               NOT NULL IDENTITY,
    [ApplicationId]   int               NOT NULL,
    [CreatedByUserId] int               NOT NULL,
    [Code]            nvarchar(80)      NOT NULL,
    [Title]           nvarchar(200)     NOT NULL,
    [Description]     nvarchar(max)     NULL,
    [Status]          nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CreatedAt]       datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]       datetimeoffset(3) NULL,
    CONSTRAINT [PK_Scenarios] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Scenarios_Status] CHECK ([Status] IN (N'Draft', N'Published', N'Archived')),
    CONSTRAINT [FK_Scenarios_Applications_ApplicationId] FOREIGN KEY ([ApplicationId])
        REFERENCES [content].[Applications] ([Id]),
    CONSTRAINT [FK_Scenarios_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [scenario].[ScenarioVersions] (
    [Id]                 int               NOT NULL IDENTITY,
    [ScenarioId]         int               NOT NULL,
    [VersionNo]          int               NOT NULL,
    [Summary]            nvarchar(max)     NULL,
    [BusinessContext]    nvarchar(max)     NOT NULL,
    [BusinessProblem]    nvarchar(max)     NOT NULL,
    [BusinessObjectives] nvarchar(max)     NOT NULL,
    [BARole]             nvarchar(500)     NOT NULL,
    [Assignment]         nvarchar(max)     NOT NULL,
    [InScope]            nvarchar(max)     NULL,
    [OutOfScope]         nvarchar(max)     NULL,
    [KnownConstraints]   nvarchar(max)     NULL,
    [Difficulty]         nvarchar(40)      NOT NULL DEFAULT N'Intermediate',
    [Status]             nvarchar(40)      NOT NULL DEFAULT N'Draft', -- Chuẩn hóa theo 5Đ (Định trạng)
    [IsPublished]        bit               NOT NULL DEFAULT 0,        -- Giữ tương thích với C# Entity
    [CreatedAt]          datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [PublishedAt]        datetimeoffset(3) NULL,
    CONSTRAINT [PK_ScenarioVersions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ScenarioVersions_VersionNo] CHECK ([VersionNo] > 0),
    CONSTRAINT [CK_ScenarioVersions_Difficulty] CHECK ([Difficulty] IN (N'Beginner', N'Intermediate', N'Advanced')),
    CONSTRAINT [CK_ScenarioVersions_Status] CHECK ([Status] IN (N'Draft', N'Published', N'Archived')),
    CONSTRAINT [FK_ScenarioVersions_Scenarios_ScenarioId] FOREIGN KEY ([ScenarioId])
        REFERENCES [scenario].[Scenarios] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [scenario].[StakeholderPersonas] (
    [Id]                 int           NOT NULL IDENTITY,
    [ScenarioVersionId]  int           NOT NULL,
    [Name]               nvarchar(200) NOT NULL,
    [StakeholderRole]    nvarchar(200) NOT NULL,
    [Department]         nvarchar(200) NULL,
    [Responsibilities]   nvarchar(max) NULL,
    [Goals]              nvarchar(max) NULL,
    [PainPoints]         nvarchar(max) NULL,
    [BusinessConcerns]   nvarchar(max) NULL,
    [CommunicationStyle] nvarchar(200) NULL,
    [Personality]        nvarchar(max) NULL,
    [DisplayOrder]       int           NOT NULL DEFAULT 1,
    [IsActive]           bit           NOT NULL DEFAULT 1,
    CONSTRAINT [PK_StakeholderPersonas] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_StakeholderPersonas_Id_ScenarioVersionId] UNIQUE ([Id], [ScenarioVersionId]), -- Đích cho FK ghép Mục 8.4
    CONSTRAINT [FK_StakeholderPersonas_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [scenario].[KnowledgeItems] (
    [Id]                int               NOT NULL IDENTITY,
    [ScenarioVersionId] int               NOT NULL,
    [Code]              nvarchar(80)      NOT NULL,
    [Title]             nvarchar(200)     NOT NULL,
    [Content]           nvarchar(max)     NOT NULL,
    [Category]          nvarchar(40)      NOT NULL,
    [Importance]        nvarchar(40)      NOT NULL DEFAULT N'Medium',
    [CreatedAt]         datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_KnowledgeItems] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_KnowledgeItems_Id_ScenarioVersionId] UNIQUE ([Id], [ScenarioVersionId]), -- Đích cho FK ghép Mục 8.4
    CONSTRAINT [CK_KnowledgeItems_Category] CHECK ([Category] IN (N'BusinessFact', N'Process', N'Data', N'Policy', N'System', N'Constraint', N'Stakeholder')),
    CONSTRAINT [CK_KnowledgeItems_Importance] CHECK ([Importance] IN (N'Low', N'Medium', N'High', N'Critical')),
    CONSTRAINT [FK_KnowledgeItems_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [scenario].[StakeholderKnowledgeAccess] (
    [Id]                   int           NOT NULL IDENTITY,
    [ScenarioVersionId]    int           NOT NULL, -- Đảm bảo Persona và KnowledgeItem cùng thuộc 1 ScenarioVersion (Mục 8.4)
    [StakeholderPersonaId] int           NOT NULL,
    [KnowledgeItemId]      int           NOT NULL,
    [AccessType]           nvarchar(40)  NOT NULL DEFAULT N'Knows',
    [DisclosureRule]       nvarchar(max) NULL,
    [IsHidden]             bit           NOT NULL DEFAULT 0,
    CONSTRAINT [PK_StakeholderKnowledgeAccess] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StakeholderKnowledgeAccess_AccessType] CHECK ([AccessType] IN (N'Knows', N'Partial', N'DoesNotKnow')),
    CONSTRAINT [FK_StakeholderKnowledgeAccess_Persona_Version] FOREIGN KEY ([StakeholderPersonaId], [ScenarioVersionId])
        REFERENCES [scenario].[StakeholderPersonas] ([Id], [ScenarioVersionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_StakeholderKnowledgeAccess_Knowledge_Version] FOREIGN KEY ([KnowledgeItemId], [ScenarioVersionId])
        REFERENCES [scenario].[KnowledgeItems] ([Id], [ScenarioVersionId])
);
GO

CREATE TABLE [scenario].[BusinessRules] (
    [Id]                int           NOT NULL IDENTITY,
    [ScenarioVersionId] int           NOT NULL,
    [Code]              nvarchar(80)  NOT NULL,
    [Title]             nvarchar(200) NOT NULL,
    [Description]       nvarchar(max) NOT NULL,
    [IsHidden]          bit           NOT NULL DEFAULT 0,
    [Importance]        nvarchar(40)  NOT NULL DEFAULT N'Medium',
    CONSTRAINT [PK_BusinessRules] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BusinessRules_Importance] CHECK ([Importance] IN (N'Low', N'Medium', N'High', N'Critical')),
    CONSTRAINT [FK_BusinessRules_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [scenario].[ScenarioEvents] (
    [Id]                   int           NOT NULL IDENTITY,
    [ScenarioVersionId]    int           NOT NULL,
    [Title]                nvarchar(200) NOT NULL,
    [Description]          nvarchar(max) NOT NULL,
    [TriggerType]          nvarchar(40)  NOT NULL,
    [TriggerConditionJson] nvarchar(max) NULL,
    [EventContent]         nvarchar(max) NOT NULL,
    [DisplayOrder]         int           NOT NULL DEFAULT 1,
    [IsActive]             bit           NOT NULL DEFAULT 1,
    CONSTRAINT [PK_ScenarioEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_ScenarioEvents_Id_ScenarioVersionId] UNIQUE ([Id], [ScenarioVersionId]), -- Đích cho FK ghép Mục 8.4
    CONSTRAINT [CK_ScenarioEvents_TriggerType] CHECK ([TriggerType] IN (N'Manual', N'MessageCount', N'TimeElapsed', N'Condition')),
    CONSTRAINT [CK_ScenarioEvents_TriggerConditionJson] CHECK ([TriggerConditionJson] IS NULL OR ISJSON([TriggerConditionJson]) = 1),
    CONSTRAINT [FK_ScenarioEvents_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Scenarios_ApplicationId] ON [scenario].[Scenarios] ([ApplicationId]);
CREATE UNIQUE INDEX [IX_Scenarios_Code] ON [scenario].[Scenarios] ([Code]);
CREATE INDEX [IX_Scenarios_CreatedByUserId] ON [scenario].[Scenarios] ([CreatedByUserId]);
CREATE UNIQUE INDEX [IX_ScenarioVersions_ScenarioId_VersionNo] ON [scenario].[ScenarioVersions] ([ScenarioId], [VersionNo]);
CREATE INDEX [IX_StakeholderPersonas_ScenarioVersionId] ON [scenario].[StakeholderPersonas] ([ScenarioVersionId]);
CREATE UNIQUE INDEX [IX_KnowledgeItems_ScenarioVersionId_Code] ON [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code]);
CREATE INDEX [IX_StakeholderKnowledgeAccess_KnowledgeItemId] ON [scenario].[StakeholderKnowledgeAccess] ([KnowledgeItemId]);
CREATE UNIQUE INDEX [IX_StakeholderKnowledgeAccess_StakeholderPersonaId_KnowledgeItemId] ON [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId]);
CREATE UNIQUE INDEX [IX_BusinessRules_ScenarioVersionId_Code] ON [scenario].[BusinessRules] ([ScenarioVersionId], [Code]);
CREATE INDEX [IX_ScenarioEvents_ScenarioVersionId] ON [scenario].[ScenarioEvents] ([ScenarioVersionId]);
GO

-- =============================================================================
-- 4. SCHEMA: [assessment] (Phần 1: BABOK & Rubric để liên kết từ Training)
-- =============================================================================
CREATE TABLE [assessment].[BabokTasks] (
    [Id]                int           NOT NULL IDENTITY,
    [KnowledgeAreaCode] nvarchar(80)  NOT NULL DEFAULT N'RADD',
    [KnowledgeAreaName] nvarchar(200) NOT NULL DEFAULT N'Requirements Analysis and Design Definition',
    [Code]              nvarchar(80)  NOT NULL,
    [Name]              nvarchar(200) NOT NULL,
    [Purpose]           nvarchar(max) NULL,
    [Description]       nvarchar(max) NULL,
    [BabokVersion]      nvarchar(50)  NOT NULL DEFAULT N'v3',
    [DisplayOrder]      int           NOT NULL DEFAULT 1,
    CONSTRAINT [PK_BabokTasks] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [assessment].[Rubrics] (
    [Id]                int               NOT NULL IDENTITY,
    [ScenarioVersionId] int               NOT NULL,
    [CreatedByUserId]   int               NOT NULL,
    [Name]              nvarchar(200)     NOT NULL,
    [Version]           int               NOT NULL DEFAULT 1,
    [Description]       nvarchar(max)     NULL,
    [Status]            nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CreatedAt]         datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]         datetimeoffset(3) NULL,
    CONSTRAINT [PK_Rubrics] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Rubrics_Version] CHECK ([Version] > 0),
    CONSTRAINT [CK_Rubrics_Status] CHECK ([Status] IN (N'Draft', N'Published', N'Archived')),
    CONSTRAINT [FK_Rubrics_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]),
    CONSTRAINT [FK_Rubrics_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [assessment].[RubricCriteria] (
    [Id]               int           NOT NULL IDENTITY,
    [RubricId]         int           NOT NULL,
    [BabokTaskId]      int           NOT NULL,
    [Code]             nvarchar(80)  NOT NULL,
    [Name]             nvarchar(200) NOT NULL,
    [Description]      nvarchar(max) NOT NULL,
    [Weight]           decimal(9,4)  NOT NULL,
    [MaxScore]         decimal(9,4)  NOT NULL,
    [EvidenceGuidance] nvarchar(max) NULL,
    [DisplayOrder]     int           NOT NULL DEFAULT 1,
    [IsRequired]       bit           NOT NULL DEFAULT 1,
    CONSTRAINT [PK_RubricCriteria] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_RubricCriteria_Id_RubricId] UNIQUE ([Id], [RubricId]), -- Đích cho FK ghép của bảng điểm chi tiết
    CONSTRAINT [CK_RubricCriteria_Weight] CHECK ([Weight] > 0),
    CONSTRAINT [CK_RubricCriteria_MaxScore] CHECK ([MaxScore] > 0),
    CONSTRAINT [FK_RubricCriteria_BabokTasks_BabokTaskId] FOREIGN KEY ([BabokTaskId])
        REFERENCES [assessment].[BabokTasks] ([Id]),
    CONSTRAINT [FK_RubricCriteria_Rubrics_RubricId] FOREIGN KEY ([RubricId])
        REFERENCES [assessment].[Rubrics] ([Id]) ON DELETE CASCADE
);
GO

-- =============================================================================
-- 5. SCHEMA: [training] — Quản lý Lớp/Cohort và Giao bài tập (Assignments)
-- =============================================================================
CREATE TABLE [training].[Cohorts] (
    [Id]           int               NOT NULL IDENTITY,
    [InstructorId] int               NOT NULL,
    [Code]         nvarchar(80)      NOT NULL,
    [Name]         nvarchar(200)     NOT NULL,
    [Description]  nvarchar(max)     NULL,
    [StartDate]    date              NULL,
    [EndDate]      date              NULL,
    [Status]       nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CreatedAt]    datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]    datetimeoffset(3) NULL,
    CONSTRAINT [PK_Cohorts] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Cohorts_DateRange] CHECK ([EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]),
    CONSTRAINT [CK_Cohorts_Status] CHECK ([Status] IN (N'Draft', N'Active', N'Completed', N'Archived')),
    CONSTRAINT [FK_Cohorts_Users_InstructorId] FOREIGN KEY ([InstructorId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [training].[CohortMembers] (
    [Id]        int               NOT NULL IDENTITY,
    [CohortId]  int               NOT NULL,
    [LearnerId] int               NOT NULL,
    [JoinedAt]  datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [Status]    nvarchar(40)      NOT NULL DEFAULT N'Active',
    CONSTRAINT [PK_CohortMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CohortMembers_Status] CHECK ([Status] IN (N'Active', N'Removed', N'Completed')),
    CONSTRAINT [FK_CohortMembers_Cohorts_CohortId] FOREIGN KEY ([CohortId])
        REFERENCES [training].[Cohorts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CohortMembers_Users_LearnerId] FOREIGN KEY ([LearnerId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [training].[Assignments] (
    [Id]                int               NOT NULL IDENTITY,
    [ScenarioVersionId] int               NOT NULL,
    [CohortId]          int               NULL,
    [LearnerId]         int               NULL,
    [RubricId]          int               NULL,
    [AssignedByUserId]  int               NOT NULL,
    [Title]             nvarchar(200)     NOT NULL,
    [Instructions]      nvarchar(max)     NULL,
    [StartAt]           datetimeoffset(3) NULL,
    [DueAt]             datetimeoffset(3) NULL,
    [MaxAttempts]       int               NULL,
    [Status]            nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CreatedAt]         datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]         datetimeoffset(3) NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Assignments_MaxAttempts] CHECK ([MaxAttempts] IS NULL OR [MaxAttempts] > 0),
    CONSTRAINT [CK_Assignments_Recipient] CHECK (([CohortId] IS NOT NULL AND [LearnerId] IS NULL) OR ([CohortId] IS NULL AND [LearnerId] IS NOT NULL)),
    CONSTRAINT [CK_Assignments_DueRange] CHECK ([DueAt] IS NULL OR [StartAt] IS NULL OR [DueAt] >= [StartAt]),
    CONSTRAINT [CK_Assignments_Status] CHECK ([Status] IN (N'Draft', N'Scheduled', N'Active', N'Closed', N'Archived')),
    CONSTRAINT [FK_Assignments_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]),
    CONSTRAINT [FK_Assignments_Cohorts_CohortId] FOREIGN KEY ([CohortId])
        REFERENCES [training].[Cohorts] ([Id]),
    CONSTRAINT [FK_Assignments_Users_LearnerId] FOREIGN KEY ([LearnerId])
        REFERENCES [identity].[Users] ([Id]),
    CONSTRAINT [FK_Assignments_Rubrics_RubricId] FOREIGN KEY ([RubricId])
        REFERENCES [assessment].[Rubrics] ([Id]),
    CONSTRAINT [FK_Assignments_Users_AssignedByUserId] FOREIGN KEY ([AssignedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Cohorts_Code] ON [training].[Cohorts] ([Code]);
CREATE INDEX [IX_Cohorts_InstructorId] ON [training].[Cohorts] ([InstructorId]);
CREATE UNIQUE INDEX [IX_CohortMembers_CohortId_LearnerId] ON [training].[CohortMembers] ([CohortId], [LearnerId]);
CREATE INDEX [IX_CohortMembers_LearnerId] ON [training].[CohortMembers] ([LearnerId]);
CREATE INDEX [IX_Assignments_AssignedByUserId] ON [training].[Assignments] ([AssignedByUserId]);
CREATE INDEX [IX_Assignments_CohortId] ON [training].[Assignments] ([CohortId]);
CREATE INDEX [IX_Assignments_LearnerId] ON [training].[Assignments] ([LearnerId]);
CREATE INDEX [IX_Assignments_RubricId] ON [training].[Assignments] ([RubricId]);
CREATE INDEX [IX_Assignments_ScenarioVersionId] ON [training].[Assignments] ([ScenarioVersionId]);
GO

-- =============================================================================
-- 6. SCHEMA: [simulation] — Phiên thực hành mô phỏng, AI Agent & Tin nhắn
-- =============================================================================
CREATE TABLE [simulation].[SimulationSessions] (
    [Id]                int               NOT NULL IDENTITY,
    [ScenarioVersionId] int               NOT NULL,
    [AssignmentId]      int               NULL,
    [LearnerId]         int               NOT NULL,
    [AttemptNo]         int               NOT NULL DEFAULT 1,
    [CurrentSprintNo]   int               NOT NULL DEFAULT 1,
    [TotalSprints]      int               NOT NULL DEFAULT 4,
    [Status]            nvarchar(40)      NOT NULL DEFAULT N'Active',
    [StartedAt]         datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [SubmittedAt]       datetimeoffset(3) NULL,
    [CompletedAt]       datetimeoffset(3) NULL,
    CONSTRAINT [PK_SimulationSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_SimulationSessions_Id_ScenarioVersionId] UNIQUE ([Id], [ScenarioVersionId]), -- Đích cho FK ghép Mục 8.4
    CONSTRAINT [CK_SimulationSessions_AttemptNo] CHECK ([AttemptNo] > 0),
    CONSTRAINT [CK_SimulationSessions_Status] CHECK ([Status] IN (N'Active', N'Submitted', N'Completed', N'Abandoned')),
    CONSTRAINT [FK_SimulationSessions_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId])
        REFERENCES [scenario].[ScenarioVersions] ([Id]),
    CONSTRAINT [FK_SimulationSessions_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId])
        REFERENCES [training].[Assignments] ([Id]),
    CONSTRAINT [FK_SimulationSessions_Users_LearnerId] FOREIGN KEY ([LearnerId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [simulation].[SessionAgents] (
    [Id]                 int               NOT NULL IDENTITY,
    [SessionId]          int               NOT NULL,
    [ScenarioVersionId]  int               NOT NULL, -- Cột cho FK ghép Mục 8.4: chặn gán Persona sai kịch bản
    [PersonaId]          int               NOT NULL,
    [MemorySummary]      nvarchar(max)     NULL,
    [CurrentMood]        nvarchar(200)     NULL,
    [TrustLevel]         tinyint           NULL,
    [PatienceLevel]      tinyint           NULL,
    [CooperationLevel]   tinyint           NULL,
    [IsActive]           bit               NOT NULL DEFAULT 1,
    [LastInteractionAt]  datetimeoffset(3) NULL,
    CONSTRAINT [PK_SessionAgents] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_SessionAgents_Id_SessionId] UNIQUE ([Id], [SessionId]), -- Đích cho FK ghép của Messages
    CONSTRAINT [CK_SessionAgents_CooperationLevel] CHECK ([CooperationLevel] IS NULL OR [CooperationLevel] <= 100),
    CONSTRAINT [CK_SessionAgents_PatienceLevel] CHECK ([PatienceLevel] IS NULL OR [PatienceLevel] <= 100),
    CONSTRAINT [CK_SessionAgents_TrustLevel] CHECK ([TrustLevel] IS NULL OR [TrustLevel] <= 100),
    CONSTRAINT [FK_SessionAgents_Session_ScenarioVersion] FOREIGN KEY ([SessionId], [ScenarioVersionId])
        REFERENCES [simulation].[SimulationSessions] ([Id], [ScenarioVersionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SessionAgents_Persona_ScenarioVersion] FOREIGN KEY ([PersonaId], [ScenarioVersionId])
        REFERENCES [scenario].[StakeholderPersonas] ([Id], [ScenarioVersionId])
);
GO

CREATE TABLE [simulation].[Messages] (
    [Id]             int               NOT NULL IDENTITY,
    [SessionId]      int               NOT NULL,
    [SessionAgentId] int               NULL,
    [SenderType]     nvarchar(40)      NOT NULL,
    [Content]        nvarchar(max)     NOT NULL,
    [CreatedAt]      datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_Messages_Id_SessionId] UNIQUE ([Id], [SessionId]), -- Đích cho FK ghép của MessageEvidenceLinks
    CONSTRAINT [CK_Messages_SenderType] CHECK ([SenderType] IN (N'Learner', N'Stakeholder', N'System')),
    CONSTRAINT [FK_Messages_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
        REFERENCES [simulation].[SimulationSessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Messages_SessionAgents_SessionAgentId_SessionId] FOREIGN KEY ([SessionAgentId], [SessionId])
        REFERENCES [simulation].[SessionAgents] ([Id], [SessionId])
);
GO

CREATE TABLE [simulation].[SessionEvents] (
    [Id]                   int               NOT NULL IDENTITY,
    [SessionId]            int               NOT NULL,
    [ScenarioVersionId]    int               NOT NULL, -- Cột cho FK ghép Mục 8.4: chặn nổ sự kiện của kịch bản khác
    [ScenarioEventId]      int               NOT NULL,
    [TriggerSource]        nvarchar(40)      NOT NULL,
    [TriggeredAt]          datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [SequenceNo]           int               NOT NULL,
    [EventContentSnapshot] nvarchar(max)     NOT NULL,
    CONSTRAINT [PK_SessionEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SessionEvents_TriggerSource] CHECK ([TriggerSource] IN (N'Automatic', N'Manual', N'Instructor')),
    CONSTRAINT [FK_SessionEvents_Session_ScenarioVersion] FOREIGN KEY ([SessionId], [ScenarioVersionId])
        REFERENCES [simulation].[SimulationSessions] ([Id], [ScenarioVersionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SessionEvents_ScenarioEvent_ScenarioVersion] FOREIGN KEY ([ScenarioEventId], [ScenarioVersionId])
        REFERENCES [scenario].[ScenarioEvents] ([Id], [ScenarioVersionId])
);
GO

CREATE TABLE [simulation].[SessionAgentKnowledgeStates] (
    [Id]              int               NOT NULL IDENTITY,
    [SessionAgentId]  int               NOT NULL,
    [KnowledgeItemId] int               NOT NULL,
    [IsRevealed]      bit               NOT NULL DEFAULT 0,
    [RevealedAt]      datetimeoffset(3) NULL,
    [RevealMessageId] int               NULL,
    CONSTRAINT [PK_SessionAgentKnowledgeStates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SessionAgentKnowledgeStates_SessionAgents_SessionAgentId] FOREIGN KEY ([SessionAgentId])
        REFERENCES [simulation].[SessionAgents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SessionAgentKnowledgeStates_KnowledgeItems_KnowledgeItemId] FOREIGN KEY ([KnowledgeItemId])
        REFERENCES [scenario].[KnowledgeItems] ([Id]),
    CONSTRAINT [FK_SessionAgentKnowledgeStates_Messages_RevealMessageId] FOREIGN KEY ([RevealMessageId])
        REFERENCES [simulation].[Messages] ([Id])
);
GO

CREATE TABLE [simulation].[CoachFeedbacks] (
    [Id]                   int               NOT NULL IDENTITY,
    [SessionId]            int               NOT NULL,
    [CreatedByUserId]      int               NOT NULL,
    [StrengthsJson]        nvarchar(max)     NOT NULL DEFAULT N'[]',
    [GapsJson]             nvarchar(max)     NOT NULL DEFAULT N'[]',
    [UnclearPointsJson]    nvarchar(max)     NOT NULL DEFAULT N'[]',
    [ImprovementHintsJson] nvarchar(max)     NOT NULL DEFAULT N'[]',
    [PromptVersion]        nvarchar(100)     NOT NULL DEFAULT N'coach@1.0.0',
    [CreatedAt]            datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_CoachFeedbacks] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CoachFeedbacks_StrengthsJson] CHECK (ISJSON([StrengthsJson]) = 1),
    CONSTRAINT [CK_CoachFeedbacks_GapsJson] CHECK (ISJSON([GapsJson]) = 1),
    CONSTRAINT [CK_CoachFeedbacks_UnclearPointsJson] CHECK (ISJSON([UnclearPointsJson]) = 1),
    CONSTRAINT [CK_CoachFeedbacks_ImprovementHintsJson] CHECK (ISJSON([ImprovementHintsJson]) = 1),
    CONSTRAINT [FK_CoachFeedbacks_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
        REFERENCES [simulation].[SimulationSessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CoachFeedbacks_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE INDEX [IX_CoachFeedbacks_SessionId_CreatedAt] ON [simulation].[CoachFeedbacks] ([SessionId], [CreatedAt]);

CREATE INDEX [IX_SimulationSessions_AssignmentId] ON [simulation].[SimulationSessions] ([AssignmentId]);
CREATE INDEX [IX_SimulationSessions_LearnerId] ON [simulation].[SimulationSessions] ([LearnerId]);
CREATE INDEX [IX_SimulationSessions_ScenarioVersionId] ON [simulation].[SimulationSessions] ([ScenarioVersionId]);
CREATE INDEX [IX_SessionAgents_PersonaId] ON [simulation].[SessionAgents] ([PersonaId]);
CREATE UNIQUE INDEX [IX_SessionAgents_SessionId_PersonaId] ON [simulation].[SessionAgents] ([SessionId], [PersonaId]);
CREATE INDEX [IX_Messages_SessionAgentId] ON [simulation].[Messages] ([SessionAgentId]);
CREATE INDEX [IX_Messages_SessionId_CreatedAt] ON [simulation].[Messages] ([SessionId], [CreatedAt]);
CREATE INDEX [IX_SessionEvents_ScenarioEventId] ON [simulation].[SessionEvents] ([ScenarioEventId]);
CREATE UNIQUE INDEX [IX_SessionEvents_SessionId_SequenceNo] ON [simulation].[SessionEvents] ([SessionId], [SequenceNo]);
CREATE INDEX [IX_SessionAgentKnowledgeStates_KnowledgeItemId] ON [simulation].[SessionAgentKnowledgeStates] ([KnowledgeItemId]);
CREATE INDEX [IX_SessionAgentKnowledgeStates_RevealMessageId] ON [simulation].[SessionAgentKnowledgeStates] ([RevealMessageId]);
CREATE UNIQUE INDEX [IX_SessionAgentKnowledgeStates_SessionAgentId_KnowledgeItemId] ON [simulation].[SessionAgentKnowledgeStates] ([SessionAgentId], [KnowledgeItemId]);
GO

-- =============================================================================
-- 7. SCHEMA: [planning] — Kế hoạch phỏng vấn BA (BAPlans & PlannedQuestions)
-- =============================================================================
CREATE TABLE [planning].[BAPlans] (
    [Id]        int               NOT NULL IDENTITY,
    [SessionId] int               NOT NULL,
    [Objective] nvarchar(max)     NOT NULL,
    [Approach]  nvarchar(max)     NULL,
    [Status]    nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CreatedAt] datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt] datetimeoffset(3) NULL,
    CONSTRAINT [PK_BAPlans] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BAPlans_Status] CHECK ([Status] IN (N'Draft', N'Ready', N'InProgress', N'Completed')),
    CONSTRAINT [FK_BAPlans_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
        REFERENCES [simulation].[SimulationSessions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [planning].[PlannedQuestions] (
    [Id]                   int               NOT NULL IDENTITY,
    [BAPlanId]             int               NOT NULL,
    [StakeholderPersonaId] int               NULL,
    [Topic]                nvarchar(max)     NULL,
    [InformationNeed]      nvarchar(max)     NULL,
    [Question]             nvarchar(max)     NOT NULL,
    [Priority]             nvarchar(40)      NOT NULL DEFAULT N'Medium',
    [DisplayOrder]         int               NOT NULL DEFAULT 1,
    [Status]               nvarchar(40)      NOT NULL DEFAULT N'Planned',
    [CreatedAt]            datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_PlannedQuestions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_PlannedQuestions_Priority] CHECK ([Priority] IN (N'Low', N'Medium', N'High', N'Critical')),
    CONSTRAINT [CK_PlannedQuestions_Status] CHECK ([Status] IN (N'Planned', N'Asked', N'Skipped')),
    CONSTRAINT [FK_PlannedQuestions_BAPlans_BAPlanId] FOREIGN KEY ([BAPlanId])
        REFERENCES [planning].[BAPlans] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlannedQuestions_StakeholderPersonas_StakeholderPersonaId] FOREIGN KEY ([StakeholderPersonaId])
        REFERENCES [scenario].[StakeholderPersonas] ([Id])
);
GO

CREATE UNIQUE INDEX [IX_BAPlans_SessionId] ON [planning].[BAPlans] ([SessionId]);
CREATE INDEX [IX_PlannedQuestions_BAPlanId_DisplayOrder] ON [planning].[PlannedQuestions] ([BAPlanId], [DisplayOrder]);
CREATE INDEX [IX_PlannedQuestions_StakeholderPersonaId] ON [planning].[PlannedQuestions] ([StakeholderPersonaId]);
GO

-- =============================================================================
-- 8. SCHEMA: [analysis] — Mục phân tích & Liên kết bằng chứng tin nhắn
-- =============================================================================
CREATE TABLE [analysis].[AnalysisItems] (
    [Id]              int               NOT NULL IDENTITY,
    [SessionId]            int               NOT NULL,
    [ParentAnalysisItemId] int               NULL,
    [ItemType]        nvarchar(40)      NOT NULL,
    [Code]            nvarchar(80)      NOT NULL,
    [Title]           nvarchar(200)     NOT NULL,
    [Description]     nvarchar(max)     NOT NULL,
    [Priority]        nvarchar(40)      NULL,
    [Status]          nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [MetadataJson]    nvarchar(max)     NULL,
    [CreatedByUserId] int               NOT NULL,
    [CreatedAt]       datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]       datetimeoffset(3) NULL,
    CONSTRAINT [PK_AnalysisItems] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_AnalysisItems_Id_SessionId] UNIQUE ([Id], [SessionId]), -- Đích cho FK ghép Mục 8.4
    CONSTRAINT [CK_AnalysisItems_ItemType] CHECK ([ItemType] IN (N'Finding', N'Requirement', N'BusinessRule', N'Constraint', N'Risk', N'OpenQuestion', N'Conflict', N'Assumption')),
    CONSTRAINT [CK_AnalysisItems_Priority] CHECK ([Priority] IS NULL OR [Priority] IN (N'Low', N'Medium', N'High', N'Critical')),
    CONSTRAINT [CK_AnalysisItems_Status] CHECK ([Status] IN (N'Draft', N'Confirmed', N'Resolved', N'Archived')),
    CONSTRAINT [CK_AnalysisItems_MetadataJson] CHECK ([MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1),
    CONSTRAINT [CK_AnalysisItems_NoSelfParent] CHECK ([ParentAnalysisItemId] IS NULL OR [ParentAnalysisItemId] <> [Id]),
    CONSTRAINT [FK_AnalysisItems_Parent] FOREIGN KEY ([ParentAnalysisItemId]) REFERENCES [analysis].[AnalysisItems] ([Id]),
    CONSTRAINT [FK_AnalysisItems_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
        REFERENCES [simulation].[SimulationSessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AnalysisItems_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [analysis].[MessageEvidenceLinks] (
    [Id]               int               NOT NULL IDENTITY,
    [SessionId]        int               NOT NULL, -- FK ghép đảm bảo Message và AnalysisItem thuộc cùng 1 Session (Mục 8.4)
    [MessageId]        int               NOT NULL,
    [AnalysisItemId]   int               NOT NULL,
    [RelationshipType] nvarchar(40)      NOT NULL,
    [CreatedAt]        datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_MessageEvidenceLinks] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MessageEvidenceLinks_RelationshipType] CHECK ([RelationshipType] IN (N'Supports', N'Contradicts', N'Clarifies', N'Confirms')),
    CONSTRAINT [FK_MessageEvidenceLinks_Message_Session] FOREIGN KEY ([MessageId], [SessionId])
        REFERENCES [simulation].[Messages] ([Id], [SessionId]),
    CONSTRAINT [FK_MessageEvidenceLinks_AnalysisItem_Session] FOREIGN KEY ([AnalysisItemId], [SessionId])
        REFERENCES [analysis].[AnalysisItems] ([Id], [SessionId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AnalysisItems_CreatedByUserId] ON [analysis].[AnalysisItems] ([CreatedByUserId]);
CREATE INDEX [IX_AnalysisItems_ParentAnalysisItemId] ON [analysis].[AnalysisItems] ([ParentAnalysisItemId]);
CREATE INDEX [IX_AnalysisItems_SessionId] ON [analysis].[AnalysisItems] ([SessionId]);
CREATE UNIQUE INDEX [IX_AnalysisItems_SessionId_Code] ON [analysis].[AnalysisItems] ([SessionId], [Code]);
CREATE INDEX [IX_AnalysisItems_SessionId_ItemType] ON [analysis].[AnalysisItems] ([SessionId], [ItemType]);
CREATE INDEX [IX_MessageEvidenceLinks_AnalysisItemId] ON [analysis].[MessageEvidenceLinks] ([AnalysisItemId]);
CREATE INDEX [IX_MessageEvidenceLinks_MessageId] ON [analysis].[MessageEvidenceLinks] ([MessageId]);
CREATE UNIQUE INDEX [IX_MessageEvidenceLinks_MessageId_AnalysisItemId_RelationshipType] ON [analysis].[MessageEvidenceLinks] ([MessageId], [AnalysisItemId], [RelationshipType]);
GO

-- =============================================================================
-- 9. SCHEMA: [artifact] — Mẫu tài liệu, Tài liệu BA, Phiên bản & Bài nộp
-- =============================================================================
CREATE TABLE [artifact].[ArtifactTemplates] (
    [Id]           int               NOT NULL IDENTITY,
    [Code]         nvarchar(80)      NOT NULL,
    [Name]         nvarchar(200)     NOT NULL,
    [ArtifactType] nvarchar(40)      NOT NULL DEFAULT N'BRD',
    [Description]  nvarchar(max)     NULL,
    [Status]       nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CreatedAt]    datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]    datetimeoffset(3) NULL,
    CONSTRAINT [PK_ArtifactTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ArtifactTemplates_ArtifactType] CHECK ([ArtifactType] IN (N'BRD', N'Generic', N'BAP', N'StakeholderRegister', N'BusinessRulesCatalog', N'SRS', N'UserStories', N'UseCaseSpecification', N'ImpactAnalysis', N'RTM')),
    CONSTRAINT [CK_ArtifactTemplates_Status] CHECK ([Status] IN (N'Draft', N'Published', N'Archived'))
);
GO

CREATE TABLE [artifact].[ArtifactTemplateSections] (
    [Id]                 int           NOT NULL IDENTITY,
    [ArtifactTemplateId] int           NOT NULL,
    [SectionKey]         nvarchar(80)  NOT NULL,
    [Title]              nvarchar(200) NOT NULL,
    [Instructions]       nvarchar(max) NULL,
    [DisplayOrder]       int           NOT NULL DEFAULT 1,
    [IsRequired]         bit           NOT NULL DEFAULT 1,
    CONSTRAINT [PK_ArtifactTemplateSections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArtifactTemplateSections_ArtifactTemplates_ArtifactTemplateId] FOREIGN KEY ([ArtifactTemplateId])
        REFERENCES [artifact].[ArtifactTemplates] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [artifact].[Artifacts] (
    [Id]                         int               NOT NULL IDENTITY,
    [SessionId]                  int               NOT NULL,
    [ArtifactTemplateId]         int               NOT NULL,
    [Title]                      nvarchar(200)     NOT NULL,
    [DraftStructuredContentJson] nvarchar(max)     NULL,
    [DraftPlainTextContent]      nvarchar(max)     NULL,
    [Status]                     nvarchar(40)      NOT NULL DEFAULT N'Draft',
    [CurrentVersionNo]           int               NOT NULL DEFAULT 0,
    [CreatedAt]                  datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]                  datetimeoffset(3) NULL,
    CONSTRAINT [PK_Artifacts] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_Artifacts_Id_SessionId] UNIQUE ([Id], [SessionId]), -- Đích cho FK ghép của ArtifactVersions
    CONSTRAINT [CK_Artifacts_Status] CHECK ([Status] IN (N'Draft', N'ReadyForSubmission', N'Submitted')),
    CONSTRAINT [CK_Artifacts_DraftStructuredContentJson] CHECK ([DraftStructuredContentJson] IS NULL OR ISJSON([DraftStructuredContentJson]) = 1),
    CONSTRAINT [FK_Artifacts_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
        REFERENCES [simulation].[SimulationSessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Artifacts_ArtifactTemplates_ArtifactTemplateId] FOREIGN KEY ([ArtifactTemplateId])
        REFERENCES [artifact].[ArtifactTemplates] ([Id])
);
GO

CREATE TABLE [artifact].[ArtifactVersions] (
    [Id]                    int               NOT NULL IDENTITY,
    [ArtifactId]            int               NOT NULL,
    [SessionId]             int               NOT NULL, -- Đích cho FK ghép từ Submissions (Mục 8.4)
    [VersionNo]             int               NOT NULL,
    [StructuredContentJson] nvarchar(max)     NOT NULL,
    [PlainTextContent]      nvarchar(max)     NOT NULL,
    [CreatedByUserId]       int               NOT NULL,
    [ProcessingStatus]      nvarchar(40)      NOT NULL DEFAULT N'Ready',
    [CreatedAt]             datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_ArtifactVersions] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_ArtifactVersions_Id_SessionId] UNIQUE ([Id], [SessionId]),
    CONSTRAINT [CK_ArtifactVersions_VersionNo] CHECK ([VersionNo] > 0),
    CONSTRAINT [CK_ArtifactVersions_ProcessingStatus] CHECK ([ProcessingStatus] IN (N'Created', N'Processing', N'Ready', N'Failed')),
    CONSTRAINT [CK_ArtifactVersions_StructuredContentJson] CHECK (ISJSON([StructuredContentJson]) = 1),
    CONSTRAINT [FK_ArtifactVersions_Artifact_Session] FOREIGN KEY ([ArtifactId], [SessionId])
        REFERENCES [artifact].[Artifacts] ([Id], [SessionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArtifactVersions_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [artifact].[Submissions] (
    [Id]                int               NOT NULL IDENTITY,
    [SessionId]         int               NOT NULL,
    [ArtifactVersionId] int               NOT NULL,
    [AttemptNo]         int               NOT NULL,
    [Status]            nvarchar(40)      NOT NULL DEFAULT N'Submitted',
    [SubmittedAt]       datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [FinalizedAt]       datetimeoffset(3) NULL,
    CONSTRAINT [PK_Submissions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Submissions_AttemptNo] CHECK ([AttemptNo] > 0),
    CONSTRAINT [CK_Submissions_Status] CHECK ([Status] IN (N'Submitted', N'Finalized', N'Withdrawn')),
    CONSTRAINT [FK_Submissions_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
        REFERENCES [simulation].[SimulationSessions] ([Id]),
    CONSTRAINT [FK_Submissions_ArtifactVersion_Session] FOREIGN KEY ([ArtifactVersionId], [SessionId])
        REFERENCES [artifact].[ArtifactVersions] ([Id], [SessionId]) -- Đảm bảo chỉ nộp phiên bản tài liệu thuộc đúng Session (Mục 8.4)
);
GO

CREATE UNIQUE INDEX [IX_ArtifactTemplates_Code] ON [artifact].[ArtifactTemplates] ([Code]);
CREATE UNIQUE INDEX [IX_ArtifactTemplateSections_ArtifactTemplateId_SectionKey] ON [artifact].[ArtifactTemplateSections] ([ArtifactTemplateId], [SectionKey]);
CREATE INDEX [IX_Artifacts_ArtifactTemplateId] ON [artifact].[Artifacts] ([ArtifactTemplateId]);
CREATE INDEX [IX_Artifacts_SessionId] ON [artifact].[Artifacts] ([SessionId]);
CREATE UNIQUE INDEX [IX_ArtifactVersions_ArtifactId_VersionNo] ON [artifact].[ArtifactVersions] ([ArtifactId], [VersionNo]);
CREATE INDEX [IX_ArtifactVersions_CreatedByUserId] ON [artifact].[ArtifactVersions] ([CreatedByUserId]);
CREATE INDEX [IX_Submissions_ArtifactVersionId] ON [artifact].[Submissions] ([ArtifactVersionId]);
CREATE INDEX [IX_Submissions_SessionId] ON [artifact].[Submissions] ([SessionId]);
CREATE UNIQUE INDEX [IX_Submissions_SessionId_AttemptNo] ON [artifact].[Submissions] ([SessionId], [AttemptNo]);
GO

-- =============================================================================
-- 10. SCHEMA: [assessment] (Phần 2: Chấm điểm AI & Chuyên gia — Chuẩn hóa 3NF)
-- =============================================================================
CREATE TABLE [assessment].[AIPracticeFeedbacks] (
    [Id]                int               NOT NULL IDENTITY,
    [SubmissionId]      int               NOT NULL,
    [RubricId]          int               NOT NULL,
    [Provider]          nvarchar(50)      NOT NULL,
    [Model]             nvarchar(100)     NOT NULL,
    [Status]            nvarchar(40)      NOT NULL DEFAULT N'Pending',
    [OverallFeedback]   nvarchar(max)     NULL,
    [FeedbackItemsJson] nvarchar(max)     NOT NULL DEFAULT N'[]', -- Giữ tương thích với C# Entity
    [SuggestedScore]    decimal(9,4)      NULL,
    [Confidence]        decimal(5,4)      NULL,
    [StartedAt]         datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [CompletedAt]       datetimeoffset(3) NULL,
    CONSTRAINT [PK_AIPracticeFeedbacks] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_AIPracticeFeedbacks_Id_RubricId] UNIQUE ([Id], [RubricId]), -- Đích cho FK ghép của AIFeedbackCriterionScores
    CONSTRAINT [CK_AIPracticeFeedbacks_Status] CHECK ([Status] IN (N'Pending', N'Processing', N'Completed', N'Failed')),
    CONSTRAINT [CK_AIPracticeFeedbacks_Confidence] CHECK ([Confidence] IS NULL OR ([Confidence] >= 0 AND [Confidence] <= 1)),
    CONSTRAINT [CK_AIPracticeFeedbacks_FeedbackItemsJson] CHECK (ISJSON([FeedbackItemsJson]) = 1),
    CONSTRAINT [FK_AIPracticeFeedbacks_Submissions_SubmissionId] FOREIGN KEY ([SubmissionId])
        REFERENCES [artifact].[Submissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AIPracticeFeedbacks_Rubrics_RubricId] FOREIGN KEY ([RubricId])
        REFERENCES [assessment].[Rubrics] ([Id])
);
GO

-- Bảng tách chuẩn 1NF-3NF cho điểm từng tiêu chí của AI (Mục 8.2 & 8.4)
CREATE TABLE [assessment].[AIFeedbackCriterionScores] (
    [AIFeedbackId]      int           NOT NULL,
    [RubricCriterionId] int           NOT NULL,
    [RubricId]          int           NOT NULL, -- FK ghép đảm bảo tiêu chí thuộc đúng Rubric của lần chấm
    [Score]             decimal(9,4)  NOT NULL,
    [Comment]           nvarchar(max) NULL,
    [EvidenceSnippet]   nvarchar(max) NULL,
    CONSTRAINT [PK_AIFeedbackCriterionScores] PRIMARY KEY ([AIFeedbackId], [RubricCriterionId]),
    CONSTRAINT [CK_AIFeedbackCriterionScores_Score] CHECK ([Score] >= 0),
    CONSTRAINT [FK_AIFeedbackCriterionScores_Feedback_Rubric] FOREIGN KEY ([AIFeedbackId], [RubricId])
        REFERENCES [assessment].[AIPracticeFeedbacks] ([Id], [RubricId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AIFeedbackCriterionScores_Criterion_Rubric] FOREIGN KEY ([RubricCriterionId], [RubricId])
        REFERENCES [assessment].[RubricCriteria] ([Id], [RubricId])
);
GO

CREATE TABLE [assessment].[HumanReviews] (
    [Id]                   int               NOT NULL IDENTITY,
    [SubmissionId]         int               NOT NULL,
    [RubricId]             int               NOT NULL,
    [ReviewerId]           int               NOT NULL,
    [FinalScore]           decimal(9,4)      NOT NULL,
    [CriterionResultsJson] nvarchar(max)     NOT NULL DEFAULT N'[]', -- Giữ tương thích với C# Entity
    [OverallFeedback]      nvarchar(max)     NULL,
    [ReviewedAt]           datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_HumanReviews] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_HumanReviews_Id_RubricId] UNIQUE ([Id], [RubricId]), -- Đích cho FK ghép của HumanReviewCriterionScores
    CONSTRAINT [CK_HumanReviews_FinalScore] CHECK ([FinalScore] >= 0),
    CONSTRAINT [CK_HumanReviews_CriterionResultsJson] CHECK (ISJSON([CriterionResultsJson]) = 1),
    CONSTRAINT [FK_HumanReviews_Submissions_SubmissionId] FOREIGN KEY ([SubmissionId])
        REFERENCES [artifact].[Submissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_HumanReviews_Rubrics_RubricId] FOREIGN KEY ([RubricId])
        REFERENCES [assessment].[Rubrics] ([Id]),
    CONSTRAINT [FK_HumanReviews_Users_ReviewerId] FOREIGN KEY ([ReviewerId])
        REFERENCES [identity].[Users] ([Id])
);
GO

-- Bảng tách chuẩn 1NF-3NF cho điểm từng tiêu chí của Chuyên gia (Mục 8.2 & 8.4)
CREATE TABLE [assessment].[HumanReviewCriterionScores] (
    [HumanReviewId]     int           NOT NULL,
    [RubricCriterionId] int           NOT NULL,
    [RubricId]          int           NOT NULL, -- FK ghép đảm bảo tiêu chí thuộc đúng Rubric của lần chấm
    [Score]             decimal(9,4)  NOT NULL,
    [Comment]           nvarchar(max) NULL,
    CONSTRAINT [PK_HumanReviewCriterionScores] PRIMARY KEY ([HumanReviewId], [RubricCriterionId]),
    CONSTRAINT [CK_HumanReviewCriterionScores_Score] CHECK ([Score] >= 0),
    CONSTRAINT [FK_HumanReviewCriterionScores_Review_Rubric] FOREIGN KEY ([HumanReviewId], [RubricId])
        REFERENCES [assessment].[HumanReviews] ([Id], [RubricId]) ON DELETE CASCADE,
    CONSTRAINT [FK_HumanReviewCriterionScores_Criterion_Rubric] FOREIGN KEY ([RubricCriterionId], [RubricId])
        REFERENCES [assessment].[RubricCriteria] ([Id], [RubricId])
);
GO

CREATE TABLE [assessment].[ReviewRequests] (
    [Id]                 int               NOT NULL IDENTITY,
    [SubmissionId]       int               NOT NULL,
    [LearnerId]          int               NOT NULL,
    [AssignedReviewerId] int               NULL,
    [RequestNote]        nvarchar(1000)    NULL,
    [Status]             nvarchar(20)      NOT NULL DEFAULT N'Pending',
    [RequestedAt]        datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [AcceptedAt]         datetimeoffset(3) NULL,
    [CompletedAt]        datetimeoffset(3) NULL,
    CONSTRAINT [PK_ReviewRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ReviewRequests_Status] CHECK ([Status] IN (N'Pending', N'Accepted', N'Completed', N'Cancelled')),
    CONSTRAINT [FK_ReviewRequests_Submissions_SubmissionId] FOREIGN KEY ([SubmissionId])
        REFERENCES [artifact].[Submissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewRequests_Users_LearnerId] FOREIGN KEY ([LearnerId])
        REFERENCES [identity].[Users] ([Id]),
    CONSTRAINT [FK_ReviewRequests_Users_AssignedReviewerId] FOREIGN KEY ([AssignedReviewerId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE INDEX [IX_ReviewRequests_SubmissionId] ON [assessment].[ReviewRequests] ([SubmissionId]);
CREATE INDEX [IX_ReviewRequests_LearnerId] ON [assessment].[ReviewRequests] ([LearnerId]);
CREATE INDEX [IX_ReviewRequests_AssignedReviewerId] ON [assessment].[ReviewRequests] ([AssignedReviewerId]);
CREATE INDEX [IX_ReviewRequests_Status] ON [assessment].[ReviewRequests] ([Status]);

CREATE UNIQUE INDEX [IX_BabokTasks_BabokVersion_Code] ON [assessment].[BabokTasks] ([BabokVersion], [Code]);
CREATE INDEX [IX_BabokTasks_KnowledgeAreaCode] ON [assessment].[BabokTasks] ([KnowledgeAreaCode]);
CREATE INDEX [IX_Rubrics_CreatedByUserId] ON [assessment].[Rubrics] ([CreatedByUserId]);
CREATE INDEX [IX_Rubrics_ScenarioVersionId] ON [assessment].[Rubrics] ([ScenarioVersionId]);
CREATE UNIQUE INDEX [IX_Rubrics_ScenarioVersionId_Version] ON [assessment].[Rubrics] ([ScenarioVersionId], [Version]);
CREATE INDEX [IX_RubricCriteria_BabokTaskId] ON [assessment].[RubricCriteria] ([BabokTaskId]);
CREATE UNIQUE INDEX [IX_RubricCriteria_RubricId_Code] ON [assessment].[RubricCriteria] ([RubricId], [Code]);
CREATE INDEX [IX_AIPracticeFeedbacks_SubmissionId] ON [assessment].[AIPracticeFeedbacks] ([SubmissionId]);
CREATE INDEX [IX_AIFeedbackCriterionScores_RubricCriterionId] ON [assessment].[AIFeedbackCriterionScores] ([RubricCriterionId]);
CREATE INDEX [IX_HumanReviews_ReviewerId] ON [assessment].[HumanReviews] ([ReviewerId]);
CREATE INDEX [IX_HumanReviews_RubricId] ON [assessment].[HumanReviews] ([RubricId]);
CREATE INDEX [IX_HumanReviews_SubmissionId] ON [assessment].[HumanReviews] ([SubmissionId]);
CREATE UNIQUE INDEX [UX_HumanReviews_SubmissionId_ReviewerId] ON [assessment].[HumanReviews] ([SubmissionId], [ReviewerId]);
CREATE INDEX [IX_HumanReviewCriterionScores_RubricCriterionId] ON [assessment].[HumanReviewCriterionScores] ([RubricCriterionId]);
GO


-- =============================================================================
-- 10B. SCHEMA: [commerce] — Gói cước, Đơn hàng, Thanh toán PayOS & Quyền truy cập
-- =============================================================================
CREATE TABLE [commerce].[Plans] (
    [Id]            int               NOT NULL IDENTITY,
    [Code]          nvarchar(50)      NOT NULL,
    [Name]          nvarchar(150)     NOT NULL,
    [Description]   nvarchar(1000)    NULL,
    [PlanType]      nvarchar(30)      NOT NULL,
    [ScopeType]     nvarchar(30)      NOT NULL DEFAULT N'All',
    [ScopeId]       int               NULL,
    [Price]         decimal(12,2)     NOT NULL DEFAULT 0,
    [Currency]      nvarchar(10)      NOT NULL DEFAULT N'VND',
    [DurationDays]  int               NULL,
    [SessionQuota]  int               NULL,
    [ReviewCredits] int               NOT NULL DEFAULT 0,
    [Status]        nvarchar(30)      NOT NULL DEFAULT N'Draft',
    [DisplayOrder]  int               NOT NULL DEFAULT 0,
    [CreatedAt]     datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_Plans] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Plans_Code] UNIQUE ([Code]),
    CONSTRAINT [CK_Plans_Price] CHECK ([Price] >= 0),
    CONSTRAINT [CK_Plans_PlanType] CHECK ([PlanType] IN (N'Free', N'ScenarioBundle', N'Subscription', N'ReviewCredit')),
    CONSTRAINT [CK_Plans_ScopeType] CHECK ([ScopeType] IN (N'Industry', N'Application', N'Scenario', N'All')),
    CONSTRAINT [CK_Plans_Status] CHECK ([Status] IN (N'Draft', N'Published', N'Archived'))
);
GO

CREATE TABLE [commerce].[Orders] (
    [Id]               int               NOT NULL IDENTITY,
    [OrderCode]        bigint            NOT NULL,
    [UserId]           int               NOT NULL,
    [PlanId]           int               NOT NULL,
    [Amount]           decimal(12,2)     NOT NULL,
    [Currency]         nvarchar(10)      NOT NULL DEFAULT N'VND',
    [PlanSnapshotJson] nvarchar(max)     NOT NULL DEFAULT N'{}',
    [Status]           nvarchar(30)      NOT NULL DEFAULT N'Pending',
    [IdempotencyKey]   nvarchar(100)     NULL,
    [CheckoutUrl]      nvarchar(500)     NULL,
    [QrCode]           nvarchar(2000)    NULL,
    [CreatedAt]        datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [ExpiresAt]        datetimeoffset(3) NOT NULL,
    [PaidAt]           datetimeoffset(3) NULL,
    [CancelledAt]      datetimeoffset(3) NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Orders_OrderCode] UNIQUE ([OrderCode]),
    CONSTRAINT [CK_Orders_Status] CHECK ([Status] IN (N'Pending', N'Paid', N'Cancelled', N'Expired', N'Failed', N'Refunded')),
    CONSTRAINT [FK_Orders_Plans] FOREIGN KEY ([PlanId]) REFERENCES [commerce].[Plans] ([Id]),
    CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id])
);
GO

CREATE TABLE [commerce].[PaymentTransactions] (
    [Id]                    int               NOT NULL IDENTITY,
    [OrderId]               int               NOT NULL,
    [Provider]              nvarchar(50)      NOT NULL DEFAULT N'PayOS',
    [ProviderTransactionId] nvarchar(100)     NOT NULL,
    [Amount]                decimal(12,2)     NOT NULL,
    [Status]                nvarchar(30)      NOT NULL DEFAULT N'Pending',
    [SignatureValid]        bit               NOT NULL DEFAULT 0,
    [RawPayloadJson]        nvarchar(max)     NULL,
    [ReceivedAt]            datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_PaymentTransactions_Provider_TxId] UNIQUE ([Provider], [ProviderTransactionId]),
    CONSTRAINT [CK_PaymentTransactions_Status] CHECK ([Status] IN (N'Pending', N'Succeeded', N'Failed')),
    CONSTRAINT [FK_PaymentTransactions_Orders] FOREIGN KEY ([OrderId])
        REFERENCES [commerce].[Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [commerce].[Entitlements] (
    [Id]                 int               NOT NULL IDENTITY,
    [UserId]             int               NOT NULL,
    [SourceType]         nvarchar(30)      NOT NULL DEFAULT N'Purchase',
    [SourceOrderId]      int               NULL,
    [ScopeType]          nvarchar(30)      NOT NULL,
    [ScopeId]            int               NULL,
    [StartsAt]           datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [EndsAt]             datetimeoffset(3) NULL,
    [SessionQuota]       int               NULL,
    [SessionsUsed]       int               NOT NULL DEFAULT 0,
    [ReviewCreditsQuota] int               NOT NULL DEFAULT 0,
    [ReviewCreditsUsed]  int               NOT NULL DEFAULT 0,
    [Status]             nvarchar(30)      NOT NULL DEFAULT N'Active',
    [CreatedAt]          datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_Entitlements] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Entitlements_SourceType] CHECK ([SourceType] IN (N'Free', N'Purchase', N'Subscription', N'CohortAssignment', N'Admin')),
    CONSTRAINT [CK_Entitlements_ScopeType] CHECK ([ScopeType] IN (N'Industry', N'Application', N'Scenario', N'All')),
    CONSTRAINT [CK_Entitlements_Status] CHECK ([Status] IN (N'Active', N'Expired', N'Revoked')),
    CONSTRAINT [FK_Entitlements_Orders] FOREIGN KEY ([SourceOrderId])
        REFERENCES [commerce].[Orders] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Entitlements_Users] FOREIGN KEY ([UserId])
        REFERENCES [identity].[Users] ([Id])
);
GO

CREATE INDEX [IX_Orders_UserId] ON [commerce].[Orders] ([UserId]);
CREATE INDEX [IX_Orders_IdempotencyKey] ON [commerce].[Orders] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL;
CREATE INDEX [IX_Entitlements_UserId_Status] ON [commerce].[Entitlements] ([UserId], [Status]);
CREATE INDEX [IX_Entitlements_Scope] ON [commerce].[Entitlements] ([ScopeType], [ScopeId]);
GO
-- =============================================================================
-- 11. TRUY VẤN BÁO CÁO ĐẦU RA — KIỂM CHỨNG THIẾT KẾ (Mục 9.4 Quy trình ERD)
-- "Báo cáo không phải là thực thể. Thiết kế dữ liệu tốt là thiết kế mà mọi
--  báo cáo đều truy vấn được qua VIEW/SELECT mà không cần lưu bảng báo cáo."
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Báo cáo 1: Biểu đồ Radar năng lực học viên theo từng BABOK Knowledge Area
-- Người xem: Learner, Instructor
-- -----------------------------------------------------------------------------
CREATE OR ALTER VIEW [assessment].[v_LearnerBabokCompetencyRadar] AS
SELECT
    ss.[LearnerId],
    u.[FullName]                                                        AS [LearnerName],
    bt.[KnowledgeAreaCode]                                                           AS [KnowledgeAreaCode],
    bt.[KnowledgeAreaName]                                                           AS [KnowledgeAreaName],
    COUNT(DISTINCT sub.[Id])                                            AS [EvaluatedSubmissionsCount],
    ROUND(
        100.0 * SUM(COALESCE(hcs.[Score], acs.[Score]) * rc.[Weight])
        / NULLIF(SUM(rc.[MaxScore] * rc.[Weight]), 0),
        2
    )                                                                   AS [MasteryPercentage]
FROM [simulation].[SimulationSessions] ss
JOIN [identity].[Users] u
    ON u.[Id] = ss.[LearnerId]
JOIN [artifact].[Submissions] sub
    ON sub.[SessionId] = ss.[Id]
JOIN [assessment].[AIPracticeFeedbacks] aif
    ON aif.[SubmissionId] = sub.[Id]
   AND aif.[Status] = N'Completed'
JOIN [assessment].[AIFeedbackCriterionScores] acs
    ON acs.[AIFeedbackId] = aif.[Id]
   AND acs.[RubricId] = aif.[RubricId]
JOIN [assessment].[RubricCriteria] rc
    ON rc.[Id] = acs.[RubricCriterionId]
   AND rc.[RubricId] = acs.[RubricId]
LEFT JOIN [assessment].[HumanReviews] hr
    ON hr.[SubmissionId] = sub.[Id]
LEFT JOIN [assessment].[HumanReviewCriterionScores] hcs
    ON hcs.[HumanReviewId] = hr.[Id]
   AND hcs.[RubricId] = hr.[RubricId]
   AND hcs.[RubricCriterionId] = rc.[Id]



JOIN [assessment].[BabokTasks] bt
    ON bt.[Id] = rc.[BabokTaskId]
GROUP BY
    ss.[LearnerId],
    u.[FullName],
    bt.[KnowledgeAreaCode],
    bt.[KnowledgeAreaName];
GO

-- -----------------------------------------------------------------------------
-- Báo cáo 2: Thống kê năng lực cả lớp (Cohort) & Phát hiện điểm yếu theo BABOK Task
-- Người xem: Instructor / Assessor
-- -----------------------------------------------------------------------------
CREATE OR ALTER VIEW [assessment].[v_CohortBabokAnalytics] AS
SELECT
    c.[Id]                                                              AS [CohortId],
    c.[Code]                                                            AS [CohortCode],
    c.[Name]                                                            AS [CohortName],
    bt.[KnowledgeAreaCode]                                                           AS [KnowledgeAreaCode],
    bt.[Code]                                                           AS [BabokTaskCode],
    bt.[Name]                                                           AS [BabokTaskName],
    COUNT(DISTINCT cm.[LearnerId])                                      AS [AssessedLearners],
    ROUND(AVG(100.0 * COALESCE(hcs.[Score], acs.[Score]) / NULLIF(rc.[MaxScore], 0)), 2)       AS [AvgAchievementPct],
    COUNT(DISTINCT CASE
        WHEN (100.0 * COALESCE(hcs.[Score], acs.[Score]) / NULLIF(rc.[MaxScore], 0)) < 50.0
        THEN cm.[LearnerId]
    END)                                                                AS [BelowThresholdLearnersCount]
FROM [training].[Cohorts] c
JOIN [training].[CohortMembers] cm
    ON cm.[CohortId] = c.[Id]
   AND cm.[Status] IN (N'Active', N'Completed')
JOIN [simulation].[SimulationSessions] ss
    ON ss.[LearnerId] = cm.[LearnerId]
JOIN [artifact].[Submissions] sub
    ON sub.[SessionId] = ss.[Id]
JOIN [assessment].[AIPracticeFeedbacks] aif
    ON aif.[SubmissionId] = sub.[Id]
   AND aif.[Status] = N'Completed'
JOIN [assessment].[AIFeedbackCriterionScores] acs
    ON acs.[AIFeedbackId] = aif.[Id]
   AND acs.[RubricId] = aif.[RubricId]
JOIN [assessment].[RubricCriteria] rc
    ON rc.[Id] = acs.[RubricCriterionId]
   AND rc.[RubricId] = acs.[RubricId]
LEFT JOIN [assessment].[HumanReviews] hr
    ON hr.[SubmissionId] = sub.[Id]
LEFT JOIN [assessment].[HumanReviewCriterionScores] hcs
    ON hcs.[HumanReviewId] = hr.[Id]
   AND hcs.[RubricId] = hr.[RubricId]
   AND hcs.[RubricCriterionId] = rc.[Id]


JOIN [assessment].[BabokTasks] bt
    ON bt.[Id] = rc.[BabokTaskId]
GROUP BY
    c.[Id],
    c.[Code],
    c.[Name],
    bt.[KnowledgeAreaCode],
    bt.[Code],
    bt.[Name];
GO

-- -----------------------------------------------------------------------------
-- Báo cáo 3: Đối chiếu điểm AI và Điểm chuẩn của Chuyên gia (Calibration & Research 3.3)
-- Người xem: Instructor, Expert, Researcher
-- -----------------------------------------------------------------------------
CREATE OR ALTER VIEW [assessment].[v_AiVsExpertScoreCalibration] AS
WITH ExpertReference AS (
    SELECT
        hr.[SubmissionId],
        hr.[RubricId],
        hcs.[RubricCriterionId],
        COUNT(DISTINCT hr.[ReviewerId])      AS [ExpertCount],
        AVG(hcs.[Score])                     AS [ExpertAvgCriterionScore],
        MAX(hcs.[Score]) - MIN(hcs.[Score])  AS [ExpertScoreSpread]
    FROM [assessment].[HumanReviews] hr
    JOIN [assessment].[HumanReviewCriterionScores] hcs
        ON hcs.[HumanReviewId] = hr.[Id]
       AND hcs.[RubricId] = hr.[RubricId]
    GROUP BY
        hr.[SubmissionId],
        hr.[RubricId],
        hcs.[RubricCriterionId]
)
SELECT
    sub.[Id]                                                    AS [SubmissionId],
    ss.[LearnerId],
    sc.[Code]                                                   AS [ScenarioCode],
    sv.[VersionNo]                                              AS [ScenarioVersionNo],
    rc.[Code]                                                   AS [CriterionCode],
    rc.[Name]                                                   AS [CriterionName],
    rc.[MaxScore],
    aif.[Model]                                                 AS [AIModel],
    acs.[Score]                                                 AS [AIScore],
    er.[ExpertCount],
    ROUND(er.[ExpertAvgCriterionScore], 4)                      AS [ExpertReferenceScore],
    ROUND(er.[ExpertScoreSpread], 4)                            AS [InterRaterSpread],
    ROUND(ABS(acs.[Score] - er.[ExpertAvgCriterionScore]), 4)   AS [AbsoluteError]
FROM [artifact].[Submissions] sub
JOIN [simulation].[SimulationSessions] ss
    ON ss.[Id] = sub.[SessionId]
JOIN [scenario].[ScenarioVersions] sv
    ON sv.[Id] = ss.[ScenarioVersionId]
JOIN [scenario].[Scenarios] sc
    ON sc.[Id] = sv.[ScenarioId]
JOIN [assessment].[AIPracticeFeedbacks] aif
    ON aif.[SubmissionId] = sub.[Id]
   AND aif.[Status] = N'Completed'
JOIN [assessment].[AIFeedbackCriterionScores] acs
    ON acs.[AIFeedbackId] = aif.[Id]
   AND acs.[RubricId] = aif.[RubricId]
JOIN [assessment].[RubricCriteria] rc
    ON rc.[Id] = acs.[RubricCriterionId]
   AND rc.[RubricId] = acs.[RubricId]
JOIN ExpertReference er
    ON er.[SubmissionId] = sub.[Id]
   AND er.[RubricId] = aif.[RubricId]
   AND er.[RubricCriterionId] = rc.[Id];
GO

-- -----------------------------------------------------------------------------
-- Báo cáo 4: Tiến độ khai thác thông tin ẩn (Elicitation Coverage) theo phiên
-- Người xem: Learner, Instructor
-- -----------------------------------------------------------------------------
CREATE OR ALTER VIEW [simulation].[v_SessionElicitationProgress] AS
SELECT
    ss.[Id]                                                                     AS [SessionId],
    ss.[LearnerId],
    sc.[Code]                                                                   AS [ScenarioCode],
    sv.[VersionNo]                                                              AS [ScenarioVersionNo],
    sp.[Name]                                                                   AS [StakeholderName],
    sp.[StakeholderRole],
    sa.[TrustLevel],
    sa.[PatienceLevel],
    sa.[CooperationLevel],
    COUNT(saks.[Id])                                                            AS [TotalAccessibleKnowledgeItems],
    SUM(CASE WHEN saks.[IsRevealed] = 1 THEN 1 ELSE 0 END)                      AS [RevealedKnowledgeItems],
    ROUND(
        100.0 * SUM(CASE WHEN saks.[IsRevealed] = 1 THEN 1 ELSE 0 END)
        / NULLIF(COUNT(saks.[Id]), 0),
        1
    )                                                                           AS [ElicitationCoveragePct]
FROM [simulation].[SimulationSessions] ss
JOIN [scenario].[ScenarioVersions] sv
    ON sv.[Id] = ss.[ScenarioVersionId]
JOIN [scenario].[Scenarios] sc
    ON sc.[Id] = sv.[ScenarioId]
JOIN [simulation].[SessionAgents] sa
    ON sa.[SessionId] = ss.[Id]
   AND sa.[ScenarioVersionId] = ss.[ScenarioVersionId]
JOIN [scenario].[StakeholderPersonas] sp
    ON sp.[Id] = sa.[PersonaId]
   AND sp.[ScenarioVersionId] = sa.[ScenarioVersionId]
LEFT JOIN [simulation].[SessionAgentKnowledgeStates] saks
    ON saks.[SessionAgentId] = sa.[Id]
GROUP BY
    ss.[Id],
    ss.[LearnerId],
    sc.[Code],
    sv.[VersionNo],
    sp.[Name],
    sp.[StakeholderRole],
    sa.[TrustLevel],
    sa.[PatienceLevel],
    sa.[CooperationLevel];
GO

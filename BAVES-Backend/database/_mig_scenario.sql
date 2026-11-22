IF OBJECT_ID(N'[scenario].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'scenario') IS NULL EXEC(N'CREATE SCHEMA [scenario];');
    CREATE TABLE [scenario].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    IF SCHEMA_ID(N'scenario') IS NULL EXEC(N'CREATE SCHEMA [scenario];');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[Scenarios] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationId] int NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_Scenarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[ScenarioVersions] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioId] int NOT NULL,
        [VersionNo] int NOT NULL,
        [Summary] nvarchar(max) NULL,
        [BusinessContext] nvarchar(max) NOT NULL,
        [BusinessProblem] nvarchar(max) NOT NULL,
        [BusinessObjectives] nvarchar(max) NOT NULL,
        [BARole] nvarchar(500) NOT NULL,
        [Assignment] nvarchar(max) NOT NULL,
        [InScope] nvarchar(max) NULL,
        [OutOfScope] nvarchar(max) NULL,
        [KnownConstraints] nvarchar(max) NULL,
        [Difficulty] nvarchar(40) NOT NULL,
        [IsPublished] bit NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [PublishedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_ScenarioVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ScenarioVersions_Scenarios_ScenarioId] FOREIGN KEY ([ScenarioId]) REFERENCES [scenario].[Scenarios] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[BusinessRules] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsHidden] bit NOT NULL,
        [Importance] nvarchar(40) NOT NULL,
        CONSTRAINT [PK_BusinessRules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BusinessRules_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId]) REFERENCES [scenario].[ScenarioVersions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[KnowledgeItems] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Category] nvarchar(40) NOT NULL,
        [Importance] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_KnowledgeItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KnowledgeItems_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId]) REFERENCES [scenario].[ScenarioVersions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[ScenarioEvents] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [TriggerType] nvarchar(40) NOT NULL,
        [TriggerConditionJson] nvarchar(max) NULL,
        [EventContent] nvarchar(max) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ScenarioEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ScenarioEvents_TriggerConditionJson] CHECK ([TriggerConditionJson] IS NULL OR ISJSON([TriggerConditionJson]) = 1),
        CONSTRAINT [FK_ScenarioEvents_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId]) REFERENCES [scenario].[ScenarioVersions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[StakeholderPersonas] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [StakeholderRole] nvarchar(200) NOT NULL,
        [Department] nvarchar(200) NULL,
        [Responsibilities] nvarchar(max) NULL,
        [Goals] nvarchar(max) NULL,
        [PainPoints] nvarchar(max) NULL,
        [BusinessConcerns] nvarchar(max) NULL,
        [CommunicationStyle] nvarchar(200) NULL,
        [Personality] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_StakeholderPersonas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StakeholderPersonas_ScenarioVersions_ScenarioVersionId] FOREIGN KEY ([ScenarioVersionId]) REFERENCES [scenario].[ScenarioVersions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE TABLE [scenario].[StakeholderKnowledgeAccess] (
        [Id] int NOT NULL IDENTITY,
        [StakeholderPersonaId] int NOT NULL,
        [KnowledgeItemId] int NOT NULL,
        [AccessType] nvarchar(40) NOT NULL,
        [DisclosureRule] nvarchar(max) NULL,
        [IsHidden] bit NOT NULL,
        CONSTRAINT [PK_StakeholderKnowledgeAccess] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StakeholderKnowledgeAccess_KnowledgeItems_KnowledgeItemId] FOREIGN KEY ([KnowledgeItemId]) REFERENCES [scenario].[KnowledgeItems] ([Id]),
        CONSTRAINT [FK_StakeholderKnowledgeAccess_StakeholderPersonas_StakeholderPersonaId] FOREIGN KEY ([StakeholderPersonaId]) REFERENCES [scenario].[StakeholderPersonas] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessRules_ScenarioVersionId_Code] ON [scenario].[BusinessRules] ([ScenarioVersionId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KnowledgeItems_ScenarioVersionId_Code] ON [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE INDEX [IX_ScenarioEvents_ScenarioVersionId] ON [scenario].[ScenarioEvents] ([ScenarioVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE INDEX [IX_Scenarios_ApplicationId] ON [scenario].[Scenarios] ([ApplicationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Scenarios_Code] ON [scenario].[Scenarios] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE INDEX [IX_Scenarios_CreatedByUserId] ON [scenario].[Scenarios] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ScenarioVersions_ScenarioId_VersionNo] ON [scenario].[ScenarioVersions] ([ScenarioId], [VersionNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE INDEX [IX_StakeholderKnowledgeAccess_KnowledgeItemId] ON [scenario].[StakeholderKnowledgeAccess] ([KnowledgeItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StakeholderKnowledgeAccess_StakeholderPersonaId_KnowledgeItemId] ON [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    CREATE INDEX [IX_StakeholderPersonas_ScenarioVersionId] ON [scenario].[StakeholderPersonas] ([ScenarioVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063532_Initial_Scenario'
)
BEGIN
    INSERT INTO [scenario].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063532_Initial_Scenario', N'9.0.13');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] DROP CONSTRAINT [FK_ScenarioVersions_Scenarios_ScenarioId];
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] ADD [Status] nvarchar(40) NOT NULL DEFAULT N'Draft';
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    EXEC(N'ALTER TABLE [scenario].[ScenarioVersions] ADD CONSTRAINT [CK_ScenarioVersions_Difficulty] CHECK ([Difficulty] IN (N''Beginner'', N''Intermediate'', N''Advanced''))');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    EXEC(N'ALTER TABLE [scenario].[ScenarioVersions] ADD CONSTRAINT [CK_ScenarioVersions_Status] CHECK ([Status] IN (N''Draft'', N''Published'', N''Archived''))');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    EXEC(N'ALTER TABLE [scenario].[ScenarioVersions] ADD CONSTRAINT [CK_ScenarioVersions_VersionNo] CHECK ([VersionNo] > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] ADD CONSTRAINT [FK_ScenarioVersions_Scenarios_ScenarioId] FOREIGN KEY ([ScenarioId]) REFERENCES [scenario].[Scenarios] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094458_AddStatusToScenarioVersions'
)
BEGIN
    INSERT INTO [scenario].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023094458_AddStatusToScenarioVersions', N'9.0.13');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] ADD [ReviewComment] nvarchar(4000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] ADD [ReviewedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] ADD [ReviewedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] ADD [SubmittedForReviewAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    ALTER TABLE [scenario].[ScenarioVersions] DROP CONSTRAINT [CK_ScenarioVersions_Status];
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    EXEC(N'ALTER TABLE [scenario].[ScenarioVersions] ADD CONSTRAINT [CK_ScenarioVersions_Status] CHECK ([Status] IN (N''Draft'', N''Published'', N''Archived'', N''PendingReview'', N''Rejected''))');
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    CREATE INDEX [IX_ScenarioVersions_Status] ON [scenario].[ScenarioVersions] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [scenario].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113130500_AddScenarioReviewGate'
)
BEGIN
    INSERT INTO [scenario].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261113130500_AddScenarioReviewGate', N'9.0.13');
END;

COMMIT;
GO


IF OBJECT_ID(N'[training].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'training') IS NULL EXEC(N'CREATE SCHEMA [training];');
    CREATE TABLE [training].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    IF SCHEMA_ID(N'training') IS NULL EXEC(N'CREATE SCHEMA [training];');
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE TABLE [training].[Cohorts] (
        [Id] int NOT NULL IDENTITY,
        [InstructorId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [StartDate] date NULL,
        [EndDate] date NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_Cohorts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE TABLE [training].[Assignments] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [CohortId] int NULL,
        [LearnerId] int NULL,
        [RubricId] int NULL,
        [AssignedByUserId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Instructions] nvarchar(max) NULL,
        [StartAt] datetimeoffset(3) NULL,
        [DueAt] datetimeoffset(3) NULL,
        [MaxAttempts] int NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Assignments_MaxAttempts] CHECK ([MaxAttempts] IS NULL OR [MaxAttempts] > 0),
        CONSTRAINT [CK_Assignments_Recipient] CHECK (([CohortId] IS NOT NULL AND [LearnerId] IS NULL) OR ([CohortId] IS NULL AND [LearnerId] IS NOT NULL)),
        CONSTRAINT [FK_Assignments_Cohorts_CohortId] FOREIGN KEY ([CohortId]) REFERENCES [training].[Cohorts] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE TABLE [training].[CohortMembers] (
        [Id] int NOT NULL IDENTITY,
        [CohortId] int NOT NULL,
        [LearnerId] int NOT NULL,
        [JoinedAt] datetimeoffset(3) NOT NULL,
        [Status] nvarchar(40) NOT NULL,
        CONSTRAINT [PK_CohortMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CohortMembers_Cohorts_CohortId] FOREIGN KEY ([CohortId]) REFERENCES [training].[Cohorts] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_Assignments_AssignedByUserId] ON [training].[Assignments] ([AssignedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_Assignments_CohortId] ON [training].[Assignments] ([CohortId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_Assignments_LearnerId] ON [training].[Assignments] ([LearnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_Assignments_RubricId] ON [training].[Assignments] ([RubricId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_Assignments_ScenarioVersionId] ON [training].[Assignments] ([ScenarioVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CohortMembers_CohortId_LearnerId] ON [training].[CohortMembers] ([CohortId], [LearnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_CohortMembers_LearnerId] ON [training].[CohortMembers] ([LearnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cohorts_Code] ON [training].[Cohorts] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    CREATE INDEX [IX_Cohorts_InstructorId] ON [training].[Cohorts] ([InstructorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [training].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063719_Initial_Training'
)
BEGIN
    INSERT INTO [training].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063719_Initial_Training', N'9.0.13');
END;

COMMIT;
GO


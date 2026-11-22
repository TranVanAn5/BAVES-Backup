IF OBJECT_ID(N'[planning].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'planning') IS NULL EXEC(N'CREATE SCHEMA [planning];');
    CREATE TABLE [planning].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    IF SCHEMA_ID(N'planning') IS NULL EXEC(N'CREATE SCHEMA [planning];');
END;

IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    CREATE TABLE [planning].[BAPlans] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [Objective] nvarchar(max) NOT NULL,
        [Approach] nvarchar(max) NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_BAPlans] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    CREATE TABLE [planning].[PlannedQuestions] (
        [Id] int NOT NULL IDENTITY,
        [BAPlanId] int NOT NULL,
        [StakeholderPersonaId] int NULL,
        [Topic] nvarchar(max) NULL,
        [InformationNeed] nvarchar(max) NULL,
        [Question] nvarchar(max) NOT NULL,
        [Priority] nvarchar(40) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_PlannedQuestions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlannedQuestions_BAPlans_BAPlanId] FOREIGN KEY ([BAPlanId]) REFERENCES [planning].[BAPlans] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BAPlans_SessionId] ON [planning].[BAPlans] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    CREATE INDEX [IX_PlannedQuestions_BAPlanId_DisplayOrder] ON [planning].[PlannedQuestions] ([BAPlanId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    CREATE INDEX [IX_PlannedQuestions_StakeholderPersonaId] ON [planning].[PlannedQuestions] ([StakeholderPersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [planning].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063744_Initial_Planning'
)
BEGIN
    INSERT INTO [planning].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063744_Initial_Planning', N'9.0.13');
END;

COMMIT;
GO


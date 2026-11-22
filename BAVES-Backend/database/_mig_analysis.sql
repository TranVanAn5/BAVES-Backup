IF OBJECT_ID(N'[analysis].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'analysis') IS NULL EXEC(N'CREATE SCHEMA [analysis];');
    CREATE TABLE [analysis].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    IF SCHEMA_ID(N'analysis') IS NULL EXEC(N'CREATE SCHEMA [analysis];');
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE TABLE [analysis].[AnalysisItems] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [ItemType] nvarchar(40) NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Priority] nvarchar(40) NULL,
        [Status] nvarchar(40) NOT NULL,
        [MetadataJson] nvarchar(max) NULL,
        [CreatedByUserId] int NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_AnalysisItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AnalysisItems_MetadataJson] CHECK ([MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE TABLE [analysis].[AnalysisItemRelations] (
        [Id] int NOT NULL IDENTITY,
        [SourceAnalysisItemId] int NOT NULL,
        [TargetAnalysisItemId] int NOT NULL,
        [RelationshipType] nvarchar(40) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_AnalysisItemRelations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AnalysisItemRelations_NoSelfLink] CHECK ([SourceAnalysisItemId] <> [TargetAnalysisItemId]),
        CONSTRAINT [FK_AnalysisItemRelations_AnalysisItems_SourceAnalysisItemId] FOREIGN KEY ([SourceAnalysisItemId]) REFERENCES [analysis].[AnalysisItems] ([Id]),
        CONSTRAINT [FK_AnalysisItemRelations_AnalysisItems_TargetAnalysisItemId] FOREIGN KEY ([TargetAnalysisItemId]) REFERENCES [analysis].[AnalysisItems] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE TABLE [analysis].[MessageEvidenceLinks] (
        [Id] int NOT NULL IDENTITY,
        [MessageId] int NOT NULL,
        [AnalysisItemId] int NOT NULL,
        [RelationshipType] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_MessageEvidenceLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MessageEvidenceLinks_AnalysisItems_AnalysisItemId] FOREIGN KEY ([AnalysisItemId]) REFERENCES [analysis].[AnalysisItems] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AnalysisItemRelations_SourceAnalysisItemId_TargetAnalysisItemId_RelationshipType] ON [analysis].[AnalysisItemRelations] ([SourceAnalysisItemId], [TargetAnalysisItemId], [RelationshipType]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE INDEX [IX_AnalysisItemRelations_TargetAnalysisItemId] ON [analysis].[AnalysisItemRelations] ([TargetAnalysisItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE INDEX [IX_AnalysisItems_CreatedByUserId] ON [analysis].[AnalysisItems] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE INDEX [IX_AnalysisItems_SessionId] ON [analysis].[AnalysisItems] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AnalysisItems_SessionId_Code] ON [analysis].[AnalysisItems] ([SessionId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE INDEX [IX_AnalysisItems_SessionId_ItemType] ON [analysis].[AnalysisItems] ([SessionId], [ItemType]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE INDEX [IX_MessageEvidenceLinks_AnalysisItemId] ON [analysis].[MessageEvidenceLinks] ([AnalysisItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE INDEX [IX_MessageEvidenceLinks_MessageId] ON [analysis].[MessageEvidenceLinks] ([MessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MessageEvidenceLinks_MessageId_AnalysisItemId_RelationshipType] ON [analysis].[MessageEvidenceLinks] ([MessageId], [AnalysisItemId], [RelationshipType]);
END;

IF NOT EXISTS (
    SELECT * FROM [analysis].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063756_Initial_Analysis'
)
BEGIN
    INSERT INTO [analysis].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063756_Initial_Analysis', N'9.0.13');
END;

COMMIT;
GO


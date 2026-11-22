IF OBJECT_ID(N'[artifact].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'artifact') IS NULL EXEC(N'CREATE SCHEMA [artifact];');
    CREATE TABLE [artifact].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    IF SCHEMA_ID(N'artifact') IS NULL EXEC(N'CREATE SCHEMA [artifact];');
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE TABLE [artifact].[ArtifactTemplates] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [ArtifactType] nvarchar(40) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_ArtifactTemplates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE TABLE [artifact].[Artifacts] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [ArtifactTemplateId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [DraftStructuredContentJson] nvarchar(max) NULL,
        [DraftPlainTextContent] nvarchar(max) NULL,
        [Status] nvarchar(40) NOT NULL,
        [CurrentVersionNo] int NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_Artifacts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Artifacts_DraftStructuredContentJson] CHECK ([DraftStructuredContentJson] IS NULL OR ISJSON([DraftStructuredContentJson]) = 1),
        CONSTRAINT [FK_Artifacts_ArtifactTemplates_ArtifactTemplateId] FOREIGN KEY ([ArtifactTemplateId]) REFERENCES [artifact].[ArtifactTemplates] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE TABLE [artifact].[ArtifactTemplateSections] (
        [Id] int NOT NULL IDENTITY,
        [ArtifactTemplateId] int NOT NULL,
        [SectionKey] nvarchar(80) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Instructions] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
        [IsRequired] bit NOT NULL,
        CONSTRAINT [PK_ArtifactTemplateSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ArtifactTemplateSections_ArtifactTemplates_ArtifactTemplateId] FOREIGN KEY ([ArtifactTemplateId]) REFERENCES [artifact].[ArtifactTemplates] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE TABLE [artifact].[ArtifactVersions] (
        [Id] int NOT NULL IDENTITY,
        [ArtifactId] int NOT NULL,
        [VersionNo] int NOT NULL,
        [StructuredContentJson] nvarchar(max) NOT NULL,
        [PlainTextContent] nvarchar(max) NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [ProcessingStatus] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_ArtifactVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ArtifactVersions_StructuredContentJson] CHECK (ISJSON([StructuredContentJson]) = 1),
        CONSTRAINT [FK_ArtifactVersions_Artifacts_ArtifactId] FOREIGN KEY ([ArtifactId]) REFERENCES [artifact].[Artifacts] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE TABLE [artifact].[Submissions] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [ArtifactVersionId] int NOT NULL,
        [AttemptNo] int NOT NULL,
        [Status] nvarchar(40) NOT NULL,
        [SubmittedAt] datetimeoffset(3) NOT NULL,
        [FinalizedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_Submissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Submissions_ArtifactVersions_ArtifactVersionId] FOREIGN KEY ([ArtifactVersionId]) REFERENCES [artifact].[ArtifactVersions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE INDEX [IX_Artifacts_ArtifactTemplateId] ON [artifact].[Artifacts] ([ArtifactTemplateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE INDEX [IX_Artifacts_SessionId] ON [artifact].[Artifacts] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ArtifactTemplates_Code] ON [artifact].[ArtifactTemplates] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ArtifactTemplateSections_ArtifactTemplateId_SectionKey] ON [artifact].[ArtifactTemplateSections] ([ArtifactTemplateId], [SectionKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ArtifactVersions_ArtifactId_VersionNo] ON [artifact].[ArtifactVersions] ([ArtifactId], [VersionNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE INDEX [IX_ArtifactVersions_CreatedByUserId] ON [artifact].[ArtifactVersions] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE INDEX [IX_Submissions_ArtifactVersionId] ON [artifact].[Submissions] ([ArtifactVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE INDEX [IX_Submissions_SessionId] ON [artifact].[Submissions] ([SessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Submissions_SessionId_AttemptNo] ON [artifact].[Submissions] ([SessionId], [AttemptNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [artifact].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063809_Initial_Artifact'
)
BEGIN
    INSERT INTO [artifact].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063809_Initial_Artifact', N'9.0.13');
END;

COMMIT;
GO


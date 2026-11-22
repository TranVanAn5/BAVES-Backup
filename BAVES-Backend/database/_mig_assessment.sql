IF OBJECT_ID(N'[assessment].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'assessment') IS NULL EXEC(N'CREATE SCHEMA [assessment];');
    CREATE TABLE [assessment].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    IF SCHEMA_ID(N'assessment') IS NULL EXEC(N'CREATE SCHEMA [assessment];');
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE TABLE [assessment].[AIPracticeFeedbacks] (
        [Id] int NOT NULL IDENTITY,
        [SubmissionId] int NOT NULL,
        [Provider] nvarchar(50) NOT NULL,
        [Model] nvarchar(100) NOT NULL,
        [Status] nvarchar(40) NOT NULL,
        [OverallFeedback] nvarchar(max) NULL,
        [FeedbackItemsJson] nvarchar(max) NOT NULL,
        [SuggestedScore] decimal(9,4) NULL,
        [Confidence] decimal(5,4) NULL,
        [StartedAt] datetimeoffset(3) NOT NULL,
        [CompletedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_AIPracticeFeedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AIPracticeFeedbacks_Confidence] CHECK ([Confidence] IS NULL OR ([Confidence] >= 0 AND [Confidence] <= 1)),
        CONSTRAINT [CK_AIPracticeFeedbacks_FeedbackItemsJson] CHECK (ISJSON([FeedbackItemsJson]) = 1)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE TABLE [assessment].[BabokKnowledgeAreas] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [BabokVersion] nvarchar(50) NOT NULL,
        [DisplayOrder] int NOT NULL,
        CONSTRAINT [PK_BabokKnowledgeAreas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE TABLE [assessment].[Rubrics] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Version] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [UpdatedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_Rubrics] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE TABLE [assessment].[BabokTasks] (
        [Id] int NOT NULL IDENTITY,
        [KnowledgeAreaId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Purpose] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [BabokVersion] nvarchar(50) NOT NULL,
        [DisplayOrder] int NOT NULL,
        CONSTRAINT [PK_BabokTasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BabokTasks_BabokKnowledgeAreas_KnowledgeAreaId] FOREIGN KEY ([KnowledgeAreaId]) REFERENCES [assessment].[BabokKnowledgeAreas] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE TABLE [assessment].[HumanReviews] (
        [Id] int NOT NULL IDENTITY,
        [SubmissionId] int NOT NULL,
        [RubricId] int NOT NULL,
        [ReviewerId] int NOT NULL,
        [FinalScore] decimal(9,4) NOT NULL,
        [CriterionResultsJson] nvarchar(max) NOT NULL,
        [OverallFeedback] nvarchar(max) NULL,
        [ReviewedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_HumanReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HumanReviews_CriterionResultsJson] CHECK (ISJSON([CriterionResultsJson]) = 1),
        CONSTRAINT [FK_HumanReviews_Rubrics_RubricId] FOREIGN KEY ([RubricId]) REFERENCES [assessment].[Rubrics] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE TABLE [assessment].[RubricCriteria] (
        [Id] int NOT NULL IDENTITY,
        [RubricId] int NOT NULL,
        [BabokTaskId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Weight] decimal(9,4) NOT NULL,
        [MaxScore] decimal(9,4) NOT NULL,
        [EvidenceGuidance] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
        [IsRequired] bit NOT NULL,
        CONSTRAINT [PK_RubricCriteria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RubricCriteria_BabokTasks_BabokTaskId] FOREIGN KEY ([BabokTaskId]) REFERENCES [assessment].[BabokTasks] ([Id]),
        CONSTRAINT [FK_RubricCriteria_Rubrics_RubricId] FOREIGN KEY ([RubricId]) REFERENCES [assessment].[Rubrics] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_AIPracticeFeedbacks_SubmissionId] ON [assessment].[AIPracticeFeedbacks] ([SubmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BabokKnowledgeAreas_Code] ON [assessment].[BabokKnowledgeAreas] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BabokTasks_BabokVersion_Code] ON [assessment].[BabokTasks] ([BabokVersion], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_BabokTasks_KnowledgeAreaId] ON [assessment].[BabokTasks] ([KnowledgeAreaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_HumanReviews_ReviewerId] ON [assessment].[HumanReviews] ([ReviewerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_HumanReviews_RubricId] ON [assessment].[HumanReviews] ([RubricId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_HumanReviews_SubmissionId] ON [assessment].[HumanReviews] ([SubmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_RubricCriteria_BabokTaskId] ON [assessment].[RubricCriteria] ([BabokTaskId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RubricCriteria_RubricId_Code] ON [assessment].[RubricCriteria] ([RubricId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_Rubrics_CreatedByUserId] ON [assessment].[Rubrics] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE INDEX [IX_Rubrics_ScenarioVersionId] ON [assessment].[Rubrics] ([ScenarioVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rubrics_ScenarioVersionId_Version] ON [assessment].[Rubrics] ([ScenarioVersionId], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063822_Initial_Assessment'
)
BEGIN
    INSERT INTO [assessment].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063822_Initial_Assessment', N'9.0.13');
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094717_AddCriterionScoreTables'
)
BEGIN
    CREATE TABLE [assessment].[AIFeedbackCriterionScores] (
        [AIFeedbackId] int NOT NULL,
        [RubricCriterionId] int NOT NULL,
        [Score] decimal(9,4) NOT NULL,
        [Comment] nvarchar(max) NULL,
        [EvidenceSnippet] nvarchar(max) NULL,
        CONSTRAINT [PK_AIFeedbackCriterionScores] PRIMARY KEY ([AIFeedbackId], [RubricCriterionId]),
        CONSTRAINT [CK_AIFeedbackCriterionScores_Score] CHECK ([Score] >= 0),
        CONSTRAINT [FK_AIFeedbackCriterionScores_AIPracticeFeedbacks_AIFeedbackId] FOREIGN KEY ([AIFeedbackId]) REFERENCES [assessment].[AIPracticeFeedbacks] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AIFeedbackCriterionScores_RubricCriteria_RubricCriterionId] FOREIGN KEY ([RubricCriterionId]) REFERENCES [assessment].[RubricCriteria] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094717_AddCriterionScoreTables'
)
BEGIN
    CREATE TABLE [assessment].[HumanReviewCriterionScores] (
        [HumanReviewId] int NOT NULL,
        [RubricCriterionId] int NOT NULL,
        [Score] decimal(9,4) NOT NULL,
        [Comment] nvarchar(max) NULL,
        CONSTRAINT [PK_HumanReviewCriterionScores] PRIMARY KEY ([HumanReviewId], [RubricCriterionId]),
        CONSTRAINT [CK_HumanReviewCriterionScores_Score] CHECK ([Score] >= 0),
        CONSTRAINT [FK_HumanReviewCriterionScores_HumanReviews_HumanReviewId] FOREIGN KEY ([HumanReviewId]) REFERENCES [assessment].[HumanReviews] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_HumanReviewCriterionScores_RubricCriteria_RubricCriterionId] FOREIGN KEY ([RubricCriterionId]) REFERENCES [assessment].[RubricCriteria] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094717_AddCriterionScoreTables'
)
BEGIN
    CREATE INDEX [IX_AIFeedbackCriterionScores_RubricCriterionId] ON [assessment].[AIFeedbackCriterionScores] ([RubricCriterionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094717_AddCriterionScoreTables'
)
BEGIN
    CREATE INDEX [IX_HumanReviewCriterionScores_RubricCriterionId] ON [assessment].[HumanReviewCriterionScores] ([RubricCriterionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [assessment].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023094717_AddCriterionScoreTables'
)
BEGIN
    INSERT INTO [assessment].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023094717_AddCriterionScoreTables', N'9.0.13');
END;

COMMIT;
GO


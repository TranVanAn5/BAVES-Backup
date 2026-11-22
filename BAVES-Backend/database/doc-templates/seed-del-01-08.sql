SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

IF COL_LENGTH('artifact.ArtifactTemplates', 'DeliverableCode') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [DeliverableCode] nvarchar(20) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'Category') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [Category] nvarchar(80) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'DefaultTitle') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [DefaultTitle] nvarchar(300) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'IssueKeyPrefix') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [IssueKeyPrefix] nvarchar(40) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'DisplayOrder') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [DisplayOrder] int NOT NULL CONSTRAINT DF_ArtifactTemplates_DisplayOrder DEFAULT 0;
IF COL_LENGTH('artifact.ArtifactTemplates', 'SkeletonHtmlContent') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [SkeletonHtmlContent] nvarchar(max) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'SampleHtmlContent') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [SampleHtmlContent] nvarchar(max) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'ChecklistJson') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [ChecklistJson] nvarchar(max) NULL;

GO
BEGIN TRANSACTION;

DELETE FROM [artifact].[Submissions] WHERE [ArtifactVersionId] IN (
  SELECT v.[Id] FROM [artifact].[ArtifactVersions] v
  INNER JOIN [artifact].[Artifacts] a ON a.[Id] = v.[ArtifactId]
);
DELETE FROM [artifact].[ArtifactVersions] WHERE [ArtifactId] IN (SELECT [Id] FROM [artifact].[Artifacts]);
DELETE FROM [artifact].[Artifacts];
DELETE FROM [artifact].[ArtifactTemplateSections];
DELETE FROM [artifact].[ArtifactTemplates];

COMMIT TRANSACTION;
SELECT [Id],[Code],[DeliverableCode],[Name], LEN([SkeletonHtmlContent]) AS SkeletonLen, LEN([SampleHtmlContent]) AS SampleLen FROM [artifact].[ArtifactTemplates] ORDER BY [DisplayOrder];
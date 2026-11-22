-- ====================================================
-- BA-VES SPRINT 4: REVIEW REQUESTS TABLE MIGRATION
-- Supports Human Expert Review Workflow
-- ====================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'assessment' AND t.name = N'ReviewRequests')
BEGIN
    CREATE TABLE [assessment].[ReviewRequests] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [SubmissionId] int NOT NULL,
        [LearnerId] int NOT NULL,
        [AssignedReviewerId] int NULL,
        [RequestNote] nvarchar(1000) NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
        [RequestedAt] datetimeoffset(3) NOT NULL,
        [AcceptedAt] datetimeoffset(3) NULL,
        [CompletedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_ReviewRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ReviewRequests_Status] CHECK ([Status] IN (N'Pending', N'Accepted', N'Completed', N'Cancelled'))
    );

    CREATE INDEX [IX_ReviewRequests_SubmissionId] ON [assessment].[ReviewRequests] ([SubmissionId]);
    CREATE INDEX [IX_ReviewRequests_LearnerId] ON [assessment].[ReviewRequests] ([LearnerId]);
    CREATE INDEX [IX_ReviewRequests_AssignedReviewerId] ON [assessment].[ReviewRequests] ([AssignedReviewerId]);
    CREATE INDEX [IX_ReviewRequests_Status] ON [assessment].[ReviewRequests] ([Status]);

    PRINT N'>> [assessment].[ReviewRequests] table created successfully.';
END
ELSE
BEGIN
    PRINT N'>> [assessment].[ReviewRequests] table already exists.';
END

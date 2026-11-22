IF OBJECT_ID(N'[simulation].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'simulation') IS NULL EXEC(N'CREATE SCHEMA [simulation];');
    CREATE TABLE [simulation].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    IF SCHEMA_ID(N'simulation') IS NULL EXEC(N'CREATE SCHEMA [simulation];');
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE TABLE [simulation].[SimulationSessions] (
        [Id] int NOT NULL IDENTITY,
        [ScenarioVersionId] int NOT NULL,
        [AssignmentId] int NULL,
        [LearnerId] int NOT NULL,
        [AttemptNo] int NOT NULL,
        [Status] nvarchar(40) NOT NULL,
        [StartedAt] datetimeoffset(3) NOT NULL,
        [SubmittedAt] datetimeoffset(3) NULL,
        [CompletedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_SimulationSessions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE TABLE [simulation].[SessionAgents] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [PersonaId] int NOT NULL,
        [MemorySummary] nvarchar(max) NULL,
        [CurrentMood] nvarchar(200) NULL,
        [TrustLevel] tinyint NULL,
        [PatienceLevel] tinyint NULL,
        [CooperationLevel] tinyint NULL,
        [IsActive] bit NOT NULL,
        [LastInteractionAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_SessionAgents] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_SessionAgents_CooperationLevel] CHECK ([CooperationLevel] IS NULL OR [CooperationLevel] <= 100),
        CONSTRAINT [CK_SessionAgents_PatienceLevel] CHECK ([PatienceLevel] IS NULL OR [PatienceLevel] <= 100),
        CONSTRAINT [CK_SessionAgents_TrustLevel] CHECK ([TrustLevel] IS NULL OR [TrustLevel] <= 100),
        CONSTRAINT [FK_SessionAgents_SimulationSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [simulation].[SimulationSessions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE TABLE [simulation].[SessionEvents] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [ScenarioEventId] int NOT NULL,
        [TriggerSource] nvarchar(40) NOT NULL,
        [TriggeredAt] datetimeoffset(3) NOT NULL,
        [SequenceNo] int NOT NULL,
        [EventContentSnapshot] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SessionEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionEvents_SimulationSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [simulation].[SimulationSessions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE TABLE [simulation].[Messages] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [SessionAgentId] int NULL,
        [SenderType] nvarchar(40) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_SessionAgents_SessionAgentId] FOREIGN KEY ([SessionAgentId]) REFERENCES [simulation].[SessionAgents] ([Id]),
        CONSTRAINT [FK_Messages_SimulationSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [simulation].[SimulationSessions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE TABLE [simulation].[SessionAgentKnowledgeStates] (
        [Id] int NOT NULL IDENTITY,
        [SessionAgentId] int NOT NULL,
        [KnowledgeItemId] int NOT NULL,
        [IsRevealed] bit NOT NULL,
        [RevealedAt] datetimeoffset(3) NULL,
        [RevealMessageId] int NULL,
        CONSTRAINT [PK_SessionAgentKnowledgeStates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionAgentKnowledgeStates_Messages_RevealMessageId] FOREIGN KEY ([RevealMessageId]) REFERENCES [simulation].[Messages] ([Id]),
        CONSTRAINT [FK_SessionAgentKnowledgeStates_SessionAgents_SessionAgentId] FOREIGN KEY ([SessionAgentId]) REFERENCES [simulation].[SessionAgents] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_Messages_SessionAgentId] ON [simulation].[Messages] ([SessionAgentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_Messages_SessionId_CreatedAt] ON [simulation].[Messages] ([SessionId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SessionAgentKnowledgeStates_KnowledgeItemId] ON [simulation].[SessionAgentKnowledgeStates] ([KnowledgeItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SessionAgentKnowledgeStates_RevealMessageId] ON [simulation].[SessionAgentKnowledgeStates] ([RevealMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionAgentKnowledgeStates_SessionAgentId_KnowledgeItemId] ON [simulation].[SessionAgentKnowledgeStates] ([SessionAgentId], [KnowledgeItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SessionAgents_PersonaId] ON [simulation].[SessionAgents] ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionAgents_SessionId_PersonaId] ON [simulation].[SessionAgents] ([SessionId], [PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SessionEvents_ScenarioEventId] ON [simulation].[SessionEvents] ([ScenarioEventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionEvents_SessionId_SequenceNo] ON [simulation].[SessionEvents] ([SessionId], [SequenceNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SimulationSessions_AssignmentId] ON [simulation].[SimulationSessions] ([AssignmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SimulationSessions_LearnerId] ON [simulation].[SimulationSessions] ([LearnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    CREATE INDEX [IX_SimulationSessions_ScenarioVersionId] ON [simulation].[SimulationSessions] ([ScenarioVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011063732_Initial_Simulation'
)
BEGIN
    INSERT INTO [simulation].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063732_Initial_Simulation', N'9.0.13');
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026051956_AddCoachFeedbacks'
)
BEGIN
    CREATE TABLE [simulation].[CoachFeedbacks] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [StrengthsJson] nvarchar(max) NOT NULL,
        [GapsJson] nvarchar(max) NOT NULL,
        [UnclearPointsJson] nvarchar(max) NOT NULL,
        [ImprovementHintsJson] nvarchar(max) NOT NULL,
        [PromptVersion] nvarchar(100) NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_CoachFeedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachFeedbacks_SimulationSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [simulation].[SimulationSessions] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026051956_AddCoachFeedbacks'
)
BEGIN
    CREATE INDEX [IX_CoachFeedbacks_SessionId_CreatedAt] ON [simulation].[CoachFeedbacks] ([SessionId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261026051956_AddCoachFeedbacks'
)
BEGIN
    INSERT INTO [simulation].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261026051956_AddCoachFeedbacks', N'9.0.13');
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113133000_AddSessionSprintReviews'
)
BEGIN
    CREATE TABLE [simulation].[SessionSprintReviews] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [SprintNo] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [DiscussionJson] nvarchar(max) NOT NULL,
        [ChairSpeakerName] nvarchar(200) NULL,
        [ChairReply] nvarchar(max) NULL,
        [ChallengeQuestionsJson] nvarchar(max) NOT NULL,
        [Decision] nvarchar(40) NULL,
        [SprintScore] decimal(9,4) NULL,
        [CompressedDigest] nvarchar(max) NULL,
        [ApprovedPointsJson] nvarchar(max) NOT NULL,
        [ActionItemsJson] nvarchar(max) NOT NULL,
        [PromptVersion] nvarchar(100) NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL,
        [FinalizedAt] datetimeoffset(3) NULL,
        CONSTRAINT [PK_SessionSprintReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_SessionSprintReviews_SprintNo] CHECK ([SprintNo] > 0),
        CONSTRAINT [CK_SessionSprintReviews_Status] CHECK ([Status] IN (N'Open', N'Finalized')),
        CONSTRAINT [FK_SessionSprintReviews_SimulationSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [simulation].[SimulationSessions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113133000_AddSessionSprintReviews'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionSprintReviews_SessionId_SprintNo] ON [simulation].[SessionSprintReviews] ([SessionId], [SprintNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [simulation].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113133000_AddSessionSprintReviews'
)
BEGIN
    INSERT INTO [simulation].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261113133000_AddSessionSprintReviews', N'9.0.13');
END;

COMMIT;
GO


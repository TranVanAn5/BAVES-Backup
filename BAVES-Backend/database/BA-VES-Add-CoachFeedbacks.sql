-- =============================================================================
-- BA-VES: bảng [simulation].[CoachFeedbacks]
--
-- Lưu nhận xét của AI Coach cho một Simulation Session, phục vụ:
--   POST /api/sessions/{sessionId}/coach-feedback  (sinh + lưu)
--   GET  /api/sessions/{sessionId}/coach-feedback  (đọc bản mới nhất, không gọi LLM)
--
-- Idempotent: chạy lại nhiều lần không lỗi.
-- Tương ứng EF migration: Modules/Simulation/Data/Migrations/*_AddCoachFeedbacks.cs
-- =============================================================================

IF OBJECT_ID(N'[simulation].[CoachFeedbacks]', N'U') IS NULL
BEGIN
    CREATE TABLE [simulation].[CoachFeedbacks] (
        [Id]                  int               NOT NULL IDENTITY,
        [SessionId]           int               NOT NULL,
        [CreatedByUserId]     int               NOT NULL,
        [StrengthsJson]       nvarchar(max)     NOT NULL,
        [GapsJson]            nvarchar(max)     NOT NULL,
        [UnclearPointsJson]   nvarchar(max)     NOT NULL,
        [ImprovementHintsJson] nvarchar(max)    NOT NULL,
        [PromptVersion]       nvarchar(100)     NOT NULL,
        [CreatedAt]           datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_CoachFeedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CoachFeedbacks_SimulationSessions_SessionId] FOREIGN KEY ([SessionId])
            REFERENCES [simulation].[SimulationSessions] ([Id])
    );

    -- GET /coach-feedback luôn lấy bản mới nhất của một session.
    CREATE INDEX [IX_CoachFeedbacks_SessionId_CreatedAt]
        ON [simulation].[CoachFeedbacks] ([SessionId], [CreatedAt]);
END
GO

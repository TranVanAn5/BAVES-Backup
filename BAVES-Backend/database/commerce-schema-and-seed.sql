-- ==============================================================================================
-- BA-VES: COMMERCE & MONETIZATION SCHEMA AND INITIAL PLANS SEED SCRIPT
-- Module: Commerce (PayOS Payment, Order Processing, Entitlements & Credit Ledger)
-- ==============================================================================================

IF SCHEMA_ID(N'commerce') IS NULL 
    EXEC(N'CREATE SCHEMA [commerce];');
GO

-- 1. Table: [commerce].[Plans]
IF OBJECT_ID(N'[commerce].[Plans]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[Plans] (
        [Id]            int               NOT NULL IDENTITY,
        [Code]          nvarchar(50)      NOT NULL,
        [Name]          nvarchar(150)     NOT NULL,
        [Description]   nvarchar(1000)     NULL,
        [PlanType]      nvarchar(40)      NOT NULL, -- Free, ScenarioBundle, Subscription, ReviewCredit
        [Price]         decimal(12,2)     NOT NULL DEFAULT 0,
        [Currency]      nvarchar(10)      NOT NULL DEFAULT N'VND',
        [DurationDays]  int               NULL,
        [SessionQuota]  int               NULL,
        [ReviewCredits] int               NOT NULL DEFAULT 0,
        [ConsultationCredits] int         NOT NULL DEFAULT 0,
        [Status]        nvarchar(40)      NOT NULL DEFAULT N'Draft', -- Draft, Published, Archived
        [DisplayOrder]  int               NOT NULL DEFAULT 0,
        [CreatedAt]     datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_Plans] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Plans_Code] UNIQUE ([Code]),
        CONSTRAINT [CK_Plans_Price] CHECK ([Price] >= 0)
    );
END;
GO

-- 2. Table: [commerce].[PlanScopes]
IF OBJECT_ID(N'[commerce].[PlanScopes]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[PlanScopes] (
        [Id]        int          NOT NULL IDENTITY,
        [PlanId]    int          NOT NULL,
        [ScopeType] nvarchar(40) NOT NULL, -- Industry, Application, Scenario, All
        [ScopeId]   int          NULL,
        CONSTRAINT [PK_PlanScopes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlanScopes_Plans] FOREIGN KEY ([PlanId])
            REFERENCES [commerce].[Plans] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_PlanScopes_PlanId_Scope] ON [commerce].[PlanScopes] ([PlanId], [ScopeType], [ScopeId]);
END;
GO

-- 3. Table: [commerce].[Orders]
IF OBJECT_ID(N'[commerce].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[Orders] (
        [Id]               int               NOT NULL IDENTITY,
        [OrderCode]        bigint            NOT NULL, -- Unique numerical order code for PayOS
        [UserId]           int               NOT NULL,
        [PlanId]           int               NOT NULL,
        [Amount]           decimal(12,2)     NOT NULL,
        [Currency]         nvarchar(10)      NOT NULL DEFAULT N'VND',
        [PlanSnapshotJson] nvarchar(max)     NOT NULL DEFAULT N'{}',
        [Status]           nvarchar(40)      NOT NULL DEFAULT N'Pending', -- Pending, Paid, Cancelled, Expired, Failed, Refunded
        [IdempotencyKey]   nvarchar(100)     NULL,
        [CheckoutUrl]      nvarchar(500)     NULL,
        [QrCode]           nvarchar(2000)    NULL,
        [CreatedAt]        datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [ExpiresAt]        datetimeoffset(3) NOT NULL,
        [PaidAt]           datetimeoffset(3) NULL,
        [CancelledAt]      datetimeoffset(3) NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_Orders_OrderCode] UNIQUE ([OrderCode]),
        CONSTRAINT [FK_Orders_Plans] FOREIGN KEY ([PlanId])
            REFERENCES [commerce].[Plans] ([Id]),
        CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId])
            REFERENCES [identity].[Users] ([Id])
    );
    CREATE INDEX [IX_Orders_UserId] ON [commerce].[Orders] ([UserId]);
    CREATE INDEX [IX_Orders_IdempotencyKey] ON [commerce].[Orders] ([IdempotencyKey]);
END;
GO

-- 4. Table: [commerce].[PaymentTransactions]
IF OBJECT_ID(N'[commerce].[PaymentTransactions]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[PaymentTransactions] (
        [Id]                    int               NOT NULL IDENTITY,
        [OrderId]               int               NOT NULL,
        [Provider]              nvarchar(50)      NOT NULL DEFAULT N'PayOS',
        [ProviderTransactionId] nvarchar(100)     NOT NULL,
        [Amount]                decimal(12,2)     NOT NULL,
        [Status]                nvarchar(40)      NOT NULL DEFAULT N'Pending',
        [SignatureValid]        bit               NOT NULL DEFAULT 0,
        [RawPayloadJson]        nvarchar(max)     NULL,
        [ReceivedAt]            datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_PaymentTransactions_Provider_TxId] UNIQUE ([Provider], [ProviderTransactionId]),
        CONSTRAINT [FK_PaymentTransactions_Orders] FOREIGN KEY ([OrderId])
            REFERENCES [commerce].[Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

-- 5. Table: [commerce].[PaymentWebhookEvents]
IF OBJECT_ID(N'[commerce].[PaymentWebhookEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[PaymentWebhookEvents] (
        [Id]          int               NOT NULL IDENTITY,
        [Provider]    nvarchar(50)      NOT NULL DEFAULT N'PayOS',
        [EventId]     nvarchar(150)     NOT NULL,
        [PayloadHash] nvarchar(100)     NOT NULL,
        [ReceivedAt]  datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [ProcessedAt] datetimeoffset(3) NULL,
        [Result]      nvarchar(50)      NOT NULL DEFAULT N'Pending',
        CONSTRAINT [PK_PaymentWebhookEvents] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_PaymentWebhookEvents_Provider_EventId] ON [commerce].[PaymentWebhookEvents] ([Provider], [EventId]);
    CREATE INDEX [IX_PaymentWebhookEvents_PayloadHash] ON [commerce].[PaymentWebhookEvents] ([PayloadHash]);
END;
GO

-- 6. Table: [commerce].[Entitlements]
IF OBJECT_ID(N'[commerce].[Entitlements]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[Entitlements] (
        [Id]            int               NOT NULL IDENTITY,
        [UserId]        int               NOT NULL,
        [SourceType]    nvarchar(40)      NOT NULL DEFAULT N'Purchase', -- Free, Purchase, Subscription, CohortAssignment, Admin
        [SourceOrderId] int               NULL,
        [ScopeType]     nvarchar(40)      NOT NULL, -- Industry, Application, Scenario, All
        [ScopeId]       int               NULL,
        [StartsAt]      datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [EndsAt]        datetimeoffset(3) NULL,
        [SessionQuota]  int               NULL,
        [SessionsUsed]  int               NOT NULL DEFAULT 0,
        [Status]        nvarchar(40)      NOT NULL DEFAULT N'Active', -- Active, Expired, Revoked
        [CreatedAt]     datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_Entitlements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Entitlements_Users] FOREIGN KEY ([UserId])
            REFERENCES [identity].[Users] ([Id]),
        CONSTRAINT [FK_Entitlements_Orders] FOREIGN KEY ([SourceOrderId])
            REFERENCES [commerce].[Orders] ([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Entitlements_UserId_Status] ON [commerce].[Entitlements] ([UserId], [Status]);
    CREATE INDEX [IX_Entitlements_Scope] ON [commerce].[Entitlements] ([ScopeType], [ScopeId]);
END;
GO

-- 7. Table: [commerce].[CreditLedger]
IF OBJECT_ID(N'[commerce].[CreditLedger]', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[CreditLedger] (
        [Id]              int               NOT NULL IDENTITY,
        [UserId]          int               NOT NULL,
        [Delta]           int               NOT NULL,
        [CreditType]      nvarchar(20)      NOT NULL DEFAULT N'Review', -- Review, Consultation
        [Reason]          nvarchar(40)      NOT NULL, -- Purchase, ReviewRequest, Refund, AdminAdjustment, BookingRequest, BookingRefund
        [OrderId]         int               NULL,
        [ReviewRequestId] int               NULL,
        [BookingId]       int               NULL,
        [Note]            nvarchar(250)     NULL,
        [CreatedAt]       datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_CreditLedger] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CreditLedger_Users] FOREIGN KEY ([UserId])
            REFERENCES [identity].[Users] ([Id])
    );
    CREATE INDEX [IX_CreditLedger_UserId] ON [commerce].[CreditLedger] ([UserId]);
    CREATE INDEX [IX_CreditLedger_Type] ON [commerce].[CreditLedger] ([UserId], [CreditType]);
END;
GO

-- ==============================================================================================
-- 8. Table: [assessment].[ExpertBookings] (Learner Booking 1-on-1 Consultation with Expert)
-- ==============================================================================================
IF OBJECT_ID(N'[assessment].[ExpertBookings]', N'U') IS NULL
BEGIN
    CREATE TABLE [assessment].[ExpertBookings] (
        [Id]                 int               NOT NULL IDENTITY,
        [ReviewRequestId]    int               NOT NULL,
        [SubmissionId]       int               NOT NULL,
        [LearnerId]          int               NOT NULL,
        [ExpertId]           int               NOT NULL,
        [Status]             nvarchar(20)      NOT NULL DEFAULT N'Pending', -- Pending, Confirmed, InProgress, Completed, Cancelled
        [ScheduledStartTime] datetimeoffset(3) NULL,
        [ScheduledEndTime]   datetimeoffset(3) NULL,
        [ActualStartTime]    datetimeoffset(3) NULL,
        [ActualEndTime]      datetimeoffset(3) NULL,
        [LearnerNote]        nvarchar(1000)    NULL,
        [ExpertSummaryNote]  nvarchar(2000)    NULL,
        [CreatedAt]          datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT [PK_ExpertBookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExpertBookings_ReviewRequests] FOREIGN KEY ([ReviewRequestId])
            REFERENCES [assessment].[ReviewRequests] ([Id]),
        CONSTRAINT [CK_ExpertBookings_Status] CHECK ([Status] IN (N'Pending', N'Confirmed', N'InProgress', N'Completed', N'Cancelled'))
    );
    CREATE INDEX [IX_ExpertBookings_LearnerId] ON [assessment].[ExpertBookings] ([LearnerId]);
    CREATE INDEX [IX_ExpertBookings_ExpertId] ON [assessment].[ExpertBookings] ([ExpertId]);
    CREATE INDEX [IX_ExpertBookings_ReviewRequestId] ON [assessment].[ExpertBookings] ([ReviewRequestId]);
END;
GO

-- ==============================================================================================
-- 9. Table: [assessment].[ConsultationMessages] (1-on-1 Chat Window Messages)
-- ==============================================================================================
IF OBJECT_ID(N'[assessment].[ConsultationMessages]', N'U') IS NULL
BEGIN
    CREATE TABLE [assessment].[ConsultationMessages] (
        [Id]         bigint            NOT NULL IDENTITY,
        [BookingId]  int               NOT NULL,
        [SenderId]   int               NOT NULL,
        [SenderRole] nvarchar(20)      NOT NULL, -- Learner, Expert
        [Content]    nvarchar(max)     NOT NULL,
        [SentAt]     datetimeoffset(3) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [IsRead]     bit               NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ConsultationMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConsultationMessages_Bookings] FOREIGN KEY ([BookingId])
            REFERENCES [assessment].[ExpertBookings] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ConsultationMessages_BookingId_SentAt] ON [assessment].[ConsultationMessages] ([BookingId], [SentAt]);
END;
GO

-- ==============================================================================================
-- SEED DATA: Learner packs v2 — chạy thêm seed-learner-plans-v2.sql trên DB đã có dữ liệu cũ
-- ==============================================================================================
PRINT N'>> Commerce schema ready. Apply seed-learner-plans-v2.sql for Learner packs 1–4 + Expert add-ons.';

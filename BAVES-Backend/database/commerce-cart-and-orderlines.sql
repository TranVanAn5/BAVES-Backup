-- Cart + OrderLines for multi-item checkout (one PayOS payment)
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'commerce.Carts', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[Carts] (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Carts] PRIMARY KEY,
        [UserId] int NOT NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL CONSTRAINT [DF_Carts_CreatedAt] DEFAULT (SYSDATETIMEOFFSET()),
        [UpdatedAt] datetimeoffset(3) NOT NULL CONSTRAINT [DF_Carts_UpdatedAt] DEFAULT (SYSDATETIMEOFFSET())
    );
    CREATE UNIQUE INDEX [IX_Carts_UserId] ON [commerce].[Carts]([UserId]);
END
GO

IF OBJECT_ID(N'commerce.CartItems', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[CartItems] (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CartItems] PRIMARY KEY,
        [CartId] int NOT NULL,
        [PlanId] int NOT NULL,
        [TargetScopeId] int NULL,
        [TargetLabel] nvarchar(300) NULL,
        [CreatedAt] datetimeoffset(3) NOT NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (SYSDATETIMEOFFSET()),
        CONSTRAINT [FK_CartItems_Carts] FOREIGN KEY ([CartId]) REFERENCES [commerce].[Carts]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Plans] FOREIGN KEY ([PlanId]) REFERENCES [commerce].[Plans]([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE INDEX [IX_CartItems_Cart_Plan_Scope]
        ON [commerce].[CartItems]([CartId], [PlanId], [TargetScopeId])
        WHERE [TargetScopeId] IS NOT NULL;
    CREATE UNIQUE INDEX [IX_CartItems_Cart_Plan_NullScope]
        ON [commerce].[CartItems]([CartId], [PlanId])
        WHERE [TargetScopeId] IS NULL;
END
GO

IF OBJECT_ID(N'commerce.OrderLines', N'U') IS NULL
BEGIN
    CREATE TABLE [commerce].[OrderLines] (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_OrderLines] PRIMARY KEY,
        [OrderId] int NOT NULL,
        [PlanId] int NOT NULL,
        [TargetScopeId] int NULL,
        [TargetLabel] nvarchar(300) NULL,
        [UnitPrice] decimal(12,2) NOT NULL,
        [Currency] nvarchar(10) NOT NULL CONSTRAINT [DF_OrderLines_Currency] DEFAULT (N'VND'),
        [PlanCode] nvarchar(80) NOT NULL,
        [PlanName] nvarchar(200) NOT NULL,
        [LineSnapshotJson] nvarchar(max) NOT NULL CONSTRAINT [DF_OrderLines_Snapshot] DEFAULT (N'{}'),
        [DisplayOrder] int NOT NULL CONSTRAINT [DF_OrderLines_DisplayOrder] DEFAULT (0),
        CONSTRAINT [FK_OrderLines_Orders] FOREIGN KEY ([OrderId]) REFERENCES [commerce].[Orders]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderLines_Plans] FOREIGN KEY ([PlanId]) REFERENCES [commerce].[Plans]([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_OrderLines_OrderId] ON [commerce].[OrderLines]([OrderId]);
END
GO

PRINT N'>> commerce.Carts / CartItems / OrderLines ready';
GO

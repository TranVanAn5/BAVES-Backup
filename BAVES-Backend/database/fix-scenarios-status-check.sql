-- Cho phép Status PendingReview / Rejected trên scenario.Scenarios
-- (SubmitForReview / Reject ghi status lên cả bảng Scenarios)
SET NOCOUNT ON;

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = N'CK_Scenarios_Status'
      AND parent_object_id = OBJECT_ID(N'scenario.Scenarios')
)
BEGIN
    ALTER TABLE [scenario].[Scenarios] DROP CONSTRAINT [CK_Scenarios_Status];
END
GO

IF COL_LENGTH(N'scenario.Scenarios', N'Status') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_Scenarios_Status'
          AND parent_object_id = OBJECT_ID(N'scenario.Scenarios')
   )
BEGIN
    ALTER TABLE [scenario].[Scenarios] WITH CHECK
    ADD CONSTRAINT [CK_Scenarios_Status]
    CHECK ([Status] IN (N'Draft', N'Published', N'Archived', N'PendingReview', N'Rejected'));
END
GO

PRINT N'>> CK_Scenarios_Status updated for review workflow.';
GO

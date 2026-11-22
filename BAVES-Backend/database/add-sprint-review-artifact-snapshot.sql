IF COL_LENGTH('simulation.SessionSprintReviews', 'ArtifactSnapshotJson') IS NULL
BEGIN
    ALTER TABLE [simulation].[SessionSprintReviews]
    ADD [ArtifactSnapshotJson] nvarchar(max) NULL;
END
GO

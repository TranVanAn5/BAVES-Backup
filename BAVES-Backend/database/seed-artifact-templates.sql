-- ====================================================
-- BA-VES: Document templates DEL-01 .. DEL-08
-- Full HTML scaffolds + samples live in the database.
-- Source of truth for seed generation:
--   database/tools/export-del-templates.mjs
--   database/doc-templates/seed-del-01-08.sql
-- ====================================================
-- Run:
--   sqlcmd -S <server> -U sa -P <pwd> -d BAVES -i database/doc-templates/seed-del-01-08.sql
-- ====================================================

PRINT N'Use database/doc-templates/seed-del-01-08.sql to replace the artifact template catalog with DEL-01..DEL-08.';
GO

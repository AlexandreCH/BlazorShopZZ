-- ==========================================
-- MARK DATABASE AS MIGRATED
-- ==========================================
-- This script will create the migrations table and mark the initial migration as applied

\c blazorshop

-- Create migrations history table if it doesn't exist
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert the initial migration record (prevents EF from trying to recreate tables)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251204160649_InitialCreate', '9.0.8')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Verify
SELECT * FROM "__EFMigrationsHistory";

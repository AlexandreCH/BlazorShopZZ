-- ==========================================
-- RESET BLAZORSHOP DATABASE
-- ==========================================
-- This script will drop and recreate the database

-- Connect to postgres database first
\c postgres

-- Terminate all connections to blazorshop
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = 'blazorshop'
  AND pid <> pg_backend_pid();

-- Drop the database
DROP DATABASE IF EXISTS blazorshop;

-- Recreate the database
CREATE DATABASE blazorshop;

-- Connect to the new database
\c blazorshop

-- Verify it's empty
SELECT 'Database blazorshop has been reset!' as status;

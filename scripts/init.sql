-- School Management System — PostgreSQL initialization
-- Enables UUID generation and prepares the public schema for the tenant registry.

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Public schema is the default; ensure it exists
CREATE SCHEMA IF NOT EXISTS public;

-- Helpful comment for operators
COMMENT ON SCHEMA public IS 'Master schema — tenant registry and super admins (EF Core migrations manage tables)';

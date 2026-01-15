-- Make username and email case-insensitive for uniqueness and lookups
-- Drop existing unique constraints and indexes, then create case-insensitive unique indexes

-- Drop the existing indexes first (they were created explicitly in the original migration)
DROP INDEX IF EXISTS idx_auth_user_email;
DROP INDEX IF EXISTS idx_auth_user_username;

-- Remove the UNIQUE constraints from the columns
-- PostgreSQL automatically creates constraints with names like {table}_{column}_key for UNIQUE columns
ALTER TABLE auth_user DROP CONSTRAINT IF EXISTS auth_user_username_key;
ALTER TABLE auth_user DROP CONSTRAINT IF EXISTS auth_user_email_key;

-- Create case-insensitive unique indexes (these enforce uniqueness case-insensitively)
CREATE UNIQUE INDEX IF NOT EXISTS idx_auth_user_username_lower ON auth_user(LOWER(username));
CREATE UNIQUE INDEX IF NOT EXISTS idx_auth_user_email_lower ON auth_user(LOWER(email));

-- Recreate the regular indexes for performance (non-unique, for general queries)
CREATE INDEX IF NOT EXISTS idx_auth_user_username ON auth_user(username);
CREATE INDEX IF NOT EXISTS idx_auth_user_email ON auth_user(email);

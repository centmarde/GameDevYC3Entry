-- ============================================
-- MIGRATION: Switch from player_id to player_name as Primary Key
-- ============================================
-- Run this in Supabase SQL Editor if you already have data
-- This will update your existing wave_leaderboards table
-- ============================================

-- OPTION 1: If you have EXISTING data you want to keep
-- ============================================

-- Step 1: Remove the UNIQUE constraint from player_id
ALTER TABLE wave_leaderboards DROP CONSTRAINT IF EXISTS wave_leaderboards_player_id_key;

-- Step 2: Add UNIQUE constraint to player_name (if not already unique)
ALTER TABLE wave_leaderboards ADD CONSTRAINT wave_leaderboards_player_name_key UNIQUE (player_name);

-- Step 3: Update the index to use player_name
DROP INDEX IF EXISTS idx_player_id;
CREATE INDEX IF NOT EXISTS idx_player_name ON wave_leaderboards(player_name);

-- Step 4: Handle any duplicate player names (keeps highest wave record)
-- WARNING: This will delete lower-scoring duplicates!
DELETE FROM wave_leaderboards
WHERE id NOT IN (
    SELECT DISTINCT ON (player_name) id
    FROM wave_leaderboards
    ORDER BY player_name, highest_wave DESC, updated_at DESC
);

-- Step 5: Update RLS policies to work with player_name
DROP POLICY IF EXISTS "Allow public update" ON wave_leaderboards;
CREATE POLICY "Allow public update" 
ON wave_leaderboards 
FOR UPDATE 
USING (true);

-- ============================================
-- OPTION 2: If you want to START FRESH (deletes all data!)
-- ============================================
-- Uncomment the lines below ONLY if you want to delete everything

-- DROP TABLE IF EXISTS wave_leaderboards CASCADE;
-- 
-- Then run the main SupabaseSchema.sql file to recreate the table

-- ============================================
-- Verification Queries
-- ============================================

-- Check for duplicate player names
SELECT player_name, COUNT(*) as count
FROM wave_leaderboards
GROUP BY player_name
HAVING COUNT(*) > 1;

-- Verify unique constraint exists
SELECT constraint_name, constraint_type
FROM information_schema.table_constraints
WHERE table_name = 'wave_leaderboards'
AND constraint_type = 'UNIQUE';

-- View all records
SELECT player_name, current_wave, highest_wave, total_kills, updated_at
FROM wave_leaderboards
ORDER BY highest_wave DESC
LIMIT 20;

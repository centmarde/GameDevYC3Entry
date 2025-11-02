-- ============================================
-- Supabase Wave Leaderboard Schema
-- ============================================
-- Run this SQL in your Supabase SQL Editor
-- Dashboard > SQL Editor > New Query > Paste > Run
-- ============================================

-- Drop existing table if you want to start fresh (CAUTION: deletes all data!)
-- DROP TABLE IF EXISTS wave_leaderboards CASCADE;

-- Create wave_leaderboards table
CREATE TABLE IF NOT EXISTS wave_leaderboards (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    
    -- Player Identity (player_name is now the unique key)
    player_name TEXT NOT NULL UNIQUE,
    player_id TEXT,
    device_id TEXT,
    
    -- Wave Progress
    current_wave INTEGER DEFAULT 0 CHECK (current_wave >= 0),
    highest_wave INTEGER DEFAULT 0 CHECK (highest_wave >= 0),
    
    -- Player Stats
    total_kills INTEGER DEFAULT 0 CHECK (total_kills >= 0),
    total_play_time INTEGER DEFAULT 0 CHECK (total_play_time >= 0),
    waves_completed INTEGER DEFAULT 0 CHECK (waves_completed >= 0),
    death_count INTEGER DEFAULT 0 CHECK (death_count >= 0),
    fastest_wave_time REAL DEFAULT 0 CHECK (fastest_wave_time >= 0),
    
    -- Metadata
    game_version TEXT DEFAULT '1.0.0',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Constraints
    CONSTRAINT highest_wave_greater_or_equal CHECK (highest_wave >= current_wave)
);

-- ============================================
-- Indexes for Performance
-- ============================================

-- Index for leaderboard queries (order by highest wave)
CREATE INDEX IF NOT EXISTS idx_highest_wave 
ON wave_leaderboards(highest_wave DESC);

-- Index for current wave
CREATE INDEX IF NOT EXISTS idx_current_wave 
ON wave_leaderboards(current_wave DESC);

-- Index for player lookup
CREATE INDEX IF NOT EXISTS idx_player_id 
ON wave_leaderboards(player_id);

-- Index for player name search
CREATE INDEX IF NOT EXISTS idx_player_name 
ON wave_leaderboards(player_name);

-- Index for recent updates
CREATE INDEX IF NOT EXISTS idx_updated_at 
ON wave_leaderboards(updated_at DESC);

-- Composite index for leaderboard queries
CREATE INDEX IF NOT EXISTS idx_leaderboard_composite 
ON wave_leaderboards(highest_wave DESC, current_wave DESC, total_kills DESC);

-- ============================================
-- Row Level Security (RLS) Policies
-- ============================================

-- Enable RLS
ALTER TABLE wave_leaderboards ENABLE ROW LEVEL SECURITY;

-- Policy 1: Allow everyone to read leaderboard data
CREATE POLICY "Allow public read access" 
ON wave_leaderboards 
FOR SELECT 
USING (true);

-- Policy 2: Allow anyone to insert new records
CREATE POLICY "Allow public insert" 
ON wave_leaderboards 
FOR INSERT 
WITH CHECK (true);

-- Policy 3: Allow anyone to update any record (Unity client handles validation)
-- For production, you should add authentication and restrict updates
CREATE POLICY "Allow public update" 
ON wave_leaderboards 
FOR UPDATE 
USING (true);

-- Policy 4: Optional - Prevent score manipulation
-- Uncomment this to prevent highest_wave from decreasing
-- CREATE POLICY "Prevent score decrease" 
-- ON wave_leaderboards 
-- FOR UPDATE 
-- USING (highest_wave >= (SELECT highest_wave FROM wave_leaderboards WHERE id = wave_leaderboards.id));

-- ============================================
-- Automatic Timestamp Updates
-- ============================================

-- Function to automatically update updated_at column
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to call the function before any update
DROP TRIGGER IF EXISTS update_wave_leaderboards_updated_at ON wave_leaderboards;
CREATE TRIGGER update_wave_leaderboards_updated_at 
BEFORE UPDATE ON wave_leaderboards 
FOR EACH ROW 
EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- Helper Functions
-- ============================================

-- Function to get player's rank
CREATE OR REPLACE FUNCTION get_player_rank(p_player_id TEXT)
RETURNS INTEGER AS $$
DECLARE
    player_rank INTEGER;
BEGIN
    SELECT COUNT(*) + 1 INTO player_rank
    FROM wave_leaderboards
    WHERE highest_wave > (
        SELECT COALESCE(highest_wave, 0) 
        FROM wave_leaderboards 
        WHERE player_id = p_player_id
        LIMIT 1
    );
    RETURN COALESCE(player_rank, 0);
END;
$$ LANGUAGE plpgsql;

-- Function to get top N players
CREATE OR REPLACE FUNCTION get_top_players(limit_count INTEGER DEFAULT 100)
RETURNS TABLE (
    rank INTEGER,
    player_name TEXT,
    highest_wave INTEGER,
    current_wave INTEGER,
    total_kills INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ROW_NUMBER() OVER (ORDER BY w.highest_wave DESC, w.current_wave DESC, w.total_kills DESC)::INTEGER as rank,
        w.player_name,
        w.highest_wave,
        w.current_wave,
        w.total_kills
    FROM wave_leaderboards w
    ORDER BY w.highest_wave DESC, w.current_wave DESC, w.total_kills DESC
    LIMIT limit_count;
END;
$$ LANGUAGE plpgsql;

-- Function to get leaderboard with player ranks
CREATE OR REPLACE FUNCTION get_leaderboard_with_ranks(limit_count INTEGER DEFAULT 100)
RETURNS TABLE (
    rank INTEGER,
    id UUID,
    player_name TEXT,
    player_id TEXT,
    current_wave INTEGER,
    highest_wave INTEGER,
    total_kills INTEGER,
    total_play_time INTEGER,
    waves_completed INTEGER,
    death_count INTEGER,
    fastest_wave_time REAL,
    updated_at TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ROW_NUMBER() OVER (ORDER BY w.highest_wave DESC, w.current_wave DESC, w.total_kills DESC)::INTEGER as rank,
        w.id,
        w.player_name,
        w.player_id,
        w.current_wave,
        w.highest_wave,
        w.total_kills,
        w.total_play_time,
        w.waves_completed,
        w.death_count,
        w.fastest_wave_time,
        w.updated_at
    FROM wave_leaderboards w
    ORDER BY w.highest_wave DESC, w.current_wave DESC, w.total_kills DESC
    LIMIT limit_count;
END;
$$ LANGUAGE plpgsql;

-- ============================================
-- Sample Data (Optional - for testing)
-- ============================================

-- Uncomment to insert test data
/*
INSERT INTO wave_leaderboards (player_name, player_id, current_wave, highest_wave, total_kills, device_id, game_version)
VALUES 
    ('TestPlayer1', 'test_001', 15, 20, 500, 'device_001', '1.0.0'),
    ('TestPlayer2', 'test_002', 10, 18, 450, 'device_002', '1.0.0'),
    ('TestPlayer3', 'test_003', 25, 25, 800, 'device_003', '1.0.0'),
    ('TestPlayer4', 'test_004', 5, 12, 200, 'device_004', '1.0.0')
ON CONFLICT (player_id) DO NOTHING;
*/

-- ============================================
-- Useful Queries
-- ============================================

-- Get top 10 players
-- SELECT * FROM get_top_players(10);

-- Get player rank
-- SELECT get_player_rank('your_player_id');

-- Get leaderboard with ranks
-- SELECT * FROM get_leaderboard_with_ranks(50);

-- Get player by ID
-- SELECT * FROM wave_leaderboards WHERE player_id = 'your_player_id';

-- Get recent activity (last 24 hours)
-- SELECT * FROM wave_leaderboards 
-- WHERE updated_at > NOW() - INTERVAL '24 hours'
-- ORDER BY updated_at DESC;

-- Get players by wave range
-- SELECT * FROM wave_leaderboards 
-- WHERE highest_wave BETWEEN 10 AND 20
-- ORDER BY highest_wave DESC;

-- Count total players
-- SELECT COUNT(*) as total_players FROM wave_leaderboards;

-- Get average highest wave
-- SELECT AVG(highest_wave)::INTEGER as avg_wave FROM wave_leaderboards;

-- ============================================
-- Maintenance Queries
-- ============================================

-- Delete old/inactive records (older than 90 days)
-- DELETE FROM wave_leaderboards 
-- WHERE updated_at < NOW() - INTERVAL '90 days';

-- Reset all data (CAUTION!)
-- DELETE FROM wave_leaderboards;

-- ============================================
-- Setup Complete!
-- ============================================
-- Your Supabase leaderboard is ready to use!
-- 
-- Next steps:
-- 1. Copy your Project URL from Settings > API
-- 2. Copy your anon key from Settings > API  
-- 3. Configure SupabaseConfig asset in Unity
-- 4. Test the connection!
-- ============================================

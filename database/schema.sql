-- Turkish Word Classification System Database Schema
-- SQLite Database: words.db
-- Created: 2026-01-02

-- ============================================
-- TABLE: Words
-- Stores classified Turkish words with their
-- morphological analysis results
-- ============================================
CREATE TABLE IF NOT EXISTS Words (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Text TEXT NOT NULL UNIQUE,
    Root TEXT,
    POS TEXT NOT NULL,  -- NOUN, VERB, ADJ, ADV, PRON, CONJ, ADP, DET, NUM
    IsValid INTEGER DEFAULT 1,  -- 0 = invalid, 1 = valid
    Length INTEGER,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for faster queries
CREATE INDEX IF NOT EXISTS idx_words_pos ON Words(POS);
CREATE INDEX IF NOT EXISTS idx_words_text ON Words(Text);

-- ============================================
-- TABLE: AnalysisHistory
-- Stores full analysis history with JSON results
-- for debugging and review purposes
-- ============================================
CREATE TABLE IF NOT EXISTS AnalysisHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InputText TEXT,
    ResultJson TEXT,  -- Full JSON response from Stanza
    AnalyzedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- TABLE: POSStatistics
-- Stores count statistics for each POS category
-- ============================================
CREATE TABLE IF NOT EXISTS POSStatistics (
    POS TEXT PRIMARY KEY,
    Count INTEGER DEFAULT 0,
    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- Initial Data: POS Categories
-- ============================================
INSERT OR IGNORE INTO POSStatistics (POS, Count) VALUES 
    ('NOUN', 0),
    ('VERB', 0),
    ('ADJ', 0),
    ('ADV', 0),
    ('PRON', 0),
    ('CONJ', 0),
    ('ADP', 0),
    ('DET', 0),
    ('NUM', 0);

-- ============================================
-- TRIGGER: Auto-update POSStatistics on INSERT
-- ============================================
CREATE TRIGGER IF NOT EXISTS update_pos_stats_insert
AFTER INSERT ON Words
BEGIN
    UPDATE POSStatistics 
    SET Count = Count + 1,
        LastUpdated = CURRENT_TIMESTAMP
    WHERE POS = NEW.POS;
END;

-- ============================================
-- TRIGGER: Auto-update POSStatistics on DELETE
-- ============================================
CREATE TRIGGER IF NOT EXISTS update_pos_stats_delete
AFTER DELETE ON Words
BEGIN
    UPDATE POSStatistics 
    SET Count = Count - 1,
        LastUpdated = CURRENT_TIMESTAMP
    WHERE POS = OLD.POS;
END;

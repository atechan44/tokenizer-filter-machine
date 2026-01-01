using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using TurkishNLP.Desktop.Models;

namespace TurkishNLP.Desktop.Services
{
    /// <summary>
    /// Singleton database service for Turkish word classification system.
    /// Provides thread-safe access to SQLite database operations.
    /// </summary>
    public sealed class DatabaseService : IDisposable
    {
        #region Singleton Implementation

        private static DatabaseService? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of DatabaseService.
        /// Thread-safe implementation using double-check locking.
        /// </summary>
        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new DatabaseService();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private readonly string _connectionString;
        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Private constructor for singleton pattern.
        /// </summary>
        private DatabaseService()
        {
            _connectionString = "Data Source=words.db";
            InitializeDatabase();
        }

        /// <summary>
        /// Constructor with custom connection string (for testing).
        /// </summary>
        /// <param name="connectionString">Custom connection string.</param>
        private DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }

        #endregion

        #region Database Initialization

        /// <summary>
        /// Initializes the database schema if not exists.
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Words (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Text TEXT NOT NULL UNIQUE,
                        Root TEXT,
                        POS TEXT NOT NULL,
                        IsValid INTEGER DEFAULT 1,
                        Length INTEGER,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE INDEX IF NOT EXISTS idx_words_pos ON Words(POS);
                    CREATE INDEX IF NOT EXISTS idx_words_text ON Words(Text);

                    CREATE TABLE IF NOT EXISTS AnalysisHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InputText TEXT,
                        ResultJson TEXT,
                        AnalyzedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS POSStatistics (
                        POS TEXT PRIMARY KEY,
                        Count INTEGER DEFAULT 0,
                        LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    INSERT OR IGNORE INTO POSStatistics (POS, Count) VALUES 
                        ('NOUN', 0), ('VERB', 0), ('ADJ', 0), ('ADV', 0),
                        ('PRON', 0), ('CONJ', 0), ('ADP', 0), ('DET', 0), ('NUM', 0);
                ";
                command.ExecuteNonQuery();

                Console.WriteLine("[DatabaseService] Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error initializing database: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new SQLite connection.
        /// </summary>
        private SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        #endregion

        #region Data Access Methods

        /// <summary>
        /// Gets all words from the database.
        /// </summary>
        /// <returns>List of WordRoot objects.</returns>
        public List<WordRoot> GetAllWords()
        {
            var words = new List<WordRoot>();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Text, Root, POS, IsValid, CreatedAt FROM Words ORDER BY CreatedAt DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var word = CreateWordRootFromReader(reader);
                    if (word != null)
                    {
                        words.Add(word);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error getting all words: {ex.Message}");
            }

            return words;
        }

        /// <summary>
        /// Gets words filtered by POS type.
        /// </summary>
        /// <param name="pos">The POS type to filter by.</param>
        /// <returns>List of WordRoot objects matching the POS.</returns>
        public List<WordRoot> GetWordsByPOS(string pos)
        {
            var words = new List<WordRoot>();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Text, Root, POS, IsValid, CreatedAt FROM Words WHERE POS = @pos ORDER BY Text";
                command.Parameters.AddWithValue("@pos", pos.ToUpperInvariant());

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var word = CreateWordRootFromReader(reader);
                    if (word != null)
                    {
                        words.Add(word);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error getting words by POS: {ex.Message}");
            }

            return words;
        }

        /// <summary>
        /// Adds a single word to the database and updates POSStatistics.
        /// </summary>
        /// <param name="word">The word to add.</param>
        public void AddWord(WordRoot word)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Insert the word
                    var insertCommand = connection.CreateCommand();
                    insertCommand.Transaction = transaction;
                    insertCommand.CommandText = @"
                        INSERT OR IGNORE INTO Words (Text, Root, POS, IsValid, Length, CreatedAt)
                        VALUES (@text, @root, @pos, @isValid, @length, @createdAt)";
                    
                    insertCommand.Parameters.AddWithValue("@text", word.Text);
                    insertCommand.Parameters.AddWithValue("@root", word.Root ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@pos", word.POS);
                    insertCommand.Parameters.AddWithValue("@isValid", word.IsValid ? 1 : 0);
                    insertCommand.Parameters.AddWithValue("@length", word.Text.Length);
                    insertCommand.Parameters.AddWithValue("@createdAt", word.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                    var rowsAffected = insertCommand.ExecuteNonQuery();

                    // Update POSStatistics if word was inserted
                    if (rowsAffected > 0)
                    {
                        var updateStatsCommand = connection.CreateCommand();
                        updateStatsCommand.Transaction = transaction;
                        updateStatsCommand.CommandText = @"
                            UPDATE POSStatistics 
                            SET Count = Count + 1, LastUpdated = CURRENT_TIMESTAMP 
                            WHERE POS = @pos";
                        updateStatsCommand.Parameters.AddWithValue("@pos", word.POS);
                        updateStatsCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error adding word: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds multiple words in a single transaction.
        /// </summary>
        /// <param name="words">List of words to add.</param>
        public void AddWords(List<WordRoot> words)
        {
            if (words == null || words.Count == 0) return;

            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    var posCountUpdates = new Dictionary<string, int>();

                    foreach (var word in words)
                    {
                        var insertCommand = connection.CreateCommand();
                        insertCommand.Transaction = transaction;
                        insertCommand.CommandText = @"
                            INSERT OR IGNORE INTO Words (Text, Root, POS, IsValid, Length, CreatedAt)
                            VALUES (@text, @root, @pos, @isValid, @length, @createdAt)";
                        
                        insertCommand.Parameters.AddWithValue("@text", word.Text);
                        insertCommand.Parameters.AddWithValue("@root", word.Root ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@pos", word.POS);
                        insertCommand.Parameters.AddWithValue("@isValid", word.IsValid ? 1 : 0);
                        insertCommand.Parameters.AddWithValue("@length", word.Text.Length);
                        insertCommand.Parameters.AddWithValue("@createdAt", word.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                        var rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            if (!posCountUpdates.ContainsKey(word.POS))
                                posCountUpdates[word.POS] = 0;
                            posCountUpdates[word.POS]++;
                        }
                    }

                    // Batch update POSStatistics
                    foreach (var kvp in posCountUpdates)
                    {
                        var updateStatsCommand = connection.CreateCommand();
                        updateStatsCommand.Transaction = transaction;
                        updateStatsCommand.CommandText = @"
                            UPDATE POSStatistics 
                            SET Count = Count + @count, LastUpdated = CURRENT_TIMESTAMP 
                            WHERE POS = @pos";
                        updateStatsCommand.Parameters.AddWithValue("@pos", kvp.Key);
                        updateStatsCommand.Parameters.AddWithValue("@count", kvp.Value);
                        updateStatsCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    Console.WriteLine($"[DatabaseService] Added {words.Count} words successfully.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error adding words batch: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a word exists in the database.
        /// </summary>
        /// <param name="text">The word text to check.</param>
        /// <returns>True if word exists; otherwise, false.</returns>
        public bool WordExists(string text)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Words WHERE Text = @text";
                command.Parameters.AddWithValue("@text", text);

                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error checking word existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a word by ID and updates POSStatistics.
        /// </summary>
        /// <param name="id">The word ID to delete.</param>
        public void DeleteWord(int id)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Get the POS before deleting
                    var getPosCommand = connection.CreateCommand();
                    getPosCommand.Transaction = transaction;
                    getPosCommand.CommandText = "SELECT POS FROM Words WHERE Id = @id";
                    getPosCommand.Parameters.AddWithValue("@id", id);
                    var pos = getPosCommand.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(pos))
                    {
                        // Delete the word
                        var deleteCommand = connection.CreateCommand();
                        deleteCommand.Transaction = transaction;
                        deleteCommand.CommandText = "DELETE FROM Words WHERE Id = @id";
                        deleteCommand.Parameters.AddWithValue("@id", id);
                        deleteCommand.ExecuteNonQuery();

                        // Update POSStatistics
                        var updateStatsCommand = connection.CreateCommand();
                        updateStatsCommand.Transaction = transaction;
                        updateStatsCommand.CommandText = @"
                            UPDATE POSStatistics 
                            SET Count = Count - 1, LastUpdated = CURRENT_TIMESTAMP 
                            WHERE POS = @pos AND Count > 0";
                        updateStatsCommand.Parameters.AddWithValue("@pos", pos);
                        updateStatsCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error deleting word: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a word by its text value and updates POSStatistics.
        /// </summary>
        /// <param name="text">The word text to delete.</param>
        /// <returns>True if deleted successfully.</returns>
        public bool DeleteWordByText(string text)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Get the POS before deleting
                    var getPosCommand = connection.CreateCommand();
                    getPosCommand.Transaction = transaction;
                    getPosCommand.CommandText = "SELECT POS FROM Words WHERE Text = @text";
                    getPosCommand.Parameters.AddWithValue("@text", text);
                    var pos = getPosCommand.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(pos))
                    {
                        // Delete the word
                        var deleteCommand = connection.CreateCommand();
                        deleteCommand.Transaction = transaction;
                        deleteCommand.CommandText = "DELETE FROM Words WHERE Text = @text";
                        deleteCommand.Parameters.AddWithValue("@text", text);
                        var deleted = deleteCommand.ExecuteNonQuery();

                        if (deleted > 0)
                        {
                            // Update POSStatistics
                            var updateStatsCommand = connection.CreateCommand();
                            updateStatsCommand.Transaction = transaction;
                            updateStatsCommand.CommandText = @"
                                UPDATE POSStatistics 
                                SET Count = Count - 1, LastUpdated = CURRENT_TIMESTAMP 
                                WHERE POS = @pos AND Count > 0";
                            updateStatsCommand.Parameters.AddWithValue("@pos", pos);
                            updateStatsCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return deleted > 0;
                    }

                    transaction.Rollback();
                    return false;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error deleting word by text: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Gets the total count of words in the database.
        /// </summary>
        /// <returns>Total word count.</returns>
        public int GetTotalWordCount()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Words";

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error getting word count: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region Statistics Methods

        /// <summary>
        /// Gets the distribution of words by POS type.
        /// </summary>
        /// <returns>Dictionary mapping POS to count.</returns>
        public Dictionary<string, int> GetPOSDistribution()
        {
            var distribution = new Dictionary<string, int>();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT POS, Count FROM POSStatistics ORDER BY Count DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var pos = reader.GetString(0);
                    var count = reader.GetInt32(1);
                    distribution[pos] = count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error getting POS distribution: {ex.Message}");
            }

            return distribution;
        }

        /// <summary>
        /// Gets the top POS types by count.
        /// </summary>
        /// <param name="limit">Maximum number of results.</param>
        /// <returns>List of tuples (POS, Count).</returns>
        public List<(string POS, int Count)> GetTopPOS(int limit = 9)
        {
            var result = new List<(string POS, int Count)>();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT POS, Count FROM POSStatistics ORDER BY Count DESC LIMIT @limit";
                command.Parameters.AddWithValue("@limit", limit);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add((reader.GetString(0), reader.GetInt32(1)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error getting top POS: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Analysis History Methods

        /// <summary>
        /// Saves an analysis result to history.
        /// </summary>
        /// <param name="input">The input text.</param>
        /// <param name="resultJson">The JSON result.</param>
        public void SaveAnalysis(string input, string resultJson)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO AnalysisHistory (InputText, ResultJson, AnalyzedAt)
                    VALUES (@input, @result, CURRENT_TIMESTAMP)";
                command.Parameters.AddWithValue("@input", input);
                command.Parameters.AddWithValue("@result", resultJson);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error saving analysis: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets recent analysis history.
        /// </summary>
        /// <param name="limit">Maximum number of results.</param>
        /// <returns>List of tuples (Date, Input, Result).</returns>
        public List<(DateTime Date, string Input, string Result)> GetRecentAnalyses(int limit = 50)
        {
            var result = new List<(DateTime Date, string Input, string Result)>();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT AnalyzedAt, InputText, ResultJson 
                    FROM AnalysisHistory 
                    ORDER BY AnalyzedAt DESC 
                    LIMIT @limit";
                command.Parameters.AddWithValue("@limit", limit);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var date = DateTime.Parse(reader.GetString(0));
                    var input = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var json = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    result.Add((date, input, json));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error getting recent analyses: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Export/Import Methods

        /// <summary>
        /// Exports all words to JSON grouped by POS.
        /// </summary>
        /// <returns>JSON string.</returns>
        public string ExportToJson()
        {
            try
            {
                var export = new Dictionary<string, List<object>>();
                var posTypes = WordRootFactory.GetValidPOSTypes();

                foreach (var pos in posTypes)
                {
                    export[pos] = new List<object>();
                }

                var words = GetAllWords();
                foreach (var word in words)
                {
                    if (export.ContainsKey(word.POS))
                    {
                        export[word.POS].Add(new
                        {
                            text = word.Text,
                            root = word.Root,
                            isValid = word.IsValid,
                            createdAt = word.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }

                return JsonSerializer.Serialize(export, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error exporting to JSON: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Imports words from JSON.
        /// </summary>
        /// <param name="json">JSON string with words grouped by POS.</param>
        public void ImportFromJson(string json)
        {
            try
            {
                var import = JsonSerializer.Deserialize<Dictionary<string, List<JsonElement>>>(json);
                if (import == null) return;

                var wordsToAdd = new List<WordRoot>();

                foreach (var kvp in import)
                {
                    var pos = kvp.Key;
                    if (!WordRootFactory.IsValidPOS(pos)) continue;

                    foreach (var item in kvp.Value)
                    {
                        var text = item.GetProperty("text").GetString();
                        var root = item.TryGetProperty("root", out var rootProp) ? rootProp.GetString() : null;

                        if (!string.IsNullOrEmpty(text))
                        {
                            var word = WordRootFactory.CreateWordRoot(text, pos, root);
                            word.Validate();
                            wordsToAdd.Add(word);
                        }
                    }
                }

                AddWords(wordsToAdd);
                Console.WriteLine($"[DatabaseService] Imported {wordsToAdd.Count} words from JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error importing from JSON: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a WordRoot from a data reader row.
        /// </summary>
        private WordRoot? CreateWordRootFromReader(SqliteDataReader reader)
        {
            try
            {
                var text = reader.GetString(0);
                var root = reader.IsDBNull(1) ? null : reader.GetString(1);
                var pos = reader.GetString(2);
                var isValid = reader.GetInt32(3) == 1;
                var createdAt = DateTime.Parse(reader.GetString(4));

                var word = WordRootFactory.CreateWordRoot(text, pos, root);
                word.IsValid = isValid;
                word.CreatedAt = createdAt;
                return word;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseService] Error creating WordRoot from reader: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes the database service.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        #endregion
    }
}

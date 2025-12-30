using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace TurkMorph.Database
{
    /// <summary>
    /// Veritabanı bağlantı yönetimi.
    /// SQLite dosya tabanlı veritabanı kullanır.
    /// Dapper ile birlikte çalışır.
    /// </summary>
    public class TurkMorphContext : IDisposable
    {
        #region Fields

        private readonly string _connectionString;
        private readonly string _databasePath;
        private SQLiteConnection _connection;
        private bool _disposed = false;

        #endregion

        #region Properties

        /// <summary>
        /// Aktif veritabanı bağlantısı.
        /// Dapper sorguları için kullanılır.
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SQLiteConnection(_connectionString);
                }

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                return _connection;
            }
        }

        /// <summary>
        /// Veritabanı dosya yolu.
        /// </summary>
        public string DatabasePath => _databasePath;

        #endregion

        #region Constructor

        /// <summary>
        /// TurkMorphContext Constructor
        /// </summary>
        /// <param name="databasePath">SQLite veritabanı dosya yolu (varsayılan: turkmorph.db)</param>
        public TurkMorphContext(string databasePath = null)
        {
            // Use the current working directory so the DB is created alongside the project
            var baseDir = Directory.GetCurrentDirectory();
            _databasePath = databasePath ?? Path.Combine(baseDir, "turkmorph.db");

            _connectionString = $"Data Source={_databasePath};Version=3;";

            // Veritabanı dosyası yoksa oluştur
            EnsureDatabaseExists();
        }

        #endregion

        #region Database Initialization

        /// <summary>
        /// Veritabanı dosyasını ve tabloları oluşturur (yoksa).
        /// </summary>
        private void EnsureDatabaseExists()
        {
            // Klasör yoksa oluştur
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Dosya yoksa oluştur
            if (!File.Exists(_databasePath))
            {
                SQLiteConnection.CreateFile(_databasePath);
            }

            // Tabloları oluştur
            CreateTables();
        }

        /// <summary>
        /// Gerekli tabloları oluşturur.
        /// </summary>
        private void CreateTables()
        {
            var createTablesSql = @"
                -- Ana kelime kök tablosu
                CREATE TABLE IF NOT EXISTS WordRoots (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Text TEXT NOT NULL,
                    Root TEXT NOT NULL,
                    WordType TEXT NOT NULL,
                    Features TEXT,
                    AddedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsValid INTEGER DEFAULT 1,
                    IsProperNoun INTEGER DEFAULT 0,
                    IsTransitive INTEGER DEFAULT 0,
                    IsComparative INTEGER DEFAULT 0,
                    UNIQUE(Text, Root, WordType)
                );

                -- Analiz geçmişi tablosu
                CREATE TABLE IF NOT EXISTS AnalysisHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InputText TEXT NOT NULL,
                    AnalyzedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    WordCount INTEGER,
                    ValidWordCount INTEGER
                );

                -- Index'ler (Performans için)
                CREATE INDEX IF NOT EXISTS IX_WordRoots_Root ON WordRoots(Root);
                CREATE INDEX IF NOT EXISTS IX_WordRoots_WordType ON WordRoots(WordType);
                CREATE INDEX IF NOT EXISTS IX_WordRoots_Text ON WordRoots(Text);
            ";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand(createTablesSql, connection);
            command.ExecuteNonQuery();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Veritabanındaki toplam kelime sayısını döndürür.
        /// </summary>
        public int GetWordCount()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT COUNT(*) FROM WordRoots", connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Veritabanını temizler (tüm verileri siler).
        /// DİKKAT: Bu işlem geri alınamaz!
        /// </summary>
        public void ClearDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand(@"
                DELETE FROM WordRoots;
                DELETE FROM AnalysisHistory;
                VACUUM;
            ", connection);
            command.ExecuteNonQuery();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null)
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }

        #endregion
    }
}

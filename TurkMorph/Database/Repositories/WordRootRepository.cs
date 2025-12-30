using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using TurkMorph.Models;

namespace TurkMorph.Database.Repositories
{
    /// <summary>
    /// Repository Pattern - Kelime köklerinin CRUD operasyonları.
    /// Dapper kullanarak veritabanı işlemlerini yönetir.
    /// </summary>
    public class WordRootRepository
    {
        #region Fields

        private readonly TurkMorphContext _context;

        #endregion

        #region Constructor

        public WordRootRepository(TurkMorphContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Create

        /// <summary>
        /// Yeni kelime kökü ekler.
        /// </summary>
        /// <param name="wordRoot">Eklenecek kelime</param>
        /// <returns>Eklenen kaydın ID'si</returns>
        public async Task<int> InsertAsync(WordRoot wordRoot)
        {
            if (wordRoot == null)
                throw new ArgumentNullException(nameof(wordRoot));

            var sql = @"
                INSERT OR IGNORE INTO WordRoots (Text, Root, WordType, Features, AddedDate, IsValid, IsProperNoun, IsTransitive, IsComparative)
                VALUES (@Text, @Root, @WordType, @Features, @AddedDate, @IsValid, @IsProperNoun, @IsTransitive, @IsComparative);
                SELECT last_insert_rowid();
            ";

            // Dinamik parametreler - kelime türüne göre özel alanlar
            var parameters = new
            {
                wordRoot.Text,
                wordRoot.Root,
                WordType = wordRoot.GetWordType(),
                wordRoot.Features,
                wordRoot.AddedDate,
                IsValid = wordRoot.IsValid ? 1 : 0,
                IsProperNoun = (wordRoot is NounRoot noun) && noun.IsProperNoun ? 1 : 0,
                IsTransitive = (wordRoot is VerbRoot verb) && verb.IsTransitive ? 1 : 0,
                IsComparative = (wordRoot is AdjectiveRoot adj) && adj.IsComparative ? 1 : 0
            };

            var id = await _context.Connection.ExecuteScalarAsync<int>(sql, parameters);
            wordRoot.Id = id;
            return id;
        }

        /// <summary>
        /// Birden fazla kelime kökü ekler (Bulk Insert).
        /// </summary>
        /// <param name="wordRoots">Eklenecek kelimeler</param>
        /// <returns>Eklenen kayıt sayısı</returns>
        public async Task<int> InsertManyAsync(IEnumerable<WordRoot> wordRoots)
        {
            int count = 0;
            foreach (var word in wordRoots)
            {
                var id = await InsertAsync(word);
                if (id > 0) count++;
            }
            return count;
        }

        #endregion

        #region Read

        /// <summary>
        /// ID'ye göre kelime kökü getirir.
        /// </summary>
        public async Task<WordRoot> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM WordRoots WHERE Id = @Id";
            var row = await _context.Connection.QueryFirstOrDefaultAsync(sql, new { Id = id });
            return MapToWordRoot(row);
        }

        /// <summary>
        /// Tüm kelime köklerini getirir.
        /// </summary>
        public async Task<List<WordRoot>> GetAllAsync()
        {
            var sql = "SELECT * FROM WordRoots ORDER BY AddedDate DESC";
            var rows = await _context.Connection.QueryAsync(sql);
            
            var results = new List<WordRoot>();
            foreach (var row in rows)
            {
                var word = MapToWordRoot(row);
                if (word != null) results.Add(word);
            }
            return results;
        }

        /// <summary>
        /// Kelime türüne göre filtreler.
        /// </summary>
        /// <param name="wordType">NOUN, VERB, ADJ vb.</param>
        public async Task<List<WordRoot>> GetByTypeAsync(string wordType)
        {
            var sql = "SELECT * FROM WordRoots WHERE WordType = @WordType ORDER BY Root";
            var rows = await _context.Connection.QueryAsync(sql, new { WordType = wordType });
            
            var results = new List<WordRoot>();
            foreach (var row in rows)
            {
                var word = MapToWordRoot(row);
                if (word != null) results.Add(word);
            }
            return results;
        }

        /// <summary>
        /// Kök'e göre arama yapar.
        /// </summary>
        /// <param name="root">Aranacak kök</param>
        public async Task<List<WordRoot>> SearchByRootAsync(string root)
        {
            var sql = "SELECT * FROM WordRoots WHERE Root LIKE @Root ORDER BY Root";
            var rows = await _context.Connection.QueryAsync(sql, new { Root = $"%{root}%" });
            
            var results = new List<WordRoot>();
            foreach (var row in rows)
            {
                var word = MapToWordRoot(row);
                if (word != null) results.Add(word);
            }
            return results;
        }

        /// <summary>
        /// Kelime sayısını türe göre gruplar.
        /// </summary>
        public async Task<Dictionary<string, int>> GetCountByTypeAsync()
        {
            var sql = "SELECT WordType, COUNT(*) as Count FROM WordRoots GROUP BY WordType";
            var rows = await _context.Connection.QueryAsync(sql);
            
            var result = new Dictionary<string, int>();
            foreach (var row in rows)
            {
                result[row.WordType] = (int)row.Count;
            }
            return result;
        }

        #endregion

        #region Update

        /// <summary>
        /// Kelime kökünü günceller.
        /// </summary>
        public async Task<bool> UpdateAsync(WordRoot wordRoot)
        {
            var sql = @"
                UPDATE WordRoots 
                SET Text = @Text, Root = @Root, Features = @Features, IsValid = @IsValid
                WHERE Id = @Id
            ";

            var rowsAffected = await _context.Connection.ExecuteAsync(sql, new
            {
                wordRoot.Id,
                wordRoot.Text,
                wordRoot.Root,
                wordRoot.Features,
                IsValid = wordRoot.IsValid ? 1 : 0
            });

            return rowsAffected > 0;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Kelime kökünü siler.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM WordRoots WHERE Id = @Id";
            var rowsAffected = await _context.Connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Geçersiz kelimeleri siler.
        /// </summary>
        public async Task<int> DeleteInvalidAsync()
        {
            var sql = "DELETE FROM WordRoots WHERE IsValid = 0";
            return await _context.Connection.ExecuteAsync(sql);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Veritabanı satırını WordRoot nesnesine dönüştürür.
        /// </summary>
        private WordRoot MapToWordRoot(dynamic row)
        {
            if (row == null) return null;

            string wordType = row.WordType;
            string text = row.Text;
            string root = row.Root;

            WordRoot word = wordType switch
            {
                "NOUN" or "PROPN" => new NounRoot(text, root)
                {
                    IsProperNoun = row.IsProperNoun == 1
                },
                "VERB" => new VerbRoot(text, root)
                {
                    IsTransitive = row.IsTransitive == 1
                },
                "ADJ" => new AdjectiveRoot(text, root)
                {
                    IsComparative = row.IsComparative == 1
                },
                _ => null
            };

            if (word != null)
            {
                word.Id = (int)row.Id;
                word.Features = row.Features;
                word.AddedDate = row.AddedDate;
                word.IsValid = row.IsValid == 1;
            }

            return word;
        }

        #endregion
    }
}

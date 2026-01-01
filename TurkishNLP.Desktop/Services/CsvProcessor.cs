using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TurkishNLP.Desktop.Models;

namespace TurkishNLP.Desktop.Services
{
    /// <summary>
    /// Utility class for processing CSV files containing Turkish words.
    /// Handles reading, processing, and exporting word lists.
    /// </summary>
    public class CsvProcessor
    {
        #region Fields

        private readonly PythonApiClient _apiClient;
        private static readonly char[] Delimiters = { ',', ';', '\t' };
        
        /// <summary>
        /// Valid Turkish characters pattern for word validation.
        /// </summary>
        private static readonly Regex TurkishWordPattern = new Regex(
            @"^[\p{L}\-']+$", 
            RegexOptions.Compiled);

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new CsvProcessor instance.
        /// </summary>
        public CsvProcessor()
        {
            _apiClient = PythonApiClient.Instance;
        }

        /// <summary>
        /// Creates a new CsvProcessor with a custom API client.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        public CsvProcessor(PythonApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Reads unique words from a CSV file.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <returns>List of unique words.</returns>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when file format is invalid.</exception>
        public List<string> ReadWordsFromCsv(string filePath)
        {
            // Validate file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            try
            {
                // Read file with UTF-8 encoding
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                
                if (lines.Length == 0)
                {
                    return new List<string>();
                }

                // Detect delimiter
                var delimiter = DetectDelimiter(lines[0]);
                
                // Check if first line is header
                var startIndex = IsHeaderRow(lines[0], delimiter) ? 1 : 0;
                
                // Extract and process words
                var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                for (int i = startIndex; i < lines.Length; i++)
                {
                    var line = lines[i]?.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Split by delimiter and process each cell
                    var cells = line.Split(delimiter);
                    
                    foreach (var cell in cells)
                    {
                        var word = CleanWord(cell);
                        if (IsValidWord(word))
                        {
                            words.Add(word);
                        }
                    }
                }

                Console.WriteLine($"[CsvProcessor] Read {words.Count} unique words from {Path.GetFileName(filePath)}");
                
                return words.ToList();
            }
            catch (IOException ex)
            {
                throw new InvalidDataException($"Error reading CSV file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CsvProcessor] Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reads words from the first column of a CSV file.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <returns>List of words from the first column.</returns>
        public List<string> ReadWordsFromFirstColumn(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                
                if (lines.Length == 0)
                {
                    return new List<string>();
                }

                var delimiter = DetectDelimiter(lines[0]);
                var startIndex = IsHeaderRow(lines[0], delimiter) ? 1 : 0;
                
                var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                for (int i = startIndex; i < lines.Length; i++)
                {
                    var line = lines[i]?.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var cells = line.Split(delimiter);
                    if (cells.Length > 0)
                    {
                        var word = CleanWord(cells[0]);
                        if (IsValidWord(word))
                        {
                            words.Add(word);
                        }
                    }
                }

                return words.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CsvProcessor] Error reading first column: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Process Methods

        /// <summary>
        /// Processes a CSV file asynchronously, analyzing each word.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <param name="progress">Progress reporter for UI updates.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of analysis results.</returns>
        public async Task<List<AnalysisResult>> ProcessCsvAsync(
            string filePath,
            IProgress<(int current, int total)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            // Read words from CSV
            var words = ReadWordsFromCsv(filePath);
            
            if (words.Count == 0)
            {
                return new List<AnalysisResult>();
            }

            Console.WriteLine($"[CsvProcessor] Processing {words.Count} words...");

            var results = new List<AnalysisResult>();
            var batchSize = 50;
            var total = words.Count;
            var current = 0;

            // Process in batches for better performance
            for (int i = 0; i < words.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = words.Skip(i).Take(batchSize).ToList();
                
                try
                {
                    var batchResults = await _apiClient.AnalyzeBatchAsync(batch);
                    results.AddRange(batchResults);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CsvProcessor] Batch error: {ex.Message}");
                    
                    // Add failed results for this batch
                    foreach (var word in batch)
                    {
                        results.Add(AnalysisResult.CreateFailure(word, ex.Message));
                    }
                }

                current = Math.Min(i + batchSize, total);
                progress?.Report((current, total));
            }

            var successCount = results.Count(r => r.Success);
            Console.WriteLine($"[CsvProcessor] Processed {results.Count} words ({successCount} successful)");

            return results;
        }

        /// <summary>
        /// Processes words one by one with individual progress updates.
        /// </summary>
        public async Task<List<AnalysisResult>> ProcessWordsOneByOneAsync(
            List<string> words,
            IProgress<(int current, int total, string currentWord)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<AnalysisResult>();
            var total = words.Count;

            for (int i = 0; i < words.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var word = words[i];
                progress?.Report((i + 1, total, word));

                try
                {
                    var result = await _apiClient.AnalyzeWordAsync(word);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(AnalysisResult.CreateFailure(word, ex.Message));
                }
            }

            return results;
        }

        #endregion

        #region Export Methods

        /// <summary>
        /// Exports words to a CSV file.
        /// </summary>
        /// <param name="words">List of words to export.</param>
        /// <param name="outputPath">Output file path.</param>
        public void ExportToCsv(List<WordRoot> words, string outputPath)
        {
            if (words == null || words.Count == 0)
            {
                throw new ArgumentException("Word list cannot be null or empty.", nameof(words));
            }

            try
            {
                var sb = new StringBuilder();
                
                // Header row
                sb.AppendLine("Word,Root,POS,IsValid,CreatedAt");

                // Data rows
                foreach (var word in words)
                {
                    var line = $"{EscapeCsvField(word.Text)}," +
                               $"{EscapeCsvField(word.Root ?? "")}," +
                               $"{word.POS}," +
                               $"{(word.IsValid ? "Yes" : "No")}," +
                               $"{word.CreatedAt:yyyy-MM-dd HH:mm:ss}";
                    sb.AppendLine(line);
                }

                // Write with UTF-8 BOM for Excel compatibility
                File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(true));
                
                Console.WriteLine($"[CsvProcessor] Exported {words.Count} words to {Path.GetFileName(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CsvProcessor] Export error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports analysis results to a CSV file.
        /// </summary>
        /// <param name="results">List of analysis results to export.</param>
        /// <param name="outputPath">Output file path.</param>
        public void ExportResultsToCsv(List<AnalysisResult> results, string outputPath)
        {
            if (results == null || results.Count == 0)
            {
                throw new ArgumentException("Results list cannot be null or empty.", nameof(results));
            }

            try
            {
                var sb = new StringBuilder();
                
                // Header row
                sb.AppendLine("Word,Root,POS,Success,Error");

                // Data rows
                foreach (var result in results)
                {
                    var line = $"{EscapeCsvField(result.Word)}," +
                               $"{EscapeCsvField(result.Root ?? "")}," +
                               $"{result.POS ?? ""}," +
                               $"{(result.Success ? "Yes" : "No")}," +
                               $"{EscapeCsvField(result.ErrorMessage ?? "")}";
                    sb.AppendLine(line);
                }

                File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(true));
                
                Console.WriteLine($"[CsvProcessor] Exported {results.Count} results to {Path.GetFileName(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CsvProcessor] Export results error: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Detects the delimiter used in a CSV line.
        /// </summary>
        private char DetectDelimiter(string line)
        {
            if (string.IsNullOrEmpty(line))
                return ',';

            // Count occurrences of each delimiter
            var counts = Delimiters.ToDictionary(
                d => d,
                d => line.Count(c => c == d)
            );

            // Return the delimiter with most occurrences (minimum 1)
            var maxDelimiter = counts
                .Where(kvp => kvp.Value > 0)
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault();

            return maxDelimiter.Value > 0 ? maxDelimiter.Key : ',';
        }

        /// <summary>
        /// Checks if the first row appears to be a header.
        /// </summary>
        private bool IsHeaderRow(string line, char delimiter)
        {
            if (string.IsNullOrEmpty(line))
                return false;

            var cells = line.Split(delimiter);
            
            // Common header keywords
            var headerKeywords = new[] 
            { 
                "word", "kelime", "text", "metin", "root", "kök", 
                "pos", "type", "tür", "id", "no", "column" 
            };

            // Check if first cell contains a header keyword
            var firstCell = cells.FirstOrDefault()?.ToLowerInvariant().Trim() ?? "";
            return headerKeywords.Any(k => firstCell.Contains(k));
        }

        /// <summary>
        /// Cleans a word by removing quotes and extra whitespace.
        /// </summary>
        private string CleanWord(string? cell)
        {
            if (string.IsNullOrEmpty(cell))
                return string.Empty;

            // Remove quotes
            var word = cell.Trim().Trim('"', '\'');
            
            // Remove extra whitespace
            word = Regex.Replace(word, @"\s+", " ").Trim();

            return word;
        }

        /// <summary>
        /// Validates if a string is a valid word.
        /// </summary>
        private bool IsValidWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            // Must be at least 1 character
            if (word.Length < 1)
                return false;

            // Must not be all digits
            if (word.All(char.IsDigit))
                return false;

            // Must contain at least one letter
            if (!word.Any(char.IsLetter))
                return false;

            // Check for valid Turkish word pattern
            return TurkishWordPattern.IsMatch(word);
        }

        /// <summary>
        /// Escapes a field for CSV output.
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // If field contains comma, quote, or newline, wrap in quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                // Double any quotes
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        #endregion

        #region Static Convenience Methods

        /// <summary>
        /// Static method to quickly read words from a file.
        /// </summary>
        public static List<string> QuickReadWords(string filePath)
        {
            var processor = new CsvProcessor();
            return processor.ReadWordsFromCsv(filePath);
        }

        /// <summary>
        /// Static method to quickly export words to CSV.
        /// </summary>
        public static void QuickExport(List<WordRoot> words, string outputPath)
        {
            var processor = new CsvProcessor();
            processor.ExportToCsv(words, outputPath);
        }

        #endregion
    }
}

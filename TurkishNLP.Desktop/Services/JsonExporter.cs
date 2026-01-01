using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TurkishNLP.Desktop.Models;

namespace TurkishNLP.Desktop.Services
{
    /// <summary>
    /// Utility class for exporting and importing words in JSON format grouped by POS.
    /// </summary>
    public class JsonExporter
    {
        #region Fields

        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null, // Keep POS names as uppercase
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        #endregion

        #region Export Methods

        /// <summary>
        /// Exports words to JSON string grouped by POS.
        /// </summary>
        /// <param name="words">List of words to export.</param>
        /// <returns>JSON string with words grouped by POS.</returns>
        public string ExportToJson(List<WordRoot> words)
        {
            if (words == null || words.Count == 0)
            {
                return CreateEmptyExport();
            }

            try
            {
                // Initialize all POS categories with empty lists
                var export = new Dictionary<string, List<string>>();
                foreach (var pos in WordRootFactory.GetValidPOSTypes())
                {
                    export[pos] = new List<string>();
                }

                // Group words by POS and extract text
                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word.Text) && export.ContainsKey(word.POS))
                    {
                        if (!export[word.POS].Contains(word.Text))
                        {
                            export[word.POS].Add(word.Text);
                        }
                    }
                }

                // Sort words alphabetically within each POS
                foreach (var pos in export.Keys.ToList())
                {
                    export[pos] = export[pos].OrderBy(w => w).ToList();
                }

                var json = JsonSerializer.Serialize(export, _serializerOptions);
                
                Console.WriteLine($"[JsonExporter] Exported {words.Count} words to JSON");
                
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonExporter] Error exporting to JSON: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports words with full details (including root) to JSON.
        /// </summary>
        /// <param name="words">List of words to export.</param>
        /// <returns>JSON string with full word details.</returns>
        public string ExportToJsonWithDetails(List<WordRoot> words)
        {
            if (words == null || words.Count == 0)
            {
                return "{}";
            }

            try
            {
                // Initialize all POS categories
                var export = new Dictionary<string, List<WordExportDto>>();
                foreach (var pos in WordRootFactory.GetValidPOSTypes())
                {
                    export[pos] = new List<WordExportDto>();
                }

                // Group words by POS with full details
                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word.Text) && export.ContainsKey(word.POS))
                    {
                        export[word.POS].Add(new WordExportDto
                        {
                            Text = word.Text,
                            Root = word.Root,
                            IsValid = word.IsValid,
                            CreatedAt = word.CreatedAt
                        });
                    }
                }

                return JsonSerializer.Serialize(export, _serializerOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonExporter] Error exporting with details: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports words to a JSON file.
        /// </summary>
        /// <param name="words">List of words to export.</param>
        /// <param name="filePath">Output file path.</param>
        public void ExportToFile(List<WordRoot> words, string filePath)
        {
            try
            {
                var json = ExportToJson(words);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json, Encoding.UTF8);
                
                Console.WriteLine($"[JsonExporter] Saved to file: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonExporter] Error saving to file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports words with full details to a JSON file.
        /// </summary>
        public void ExportToFileWithDetails(List<WordRoot> words, string filePath)
        {
            try
            {
                var json = ExportToJsonWithDetails(words);
                
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json, Encoding.UTF8);
                
                Console.WriteLine($"[JsonExporter] Saved detailed export to: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonExporter] Error saving detailed file: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Import Methods

        /// <summary>
        /// Imports words from JSON string.
        /// </summary>
        /// <param name="json">JSON string with words grouped by POS.</param>
        /// <returns>List of WordRoot objects.</returns>
        public List<WordRoot> ImportFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<WordRoot>();
            }

            try
            {
                // Validate JSON structure
                if (!IsValidJsonStructure(json))
                {
                    throw new JsonException("Invalid JSON structure. Expected object with POS keys.");
                }

                var words = new List<WordRoot>();

                // Try to parse as simple format first (List<string> per POS)
                try
                {
                    var simpleImport = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    if (simpleImport != null)
                    {
                        words = ImportFromSimpleFormat(simpleImport);
                    }
                }
                catch
                {
                    // Try detailed format
                    var detailedImport = JsonSerializer.Deserialize<Dictionary<string, List<WordExportDto>>>(json);
                    if (detailedImport != null)
                    {
                        words = ImportFromDetailedFormat(detailedImport);
                    }
                }

                Console.WriteLine($"[JsonExporter] Imported {words.Count} words from JSON");
                
                return words;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[JsonExporter] JSON parsing error: {ex.Message}");
                throw new InvalidDataException($"Invalid JSON format: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonExporter] Import error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Imports words from a JSON file.
        /// </summary>
        /// <param name="filePath">Path to the JSON file.</param>
        /// <returns>List of WordRoot objects.</returns>
        public List<WordRoot> ImportFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}");
            }

            try
            {
                var json = File.ReadAllText(filePath, Encoding.UTF8);
                return ImportFromJson(json);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[JsonExporter] File read error: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private string CreateEmptyExport()
        {
            var empty = new Dictionary<string, List<string>>();
            foreach (var pos in WordRootFactory.GetValidPOSTypes())
            {
                empty[pos] = new List<string>();
            }
            return JsonSerializer.Serialize(empty, _serializerOptions);
        }

        private bool IsValidJsonStructure(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.ValueKind == JsonValueKind.Object;
            }
            catch
            {
                return false;
            }
        }

        private List<WordRoot> ImportFromSimpleFormat(Dictionary<string, List<string>> import)
        {
            var words = new List<WordRoot>();

            foreach (var kvp in import)
            {
                var pos = kvp.Key.ToUpperInvariant();
                
                if (!WordRootFactory.IsValidPOS(pos))
                {
                    Console.WriteLine($"[JsonExporter] Warning: Skipping unknown POS '{pos}'");
                    continue;
                }

                foreach (var text in kvp.Value ?? new List<string>())
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        try
                        {
                            var word = WordRootFactory.CreateWordRoot(text.Trim(), pos);
                            word.Validate();
                            words.Add(word);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[JsonExporter] Warning: Could not create word '{text}': {ex.Message}");
                        }
                    }
                }
            }

            return words;
        }

        private List<WordRoot> ImportFromDetailedFormat(Dictionary<string, List<WordExportDto>> import)
        {
            var words = new List<WordRoot>();

            foreach (var kvp in import)
            {
                var pos = kvp.Key.ToUpperInvariant();
                
                if (!WordRootFactory.IsValidPOS(pos))
                {
                    Console.WriteLine($"[JsonExporter] Warning: Skipping unknown POS '{pos}'");
                    continue;
                }

                foreach (var dto in kvp.Value ?? new List<WordExportDto>())
                {
                    if (!string.IsNullOrWhiteSpace(dto.Text))
                    {
                        try
                        {
                            var word = WordRootFactory.CreateWordRoot(dto.Text.Trim(), pos, dto.Root);
                            word.IsValid = dto.IsValid;
                            if (dto.CreatedAt != default)
                            {
                                word.CreatedAt = dto.CreatedAt;
                            }
                            words.Add(word);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[JsonExporter] Warning: Could not create word '{dto.Text}': {ex.Message}");
                        }
                    }
                }
            }

            return words;
        }

        #endregion

        #region Static Convenience Methods

        /// <summary>
        /// Static method to quickly export words to JSON string.
        /// </summary>
        public static string QuickExport(List<WordRoot> words)
        {
            return new JsonExporter().ExportToJson(words);
        }

        /// <summary>
        /// Static method to quickly import words from JSON string.
        /// </summary>
        public static List<WordRoot> QuickImport(string json)
        {
            return new JsonExporter().ImportFromJson(json);
        }

        /// <summary>
        /// Static method to quickly export words to file.
        /// </summary>
        public static void QuickExportToFile(List<WordRoot> words, string filePath)
        {
            new JsonExporter().ExportToFile(words, filePath);
        }

        /// <summary>
        /// Static method to quickly import words from file.
        /// </summary>
        public static List<WordRoot> QuickImportFromFile(string filePath)
        {
            return new JsonExporter().ImportFromFile(filePath);
        }

        #endregion

        #region DTOs

        /// <summary>
        /// DTO for detailed word export.
        /// </summary>
        private class WordExportDto
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("root")]
            public string? Root { get; set; }

            [JsonPropertyName("isValid")]
            public bool IsValid { get; set; } = true;

            [JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }
        }

        #endregion
    }
}

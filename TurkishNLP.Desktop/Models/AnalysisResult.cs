using System;
using System.Collections.Generic;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) representing the result of a word analysis.
    /// </summary>
    public class AnalysisResult
    {
        #region Properties

        /// <summary>
        /// Gets or sets the original word that was analyzed.
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the root/lemma form of the word.
        /// </summary>
        public string? Root { get; set; }

        /// <summary>
        /// Gets or sets the Part of Speech tag.
        /// </summary>
        public string? POS { get; set; }

        /// <summary>
        /// Gets or sets the morphological features extracted from the word.
        /// </summary>
        public Dictionary<string, string> Features { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets whether the analysis was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if analysis failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the analysis was performed.
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.Now;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public AnalysisResult()
        {
        }

        /// <summary>
        /// Creates a successful analysis result.
        /// </summary>
        /// <param name="word">The analyzed word.</param>
        /// <param name="root">The root form.</param>
        /// <param name="pos">The POS tag.</param>
        /// <param name="features">The morphological features.</param>
        public AnalysisResult(string word, string? root, string? pos, Dictionary<string, string>? features = null)
        {
            Word = word;
            Root = root;
            POS = pos;
            Features = features ?? new Dictionary<string, string>();
            Success = true;
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a successful analysis result.
        /// </summary>
        public static AnalysisResult CreateSuccess(string word, string? root, string? pos, 
            Dictionary<string, string>? features = null)
        {
            return new AnalysisResult(word, root, pos, features);
        }

        /// <summary>
        /// Creates a failed analysis result with an error message.
        /// </summary>
        public static AnalysisResult CreateFailure(string word, string errorMessage)
        {
            return new AnalysisResult
            {
                Word = word,
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the analysis result to a WordRoot instance.
        /// </summary>
        /// <returns>A WordRoot instance, or null if POS is not set.</returns>
        public WordRoot? ToWordRoot()
        {
            if (string.IsNullOrWhiteSpace(POS))
                return null;

            try
            {
                var wordRoot = WordRootFactory.CreateWordRoot(Word, POS, Root);
                wordRoot.Validate();
                return wordRoot;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a string representation of the analysis result.
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"[{POS}] {Word} → {Root ?? "(no root)"}";
            }
            return $"Error: {ErrorMessage}";
        }

        #endregion
    }
}

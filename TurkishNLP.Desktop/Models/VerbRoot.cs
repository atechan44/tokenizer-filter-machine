using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents a verb word root in Turkish.
    /// Validates Turkish verb patterns (typically ending with -mak/-mek in infinitive).
    /// </summary>
    public class VerbRoot : WordRoot
    {
        #region Constants

        /// <summary>
        /// Common Turkish verb suffixes for validation.
        /// </summary>
        private static readonly string[] VerbSuffixes = new[]
        {
            "mak", "mek",           // Infinitive
            "yor", "iyor", "ıyor", "uyor", "üyor",  // Present continuous
            "dı", "di", "du", "dü", "tı", "ti", "tu", "tü",  // Past tense
            "mış", "miş", "muş", "müş",  // Past reported
            "acak", "ecek",         // Future
            "malı", "meli",         // Necessity
            "abil", "ebil"          // Ability
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public VerbRoot() : base()
        {
            POS = "VERB";
        }

        /// <summary>
        /// Initializes a new VerbRoot with the specified text.
        /// </summary>
        /// <param name="text">The verb text.</param>
        public VerbRoot(string text) : base(text, "VERB")
        {
        }

        /// <summary>
        /// Initializes a new VerbRoot with text and root.
        /// </summary>
        /// <param name="text">The verb text.</param>
        /// <param name="root">The root/lemma form.</param>
        public VerbRoot(string text, string? root) : base(text, "VERB", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates Turkish verb patterns.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsValid = false;
                return false;
            }

            // Verbs should be at least 2 characters
            if (Text.Length < 2)
            {
                IsValid = false;
                return false;
            }

            // Must not contain digits
            if (Text.Any(char.IsDigit))
            {
                IsValid = false;
                return false;
            }

            // Check if it contains any Turkish letter
            var turkishPattern = new Regex(@"[a-zA-ZçÇğĞıİöÖşŞüÜ]");
            if (!turkishPattern.IsMatch(Text))
            {
                IsValid = false;
                return false;
            }

            IsValid = true;
            return true;
        }

        /// <summary>
        /// Returns formatted display info for the verb.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"🔄 VERB: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

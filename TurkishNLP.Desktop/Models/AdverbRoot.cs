using System;
using System.Linq;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents an adverb word root in Turkish.
    /// </summary>
    public class AdverbRoot : WordRoot
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public AdverbRoot() : base()
        {
            POS = "ADV";
        }

        /// <summary>
        /// Initializes a new AdverbRoot with the specified text.
        /// </summary>
        /// <param name="text">The adverb text.</param>
        public AdverbRoot(string text) : base(text, "ADV")
        {
        }

        /// <summary>
        /// Initializes a new AdverbRoot with text and root.
        /// </summary>
        /// <param name="text">The adverb text.</param>
        /// <param name="root">The root/lemma form.</param>
        public AdverbRoot(string text, string? root) : base(text, "ADV", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the adverb.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
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

            IsValid = true;
            return true;
        }

        /// <summary>
        /// Returns formatted display info for the adverb.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"⚡ ADV: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

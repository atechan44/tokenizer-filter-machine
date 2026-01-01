using System;
using System.Linq;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents an adjective word root in Turkish.
    /// </summary>
    public class AdjectiveRoot : WordRoot
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public AdjectiveRoot() : base()
        {
            POS = "ADJ";
        }

        /// <summary>
        /// Initializes a new AdjectiveRoot with the specified text.
        /// </summary>
        /// <param name="text">The adjective text.</param>
        public AdjectiveRoot(string text) : base(text, "ADJ")
        {
        }

        /// <summary>
        /// Initializes a new AdjectiveRoot with text and root.
        /// </summary>
        /// <param name="text">The adjective text.</param>
        /// <param name="root">The root/lemma form.</param>
        public AdjectiveRoot(string text, string? root) : base(text, "ADJ", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the adjective.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsValid = false;
                return false;
            }

            // Adjectives should be at least 2 characters
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

            IsValid = true;
            return true;
        }

        /// <summary>
        /// Returns formatted display info for the adjective.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"🎨 ADJ: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

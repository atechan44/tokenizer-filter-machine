using System;
using System.Linq;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents a noun word root in Turkish.
    /// Validates that the word is at least 2 characters and contains no digits.
    /// </summary>
    public class NounRoot : WordRoot
    {
        #region Constants

        private const int MinimumLength = 2;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public NounRoot() : base()
        {
            POS = "NOUN";
        }

        /// <summary>
        /// Initializes a new NounRoot with the specified text.
        /// </summary>
        /// <param name="text">The noun text.</param>
        public NounRoot(string text) : base(text, "NOUN")
        {
        }

        /// <summary>
        /// Initializes a new NounRoot with text and root.
        /// </summary>
        /// <param name="text">The noun text.</param>
        /// <param name="root">The root/lemma form.</param>
        public NounRoot(string text, string? root) : base(text, "NOUN", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates that the noun is at least 2 characters and contains no digits.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsValid = false;
                return false;
            }

            // Must be at least 2 characters
            if (Text.Length < MinimumLength)
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
        /// Returns formatted display info for the noun.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"📦 NOUN: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

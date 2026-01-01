using System;
using System.Linq;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents a pronoun word root in Turkish.
    /// </summary>
    public class PronounRoot : WordRoot
    {
        #region Constants

        /// <summary>
        /// Common Turkish pronouns for reference.
        /// </summary>
        private static readonly string[] CommonPronouns = new[]
        {
            "ben", "sen", "o", "biz", "siz", "onlar",  // Personal
            "bu", "şu", "bunlar", "şunlar",  // Demonstrative
            "kim", "ne", "hangi", "hangisi",  // Interrogative
            "kendi", "kendisi", "kendileri"  // Reflexive
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public PronounRoot() : base()
        {
            POS = "PRON";
        }

        /// <summary>
        /// Initializes a new PronounRoot with the specified text.
        /// </summary>
        /// <param name="text">The pronoun text.</param>
        public PronounRoot(string text) : base(text, "PRON")
        {
        }

        /// <summary>
        /// Initializes a new PronounRoot with text and root.
        /// </summary>
        /// <param name="text">The pronoun text.</param>
        /// <param name="root">The root/lemma form.</param>
        public PronounRoot(string text, string? root) : base(text, "PRON", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the pronoun.
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
        /// Returns formatted display info for the pronoun.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"👤 PRON: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

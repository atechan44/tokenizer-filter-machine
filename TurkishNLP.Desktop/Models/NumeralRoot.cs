using System;
using System.Linq;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents a numeral word root in Turkish.
    /// </summary>
    public class NumeralRoot : WordRoot
    {
        #region Constants

        /// <summary>
        /// Turkish numeral words for reference.
        /// </summary>
        private static readonly string[] TurkishNumerals = new[]
        {
            "bir", "iki", "üç", "dört", "beş", "altı", "yedi", "sekiz", "dokuz", "on",
            "yirmi", "otuz", "kırk", "elli", "altmış", "yetmiş", "seksen", "doksan",
            "yüz", "bin", "milyon", "milyar"
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public NumeralRoot() : base()
        {
            POS = "NUM";
        }

        /// <summary>
        /// Initializes a new NumeralRoot with the specified text.
        /// </summary>
        /// <param name="text">The numeral text.</param>
        public NumeralRoot(string text) : base(text, "NUM")
        {
        }

        /// <summary>
        /// Initializes a new NumeralRoot with text and root.
        /// </summary>
        /// <param name="text">The numeral text.</param>
        /// <param name="root">The root/lemma form.</param>
        public NumeralRoot(string text, string? root) : base(text, "NUM", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the numeral.
        /// Numerals can contain digits or be spelled out.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsValid = false;
                return false;
            }

            // Numerals are valid if they contain digits or are spelled out
            IsValid = true;
            return true;
        }

        /// <summary>
        /// Returns formatted display info for the numeral.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"🔢 NUM: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

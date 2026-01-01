using System;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents a determiner word root in Turkish.
    /// </summary>
    public class DeterminerRoot : WordRoot
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public DeterminerRoot() : base()
        {
            POS = "DET";
        }

        /// <summary>
        /// Initializes a new DeterminerRoot with the specified text.
        /// </summary>
        /// <param name="text">The determiner text.</param>
        public DeterminerRoot(string text) : base(text, "DET")
        {
        }

        /// <summary>
        /// Initializes a new DeterminerRoot with text and root.
        /// </summary>
        /// <param name="text">The determiner text.</param>
        /// <param name="root">The root/lemma form.</param>
        public DeterminerRoot(string text, string? root) : base(text, "DET", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the determiner.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsValid = false;
                return false;
            }

            IsValid = true;
            return true;
        }

        /// <summary>
        /// Returns formatted display info for the determiner.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"📌 DET: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

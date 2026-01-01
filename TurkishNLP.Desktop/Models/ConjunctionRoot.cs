using System;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents a conjunction word root in Turkish.
    /// </summary>
    public class ConjunctionRoot : WordRoot
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public ConjunctionRoot() : base()
        {
            POS = "CONJ";
        }

        /// <summary>
        /// Initializes a new ConjunctionRoot with the specified text.
        /// </summary>
        /// <param name="text">The conjunction text.</param>
        public ConjunctionRoot(string text) : base(text, "CONJ")
        {
        }

        /// <summary>
        /// Initializes a new ConjunctionRoot with text and root.
        /// </summary>
        /// <param name="text">The conjunction text.</param>
        /// <param name="root">The root/lemma form.</param>
        public ConjunctionRoot(string text, string? root) : base(text, "CONJ", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the conjunction.
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
        /// Returns formatted display info for the conjunction.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"🔗 CONJ: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

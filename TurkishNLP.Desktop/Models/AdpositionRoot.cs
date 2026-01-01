using System;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Represents an adposition (preposition/postposition) word root in Turkish.
    /// Turkish primarily uses postpositions.
    /// </summary>
    public class AdpositionRoot : WordRoot
    {
        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public AdpositionRoot() : base()
        {
            POS = "ADP";
        }

        /// <summary>
        /// Initializes a new AdpositionRoot with the specified text.
        /// </summary>
        /// <param name="text">The adposition text.</param>
        public AdpositionRoot(string text) : base(text, "ADP")
        {
        }

        /// <summary>
        /// Initializes a new AdpositionRoot with text and root.
        /// </summary>
        /// <param name="text">The adposition text.</param>
        /// <param name="root">The root/lemma form.</param>
        public AdpositionRoot(string text, string? root) : base(text, "ADP", root)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Validates the adposition.
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
        /// Returns formatted display info for the adposition.
        /// </summary>
        public override string GetDisplayInfo()
        {
            return $"📍 ADP: {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion
    }
}

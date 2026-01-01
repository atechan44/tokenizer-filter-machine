using System;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Abstract base class for all word root types in the Turkish NLP system.
    /// Provides common properties and abstract methods for word validation and display.
    /// </summary>
    public abstract class WordRoot
    {
        #region Fields

        private string _text = string.Empty;
        private string _pos = string.Empty;
        private string? _root;
        private bool _isValid;
        private DateTime _createdAt;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the original text of the word.
        /// </summary>
        public string Text
        {
            get => _text;
            protected set => _text = value ?? string.Empty;
        }

        /// <summary>
        /// Gets the Part of Speech tag (NOUN, VERB, ADJ, etc.).
        /// </summary>
        public string POS
        {
            get => _pos;
            protected set => _pos = value ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the root/lemma form of the word.
        /// </summary>
        public string? Root
        {
            get => _root;
            set => _root = value;
        }

        /// <summary>
        /// Gets or sets whether the word passed validation.
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            set => _isValid = value;
        }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        protected WordRoot()
        {
            _createdAt = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of WordRoot with the specified text and POS.
        /// </summary>
        /// <param name="text">The word text.</param>
        /// <param name="pos">The Part of Speech tag.</param>
        protected WordRoot(string text, string pos) : this()
        {
            Text = text;
            POS = pos;
        }

        /// <summary>
        /// Initializes a new instance of WordRoot with all properties.
        /// </summary>
        protected WordRoot(string text, string pos, string? root) : this(text, pos)
        {
            Root = root;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Validates the word according to specific rules for its POS type.
        /// </summary>
        /// <returns>True if the word is valid; otherwise, false.</returns>
        public abstract bool Validate();

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Returns a formatted display string for the word.
        /// </summary>
        /// <returns>A formatted string containing word information.</returns>
        public virtual string GetDisplayInfo()
        {
            return $"[{POS}] {Text} → {Root ?? "(no root)"} | Valid: {IsValid}";
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return GetDisplayInfo();
        }

        #endregion
    }
}

using System;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Factory class for creating WordRoot instances based on POS type.
    /// Implements the Factory design pattern.
    /// </summary>
    public static class WordRootFactory
    {
        /// <summary>
        /// Creates a WordRoot instance based on the specified POS type.
        /// </summary>
        /// <param name="text">The word text.</param>
        /// <param name="pos">The Part of Speech tag.</param>
        /// <returns>A concrete WordRoot instance.</returns>
        /// <exception cref="ArgumentException">Thrown when POS is unknown.</exception>
        /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
        public static WordRoot CreateWordRoot(string text, string pos)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "Word text cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(pos))
            {
                throw new ArgumentException("POS cannot be null or empty.", nameof(pos));
            }

            // Normalize POS to uppercase
            var normalizedPos = pos.Trim().ToUpperInvariant();

            return normalizedPos switch
            {
                "NOUN" => new NounRoot(text),
                "PROPN" => new NounRoot(text), // Map Proper Noun to Noun
                "VERB" => new VerbRoot(text),
                "ADJ" => new AdjectiveRoot(text),
                "ADV" => new AdverbRoot(text),
                "PRON" => new PronounRoot(text),
                "CONJ" => new ConjunctionRoot(text),
                "CCONJ" => new ConjunctionRoot(text), // Map CCONJ to CONJ
                "SCONJ" => new ConjunctionRoot(text), // Map SCONJ to CONJ
                "ADP" => new AdpositionRoot(text),
                "DET" => new DeterminerRoot(text),
                "NUM" => new NumeralRoot(text),
                "OTHER" => new NounRoot(text), // Fallback for OTHER
                "PUNCT" => new NounRoot(text), // Fallback
                "X" => new NounRoot(text), // Fallback
                _ => new NounRoot(text) // Graceful degradation: treat unknown as Noun instead of crashing
            };
        }

        /// <summary>
        /// Creates a WordRoot instance with root information.
        /// </summary>
        /// <param name="text">The word text.</param>
        /// <param name="pos">The Part of Speech tag.</param>
        /// <param name="root">The root/lemma form of the word.</param>
        /// <returns>A concrete WordRoot instance with root set.</returns>
        public static WordRoot CreateWordRoot(string text, string pos, string? root)
        {
            var wordRoot = CreateWordRoot(text, pos);
            wordRoot.Root = root;
            return wordRoot;
        }

        /// <summary>
        /// Checks if a POS type is valid.
        /// </summary>
        /// <param name="pos">The POS to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValidPOS(string pos)
        {
            if (string.IsNullOrWhiteSpace(pos))
                return false;

            var normalizedPos = pos.Trim().ToUpperInvariant();
            return normalizedPos is "NOUN" or "VERB" or "ADJ" or "ADV" 
                or "PRON" or "CONJ" or "ADP" or "DET" or "NUM";
        }

        /// <summary>
        /// Gets all valid POS types.
        /// </summary>
        /// <returns>Array of valid POS strings.</returns>
        public static string[] GetValidPOSTypes()
        {
            return new[] { "NOUN", "VERB", "ADJ", "ADV", "PRON", "CONJ", "ADP", "DET", "NUM" };
        }
    }
}

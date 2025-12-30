using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Bağlaç (Conjunction) kökleri için türetilmiş sınıf.
    /// </summary>
    public class ConjunctionRoot : WordRoot
    {
        /// <summary>
        /// Sıralama bağlacı mı? (ve, ile, veya)
        /// </summary>
        public bool IsCoordinatingConjunction { get; set; }

        /// <summary>
        /// Bağımlılık bağlacı mı? (çünkü, ama, fakat)
        /// </summary>
        public bool IsSubordinatingConjunction { get; set; }

        public ConjunctionRoot() : base() { }

        public ConjunctionRoot(string text, string root) : base(text, root)
        {
            var lowerRoot = root.ToLowerInvariant();
            IsCoordinatingConjunction = lowerRoot == "ve" || lowerRoot == "ile" || 
                                        lowerRoot == "veya" || lowerRoot == "ya";
            IsSubordinatingConjunction = lowerRoot == "ama" || lowerRoot == "fakat" || 
                                         lowerRoot == "çünkü" || lowerRoot == "ancak";
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 1)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return "CONJ";
        }

        public override string ToString()
        {
            return $"{Text} → {Root} [Bağlaç]";
        }
    }
}

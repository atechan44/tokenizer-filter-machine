using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Zamir (Pronoun) kökleri için türetilmiş sınıf.
    /// </summary>
    public class PronounRoot : WordRoot
    {
        /// <summary>
        /// Kişi zamiri mi? (ben, sen, o)
        /// </summary>
        public bool IsPersonalPronoun { get; set; }

        /// <summary>
        /// İşaret zamiri mi? (bu, şu, o)
        /// </summary>
        public bool IsDemonstrativePronoun { get; set; }

        /// <summary>
        /// Soru zamiri mi? (kim, ne, hangisi)
        /// </summary>
        public bool IsInterrogativePronoun { get; set; }

        public PronounRoot() : base() { }

        public PronounRoot(string text, string root) : base(text, root)
        {
            // Basit sınıflandırma
            var lowerRoot = root.ToLowerInvariant();
            IsPersonalPronoun = lowerRoot == "ben" || lowerRoot == "sen" || 
                               lowerRoot == "o" || lowerRoot == "biz" || 
                               lowerRoot == "siz" || lowerRoot == "onlar";
            IsDemonstrativePronoun = lowerRoot == "bu" || lowerRoot == "şu";
            IsInterrogativePronoun = lowerRoot == "kim" || lowerRoot == "ne" || 
                                    lowerRoot == "hangi" || lowerRoot == "nere";
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 1)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return "PRON";
        }

        public override string ToString()
        {
            string type = IsPersonalPronoun ? "Kişi" : 
                         (IsDemonstrativePronoun ? "İşaret" : 
                         (IsInterrogativePronoun ? "Soru" : "Diğer"));
            return $"{Text} → {Root} [Zamir - {type}]";
        }
    }
}

using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Zarf (Adverb) kökleri için türetilmiş sınıf.
    /// </summary>
    public class AdverbRoot : WordRoot
    {
        /// <summary>
        /// Zaman zarfı mı? (dün, bugün, yarın)
        /// </summary>
        public bool IsTemporalAdverb { get; set; }

        /// <summary>
        /// Yer zarfı mı? (burada, orada, içeride)
        /// </summary>
        public bool IsLocationalAdverb { get; set; }

        /// <summary>
        /// Durum zarfı mı? (hızlıca, yavaşça)
        /// </summary>
        public bool IsMannerAdverb { get; set; }

        public AdverbRoot() : base() { }

        public AdverbRoot(string text, string root) : base(text, root)
        {
            // Basit analiz
            IsMannerAdverb = text.EndsWith("ca") || text.EndsWith("ce") || 
                            text.EndsWith("ça") || text.EndsWith("çe");
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 2)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return "ADV";
        }

        public override string ToString()
        {
            return $"{Text} → {Root} [Zarf]";
        }
    }
}

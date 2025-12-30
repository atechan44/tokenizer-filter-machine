using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Sayı (Numeral) kökleri için türetilmiş sınıf.
    /// </summary>
    public class NumeralRoot : WordRoot
    {
        /// <summary>
        /// Asıl sayı mı? (bir, iki, üç)
        /// </summary>
        public bool IsCardinalNumber { get; set; }

        /// <summary>
        /// Sıra sayısı mı? (birinci, ikinci)
        /// </summary>
        public bool IsOrdinalNumber { get; set; }

        public NumeralRoot() : base() { }

        public NumeralRoot(string text, string root) : base(text, root)
        {
            IsOrdinalNumber = text.EndsWith("inci") || text.EndsWith("ıncı") ||
                             text.EndsWith("üncü") || text.EndsWith("uncu");
            IsCardinalNumber = !IsOrdinalNumber;
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 1)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return "NUM";
        }

        public override string ToString()
        {
            string type = IsOrdinalNumber ? "Sıra" : "Asıl";
            return $"{Text} → {Root} [Sayı - {type}]";
        }
    }
}

using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Belirteç (Determiner) kökleri için türetilmiş sınıf.
    /// </summary>
    public class DeterminerRoot : WordRoot
    {
        public DeterminerRoot() : base() { }

        public DeterminerRoot(string text, string root) : base(text, root) { }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 1)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return "DET";
        }

        public override string ToString()
        {
            return $"{Text} → {Root} [Belirteç]";
        }
    }
}

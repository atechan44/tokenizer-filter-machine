using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Edat/İlgeç (Adposition) kökleri için türetilmiş sınıf.
    /// </summary>
    public class AdpositionRoot : WordRoot
    {
        public AdpositionRoot() : base() { }

        public AdpositionRoot(string text, string root) : base(text, root) { }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 1)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return "ADP";
        }

        public override string ToString()
        {
            return $"{Text} → {Root} [Edat]";
        }
    }
}

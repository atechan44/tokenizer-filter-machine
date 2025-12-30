using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Diğer/Bilinmeyen türler için genel sınıf.
    /// </summary>
    public class OtherRoot : WordRoot
    {
        /// <summary>
        /// Orijinal POS etiketi (Stanza'dan gelen)
        /// </summary>
        public string OriginalPos { get; set; }

        public OtherRoot() : base() { }

        public OtherRoot(string text, string root, string originalPos = "X") : base(text, root)
        {
            OriginalPos = originalPos;
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(Root) || Root.Length < 1)
                return false;
            return true;
        }

        public override string GetWordType()
        {
            return OriginalPos ?? "X";
        }

        public override string ToString()
        {
            return $"{Text} → {Root} [Diğer - {OriginalPos}]";
        }
    }
}

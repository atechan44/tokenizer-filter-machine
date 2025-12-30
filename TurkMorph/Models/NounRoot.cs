using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - İsim kökleri için türetilmiş sınıf.
    /// WordRoot'tan miras alır ve kendi özelliklerini ekler.
    /// </summary>
    public class NounRoot : WordRoot
    {
        #region NounRoot Specific Properties

        /// <summary>
        /// Özel isim mi? (Ankara, Atatürk gibi)
        /// </summary>
        public bool IsProperNoun { get; set; }

        /// <summary>
        /// Somut isim mi? (masa, kitap) vs Soyut isim (sevgi, özlem)
        /// </summary>
        public bool IsConcrete { get; set; }

        /// <summary>
        /// Çoğul hali kullanılmış mı?
        /// </summary>
        public bool IsPlural { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Parametresiz constructor
        /// </summary>
        public NounRoot() : base() { }

        /// <summary>
        /// Ana constructor
        /// </summary>
        public NounRoot(string text, string root) : base(text, root)
        {
            IsProperNoun = char.IsUpper(text[0]); // İlk harf büyükse özel isim
            IsConcrete = true; // Varsayılan
            IsPlural = text.EndsWith("lar") || text.EndsWith("ler");
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// POLYMORPHISM - İsimler için doğrulama kuralları.
        /// </summary>
        public override bool Validate()
        {
            // Kural 1: Kök en az 2 harfli olmalı
            if (string.IsNullOrEmpty(Root) || Root.Length < 2)
                return false;

            // Kural 2: Kök sadece harflerden oluşmalı (Türkçe karakterler dahil)
            foreach (char c in Root)
            {
                if (!char.IsLetter(c))
                    return false;
            }

            // Kural 3: Kök çok uzun olmamalı (Türkçe'de 12 harften uzun kök nadir)
            if (Root.Length > 12)
                return false;

            return true;
        }

        /// <summary>
        /// Kelime türünü döndürür.
        /// </summary>
        public override string GetWordType()
        {
            return IsProperNoun ? "PROPN" : "NOUN";
        }

        /// <summary>
        /// ToString override - İsim'e özel format
        /// </summary>
        public override string ToString()
        {
            string type = IsProperNoun ? "Özel İsim" : "İsim";
            string plural = IsPlural ? " (Çoğul)" : "";
            return $"{Text} → {Root} [{type}{plural}]";
        }

        #endregion
    }
}

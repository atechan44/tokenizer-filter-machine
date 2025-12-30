using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE - Sıfat kökleri için türetilmiş sınıf.
    /// Sıfatlara özel özellikler ve doğrulama kuralları içerir.
    /// </summary>
    public class AdjectiveRoot : WordRoot
    {
        #region AdjectiveRoot Specific Properties

        /// <summary>
        /// Karşılaştırma derecesi mi? (daha güzel, en güzel)
        /// </summary>
        public bool IsComparative { get; set; }

        /// <summary>
        /// Üstünlük derecesi mi? (en güzel)
        /// </summary>
        public bool IsSuperlative { get; set; }

        /// <summary>
        /// Niteleme sıfatı mı? (güzel, büyük) vs Belirtme sıfatı (bu, şu)
        /// </summary>
        public bool IsQualitative { get; set; }

        /// <summary>
        /// Olumsuz anlam mı? (çirkin, kötü)
        /// </summary>
        public bool IsNegative { get; set; }

        #endregion

        #region Constructors

        public AdjectiveRoot() : base() { }

        public AdjectiveRoot(string text, string root) : base(text, root)
        {
            IsQualitative = true; // Varsayılan olarak niteleme sıfatı
            IsNegative = false;
            IsComparative = false;
            IsSuperlative = false;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// POLYMORPHISM - Sıfatlar için doğrulama kuralları.
        /// </summary>
        public override bool Validate()
        {
            // Kural 1: Kök en az 2 harfli olmalı
            if (string.IsNullOrEmpty(Root) || Root.Length < 2)
                return false;

            // Kural 2: Sadece harflerden oluşmalı
            foreach (char c in Root)
            {
                if (!char.IsLetter(c))
                    return false;
            }

            // Kural 3: Türkçe sıfat kökleri genelde 2-8 harf arası
            if (Root.Length > 10)
                return false;

            return true;
        }

        /// <summary>
        /// Kelime türünü döndürür.
        /// </summary>
        public override string GetWordType()
        {
            return "ADJ";
        }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            string type = IsQualitative ? "Niteleme" : "Belirtme";
            string degree = IsSuperlative ? " (Üstünlük)" : (IsComparative ? " (Karşılaştırma)" : "");
            return $"{Text} → {Root} [Sıfat - {type}{degree}]";
        }

        #endregion

        #region AdjectiveRoot Specific Methods

        /// <summary>
        /// Sıfatın karşılaştırma halini oluşturur.
        /// güzel → daha güzel
        /// </summary>
        public string GetComparative()
        {
            return "daha " + Root;
        }

        /// <summary>
        /// Sıfatın üstünlük halini oluşturur.
        /// güzel → en güzel
        /// </summary>
        public string GetSuperlative()
        {
            return "en " + Root;
        }

        /// <summary>
        /// Sıfattan isim türetir.
        /// güzel → güzellik
        /// </summary>
        public string DeriveNoun()
        {
            // Basit -lik/-lık eki (ünlü uyumu yok, basitleştirilmiş)
            char[] backVowels = { 'a', 'ı', 'o', 'u' };
            char lastVowel = ' ';

            foreach (char c in Root.ToLowerInvariant())
            {
                if ("aeıioöuü".Contains(c))
                    lastVowel = c;
            }

            if (Array.Exists(backVowels, v => v == lastVowel))
                return Root + "lık";
            else
                return Root + "lik";
        }

        #endregion
    }
}

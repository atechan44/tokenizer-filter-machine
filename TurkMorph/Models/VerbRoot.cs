using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// INHERITANCE + POLYMORPHISM - Fiil kökleri için türetilmiş sınıf.
    /// Fiillere özel doğrulama kuralları içerir.
    /// </summary>
    public class VerbRoot : WordRoot
    {
        #region VerbRoot Specific Properties

        /// <summary>
        /// Geçişli fiil mi? (Nesne alabilir: "kitabı okudum")
        /// </summary>
        public bool IsTransitive { get; set; }

        /// <summary>
        /// Dönüşlü fiil mi? (yıkanmak, giyinmek)
        /// </summary>
        public bool IsReflexive { get; set; }

        /// <summary>
        /// Edilgen çatı mı? (okunmak, yazılmak)
        /// </summary>
        public bool IsPassive { get; set; }

        /// <summary>
        /// Olumsuz form mu? (gelmemek, yapmamak)
        /// </summary>
        public bool IsNegative { get; set; }

        #endregion

        #region Constructors

        public VerbRoot() : base() { }

        public VerbRoot(string text, string root) : base(text, root)
        {
            // Basit analiz - daha gelişmiş analiz Stanza'dan gelir
            IsNegative = text.Contains("me") || text.Contains("ma");
            IsPassive = root.EndsWith("l") || root.EndsWith("n");
            IsReflexive = root.EndsWith("n") && text.Contains("ın");
            IsTransitive = true; // Varsayılan olarak geçişli kabul et
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// POLYMORPHISM - Fiiller için doğrulama kuralları.
        /// İsimlerden farklı kurallar!
        /// </summary>
        public override bool Validate()
        {
            // Kural 1: Kök en az 2 harfli olmalı
            if (string.IsNullOrEmpty(Root) || Root.Length < 2)
                return false;

            // Kural 2: Mastar eki ile bitmemeli (kök halini istiyoruz)
            if (Root.EndsWith("mek") || Root.EndsWith("mak"))
                return false;

            // Kural 3: Sadece harflerden oluşmalı
            foreach (char c in Root)
            {
                if (!char.IsLetter(c))
                    return false;
            }

            // Kural 4: Türkçe fiil kökleri genelde kısa (en fazla 10 harf)
            if (Root.Length > 10)
                return false;

            return true;
        }

        /// <summary>
        /// Kelime türünü döndürür.
        /// </summary>
        public override string GetWordType()
        {
            return "VERB";
        }

        /// <summary>
        /// ToString override - Fiil'e özel format
        /// </summary>
        public override string ToString()
        {
            string transitive = IsTransitive ? "Geçişli" : "Geçişsiz";
            string negative = IsNegative ? " Olumsuz" : "";
            return $"{Text} → {Root} [Fiil - {transitive}{negative}]";
        }

        #endregion

        #region VerbRoot Specific Methods

        /// <summary>
        /// Fiil kökünün mastar halini oluşturur.
        /// Ünlü uyumuna dikkat eder.
        /// </summary>
        /// <returns>Mastar hali (gel→gelmek, yaz→yazmak)</returns>
        public string GetInfinitive()
        {
            if (string.IsNullOrEmpty(Root))
                return string.Empty;

            // Son ünlüye göre uyum belirle
            char[] backVowels = { 'a', 'ı', 'o', 'u' };
            char[] frontVowels = { 'e', 'i', 'ö', 'ü' };

            char lastVowel = ' ';
            foreach (char c in Root.ToLowerInvariant())
            {
                if (Array.Exists(backVowels, v => v == c) || Array.Exists(frontVowels, v => v == c))
                {
                    lastVowel = c;
                }
            }

            // Kalın ünlü → -mak, İnce ünlü → -mek
            if (Array.Exists(backVowels, v => v == lastVowel))
                return Root + "mak";
            else
                return Root + "mek";
        }

        #endregion
    }
}

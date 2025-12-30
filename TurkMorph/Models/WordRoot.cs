using System;

namespace TurkMorph.Models
{
    /// <summary>
    /// SOYUT SINIF (Abstract Class) - OOP Şartı
    /// Tüm kelime kök sınıflarının temel sınıfı.
    /// Doğrudan instance oluşturulamaz, türetilmeli.
    /// </summary>
    public abstract class WordRoot
    {
        #region Properties

        /// <summary>
        /// Veritabanı ID'si
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Orijinal kelime metni.
        /// ENCAPSULATION: protected set - sadece türetilen sınıflar değiştirebilir.
        /// </summary>
        public string Text { get; protected set; }

        /// <summary>
        /// Kelimenin kök hali (lemma).
        /// ENCAPSULATION: protected set
        /// </summary>
        public string Root { get; protected set; }

        /// <summary>
        /// Morfolojik özellikler (Stanza'dan gelen feats)
        /// </summary>
        public string Features { get; set; }

        /// <summary>
        /// Veritabanına eklenme tarihi
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Kelimenin kurallara uygun olup olmadığı
        /// </summary>
        public bool IsValid { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Parametresiz constructor (Serialization için gerekli)
        /// </summary>
        protected WordRoot()
        {
            AddedDate = DateTime.Now;
            IsValid = true;
        }

        /// <summary>
        /// Ana constructor
        /// </summary>
        /// <param name="text">Orijinal kelime</param>
        /// <param name="root">Kök hali</param>
        protected WordRoot(string text, string root) : this()
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// POLYMORPHISM - Soyut Metot
        /// Her kelime türü kendi doğrulama mantığını ZORUNDA uygulamak.
        /// NounRoot, VerbRoot, AdjectiveRoot hepsi farklı kurallar uygular.
        /// </summary>
        /// <returns>Kelime kurallara uygunsa true</returns>
        public abstract bool Validate();

        /// <summary>
        /// Kelime türünü string olarak döndürür.
        /// Her türetilen sınıf kendi türünü bildirir.
        /// </summary>
        /// <returns>NOUN, VERB, ADJ vb.</returns>
        public abstract string GetWordType();

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Kelimenin görüntüleme formatı.
        /// Virtual: Alt sınıflar override edebilir.
        /// </summary>
        public override string ToString()
        {
            return $"{Text} → {Root} ({GetWordType()})";
        }

        /// <summary>
        /// Kelimelerin eşitlik kontrolü.
        /// Aynı Text ve Root varsa eşittir.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is WordRoot other)
            {
                return Text.Equals(other.Text, StringComparison.OrdinalIgnoreCase) &&
                       Root.Equals(other.Root, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// HashCode - Dictionary ve HashSet için gerekli
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Text?.ToLowerInvariant(), Root?.ToLowerInvariant());
        }

        #endregion
    }
}

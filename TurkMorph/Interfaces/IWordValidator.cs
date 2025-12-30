namespace TurkMorph.Interfaces
{
    /// <summary>
    /// INTERFACE Pattern - OOP Şartı
    /// Kelime doğrulama davranışını tanımlar.
    /// Bu interface'i implemente eden herhangi bir sınıf,
    /// kelime doğrulama yeteneği kazanır.
    /// </summary>
    public interface IWordValidator
    {
        /// <summary>
        /// Verilen metnin geçerli bir kök olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="text">Kontrol edilecek metin</param>
        /// <returns>Geçerliyse true</returns>
        bool IsValidRoot(string text);

        /// <summary>
        /// Kelime türünü döndürür.
        /// </summary>
        /// <returns>NOUN, VERB, ADJ vb.</returns>
        string GetWordType();

        /// <summary>
        /// Doğrulama kurallarını açıklayan mesaj.
        /// </summary>
        /// <returns>Kural açıklaması</returns>
        string GetValidationRules();
    }
}

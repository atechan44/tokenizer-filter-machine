using System.Collections.Generic;
using System.Threading.Tasks;
using TurkMorph.Services.DTOs;

namespace TurkMorph.Interfaces
{
    /// <summary>
    /// NLP Servis Interface
    /// Dependency Injection için kullanılır.
    /// Farklı NLP servisleri (Stanza, Zemberek vb.) için aynı interface.
    /// </summary>
    public interface INlpService
    {
        /// <summary>
        /// Metni analiz eder ve kelime listesi döner.
        /// </summary>
        /// <param name="text">Analiz edilecek metin</param>
        /// <returns>Analiz sonuçları</returns>
        Task<List<AnalysisResult>> AnalyzeTextAsync(string text);

        /// <summary>
        /// Metni temizler (noktalama, rakam vb. kaldırır).
        /// </summary>
        /// <param name="text">Temizlenecek metin</param>
        /// <returns>Temizlenmiş metin</returns>
        Task<string> CleanTextAsync(string text);

        /// <summary>
        /// Servisin hazır olup olmadığını kontrol eder.
        /// </summary>
        /// <returns>Hazırsa true</returns>
        Task<bool> IsHealthyAsync();
    }
}

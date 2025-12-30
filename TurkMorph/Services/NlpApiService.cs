using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TurkMorph.Interfaces;
using TurkMorph.Services.DTOs;

namespace TurkMorph.Services
{
    /// <summary>
    /// Python NLP API ile iletişim kuran servis.
    /// HttpClient kullanarak FastAPI'ye async istekler atar.
    /// </summary>
    public class NlpApiService : INlpService, IDisposable
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// NLP API Service Constructor
        /// </summary>
        /// <param name="baseUrl">API Base URL (varsayılan: http://127.0.0.1:8000)</param>
        public NlpApiService(string baseUrl = "http://127.0.0.1:8000")
        {
            _baseUrl = baseUrl.TrimEnd('/');
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60) // Stanza işlemleri uzun sürebilir
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Metni analiz et ve kelime listesi döndür.
        /// </summary>
        /// <param name="text">Analiz edilecek metin</param>
        /// <returns>Analiz sonuçları listesi</returns>
        public async Task<List<AnalysisResult>> AnalyzeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<AnalysisResult>();

            try
            {
                var payload = new { text = text };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/analyze/word", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<AnalysisResult>>(responseString, _jsonOptions);

                return results ?? new List<AnalysisResult>();
            }
            catch (HttpRequestException ex)
            {
                // API kapalı veya bağlantı hatası
                Console.WriteLine($"API Bağlantı Hatası: {ex.Message}");
                throw new InvalidOperationException("Python NLP API'ye bağlanılamadı. Sunucunun çalıştığından emin olun.", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout
                Console.WriteLine($"API Timeout: {ex.Message}");
                throw new TimeoutException("NLP analizi zaman aşımına uğradı.", ex);
            }
            catch (JsonException ex)
            {
                // JSON parse hatası
                Console.WriteLine($"JSON Parse Hatası: {ex.Message}");
                throw new InvalidOperationException("API yanıtı parse edilemedi.", ex);
            }
        }

        /// <summary>
        /// Metni temizle (noktalama, rakam vb. kaldır).
        /// </summary>
        /// <param name="text">Temizlenecek metin</param>
        /// <returns>Temizlenmiş metin</returns>
        public async Task<string> CleanTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            try
            {
                var payload = new { text = text };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/clean", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CleanResult>(responseString, _jsonOptions);

                return result?.Cleaned ?? text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Temizleme Hatası: {ex.Message}");
                return text; // Hata durumunda orijinal metni döndür
            }
        }

        /// <summary>
        /// API'nin hazır olup olmadığını kontrol et.
        /// </summary>
        /// <returns>API hazırsa true</returns>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                
                if (!response.IsSuccessStatusCode)
                    return false;

                var responseString = await response.Content.ReadAsStringAsync();
                var health = JsonSerializer.Deserialize<HealthResult>(responseString, _jsonOptions);

                return health?.StanzaLoaded ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// API'nin hazır olmasını bekle (polling).
        /// </summary>
        /// <param name="maxWaitSeconds">Maksimum bekleme süresi</param>
        /// <param name="pollIntervalMs">Kontrol aralığı (ms)</param>
        /// <returns>API hazırsa true</returns>
        public async Task<bool> WaitForApiAsync(int maxWaitSeconds = 120, int pollIntervalMs = 2000)
        {
            var startTime = DateTime.Now;
            var maxWait = TimeSpan.FromSeconds(maxWaitSeconds);

            while (DateTime.Now - startTime < maxWait)
            {
                if (await IsHealthyAsync())
                    return true;

                await Task.Delay(pollIntervalMs);
            }

            return false;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}

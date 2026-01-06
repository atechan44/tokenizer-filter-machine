using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TurkishNLP.Desktop.Models;

namespace TurkishNLP.Desktop.Services
{
    /// <summary>
    /// HTTP client for communicating with the Python FastAPI backend.
    /// Singleton pattern with retry logic and proper error handling.
    /// </summary>
    public sealed class PythonApiClient : IDisposable
    {
        #region Singleton Implementation

        private static PythonApiClient? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of PythonApiClient.
        /// </summary>
        public static PythonApiClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new PythonApiClient();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the base URL of the Python API.
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:8000";

        /// <summary>
        /// Gets or sets the request timeout in seconds.
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for singleton pattern.
        /// </summary>
        private PythonApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Timeout)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Creates a new instance with custom base URL.
        /// </summary>
        /// <param name="baseUrl">The API base URL.</param>
        public PythonApiClient(string baseUrl) : this()
        {
            BaseUrl = baseUrl;
        }

        #endregion

        #region API Request DTOs

        private class AnalyzeRequest
        {
            [JsonPropertyName("word")]
            public string Word { get; set; } = string.Empty;
        }

        private class BatchRequest
        {
            [JsonPropertyName("words")]
            public List<string> Words { get; set; } = new List<string>();
        }

        private class ArticleRequest
        {
            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }

        private class ApiAnalysisResponse
        {
            [JsonPropertyName("word")]
            public string? Word { get; set; }

            [JsonPropertyName("root")]
            public string? Root { get; set; }

            [JsonPropertyName("pos")]
            public string? POS { get; set; }

            [JsonPropertyName("features")]
            public Dictionary<string, string>? Features { get; set; }

            [JsonPropertyName("all_analyses")]
            public List<object>? AllAnalyses { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }

        private class HealthResponse
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("model_loaded")]
            public bool ModelLoaded { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }

        private class ApiArticleResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }

            [JsonPropertyName("word_count")]
            public int WordCount { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Analyzes a single word using the Python backend.
        /// </summary>
        /// <param name="word">The word to analyze.</param>
        /// <returns>AnalysisResult with the analysis outcome.</returns>
        public async Task<AnalysisResult> AnalyzeWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return AnalysisResult.CreateFailure(word, "Word cannot be empty.");
            }

            var request = new AnalyzeRequest { Word = word };

            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/analyze",
                    request,
                    _jsonOptions
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return AnalysisResult.CreateFailure(word, $"API error: {response.StatusCode} - {errorContent}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiAnalysisResponse>(_jsonOptions);

                if (apiResponse == null)
                {
                    return AnalysisResult.CreateFailure(word, "Invalid response from API.");
                }

                if (!string.IsNullOrEmpty(apiResponse.Error))
                {
                    return AnalysisResult.CreateFailure(word, apiResponse.Error);
                }

                return AnalysisResult.CreateSuccess(
                    apiResponse.Word ?? word,
                    apiResponse.Root,
                    apiResponse.POS,
                    apiResponse.Features
                );
            }, word);
        }

        /// <summary>
        /// Analyzes multiple words in a batch using the Python backend.
        /// </summary>
        /// <param name="words">List of words to analyze.</param>
        /// <param name="progressCallback">Optional callback for progress updates.</param>
        /// <returns>List of AnalysisResult objects.</returns>
        public async Task<List<AnalysisResult>> AnalyzeBatchAsync(
            List<string> words, 
            Action<int, int>? progressCallback = null)
        {
            if (words == null || words.Count == 0)
            {
                return new List<AnalysisResult>();
            }

            var request = new BatchRequest { Words = words };

            try
            {
                progressCallback?.Invoke(0, words.Count);

                var response = await ExecuteWithRetryAsync(async () =>
                {
                    var httpResponse = await _httpClient.PostAsJsonAsync(
                        $"{BaseUrl}/analyze-batch",
                        request,
                        _jsonOptions
                    );

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await httpResponse.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"API error: {httpResponse.StatusCode} - {errorContent}");
                    }

                    return await httpResponse.Content.ReadFromJsonAsync<List<ApiAnalysisResponse>>(_jsonOptions);
                });

                if (response == null)
                {
                    return words.ConvertAll(w => AnalysisResult.CreateFailure(w, "Invalid response from API."));
                }

                var results = new List<AnalysisResult>();

                for (int i = 0; i < response.Count; i++)
                {
                    var apiResponse = response[i];

                    if (!string.IsNullOrEmpty(apiResponse.Error))
                    {
                        results.Add(AnalysisResult.CreateFailure(apiResponse.Word ?? words[i], apiResponse.Error));
                    }
                    else
                    {
                        results.Add(AnalysisResult.CreateSuccess(
                            apiResponse.Word ?? words[i],
                            apiResponse.Root,
                            apiResponse.POS,
                            apiResponse.Features
                        ));
                    }

                    progressCallback?.Invoke(i + 1, words.Count);
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonApiClient] Batch analysis error: {ex.Message}");
                return words.ConvertAll(w => AnalysisResult.CreateFailure(w, ex.Message));
            }
        }

        /// <summary>
        /// Checks if the Python backend is running and healthy.
        /// </summary>
        /// <returns>True if backend is healthy; otherwise, false.</returns>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/health",
                    cts.Token
                );

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var healthResponse = await response.Content.ReadFromJsonAsync<HealthResponse>(_jsonOptions, cts.Token);

                return healthResponse?.Status == "healthy" && healthResponse.ModelLoaded;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[PythonApiClient] Health check timed out.");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[PythonApiClient] Health check failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PythonApiClient] Health check error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets detailed health information from the backend.
        /// </summary>
        /// <returns>Tuple of (isHealthy, message).</returns>
        public async Task<(bool IsHealthy, string Message)> GetHealthDetailsAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/health",
                    cts.Token
                );

                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"Backend returned status {response.StatusCode}");
                }

                var healthResponse = await response.Content.ReadFromJsonAsync<HealthResponse>(_jsonOptions, cts.Token);

                if (healthResponse == null)
                {
                    return (false, "Invalid health response");
                }

                return (
                    healthResponse.Status == "healthy" && healthResponse.ModelLoaded,
                    healthResponse.Message ?? "No message"
                );
            }
            catch (TaskCanceledException)
            {
                return (false, "Connection timed out");
            }
            catch (HttpRequestException)
            {
                return (false, "Backend is not running");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public class ArticleFetchResult
        {
            public bool Success { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public int WordCount { get; set; }
            public string Error { get; set; } = string.Empty;
        }

        public async Task<ArticleFetchResult> FetchArticleAsync(string url)
        {
            try
            {
                var request = new ArticleRequest { Url = url };

                var response = await ExecuteWithRetryAsync(async () => 
                {
                     var httpResponse = await _httpClient.PostAsJsonAsync(
                        $"{BaseUrl}/fetch-article",
                        request,
                        _jsonOptions
                    );

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        var error = await httpResponse.Content.ReadAsStringAsync();
                         return new ArticleFetchResult { Success = false, Error = $"HTTP {httpResponse.StatusCode}: {error}" };
                    }

                    var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiArticleResponse>(_jsonOptions);
                    if (apiResponse == null)
                        return new ArticleFetchResult { Success = false, Error = "Empty response" };

                    if (!apiResponse.Success)
                        return new ArticleFetchResult { Success = false, Error = apiResponse.Error ?? "Unknown API error" };

                    return new ArticleFetchResult
                    {
                        Success = true,
                        Title = apiResponse.Title ?? "",
                        Text = apiResponse.Text ?? "",
                        WordCount = apiResponse.WordCount
                    };
                });

                return response ?? new ArticleFetchResult { Success = false, Error = "Failed to execute request" };
            }
            catch (Exception ex)
            {
                return new ArticleFetchResult { Success = false, Error = ex.Message };
            }
        }

        #endregion

        #region Retry Logic

        /// <summary>
        /// Executes an async function with exponential backoff retry logic.
        /// </summary>
        private async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
        {
            Exception? lastException = null;

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    Console.WriteLine($"[PythonApiClient] Attempt {attempt + 1}/{MaxRetries} failed: {ex.Message}");

                    if (attempt < MaxRetries - 1)
                    {
                        var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                        await Task.Delay(delay);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    Console.WriteLine($"[PythonApiClient] Request timed out on attempt {attempt + 1}");

                    if (attempt < MaxRetries - 1)
                    {
                        var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                        await Task.Delay(delay);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[PythonApiClient] JSON parsing error: {ex.Message}");
                    throw; // Don't retry JSON errors
                }
            }

            throw lastException ?? new Exception("Unknown error during API call");
        }

        /// <summary>
        /// Executes an async function with retry, returning AnalysisResult on failure.
        /// </summary>
        private async Task<AnalysisResult> ExecuteWithRetryAsync(
            Func<Task<AnalysisResult>> action, 
            string word)
        {
            Exception? lastException = null;

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    Console.WriteLine($"[PythonApiClient] Attempt {attempt + 1}/{MaxRetries} failed: {ex.Message}");

                    if (attempt < MaxRetries - 1)
                    {
                        var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                        await Task.Delay(delay);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    Console.WriteLine($"[PythonApiClient] Request timed out on attempt {attempt + 1}");

                    if (attempt < MaxRetries - 1)
                    {
                        var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                        await Task.Delay(delay);
                    }
                }
                catch (JsonException ex)
                {
                    return AnalysisResult.CreateFailure(word, $"Invalid JSON response: {ex.Message}");
                }
            }

            return AnalysisResult.CreateFailure(word, 
                lastException?.Message ?? "Failed after multiple attempts");
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes the HTTP client.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}

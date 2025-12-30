using System.Text.Json.Serialization;

namespace TurkMorph.Services.DTOs
{
    /// <summary>
    /// Data Transfer Object - API'den gelen analiz sonucu.
    /// Python FastAPI ile aynı yapıda olmalı.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Orijinal kelime
        /// </summary>
        [JsonPropertyName("word")]
        public string Word { get; set; }

        /// <summary>
        /// Kelimenin kökü (lemma)
        /// </summary>
        [JsonPropertyName("lemma")]
        public string Lemma { get; set; }

        /// <summary>
        /// Kelime türü (NOUN, VERB, ADJ, ADV, PRON, CONJ, NUM, ADP, DET, X)
        /// </summary>
        [JsonPropertyName("pos")]
        public string Pos { get; set; }

        /// <summary>
        /// Morfolojik özellikler (Case=Acc|Number=Sing vb.)
        /// </summary>
        [JsonPropertyName("feats")]
        public string Feats { get; set; }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"{Word} → {Lemma} [{Pos}]";
        }
    }

    /// <summary>
    /// Metin temizleme sonucu
    /// </summary>
    public class CleanResult
    {
        [JsonPropertyName("original")]
        public string Original { get; set; }

        [JsonPropertyName("cleaned")]
        public string Cleaned { get; set; }
    }

    /// <summary>
    /// API sağlık kontrolü sonucu
    /// </summary>
    public class HealthResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("stanza_loaded")]
        public bool StanzaLoaded { get; set; }
    }
}

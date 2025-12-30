using System;
using System.Collections.Generic;
using TurkMorph.Models;
using TurkMorph.Services.DTOs;

namespace TurkMorph.Services
{
    /// <summary>
    /// FACTORY PATTERN - Creational Design Pattern
    /// API'den gelen veriye göre doğru WordRoot türünü oluşturur.
    /// Bu sayede switch/case mantığı tek bir yerde toplanır.
    /// </summary>
    public static class WordFactory
    {
        /// <summary>
        /// API sonucuna göre uygun WordRoot nesnesi oluşturur.
        /// POLYMORPHISM: Dönen nesne WordRoot tipinde ama
        /// runtime'da gerçek tipi NounRoot, VerbRoot vs. olabilir.
        /// </summary>
        /// <param name="analysisResult">API'den gelen analiz sonucu</param>
        /// <returns>WordRoot türetilmiş sınıf instance'ı veya null</returns>
        public static WordRoot Create(AnalysisResult analysisResult)
        {
            if (analysisResult == null || string.IsNullOrEmpty(analysisResult.Word))
                return null;

            return Create(
                wordType: analysisResult.Pos,
                text: analysisResult.Word,
                root: analysisResult.Lemma ?? analysisResult.Word,
                features: analysisResult.Feats
            );
        }

        /// <summary>
        /// Kelime türüne göre uygun WordRoot nesnesi oluşturur.
        /// </summary>
        /// <param name="wordType">Kelime türü (NOUN, VERB, ADJ, vb.)</param>
        /// <param name="text">Orijinal kelime</param>
        /// <param name="root">Kök (lemma)</param>
        /// <param name="features">Morfolojik özellikler</param>
        /// <returns>Uygun WordRoot türevi</returns>
        public static WordRoot Create(string wordType, string text, string root, string features = null)
        {
            if (string.IsNullOrEmpty(wordType) || string.IsNullOrEmpty(text))
                return null;

            WordRoot wordObj = wordType.ToUpperInvariant() switch
            {
                // İsimler
                "NOUN" => new NounRoot(text, root) { Features = features },
                "PROPN" => new NounRoot(text, root) { IsProperNoun = true, Features = features },
                
                // Fiiller
                "VERB" => new VerbRoot(text, root) { Features = features },
                
                // Sıfatlar
                "ADJ" => new AdjectiveRoot(text, root) { Features = features },
                
                // Zarflar
                "ADV" => new AdverbRoot(text, root) { Features = features },
                
                // Zamirler
                "PRON" => new PronounRoot(text, root) { Features = features },
                
                // Bağlaçlar
                "CONJ" or "CCONJ" or "SCONJ" => new ConjunctionRoot(text, root) { Features = features },
                
                // Sayılar
                "NUM" => new NumeralRoot(text, root) { Features = features },
                
                // Edatlar
                "ADP" => new AdpositionRoot(text, root) { Features = features },
                
                // Belirteçler
                "DET" => new DeterminerRoot(text, root) { Features = features },
                
                // Diğer/Bilinmeyen türler
                _ => new OtherRoot(text, root, wordType) { Features = features }
            };

            // Polymorphism: Her nesne kendi Validate() metodunu çalıştırır
            if (wordObj != null)
            {
                wordObj.IsValid = wordObj.Validate();
            }

            return wordObj;
        }

        /// <summary>
        /// API sonuç listesini WordRoot listesine dönüştürür.
        /// Null dönen nesneleri filtreler.
        /// </summary>
        /// <param name="analysisResults">API sonuç listesi</param>
        /// <returns>WordRoot listesi</returns>
        public static List<WordRoot> CreateMany(IEnumerable<AnalysisResult> analysisResults)
        {
            var wordRoots = new List<WordRoot>();

            if (analysisResults == null)
                return wordRoots;

            foreach (var result in analysisResults)
            {
                var wordObj = Create(result);
                if (wordObj != null)
                {
                    wordRoots.Add(wordObj);
                }
            }

            return wordRoots;
        }

        /// <summary>
        /// Desteklenen kelime türlerini döndürür.
        /// </summary>
        public static string[] GetSupportedTypes()
        {
            return new[] { "NOUN", "PROPN", "VERB", "ADJ", "ADV", "PRON", "CONJ", "NUM", "ADP", "DET" };
        }

        /// <summary>
        /// Kelime türünün desteklenip desteklenmediğini kontrol eder.
        /// </summary>
        public static bool IsSupported(string wordType)
        {
            if (string.IsNullOrEmpty(wordType))
                return false;

            return wordType.ToUpperInvariant() switch
            {
                "NOUN" or "PROPN" or "VERB" or "ADJ" or "ADV" or "PRON" or "CONJ" or "CCONJ" or "SCONJ" or "NUM" or "ADP" or "DET" => true,
                _ => true // Now all types are supported via OtherRoot
            };
        }
    }
}

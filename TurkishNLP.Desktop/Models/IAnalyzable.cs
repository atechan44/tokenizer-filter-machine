using System.Threading.Tasks;

namespace TurkishNLP.Desktop.Models
{
    /// <summary>
    /// Interface for objects that can be analyzed asynchronously.
    /// </summary>
    public interface IAnalyzable
    {
        /// <summary>
        /// Performs asynchronous analysis on the object.
        /// </summary>
        /// <returns>An AnalysisResult containing the analysis outcome.</returns>
        Task<AnalysisResult> AnalyzeAsync();
    }
}

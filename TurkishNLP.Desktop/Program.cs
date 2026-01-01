using System;
using System.Windows.Forms;
using TurkishNLP.Desktop.Forms;
using TurkishNLP.Desktop.Services;

namespace TurkishNLP.Desktop
{
    /// <summary>
    /// Application configuration settings.
    /// </summary>
    public class AppSettings
    {
        public string ApiBaseUrl { get; set; } = "http://localhost:8000";
        public string DatabasePath { get; set; } = "words.db";
    }

    /// <summary>
    /// Application entry point for Turkish NLP Analyzer.
    /// </summary>
    static class Program
    {
        public static AppSettings Settings { get; private set; } = new AppSettings();

        [STAThread]
        static void Main()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Turkish NLP Analyzer - Starting...");
            Console.WriteLine("========================================");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize database
            try
            {
                var db = DatabaseService.Instance;
                Console.WriteLine("[OK] Database initialized");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Initialize API client
            var api = PythonApiClient.Instance;
            Console.WriteLine("[OK] API client initialized");

            // Global exception handler
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show($"Error: {e.Exception.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.Run(new MainForm());
        }
    }
}

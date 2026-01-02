using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using TurkishNLP.Desktop.Models;
using TurkishNLP.Desktop.Services;

namespace TurkishNLP.Desktop.Forms
{
    /// <summary>
    /// Main application form for Turkish NLP Analyzer.
    /// DevExpress Windows Forms version.
    /// </summary>
    public partial class MainForm : XtraForm
    {
        #region Fields

        private readonly PythonApiClient _apiClient;
        private readonly DatabaseService _dbService;
        private readonly CsvProcessor _csvProcessor;
        private readonly JsonExporter _jsonExporter;
        
        private AnalysisResult? _currentAnalysisResult;
        private List<AnalysisResult> _batchResults = new List<AnalysisResult>();
        private CancellationTokenSource? _batchCts;
        private bool _isBackendHealthy = false;

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();

            _apiClient = PythonApiClient.Instance;
            _dbService = DatabaseService.Instance;
            _csvProcessor = new CsvProcessor();
            _jsonExporter = new JsonExporter();

            // Apply DevExpress theme
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("The Bezier");

            SetupEventHandlers();
            SetupKeyboardShortcuts();
        }

        #endregion

        #region Initialization

        private void SetupEventHandlers()
        {
            // Dashboard
            btnRefresh.Click += async (s, e) => await LoadDashboardAsync();

            // Analysis
            btnAnalyze.Click += async (s, e) => await AnalyzeWordAsync();
            btnSaveWord.Click += (s, e) => SaveCurrentWord();
            txtWord.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    await AnalyzeWordAsync();
                    e.Handled = true;
                }
            };

            // Batch
            btnSelectFile.Click += async (s, e) => await SelectAndProcessFileAsync();
            btnSaveBatch.Click += (s, e) => SaveBatchToDatabase();

            // Database
            cmbPosFilter.SelectedIndexChanged += (s, e) => LoadWordsFromDatabase();
            btnExport.Click += (s, e) => ExportToJson();
            btnDelete.Click += (s, e) => DeleteSelectedWords();
            txtSearch.TextChanged += (s, e) => FilterGrid();
        }

        private void SetupKeyboardShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        _ = LoadDashboardAsync();
                        e.Handled = true;
                        break;
                    case Keys.O when e.Control:
                        _ = SelectAndProcessFileAsync();
                        e.Handled = true;
                        break;
                    case Keys.S when e.Control:
                        ExportToJson();
                        e.Handled = true;
                        break;
                    case Keys.F1:
                        ShowAbout();
                        e.Handled = true;
                        break;
                }
            };
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize combo boxes
            cmbPosFilter.Properties.Items.Add("All");
            foreach (var pos in WordRootFactory.GetValidPOSTypes())
            {
                cmbPosFilter.Properties.Items.Add(pos);
            }
            cmbPosFilter.SelectedIndex = 0;

            // Check backend
            await CheckBackendHealthAsync();

            // Load data
            await LoadDashboardAsync();
            LoadWordsFromDatabase();

            UpdateTitle();
        }

        #endregion

        #region Dashboard

        private async Task LoadDashboardAsync()
        {
            var distribution = _dbService.GetPOSDistribution();
            var total = _dbService.GetTotalWordCount();

            // Update stat labels
            lblNoun.Text = $"NOUN: {distribution.GetValueOrDefault("NOUN", 0)}";
            lblVerb.Text = $"VERB: {distribution.GetValueOrDefault("VERB", 0)}";
            lblAdj.Text = $"ADJ: {distribution.GetValueOrDefault("ADJ", 0)}";
            lblAdv.Text = $"ADV: {distribution.GetValueOrDefault("ADV", 0)}";
            lblPron.Text = $"PRON: {distribution.GetValueOrDefault("PRON", 0)}";
            lblConj.Text = $"CONJ: {distribution.GetValueOrDefault("CONJ", 0)}";
            lblAdp.Text = $"ADP: {distribution.GetValueOrDefault("ADP", 0)}";
            lblDet.Text = $"DET: {distribution.GetValueOrDefault("DET", 0)}";
            lblNum.Text = $"NUM: {distribution.GetValueOrDefault("NUM", 0)}";
            lblTotal.Text = $"Total Words: {total}";

            await CheckBackendHealthAsync();
            UpdateTitle();
        }

        #endregion

        #region Word Analysis

        private async Task AnalyzeWordAsync()
        {
            var word = txtWord.Text?.Trim();
            if (string.IsNullOrEmpty(word))
            {
                XtraMessageBox.Show("Please enter a word.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_isBackendHealthy)
            {
                XtraMessageBox.Show("Backend is offline. Please start the Python API.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                btnAnalyze.Enabled = false;
                memoResult.Text = "Analyzing...";

                var result = await _apiClient.AnalyzeWordAsync(word);
                _currentAnalysisResult = result;

                if (result.Success)
                {
                    var features = result.Features.Count > 0
                        ? string.Join("\r\n", result.Features.Select(f => $"  • {f.Key}: {f.Value}"))
                        : "  (none)";

                    memoResult.Text = $"✅ Analysis Complete\r\n\r\n" +
                        $"Word: {result.Word}\r\n" +
                        $"Root: {result.Root ?? "(not found)"}\r\n" +
                        $"POS: {result.POS ?? "(unknown)"}\r\n\r\n" +
                        $"Features:\r\n{features}";

                    btnSaveWord.Enabled = true;
                    _dbService.SaveAnalysis(word, JsonSerializer.Serialize(result));
                }
                else
                {
                    memoResult.Text = $"❌ Failed: {result.ErrorMessage}";
                    btnSaveWord.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                memoResult.Text = $"❌ Error: {ex.Message}";
            }
            finally
            {
                btnAnalyze.Enabled = true;
            }
        }

        private void SaveCurrentWord()
        {
            if (_currentAnalysisResult == null || !_currentAnalysisResult.Success)
            {
                XtraMessageBox.Show("No valid result to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var wordRoot = _currentAnalysisResult.ToWordRoot();
            if (wordRoot != null)
            {
                _dbService.AddWord(wordRoot);
                XtraMessageBox.Show($"Saved: {wordRoot.Text}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _ = LoadDashboardAsync();
                txtWord.Text = "";
                txtWord.Focus();
            }
        }

        #endregion

        #region Batch Processing

        private async Task SelectAndProcessFileAsync()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "CSV Files|*.csv|Text Files|*.txt|All Files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            if (!_isBackendHealthy)
            {
                XtraMessageBox.Show("Backend is offline.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                btnSelectFile.Enabled = false;
                progressBatch.Position = 0;

                var words = _csvProcessor.ReadWordsFromCsv(dialog.FileName);
                if (words.Count == 0)
                {
                    XtraMessageBox.Show("No words found.", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                words = words.Take(500).ToList();
                progressBatch.Properties.Maximum = words.Count;
                lblProgress.Text = $"0/{words.Count}";

                var progress = new Progress<(int current, int total)>(p =>
                {
                    progressBatch.Position = p.current;
                    lblProgress.Text = $"{p.current}/{p.total}";
                });

                _batchResults = await _csvProcessor.ProcessCsvAsync(dialog.FileName, progress);

                gridBatch.DataSource = _batchResults.Select(r => new
                {
                    Word = r.Word,
                    Root = r.Root ?? "",
                    POS = r.POS ?? "",
                    OK = r.Success ? "✓" : "✗"
                }).ToList();

                var success = _batchResults.Count(r => r.Success);
                lblProgress.Text = $"Done: {success}/{_batchResults.Count}";
                btnSaveBatch.Enabled = success > 0;
            }
            finally
            {
                btnSelectFile.Enabled = true;
            }
        }

        private void SaveBatchToDatabase()
        {
            var successful = _batchResults.Where(r => r.Success).ToList();
            if (successful.Count == 0) return;

            var words = successful.Select(r => r.ToWordRoot()).Where(w => w != null).Cast<WordRoot>().ToList();
            _dbService.AddWords(words);
            XtraMessageBox.Show($"Saved {words.Count} words!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ = LoadDashboardAsync();
            LoadWordsFromDatabase();
        }

        #endregion

        #region Database Viewer

        private void LoadWordsFromDatabase()
        {
            var pos = cmbPosFilter.SelectedItem?.ToString();
            var words = pos == "All" || string.IsNullOrEmpty(pos)
                ? _dbService.GetAllWords()
                : _dbService.GetWordsByPOS(pos);

            gridDatabase.DataSource = words.Select(w => new
            {
                Text = w.Text,
                Root = w.Root ?? "",
                POS = w.POS,
                Valid = w.IsValid ? "✓" : "✗"
            }).ToList();

            lblDbCount.Text = $"{words.Count} words";
        }

        private void FilterGrid()
        {
            var search = txtSearch.Text?.Trim().ToLower();
            if (string.IsNullOrEmpty(search))
            {
                LoadWordsFromDatabase();
                return;
            }

            var pos = cmbPosFilter.SelectedItem?.ToString();
            var words = pos == "All" || string.IsNullOrEmpty(pos)
                ? _dbService.GetAllWords()
                : _dbService.GetWordsByPOS(pos);

            var filtered = words.Where(w => w.Text.ToLower().Contains(search) || 
                (w.Root?.ToLower().Contains(search) ?? false)).ToList();

            gridDatabase.DataSource = filtered.Select(w => new
            {
                Text = w.Text,
                Root = w.Root ?? "",
                POS = w.POS,
                Valid = w.IsValid ? "✓" : "✗"
            }).ToList();

            lblDbCount.Text = $"{filtered.Count} words";
        }

        private void ExportToJson()
        {
            var words = _dbService.GetAllWords();
            if (words.Count == 0)
            {
                XtraMessageBox.Show("No words to export.", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "JSON|*.json",
                FileName = $"words_{DateTime.Now:yyyyMMdd}.json"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _jsonExporter.ExportToFile(words, dialog.FileName);
                XtraMessageBox.Show($"Exported {words.Count} words.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSelectedWords()
        {
            var gridView = gridDatabase.MainView as GridView;
            if (gridView == null || gridView.SelectedRowsCount == 0)
            {
                XtraMessageBox.Show("Select rows to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (XtraMessageBox.Show($"Delete {gridView.SelectedRowsCount} word(s)?", "Confirm", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int deletedCount = 0;
            foreach (var rowHandle in gridView.GetSelectedRows())
            {
                var text = gridView.GetRowCellValue(rowHandle, "Text")?.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    if (_dbService.DeleteWordByText(text))
                    {
                        deletedCount++;
                    }
                }
            }

            XtraMessageBox.Show($"Deleted {deletedCount} word(s).", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadWordsFromDatabase();
            _ = LoadDashboardAsync();
        }

        #endregion

        #region Helpers

        private async Task CheckBackendHealthAsync()
        {
            _isBackendHealthy = await _apiClient.CheckHealthAsync();
        }

        private void UpdateTitle()
        {
            var status = _isBackendHealthy ? "🟢 Online" : "🔴 Offline";
            this.Text = $"Turkish NLP Analyzer - Backend {status}";
        }

        private void ShowAbout()
        {
            XtraMessageBox.Show(
                "Turkish NLP Analyzer v1.0\n\n" +
                "DevExpress WinForms Edition\n\n" +
                "Shortcuts:\n• F5 - Refresh\n• Ctrl+O - Open CSV\n• Ctrl+S - Export\n• F1 - About",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}

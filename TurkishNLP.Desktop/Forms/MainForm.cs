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
using TurkishNLP.Desktop.Controls;
using TurkishNLP.Desktop.Utils;

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
        private Panel? _pieChartPanel;
        private Dictionary<string, int> _currentDistribution = new Dictionary<string, int>();
        private bool _isDarkMode = false;
        private SimpleButton? _btnThemeToggle;

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
            SetupEventHandlers();
            SetupKeyboardShortcuts();
            SetupFormStyle();
            
            // Initial Theme Apply
            ApplyTheme();
        }

        private void SetupFormStyle()
        {
            this.Text = "Turkish NLP Analyzer v1.0";
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status Strip
            var statusStrip = new StatusStrip();
            var statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "✅ Backend: Online | 📁 Database: Connected | 🕒 " + DateTime.Now.ToString("HH:mm");
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => {
                statusLabel.Text = $"✅ Backend: {(_isBackendHealthy ? "Online" : "Offline")} | 📁 Database: Connected | 🕒 " + DateTime.Now.ToString("HH:mm:ss");
            };
            timer.Start();

            // Theme Toggle Button
            _btnThemeToggle = new SimpleButton();
            _btnThemeToggle.Text = "🌙";
            _btnThemeToggle.ToolTip = "Toggle Dark/Light Mode";
            _btnThemeToggle.Size = new Size(40, 40);
            _btnThemeToggle.Location = new Point(this.ClientSize.Width - 60, 10);
            _btnThemeToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnThemeToggle.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _btnThemeToggle.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            _btnThemeToggle.Appearance.Font = new Font("Segoe UI", 12f);
            _btnThemeToggle.Cursor = Cursors.Hand;
            _btnThemeToggle.Click += (s, e) => ToggleTheme();
            this.Controls.Add(_btnThemeToggle);
            _btnThemeToggle.BringToFront();

            // Style buttons initially
            UpdateButtonColors(); 
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var theme = _isDarkMode ? ThemeManager.Dark : ThemeManager.Light;
            var currentSkin = _isDarkMode ? "Office 2019 Black" : "The Bezier";
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(currentSkin);

            // 1. Form & Backgrounds
            this.BackColor = theme.Background;
            if (_btnThemeToggle != null) 
            {
                _btnThemeToggle.Text = _isDarkMode ? "☀️" : "🌙";
                _btnThemeToggle.Appearance.ForeColor = theme.Text;
            }

            // Dictionary of POS colors (same as pie chart)
            var posColors = new Dictionary<string, Color>
            {
                { "NOUN", Color.FromArgb(52, 152, 219) },
                { "VERB", Color.FromArgb(46, 204, 113) },
                { "ADJ", Color.FromArgb(230, 126, 34) },
                { "ADV", Color.FromArgb(155, 89, 182) },
                { "PRON", Color.FromArgb(241, 196, 15) },
                { "CONJ", Color.FromArgb(26, 188, 156) },
                { "ADP", Color.FromArgb(231, 76, 60) },
                { "DET", Color.FromArgb(149, 165, 166) },
                { "NUM", Color.FromArgb(52, 73, 94) }
            };

            // 2. Dashboards Elements (Recursively update KpiCards and Labels if needed)
            foreach (Control ctrl in tabDashboard.Controls)
            {
                if (ctrl is KpiCard card)
                {
                    // Default to theme surface
                    Color targetBackColor = theme.Surface;
                    Color targetTextColor = theme.Text;

                    // Check if this card corresponds to a POS tag
                    foreach (Control child in card.Controls)
                    {
                        if (child is LabelControl lbl)
                        {
                            // Label text is like "NOUN: 22"
                            // Identify POS key
                            string labelText = lbl.Text;
                            string? posKey = posColors.Keys.FirstOrDefault(k => labelText.StartsWith(k + ":"));
                            
                            if (posKey != null)
                            {
                                // Apply Color!
                                targetBackColor = posColors[posKey];
                                targetTextColor = Color.White; // Always white on colored cards
                            }
                            else if (labelText.StartsWith("Total Words"))
                            {
                                // Give "Total Words" a neutral but distinct color, e.g., Dark Grey or Theme Primary
                                // user didn't ask for this specifically, but let's keep it safe.
                                // Actually user said "color codes to KPIs". Total Words isn't a POS.
                                // Let's keep Total Words as normal theme card.
                            }
                            
                            lbl.Appearance.ForeColor = targetTextColor;
                        }
                    }

                    card.BackColor = targetBackColor;
                    card.ShadowColor = _isDarkMode ? Color.Transparent : Color.FromArgb(30, 0, 0, 0); 
                    card.BorderColor = theme.Border;
                    card.Invalidate();
                }
                else if (ctrl is LabelControl lbl)
                {
                    // ... existing label code ...
                    if (lbl.Font.Size > 12) lbl.Appearance.ForeColor = theme.Text;
                    else lbl.Appearance.ForeColor = theme.TextSecondary;
                }
            }
            
            // 3. Status Strip - User requested "Same as White Mode colors" (Light)
            var statusStrip = this.Controls.OfType<StatusStrip>().FirstOrDefault();
            if (statusStrip != null)
            {
                // Force Light Mode colors for Status Strip even in Dark Mode
                statusStrip.BackColor = Color.White; // or SystemColors.Control
                statusStrip.ForeColor = Color.Black;
                foreach(ToolStripItem item in statusStrip.Items)
                {
                    item.ForeColor = Color.Black;
                }
            }

            // 3. Update Pie Chart
            if (_pieChartPanel != null)
            {
                _pieChartPanel.Invalidate(); // Will trigger paint with new colors
            }
            
            // 4. Update Button Colors (to prevent Skin override)
            UpdateButtonColors();
        }

        private void UpdateButtonColors()
        {
            // Re-apply intended colors
            StyleButton(btnRefresh, Color.FromArgb(41, 128, 185));
            StyleButton(btnAnalyze, Color.FromArgb(46, 204, 113));
            StyleButton(btnSaveWord, Color.FromArgb(26, 188, 156));
            StyleButton(btnSelectFile, Color.FromArgb(52, 152, 219));
            StyleButton(btnSaveBatch, Color.FromArgb(26, 188, 156));
            StyleButton(btnExport, Color.FromArgb(52, 152, 219));
            StyleButton(btnDelete, Color.FromArgb(231, 76, 60));
        }

        private void StyleButton(SimpleButton btn, Color bgColor)
        {
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btn.Appearance.BackColor = bgColor;
            
            // Contrast Check
            // User requested Black text in Light Mode.
            // In Dark Mode, White text is standard.
            btn.Appearance.ForeColor = _isDarkMode ? Color.White : Color.Black; 
            
            btn.Appearance.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Height = 40;
            
            // Note: SimpleButton handles hover automatically with LookAndFeel, 
            // but we explicitly set BackColor for flat look.
            btn.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            // For true flat color with DevExpress SimpleButton, we rely on its own skinning or these properties
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

            // Setup Dashboard UI (Headers, PieChart, KPI Cards)
            SetupDashboardUI();

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

            // Update pie chart
            UpdatePieChart(distribution);

            await CheckBackendHealthAsync();
            UpdateTitle();
        }

        private void SetupDashboardUI()
        {
            // 1. Headers
            var headerLabel = new LabelControl();
            headerLabel.Text = "📊 Dashboard Overview";
            headerLabel.Appearance.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            headerLabel.Appearance.ForeColor = Color.FromArgb(44, 62, 80);
            headerLabel.Location = new Point(20, 20);
            tabDashboard.Controls.Add(headerLabel);

            var subtitleLabel = new LabelControl();
            subtitleLabel.Text = "Real-time Turkish NLP Statistics";
            subtitleLabel.Appearance.Font = new Font("Segoe UI", 11f);
            subtitleLabel.Appearance.ForeColor = Color.FromArgb(127, 140, 141);
            subtitleLabel.Location = new Point(20, 55);
            tabDashboard.Controls.Add(subtitleLabel);

            // 2. Adjust for Header (Move down by 60px)
            MoveControl(lblNoun, 0, 60); MoveControl(lblVerb, 0, 60); MoveControl(lblAdj, 0, 60);
            MoveControl(lblAdv, 0, 60); MoveControl(lblPron, 0, 60); MoveControl(lblConj, 0, 60);
            MoveControl(lblAdp, 0, 60); MoveControl(lblDet, 0, 60); MoveControl(lblNum, 0, 60);
            MoveControl(lblTotal, 0, 60); MoveControl(btnRefresh, 0, 60);

            // 3. Wrap Labels in KPI Cards
            WrapInKpiCard(lblNoun); WrapInKpiCard(lblVerb); WrapInKpiCard(lblAdj);
            WrapInKpiCard(lblAdv); WrapInKpiCard(lblPron); WrapInKpiCard(lblConj);
            WrapInKpiCard(lblAdp); WrapInKpiCard(lblDet); WrapInKpiCard(lblNum);

            // 4. Initialize Pie Chart
            InitializePieChart();
        }

        private void MoveControl(Control ctrl, int dx, int dy)
        {
            ctrl.Location = new Point(ctrl.Location.X + dx, ctrl.Location.Y + dy);
        }

        private void WrapInKpiCard(LabelControl label)
        {
            var card = new KpiCard();
            card.Location = label.Location;
            card.Size = label.Size;
            card.BackColor = label.Appearance.BackColor;
            
            // Adjust label to be inside
            tabDashboard.Controls.Remove(label);
            label.Parent = card;
            label.Dock = DockStyle.Fill;
            label.Appearance.BackColor = Color.Transparent; // Important for card bg to show
            label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            
            card.Controls.Add(label);
            tabDashboard.Controls.Add(card);
            card.BringToFront();
        }

        private void InitializePieChart()
        {
            _pieChartPanel = new Panel();
            _pieChartPanel.Size = new Size(400, 400);
            _pieChartPanel.Location = new Point(660, 80); // Adjusted Y
            _pieChartPanel.BackColor = Color.Transparent;
            _pieChartPanel.Paint += PieChartPanel_Paint;
            // Enable double buffering via reflection
            typeof(Panel).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic, 
                null, _pieChartPanel, new object[] { true });

            tabDashboard.Controls.Add(_pieChartPanel);
        }

        private void PieChartPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            var theme = _isDarkMode ? ThemeManager.Dark : ThemeManager.Light;

            // Draw Border
            using (var borderPen = new Pen(theme.Border, 1))
            {
                var borderRect = new Rectangle(0, 0, _pieChartPanel.Width - 1, _pieChartPanel.Height - 1);
                g.DrawRectangle(borderPen, borderRect);
            }

            // Draw Title
            using (var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold))
            {
                var titleColor = theme.Text;
                g.DrawString("POS Distribution", titleFont, new SolidBrush(titleColor), 20, 10);
            }

            if (_currentDistribution == null || !_currentDistribution.Values.Any(v => v > 0))
            {
                using var font = new Font("Segoe UI", 12F);
                g.DrawString("No data", font, Brushes.Gray, 150, 180);
                return;
            }

            var posColors = new Dictionary<string, Color>
            {
                { "NOUN", Color.FromArgb(52, 152, 219) },
                { "VERB", Color.FromArgb(46, 204, 113) },
                { "ADJ", Color.FromArgb(230, 126, 34) },
                { "ADV", Color.FromArgb(155, 89, 182) },
                { "PRON", Color.FromArgb(241, 196, 15) },
                { "CONJ", Color.FromArgb(26, 188, 156) },
                { "ADP", Color.FromArgb(231, 76, 60) },
                { "DET", Color.FromArgb(149, 165, 166) },
                { "NUM", Color.FromArgb(52, 73, 94) }
            };

            var total = _currentDistribution.Values.Sum();
            if (total == 0) return;

            var rect = new Rectangle(20, 50, 220, 220); // Pie area
            float startAngle = 0;

            // Sort logic
            var sortedData = _currentDistribution.Where(x => x.Value > 0).OrderByDescending(x => x.Value).ToList();

            foreach (var kvp in sortedData)
            {
                float sweepAngle = (kvp.Value / (float)total) * 360;
                var color = posColors.GetValueOrDefault(kvp.Key, Color.Gray);
                
                // Explode logic
                float pct = (kvp.Value / (float)total);
                bool explode = pct < 0.05;
                
                Rectangle drawRect = rect;
                if (explode)
                {
                    float midAngle = startAngle + sweepAngle / 2;
                    float radians = midAngle * (float)(Math.PI / 180);
                    int offset = 10;
                    drawRect = new Rectangle(
                        rect.X + (int)(Math.Cos(radians) * offset),
                        rect.Y + (int)(Math.Sin(radians) * offset),
                        rect.Width, rect.Height);
                }

                using (var brush = new SolidBrush(color))
                {
                    g.FillPie(brush, drawRect, startAngle, sweepAngle);
                }
                
                // Draw Percentage on Slice
                if (pct > 0.05) 
                {
                    float labelAngle = startAngle + sweepAngle / 2;
                    float rad = labelAngle * (float)(Math.PI / 180);
                    float r = rect.Width / 2 * 0.65f; 
                    float cx = drawRect.X + drawRect.Width / 2 + (float)(Math.Cos(rad) * r);
                    float cy = drawRect.Y + drawRect.Height / 2 + (float)(Math.Sin(rad) * r);
                    
                    var pctStr = pct.ToString("P0");
                    using (var font = new Font("Segoe UI", 10F, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var size = g.MeasureString(pctStr, font);
                        g.DrawString(pctStr, font, brush, cx - size.Width / 2, cy - size.Height / 2);
                    }
                }

                startAngle += sweepAngle;
            }

            int legendY = 60;
            using var legendFont = new Font("Segoe UI", 10F);
            foreach (var kvp in sortedData)
            {
                var color = posColors.GetValueOrDefault(kvp.Key, Color.Gray);
                var pct = (kvp.Value / (float)total);
                
                var legendBrush = new SolidBrush(color);
                var legendTextBrush = new SolidBrush(theme.Text); // Dynamic text color
                g.FillRectangle(legendBrush, 260, legendY, 15, 15);
                
                string percText = $"{pct*100:F1}%";
                g.DrawString($"{kvp.Key}: {kvp.Value} ({percText})", legendFont, legendTextBrush, 280, legendY - 2);
                
                legendY += 20;
                
                legendBrush.Dispose();
                legendTextBrush.Dispose();
            }
        }

        private void UpdatePieChart(Dictionary<string, int> distribution)
        {
            _currentDistribution = distribution;
            _pieChartPanel?.Invalidate();
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

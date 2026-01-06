using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TurkishNLP.Desktop.Controls;
using TurkishNLP.Desktop.Services;

namespace TurkishNLP.Desktop.Forms
{
    public partial class MainForm : Form
    {
        // Services
        private readonly PythonApiClient _apiClient;
        private readonly DatabaseService _dbService;
        private List<string> _notifications = new List<string>();

        // Sidebar Controls
        private ModernButton btnNewAnalysis;
        private ModernButton btnDictionary;
        private ModernButton btnHistory;
        private ModernButton btnSettings;
        private ModernButton btnHelp;

        // Right Panel Controls
        private ModernPanel pnlUpload;
        private ModernButton btnDataUrl;
        
        // Status Labels
        private System.Windows.Forms.Label lblBackendStatus;
        private System.Windows.Forms.Panel lblBackendIndicator; 
        private System.Windows.Forms.Label lblLlmStatus;
        private System.Windows.Forms.Panel lblLlmIndicator;
        
        // Main Content Controls
        private ModernTextBox txtSearch;
        private ModernTabControl tabMain;
        
        // Dashboard Controls
        private ModernKpiCard cardTotal;
        private ModernKpiCard cardRoots;
        private ModernKpiCard cardCategory;
        private ModernPanel panelWordTypeChart;
        private ModernTextBox txtQuickAnalysis;
        private ModernButton btnQuickAnalyze;
        private System.Windows.Forms.Label lblRecentActivity;

        public MainForm()
        {
            InitializeComponent();
            
            // Initialize Services
            _apiClient = PythonApiClient.Instance;
            _dbService = DatabaseService.Instance;

            SetupUI();
            
            // Event Handlers
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            LoadDashboardMetrics();
            StartBackendStatusMonitoring();
            _ = CheckBackendStatus(); 
        }

        private void StartBackendStatusMonitoring()
        {
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 10000; // 10 seconds
            timer.Tick += async (s, e) => await CheckBackendStatus();
            timer.Start();
        }

        private void SetupUI()
        {
            InitializeSidebar();
            InitializeRightPanel();
            InitializeContent();
        }

        private void InitializeSidebar()
        {
            pnlSidebar.Controls.Clear();
            int y = 20;
            int x = 20;
            int width = pnlSidebar.Width - 40;

            // 1. Logo
            var lblLogo = new System.Windows.Forms.Label
            {
                Text = "🅿️ NLP Workbench",
                Font = ModernTheme.GetTitleFont(14f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(x, y)
            };
            pnlSidebar.Controls.Add(lblLogo);
            y += 60;

            // 2. Section: ANALİZ YÖNETİMİ
            AddSidebarSectionHeader("ANALİZ YÖNETİMİ", x, ref y);
            
            btnNewAnalysis = CreateModernButton("➕ Yeni Analiz Başlat", width, ModernTheme.AccentGreen, new Point(x, y));
            pnlSidebar.Controls.Add(btnNewAnalysis);
            y += 50;

            btnDictionary = CreateModernButton("📚 Sözlük", width, ModernTheme.SidebarColor, new Point(x, y));
            pnlSidebar.Controls.Add(btnDictionary);
            y += 50;

            btnHistory = CreateModernButton("🕐 Geçmiş Analizler", width, ModernTheme.SidebarColor, new Point(x, y));
            pnlSidebar.Controls.Add(btnHistory);
            y += 50;
            
            y += 20;

            // 3. Section: SİSTEM
            AddSidebarSectionHeader("SİSTEM", x, ref y);
            
            btnSettings = CreateModernButton("⚙️ Ayarlar", width, ModernTheme.SidebarColor, new Point(x, y));
            pnlSidebar.Controls.Add(btnSettings);
            y += 50;

            btnHelp = CreateModernButton("❓ Yardım", width, ModernTheme.SidebarColor, new Point(x, y));
            pnlSidebar.Controls.Add(btnHelp);

            // 4. User Info (Bottom)
            var pnlUser = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(30,30,30)
            };
            
            var pnlAvatar = new ModernPanel
            {
                Size = new Size(40,40),
                Location = new Point(15, 20),
                BorderRadius = 20, 
                BackColor = Color.Gray 
            };
            
            var lblUser = new System.Windows.Forms.Label
            {
                Text = "Ahmet Yılmaz\nVeri Bilimci",
                Font = ModernTheme.GetBodyFont(9f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(60, 25)
            };
            
            pnlUser.Controls.Add(lblUser);
            pnlUser.Controls.Add(pnlAvatar);
            
            pnlSidebar.Controls.Add(pnlUser);
        }

        private void InitializeRightPanel()
        {
            pnlRightPanel.Controls.Clear();
            int y = 20;
            int x = 20;
            int width = pnlRightPanel.Width - 40;

            // A) Data Upload Section
            var lblUploadTitle = new System.Windows.Forms.Label
            {
                Text = "☁️ Veri Yükleme",
                Font = ModernTheme.GetTitleFont(12f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(x, y)
            };
            pnlRightPanel.Controls.Add(lblUploadTitle);
            y += 30;

            pnlUpload = new ModernPanel
            {
                Size = new Size(width, 150),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(35, 35, 35),
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderRadius = 12,
                AllowDrop = true, // Enable Drag & Drop
                Cursor = Cursors.Hand
            };
            
            var lblUploadHint = new System.Windows.Forms.Label
            {
                Text = "Dosyayı sürükleyin\nveya seçmek için tıklayın",
                Font = ModernTheme.GetBodyFont(9f),
                ForeColor = ModernTheme.TextSecondary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            
            // Wire up events
            pnlUpload.DragEnter += PnlUpload_DragEnter;
            pnlUpload.DragLeave += PnlUpload_DragLeave;
            pnlUpload.DragDrop += PnlUpload_DragDrop;
            pnlUpload.Click += PnlUpload_Click;
            lblUploadHint.Click += (s,e) => PnlUpload_Click(pnlUpload, e); // Pass click to panel

            pnlUpload.Controls.Add(lblUploadHint);
            pnlRightPanel.Controls.Add(pnlUpload);
            
            y += 170;

            
            btnDataUrl = CreateModernButton("🔗 URL'den Veri Çek", width, ModernTheme.CardColor, new Point(x, y));
            btnDataUrl.Click += async (s, e) => await HandleUrlFetchAsync();
            pnlRightPanel.Controls.Add(btnDataUrl);
            y += 50;
            y += 50;
            
            y += 20;

            // C) System Status Header
             var lblStatusHead = new System.Windows.Forms.Label
            {
                Text = "🟢 Sistem Durumu",
                Font = ModernTheme.GetTitleFont(11f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(x, y)
            };
            pnlRightPanel.Controls.Add(lblStatusHead);
            y += 30;
            
            // 1) Backend API Status (Manual Creation)
            var pnlBackend = new System.Windows.Forms.Panel { Size = new Size(pnlRightPanel.Width - 40, 25), Location = new Point(x, y) };
            var lblBackendName = new System.Windows.Forms.Label { Text = "Backend API", ForeColor = ModernTheme.TextSecondary, AutoSize = true, Location = new Point(0, 0) };
            lblBackendIndicator = new System.Windows.Forms.Panel { Size = new Size(10, 10), BackColor = ModernTheme.StatusWaiting, Location = new Point(135, 7) }; // Centered vertically
            lblBackendStatus = new System.Windows.Forms.Label { Text = "Bekleniyor...", ForeColor = ModernTheme.StatusWaiting, AutoSize = true, Location = new Point(150, 0), TextAlign = ContentAlignment.TopRight };
            
            pnlBackend.Controls.Add(lblBackendName);
            pnlBackend.Controls.Add(lblBackendIndicator);
            pnlBackend.Controls.Add(lblBackendStatus);
            pnlRightPanel.Controls.Add(pnlBackend);
            y += 30;
            
            // 2) LLM Status (Disabled per request)
            /*
            var pnlLlm = new System.Windows.Forms.Panel { Size = new Size(pnlRightPanel.Width - 40, 25), Location = new Point(x, y) };
            var lblLlmName = new System.Windows.Forms.Label { Text = "LLM Bağlantısı", ForeColor = ModernTheme.TextSecondary, AutoSize = true, Location = new Point(0, 0) };
            lblLlmIndicator = new System.Windows.Forms.Panel { Size = new Size(10, 10), BackColor = ModernTheme.StatusWaiting, Location = new Point(135, 7) };
            lblLlmStatus = new System.Windows.Forms.Label { Text = "Beklemede", ForeColor = ModernTheme.StatusWaiting, AutoSize = true, Location = new Point(150, 0), TextAlign = ContentAlignment.TopRight };
            
            pnlLlm.Controls.Add(lblLlmName);
            pnlLlm.Controls.Add(lblLlmIndicator);
            pnlLlm.Controls.Add(lblLlmStatus);
            pnlRightPanel.Controls.Add(pnlLlm);
            y += 30;
            */
        }

        private void InitializeContent()
        {
            pnlContent.Controls.Clear();
            
            // 1. Top Bar
            var pnlTop = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ModernTheme.BackColor
            };
            
            var lblBreadcrumb = new System.Windows.Forms.Label
            {
                Text = "🏠 / Genel Bakış",
                Font = ModernTheme.GetBodyFont(10f),
                ForeColor = ModernTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 20)
            };
            
            txtSearch = new ModernTextBox
            {
                Size = new Size(400, 35),
                Location = new Point(200, 12),
                
            };
            txtSearch.InnerTextBox.Text = "Veri, analiz veya komut ara..."; // Placeholder
            txtSearch.InnerTextBox.ForeColor = Color.Gray;

            var btnNotification = new System.Windows.Forms.Label { Text = "🔔", ForeColor = Color.White, AutoSize = true, Location = new Point(pnlContent.Width - 150, 20), Font = new Font("Segoe UI", 12f), Cursor = Cursors.Hand };
            var btnExport = CreateModernButton("Dışa Aktar", 100, ModernTheme.CardColor, new Point(pnlContent.Width - 120, 10)); 
            
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNotification.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            
            // Wire up new events
            btnExport.Click += (s, e) => ExportData();
            btnNotification.Click += (s, e) => ShowNotifications();

            pnlTop.Controls.Add(lblBreadcrumb);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnNotification);
            pnlTop.Controls.Add(btnExport);

            // 2. Tab Control
            tabMain = new ModernTabControl
            {
                Dock = DockStyle.Fill
            };
            
            var tabDashboard = new TabPage("📊 Genel Bakış");
            var tabDictionary = new TabPage("📚 Sözlük");
            var tabHistory = new TabPage("🕑 Geçmiş");
            var tabSettings = new TabPage("⚙️ Ayarlar"); // Added Settings
            var tabLlm = new TabPage("🤖 LLM Asistanı");
            var tabEditor = new TabPage("📝 Metin Editörü");
            
            tabMain.TabPages.Add(tabDashboard);
            tabMain.TabPages.Add(tabDictionary);
            tabMain.TabPages.Add(tabHistory);
            tabMain.TabPages.Add(tabSettings); // Added Settings
            var tabHelp = new TabPage("❓ Yardım");
            tabMain.TabPages.Add(tabHelp);
            tabMain.TabPages.Add(tabLlm);
            tabMain.TabPages.Add(tabEditor);
            
            InitializeDashboard(tabDashboard);
            InitializeDictionaryTab(tabDictionary);
            InitializeRealHistoryTab(tabHistory);
            InitializeSettingsTab(tabSettings); // Added Initialization
            InitializeHelpTab(tabHelp);

            pnlContent.Controls.Add(tabMain);
            pnlContent.Controls.Add(pnlTop); 
            tabMain.BringToFront();
            pnlTop.BringToFront(); 
            
            // Wire up Sidebar buttons to these tabs
            if (btnNewAnalysis != null) btnNewAnalysis.Click += (s,e) => 
            { 
                tabMain.SelectedTab = tabDashboard; 
                txtQuickAnalysis.InnerTextBox.Text = "Analiz etmek veya LLM'e göndermek için metni buraya yapıştırın...";
                txtQuickAnalysis.InnerTextBox.ForeColor = Color.Gray;
            };
            if (btnDictionary != null) btnDictionary.Click += (s,e) => { tabMain.SelectedTab = tabDictionary; LoadDictionaryData(); };
            if (btnHistory != null) btnHistory.Click += (s,e) => { tabMain.SelectedTab = tabHistory; LoadRealHistoryData(); };
            if (btnSettings != null) btnSettings.Click += (s,e) => tabMain.SelectedTab = tabSettings; // Wire Settings
            if (btnHelp != null) btnHelp.Click += (s,e) => tabMain.SelectedTab = tabHelp;
        }
        
        private void InitializeDashboard(TabPage tab)
        {
            tab.BackColor = ModernTheme.BackColor;
            // Use Panel for scrollable content
            var pnlDashContent = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 20, 0, 0)
            };
            
            int y = 20;
            
            // A) Metric Cards
            var flowMetrics = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(0, y),
                Width = 1000, 
                Height = 120
            };
            
            cardTotal = new ModernKpiCard { Title = "Toplam Kelime", Value = "0", ChangeText = "+0%", StartColor = Color.FromArgb(20, 40, 30), EndColor=Color.FromArgb(25, 55, 35) }; 
            cardRoots = new ModernKpiCard { Title = "Benzersiz Kökler", Value = "0", ChangeText = "+0%", StartColor = Color.FromArgb(20, 30, 50), EndColor=Color.FromArgb(25, 35, 60) };
            cardCategory = new ModernKpiCard { Title = "Baskın Kategori", Value = "-", ChangeText = "0.00", StartColor = Color.FromArgb(40, 20, 40), EndColor=Color.FromArgb(50, 25, 50) };
            
            flowMetrics.Controls.Add(cardTotal);
            flowMetrics.Controls.Add(cardRoots);
            flowMetrics.Controls.Add(cardCategory);
            
            pnlDashContent.Controls.Add(flowMetrics);
            y += 140;
            
            // B) Quick Analysis Panel
            var lblQuickTitle = new System.Windows.Forms.Label { Text = "⚡ Hızlı Kelime/Cümle Analizi", Font = ModernTheme.GetTitleFont(12f), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
            pnlDashContent.Controls.Add(lblQuickTitle);
            y += 30;
            
            var pnlQuick = new ModernPanel
            {
                 Size = new Size(800, 200),
                 Location = new Point(0, y),
                 BorderRadius = 12
            };
            
            txtQuickAnalysis = new ModernTextBox
            {
                Size = new Size(760, 100),
                Location = new Point(20, 20),
                Multiline = true
            };
            txtQuickAnalysis.InnerTextBox.Font = new Font("Consolas", 10f);
            txtQuickAnalysis.InnerTextBox.Text = "Analiz etmek veya LLM'e göndermek için metni buraya yapıştırın...";
            
             btnQuickAnalyze = CreateModernButton("▶️ Analiz Et", 120, ModernTheme.AccentGreen, new Point(660, 140));
             btnQuickAnalyze.Click += btnQuickAnalyze_Click;
             
             pnlQuick.Controls.Add(txtQuickAnalysis);
             pnlQuick.Controls.Add(btnQuickAnalyze);
             
             pnlDashContent.Controls.Add(pnlQuick);
             y += 220;
             
             // C) Charts & Activity
             var lblCharts = new System.Windows.Forms.Label { Text = "Kelime Türü Dağılımı", Font = ModernTheme.GetTitleFont(12f), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
             pnlDashContent.Controls.Add(lblCharts);
             y += 30;
             
             panelWordTypeChart = new ModernPanel
             {
                 Size = new Size(450, 250),
                 Location = new Point(0, y),
                 BorderRadius = 12
             };
             
             var pnlActivity = new ModernPanel
             {
                 Size = new Size(330, 250),
                 Location = new Point(470, y),
                 BorderRadius = 12
             };
             pnlActivity.Controls.Add(new System.Windows.Forms.Label { Text = "Son Aktiviteler", Font = ModernTheme.GetBodyFont(11f), ForeColor = Color.White, Location = new Point(15, 15), AutoSize = true });
             
             lblRecentActivity = new System.Windows.Forms.Label { 
                 Text = "• Sistem başlatıldı.", 
                 ForeColor = ModernTheme.TextSecondary, 
                 Location = new Point(15, 50), 
                 Size = new Size(300, 150),
                 AutoSize = false
             };
             pnlActivity.Controls.Add(lblRecentActivity);
             
             pnlDashContent.Controls.Add(panelWordTypeChart);
             pnlDashContent.Controls.Add(pnlActivity);

            tab.Controls.Add(pnlDashContent);
        }
        
        private async void LoadDashboardMetrics()
        {
            try
            {
                var totalWords = _dbService.GetTotalWordCount();
                var posDistribution = _dbService.GetPOSDistribution();
                var uniqueRoots = posDistribution.Values.Sum(); 
                
                var dominantPOS = posDistribution.OrderByDescending(x => x.Value).FirstOrDefault();
                
                cardTotal.Value = totalWords.ToString("N0");
                cardTotal.ChangeText = "+0%"; 
                
                cardRoots.Value = uniqueRoots.ToString("N0");
                cardRoots.ChangeText = "+0%";
                
                if (dominantPOS.Key != null)
                {
                    cardCategory.Value = dominantPOS.Key;
                    cardCategory.ChangeText = ""; 
                }
                else
                {
                     cardCategory.Value = "-";
                     cardCategory.ChangeText = "0.0";
                }
                
                CreateWordTypeDistributionChart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading metrics: {ex.Message}");
            }
        }

        private void CreateWordTypeDistributionChart()
        {
            var chartPanel = panelWordTypeChart;
            if (chartPanel == null) return;
            chartPanel.Controls.Clear();
            
            var distribution = _dbService.GetPOSDistribution();
            var total = distribution.Values.Sum();
            
            // if (total == 0) return; // Allow drawing labels even if empty
            
            int y = 20;
            var categories = new[] { "İsim", "Fiil", "Sıfat", "Zarf", "Diğer" };
            var posMap = new System.Collections.Generic.Dictionary<string, string>
            {
                {"İsim", "NOUN"},
                {"Fiil", "VERB"},
                {"Sıfat", "ADJ"},
                {"Zarf", "ADV"},
                {"Diğer", "OTHER"}
            };
            
            foreach (var category in categories)
            {
                var pos = posMap[category];
                var count = distribution.ContainsKey(pos) ? distribution[pos] : 0;
                var percentage = total > 0 ? (count * 100.0 / total) : 0;
                
                // Category label
                var label = new System.Windows.Forms.Label
                {
                    Text = category,
                    ForeColor = Color.White,
                    Location = new Point(20, y),
                    AutoSize = true,
                    Font = ModernTheme.GetBodyFont(10f)
                };
                
                // Bar (simple Panel)
                var bar = new System.Windows.Forms.Panel
                {
                    BackColor = GetCategoryColor(pos),
                    Location = new Point(100, y + 5),
                    Size = new Size((int)(percentage * 2.5), 12) // Scale
                };
                
                // Percentage label
                var percentLabel = new System.Windows.Forms.Label
                {
                    Text = $"{percentage:F1}%",
                    ForeColor = Color.LightGray,
                    Location = new Point(360, y),
                    AutoSize = true,
                    Font = ModernTheme.GetBodyFont(9f)
                };
                
                chartPanel.Controls.Add(label);
                chartPanel.Controls.Add(bar);
                chartPanel.Controls.Add(percentLabel);
                
                y += 35;
            }
        }

        private Color GetCategoryColor(string pos)
        {
            return pos switch
            {
                "NOUN" => Color.FromArgb(52, 152, 219),
                "VERB" => Color.FromArgb(46, 204, 113),
                "ADJ" => Color.FromArgb(230, 126, 34),
                "ADV" => Color.FromArgb(155, 89, 182),
                _ => Color.Gray
            };
        }

        private async Task CheckBackendStatus()
        {
             if (lblBackendStatus == null || lblBackendIndicator == null) return;
             
             try
             {
                 bool healthy = await _apiClient.CheckHealthAsync();
                 if (healthy)
                 {
                     lblBackendStatus.Text = "Çevrimiçi";
                     lblBackendStatus.ForeColor = Color.FromArgb(16, 185, 129); // Green
                     lblBackendIndicator.BackColor = Color.FromArgb(16, 185, 129);
                 }
                 else
                 {
                     lblBackendStatus.Text = "Çevrimdışı";
                     lblBackendStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red
                     lblBackendIndicator.BackColor = Color.FromArgb(239, 68, 68);
                 }
             }
             catch
             {
                 lblBackendStatus.Text = "Bağlantı Hatası";
                 lblBackendStatus.ForeColor = Color.Orange;
                 lblBackendIndicator.BackColor = Color.Orange;
             }
        }

        private void AddRecentActivity(string activity, DateTime time)
        {
             if (lblRecentActivity == null) return;
             var current = lblRecentActivity.Text;
             var lines = current.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
             if (lines.Count >= 5) lines.RemoveAt(lines.Count - 1); // Keep last 5
             
             var newLine = $"• {activity} ({time:HH:mm})";
             lines.Insert(0, newLine);
             
             lblRecentActivity.Text = string.Join("\n", lines);
        }

        private async void btnQuickAnalyze_Click(object? sender, EventArgs e)
        {
            var text = txtQuickAnalysis.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text) || text.Contains("yapıştırın..."))
            {
                 MessageBox.Show("Lütfen analiz edilecek metni girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 return;
            }

            btnQuickAnalyze.Enabled = false;
            var originalText = btnQuickAnalyze.Text;
            btnQuickAnalyze.Text = "Analiz ediliyor...";
            
            try
            {
                // Health Check
                bool healthy = await _apiClient.CheckHealthAsync();
                if (!healthy)
                {
                     MessageBox.Show("Backend veya Veritabanı servisi çevrimdışı! Lütfen servisi başlatın.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     return;
                }

                // Split text into words
                var words = text.Split(new[] { ' ', '\n', '\r', '.', ',', ';', ':', '!', '?', '"', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(w => w.Length > 1 && !char.IsDigit(w[0]))
                                .Take(100) // Limit to 100 for quick analysis to prevent UI freeze
                                .ToList();

                if (words.Count == 0)
                {
                    MessageBox.Show("Analiz edilecek geçerli kelime bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (words.Count == 1)
                {
                    // Single word analysis
                    var result = await _apiClient.AnalyzeWordAsync(words[0]);
                    
                    if (result.Success)
                    {
                        txtQuickAnalysis.Text = $"✅ Tek Kelime Analizi:\r\nKelime: {result.Word}\r\nKök: {result.Root}\r\nTür: {result.POS}\r\n\r\nFeatures: {string.Join(", ", result.Features.Select(f => f.Key + ":" + f.Value))}";
                        AddRecentActivity($"Analiz: {result.Word} -> {result.Root}", DateTime.Now);
                         _dbService.SaveAnalysis(result.Word, System.Text.Json.JsonSerializer.Serialize(result));
                         LoadDashboardMetrics();
                    }
                    else
                    {
                        txtQuickAnalysis.Text = $"❌ Hata: {result.ErrorMessage}";
                    }
                }
                else
                {
                    // Batch Analysis
                    AddNotification($"{words.Count} kelime analiz ediliyor...");
                    var results = await _apiClient.AnalyzeBatchAsync(words);
                    
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"📊 Toplu Analiz Sonucu ({results.Count} Kelime)");
                    sb.AppendLine("----------------------------------------");
                    
                    int successCount = 0;
                    var wordsToSave = new System.Collections.Generic.List<TurkishNLP.Desktop.Models.WordRoot>();

                    foreach(var res in results)
                    {
                        if (res.Success)
                        {
                            successCount++;
                            sb.AppendLine($"✓ {res.Word,-15} -> {res.Root,-10} ({res.POS})");
                            
                            // Prepare for DB
                            wordsToSave.Add(TurkishNLP.Desktop.Models.WordRootFactory.CreateWordRoot(
                                res.Word, res.POS ?? "OTHER", res.Root));
                        }
                        else
                        {
                            sb.AppendLine($"✗ {res.Word,-15} -> Hata: {res.ErrorMessage}");
                        }
                    }

                    txtQuickAnalysis.Text = sb.ToString();
                    
                    // Bulk Save to DB
                    if (wordsToSave.Count > 0)
                    {
                        _dbService.AddWords(wordsToSave);
                        AddRecentActivity($"Toplu Analiz: {successCount}/{words.Count} başarılı", DateTime.Now);
                        LoadDashboardMetrics();
                        AddNotification($"{successCount} kelime veritabanına eklendi.");
                    }
                }
            }
            catch (Exception ex)
            {
                txtQuickAnalysis.Text = $"❌ Sistem Hatası: {ex.Message}";
            }
            finally
            {
                btnQuickAnalyze.Enabled = true;
                btnQuickAnalyze.Text = originalText;
            }
        }

        // Dictionary Logic
        private System.Windows.Forms.DataGridView dgvDictionary;

        private void InitializeDictionaryTab(TabPage tab)
        {
            tab.BackColor = ModernTheme.BackColor;
            tab.Padding = new Padding(20);

            var lblTitle = new System.Windows.Forms.Label
            {
                Text = "📚 Sözlük Veritabanı",
                Font = ModernTheme.GetTitleFont(14f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            tab.Controls.Add(lblTitle);

            var btnRefresh = CreateModernButton("🔄 Yenile", 100, ModernTheme.CardColor, new Point(tab.Width - 140, 20));
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (s, e) => LoadDictionaryData();
            tab.Controls.Add(btnRefresh);

            dgvDictionary = new System.Windows.Forms.DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(tab.Width - 40, tab.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = ModernTheme.CardColor,
                BorderStyle = BorderStyle.None,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Style Grid
            dgvDictionary.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgvDictionary.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDictionary.ColumnHeadersDefaultCellStyle.Font = ModernTheme.GetBodyFont(10f);
            dgvDictionary.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            
            dgvDictionary.DefaultCellStyle.BackColor = ModernTheme.CardColor;
            dgvDictionary.DefaultCellStyle.ForeColor = Color.LightGray;
            dgvDictionary.DefaultCellStyle.SelectionBackColor = ModernTheme.AccentGreen;
            dgvDictionary.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDictionary.DefaultCellStyle.Font = ModernTheme.GetBodyFont(9f);
            dgvDictionary.GridColor = Color.FromArgb(60, 60, 60);

            // Context Menu for Deletion
            var ctxMenu = new ContextMenuStrip();
            var itemDelete = new ToolStripMenuItem("🗑️ Sil");
            itemDelete.Click += (s, e) => DeleteSelectedWord();
            ctxMenu.Items.Add(itemDelete);
            dgvDictionary.ContextMenuStrip = ctxMenu;
            
            // Allow Delete key
            dgvDictionary.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Delete) DeleteSelectedWord();
            };

            tab.Controls.Add(dgvDictionary);
        }

        private void LoadDictionaryData()
        {
            if (dgvDictionary == null) return;
            
            try
            {
                var words = _dbService.GetAllWords();
                
                var viewData = words.Select(w => new 
                {
                    Kelime = w.Text,
                    Kök = w.Root,
                    Tür = w.POS,
                    Tarih = w.CreatedAt.ToString("g")
                }).ToList();
                
                dgvDictionary.DataSource = viewData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sözlük yüklenirken hata: " + ex.Message);
            }
        }

        // History Timeline Logic
        private FlowLayoutPanel pnlHistoryTimeline;

        private void InitializeRealHistoryTab(TabPage tab)
        {
            tab.BackColor = ModernTheme.BackColor;
            tab.Padding = new Padding(20);

            var lblTitle = new System.Windows.Forms.Label
            {
                Text = "🕑 İşlem Geçmişi",
                Font = ModernTheme.GetTitleFont(14f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            tab.Controls.Add(lblTitle);
            
            var btnRefresh = CreateModernButton("🔄 Yenile", 100, ModernTheme.CardColor, new Point(tab.Width - 140, 20));
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (s, e) => LoadRealHistoryData();
            tab.Controls.Add(btnRefresh);

            pnlHistoryTimeline = new FlowLayoutPanel
            {
                Location = new Point(20, 70),
                Size = new Size(tab.Width - 40, tab.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            
            tab.Controls.Add(pnlHistoryTimeline);
        }

        private void LoadRealHistoryData()
        {
            if (pnlHistoryTimeline == null) return;
            pnlHistoryTimeline.Controls.Clear();
            
            try
            {
                var history = _dbService.GetRecentAnalyses(50);
                
                foreach (var item in history)
                {
                    var card = new ModernPanel
                    {
                        Size = new Size(pnlHistoryTimeline.Width - 30, 80),
                        BackColor = Color.FromArgb(40, 40, 40),
                        BorderColor = Color.FromArgb(60, 60, 60),
                        BorderRadius = 8,
                        Margin = new Padding(0, 0, 0, 10)
                    };
                    
                    var lblTime = new System.Windows.Forms.Label
                    {
                        Text = item.Date.ToString("g"),
                        ForeColor = Color.Gray,
                        Font = ModernTheme.GetBodyFont(8f),
                        Location = new Point(card.Width - 130, 10),
                        AutoSize = true,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };
                    
                    var lblInput = new System.Windows.Forms.Label
                    {
                        Text = item.Input.Length > 50 ? item.Input.Substring(0, 47) + "..." : item.Input,
                        ForeColor = Color.White,
                        Font = ModernTheme.GetTitleFont(10f),
                        Location = new Point(15, 10),
                        AutoSize = true
                    };
                    
                    var lblResult = new System.Windows.Forms.Label
                    {
                        Text = item.Result.Length > 100 ? item.Result.Substring(0, 97) + "..." : item.Result,
                        ForeColor = ModernTheme.TextSecondary,
                        Font = new Font("Consolas", 9f),
                        Location = new Point(15, 35),
                        Size = new Size(card.Width - 30, 35),
                        AutoEllipsis = true
                    };
                    
                    card.Controls.Add(lblTime);
                    card.Controls.Add(lblInput);
                    card.Controls.Add(lblResult);
                    
                    pnlHistoryTimeline.Controls.Add(card);
                }
                
                if (history.Count == 0)
                {
                    var lblEmpty = new System.Windows.Forms.Label { Text = "Henüz kayıtlı işlem yok.", ForeColor = Color.Gray, AutoSize = true };
                    pnlHistoryTimeline.Controls.Add(lblEmpty);
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Geçmiş yüklenirken hata: " + ex.Message);
            }
        }

        // Helpers
        private void AddSidebarSectionHeader(string text, int x, ref int y)
        {
            var lbl = new System.Windows.Forms.Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = ModernTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(x, y)
            };
            pnlSidebar.Controls.Add(lbl);
            y += 30;
        }

        private ModernButton CreateModernButton(string text, int w, Color color, Point loc)
        {
             var btn = new ModernButton
            {
                Text = text,
                PrimaryColor = color,
                Width = w,
                Height = 40,
                Location = loc,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10,0,0,0)
            };
            return btn;
        }
        // Drag & Drop & Upload Logic
        private void PnlUpload_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                if (sender is ModernPanel pnl)
                {
                    pnl.BorderColor = ModernTheme.AccentGreen;
                }
            }
        }

        private void PnlUpload_DragLeave(object? sender, EventArgs e)
        {
             if (sender is ModernPanel pnl)
            {
                pnl.BorderColor = Color.FromArgb(60, 60, 60); // Reset to original
            }
        }

        private async void PnlUpload_DragDrop(object? sender, DragEventArgs e)
        {
            if (sender is ModernPanel pnl)
            {
                pnl.BorderColor = Color.FromArgb(60, 60, 60);
            }

            if (e.Data != null && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                foreach (var file in files) // Handle multiple dropped files
                {
                    await ProcessFileAsync(file);
                }
            }
        }

        private async void PnlUpload_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files|*.txt;*.csv|All Files|*.*";
                ofd.Title = "Analiz için dosya seçin";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    await ProcessFileAsync(ofd.FileName);
                }
            }
        }

        private async Task ProcessFileAsync(string filePath)
        {
            try
            {
                var filename = System.IO.Path.GetFileName(filePath);
                AddRecentActivity($"Dosya Yüklendi: {filename}", DateTime.Now);
                
                // Read file
                var lines = await System.IO.File.ReadAllLinesAsync(filePath);
                var words = new System.Collections.Generic.List<string>();
                
                foreach (var line in lines)
                {
                    var split = line.Split(new[] { ' ', ',', '\t', ';', '"' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var w in split)
                    {
                         if (!string.IsNullOrWhiteSpace(w) && w.Length > 1 && !char.IsDigit(w[0]))
                            words.Add(w.Trim());
                    }
                }
                
                if (words.Count == 0)
                {
                    MessageBox.Show("Dosyada işlenebilecek kelime bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var batch = words.Take(50).ToList(); // Limit for demo
                
                AddRecentActivity($"Analiz Başladı: {batch.Count} kelime", DateTime.Now);
                
                // Call API
                var results = await _apiClient.AnalyzeBatchAsync(batch);
                
                // Save to Database
                var wordsToSave = new System.Collections.Generic.List<TurkishNLP.Desktop.Models.WordRoot>();
                int successCount = 0;

                foreach (var result in results)
                {
                    if (result.Success)
                    {
                        successCount++;
                        // Create WordRoot model
                        var wordRoot = TurkishNLP.Desktop.Models.WordRootFactory.CreateWordRoot(
                            result.Word, 
                            result.POS ?? "OTHER", 
                            result.Root
                        );
                        wordsToSave.Add(wordRoot);
                    }
                }

                if (wordsToSave.Count > 0)
                {
                    _dbService.AddWords(wordsToSave);
                }
                
                // Save Batch Log to History
                _dbService.SaveAnalysis($"Dosya Yükleme: {filename}", 
                    $"{{\"total\": {words.Count}, \"processed\": {batch.Count}, \"success\": {successCount}, \"saved\": {wordsToSave.Count}}}");
                
                MessageBox.Show($"Analiz Tamamlandı!\n\nToplam: {words.Count} (İlk 50 işlendi)\nBaşarılı: {successCount}\nVeritabanına Kaydedilen: {wordsToSave.Count}\n\nDetaylar aktivite günlüğüne eklendi.", "İşlem Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                AddRecentActivity($"Batch Tamamlandı: {successCount}/{batch.Count} başarılı", DateTime.Now);
                
                // Refresh Dashboard to reflect new data
                LoadDashboardMetrics();
                AddNotification($"Dosya işleme tamamlandı: {filename}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya işleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Settings Tab Logic
        private void InitializeSettingsTab(TabPage tab)
        {
            tab.BackColor = ModernTheme.BackColor;
            tab.Padding = new Padding(30);

            int y = 30;

            // Header
            var lblTitle = new System.Windows.Forms.Label
            {
                Text = "⚙️ Ayarlar",
                Font = ModernTheme.GetTitleFont(16f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(30, y)
            };
            tab.Controls.Add(lblTitle);
            y += 50;

            // Section 1: Connection
            var lblConn = new System.Windows.Forms.Label { Text = "🔗 Bağlantı Ayarları", Font = ModernTheme.GetTitleFont(11f), ForeColor = ModernTheme.AccentBlue, AutoSize = true, Location = new Point(30, y) };
            tab.Controls.Add(lblConn);
            y += 30;

            var pnlConn = new ModernPanel { Size = new Size(500, 80), Location = new Point(30, y), BackColor = ModernTheme.CardColor, BorderColor = ModernTheme.BorderColor, BorderRadius = 8 };
            
            var lblUrl = new System.Windows.Forms.Label { Text = "Backend URL:", ForeColor = ModernTheme.TextSecondary, Location = new Point(15, 15), AutoSize = true };
            var txtUrl = new ModernTextBox { Location = new Point(15, 35), Size = new Size(400, 30) };
            txtUrl.InnerTextBox.Text = "http://localhost:8000"; // Readonly for now
            txtUrl.InnerTextBox.ReadOnly = true; 
            
            pnlConn.Controls.Add(lblUrl);
            pnlConn.Controls.Add(txtUrl);
            tab.Controls.Add(pnlConn);
            y += 100;

            // Section 2: Data Management
            var lblData = new System.Windows.Forms.Label { Text = "💾 Veri Yönetimi", Font = ModernTheme.GetTitleFont(11f), ForeColor = ModernTheme.AccentGreen, AutoSize = true, Location = new Point(30, y) };
            tab.Controls.Add(lblData);
            y += 30;

            var pnlData = new ModernPanel { Size = new Size(500, 80), Location = new Point(30, y), BackColor = ModernTheme.CardColor, BorderColor = ModernTheme.BorderColor, BorderRadius = 8 };
            
            var btnBackup = CreateModernButton("📂 Verileri Yedekle (JSON)", 220, ModernTheme.AccentBlue, new Point(15, 20));
            btnBackup.Click += (s, e) => ExportData();

            var btnClearHistory = CreateModernButton("🗑️ Geçmişi Temizle", 220, ModernTheme.StatusOffline, new Point(250, 20));
            btnClearHistory.Click += (s, e) => 
            {
                if (MessageBox.Show("Tüm analiz geçmişi silinecek. Emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Basic placeholder - real implementation requires adding ClearHistory to DatabaseService
                    // _dbService.ClearHistory(); // TODO: Implement
                    MessageBox.Show("Geçmiş temizlendi (Simüle)", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AddNotification("Geçmiş temizlendi.");
                }
            };
            
            pnlData.Controls.Add(btnBackup);
            pnlData.Controls.Add(btnClearHistory);
            tab.Controls.Add(pnlData);
            y += 100;

            // Section 3: About
            var lblAbout = new System.Windows.Forms.Label { Text = "ℹ️ Hakkında", Font = ModernTheme.GetTitleFont(11f), ForeColor = ModernTheme.TextPrimary, AutoSize = true, Location = new Point(30, y) };
            tab.Controls.Add(lblAbout);
            y += 30;
            
            var lblVersion = new System.Windows.Forms.Label 
            { 
                Text = "Turkish NLP Workbench v1.0.2\nDeveloped by Atakan Yılmaz\n© 2026", 
                ForeColor = ModernTheme.TextSecondary, 
                AutoSize = true, 
                Location = new Point(30, y) 
            };
            tab.Controls.Add(lblVersion);
        }

        // --- New Features Logic ---

        private void InitializeHelpTab(TabPage tab)
        {
            tab.BackColor = ModernTheme.BackColor;
            tab.Padding = new Padding(40);
            
            var pnlContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            tab.Controls.Add(pnlContainer);

            // Title
            var lblTitle = new System.Windows.Forms.Label
            {
                Text = "Nasıl Kullanılır?",
                Font = ModernTheme.GetTitleFont(24f),
                ForeColor = ModernTheme.AccentGreen,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30)
            };
            pnlContainer.Controls.Add(lblTitle);

            // Section 1: Data Upload
            AddHelpSection(pnlContainer, "1. Veri Yükleme", 
                "Sağ paneldeki 'Veri Yükleme' alanına metin dosyalarınızı (.txt) sürükleyip bırakın veya tıklayarak seçin. " +
                "Yüklenen dosya otomatik olarak sıraya alınır ve analiz edilir.");

            // Section 2: Analysis
            AddHelpSection(pnlContainer, "2. Analiz Takibi", 
                "Ana ekrandaki (Genel Bakış) grafiklerden kelime türü dağılımını (İsim, Fiil, vb.) izleyebilirsiniz. " +
                "Analiz tamamlandığında bildirim alırsınız.");

            // Section 3: Dictionary
            AddHelpSection(pnlContainer, "3. Sözlük Yönetimi", 
                "'Sözlük' sekmesinden analiz edilen tüm benzersiz kelimeleri inceleyebilirsiniz. " +
                "İstemediğiniz kelimeleri sağ tıklayarak veya 'Delete' tuşu ile silebilirsiniz.");
            
            // Section 4: History
            AddHelpSection(pnlContainer, "4. Geçmiş", 
                "'Geçmiş Analizler' sekmesinde daha önce yapılan tüm işlem kayıtlarını görebilirsiniz.");
        }

        private void AddHelpSection(Control parent, string title, string content)
        {
            var lblHeader = new System.Windows.Forms.Label
            {
                Text = title,
                Font = ModernTheme.GetTitleFont(14f),
                ForeColor = ModernTheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 10)
            };
            parent.Controls.Add(lblHeader);

            var lblContent = new System.Windows.Forms.Label
            {
                Text = content,
                Font = ModernTheme.GetBodyFont(10f),
                ForeColor = ModernTheme.TextSecondary,
                AutoSize = true,
                MaximumSize = new Size(700, 0), // Wrap text
                Margin = new Padding(10, 0, 0, 10)
            };
            parent.Controls.Add(lblContent);
        }

        private void AddNotification(string message)
        {
            _notifications.Add($"[{DateTime.Now:HH:mm}] {message}");
            // Optional: Play sound or flash UI
        }

        private void ShowNotifications()
        {
            if (_notifications.Count == 0)
            {
                MessageBox.Show("Yeni bildiriminiz yok.", "Bildirimler", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var text = string.Join("\n\n", _notifications.OrderByDescending(x => x));
            MessageBox.Show(text, "📢 Bildirimler", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Clear notifications after showing? Or keep them? User didn't specify. Keeping them is safer.
            // _notifications.Clear(); 
        }

        private void ExportData()
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "JSON Files|*.json|All Files|*.*";
                    sfd.FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmm}.json";
                    
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var json = _dbService.ExportToJson();
                        System.IO.File.WriteAllText(sfd.FileName, json);
                        MessageBox.Show("Veriler başarıyla dışa aktarıldı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AddNotification($"Veri dışa aktarıldı: {System.IO.Path.GetFileName(sfd.FileName)}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedWord()
        {
            if (dgvDictionary == null || dgvDictionary.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silinecek bir kelime seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvDictionary.SelectedRows[0];
            // View data was: { Kelime = w.Text, ... }
            // We need to access by column name or property
            // DataGridView with anonymous list binding: use Cells["Kelime"].Value
            
            var text = row.Cells["Kelime"].Value?.ToString();
            
            if (string.IsNullOrEmpty(text)) return;

            if (MessageBox.Show($"'{text}' kelimesini silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_dbService.DeleteWordByText(text))
                {
                    LoadDictionaryData(); // Refresh grid
                    LoadDashboardMetrics(); // Refresh stats
                    MessageBox.Show("Kelime silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AddNotification($"Kelime silindi: {text}");
                }
                else
                {
                    MessageBox.Show("Silme işlemi başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task HandleUrlFetchAsync()
        {
            string url = "";
            using (var form = new Form())
            {
                form.Text = "URL Girin";
                form.ClientSize = new Size(400, 120);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = ModernTheme.BackColor;

                var lbl = new System.Windows.Forms.Label { Text = "Makale URL'si:", ForeColor = ModernTheme.TextPrimary, Location = new Point(10, 10), AutoSize = true, Font = ModernTheme.GetBodyFont(10f) };
                var txtUrl = new System.Windows.Forms.TextBox { Location = new Point(10, 35), Size = new Size(360, 25), BackColor = ModernTheme.CardColor, ForeColor = ModernTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle, Font = ModernTheme.GetBodyFont(10f) };
                var btnOk = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Location = new Point(200, 75), BackColor = ModernTheme.AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnCancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(290, 75), BackColor = ModernTheme.CardColor, ForeColor = ModernTheme.TextPrimary, FlatStyle = FlatStyle.Flat };
                
                form.Controls.AddRange(new Control[] { lbl, txtUrl, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    url = txtUrl.Text.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(url)) return;

            try 
            {
                btnDataUrl.Enabled = false;
                btnDataUrl.Text = "⏳ Çekiliyor...";
                
                var result = await _apiClient.FetchArticleAsync(url);
                
                if (result.Success)
                {
                    tabMain.SelectedTab = tabMain.TabPages[0]; // Dashboard
                    if (txtQuickAnalysis != null)
                    {
                        txtQuickAnalysis.InnerTextBox.Text = result.Text;
                    }
                    AddNotification($"Makale çekildi: {result.Title}");
                    MessageBox.Show($"Başarılı!\nBaşlık: {result.Title}\nKelime: {result.WordCount}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Hata: {result.Error}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AddNotification("URL çekme başarısız.");
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Beklenmedik hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDataUrl.Enabled = true;
                btnDataUrl.Text = "🔗 URL'den Veri Çek";
            }
        }
    }
}


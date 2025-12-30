using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using TurkMorph.Models;
using TurkMorph.Services;
using TurkMorph.Services.DTOs;
using TurkMorph.Database;
using TurkMorph.Database.Repositories;

namespace TurkMorph.Forms
{
    /// <summary>
    /// Ana Uygulama Formu - DevExpress RibbonForm
    /// Modern ve canlı arayüz ile NLP analiz işlemleri.
    /// </summary>
    public class MainForm : RibbonForm
    {
        #region Fields

        private readonly NlpApiService _nlpService;
        private readonly TurkMorphContext _dbContext;
        private readonly WordRootRepository _wordRepository;
        private List<WordRoot> _analyzedWords;

        // DevExpress Controls
        private RibbonControl ribbonControl;
        private RibbonPage ribbonPageMain;
        private RibbonPageGroup ribbonGroupAnalysis;
        private RibbonPageGroup ribbonGroupDatabase;
        private BarButtonItem btnAnalyze;
        private BarButtonItem btnClean;
        private BarButtonItem btnSaveToDb;
        private BarButtonItem btnClearGrid;
        private BarStaticItem lblStatus;
        private MemoEdit txtInput;
        private GridControl gridControl;
        private GridView gridView;
        private Panel panelStats;
        private Panel panelHeader;

        // Stats Labels
        private Dictionary<string, Label> statLabels;

        // Renk Paleti - Canlı Renkler
        private static readonly Color PrimaryColor = Color.FromArgb(99, 102, 241);      // Indigo
        private static readonly Color SecondaryColor = Color.FromArgb(139, 92, 246);   // Purple
        private static readonly Color AccentColor = Color.FromArgb(236, 72, 153);       // Pink
        private static readonly Color SuccessColor = Color.FromArgb(34, 197, 94);       // Green
        private static readonly Color WarningColor = Color.FromArgb(251, 146, 60);      // Orange
        private static readonly Color InfoColor = Color.FromArgb(59, 130, 246);         // Blue
        private static readonly Color DarkBg = Color.FromArgb(30, 30, 46);              // Dark background
        private static readonly Color CardBg = Color.FromArgb(45, 45, 65);              // Card background

        #endregion

        #region Constructor

        public MainForm()
        {
            // Önce koleksiyonları başlat (InitializeControls bunları kullanıyor)
            _analyzedWords = new List<WordRoot>();
            statLabels = new Dictionary<string, Label>();

            // Modern tema
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");

            InitializeControls();

            // Servisler
            _nlpService = new NlpApiService();
            _dbContext = new TurkMorphContext();
            _wordRepository = new WordRootRepository(_dbContext);

            // Form yüklendiğinde
            this.Load += MainForm_Load;
        }

        #endregion

        #region Form Initialization

        private void InitializeControls()
        {
            // Form ayarları
            this.Text = "🔮 TurkMorph - Türkçe Morfolojik Analiz";
            this.Size = new Size(1300, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.BackColor = DarkBg;

            // === RIBBON CONTROL ===
            ribbonControl = new RibbonControl();
            ribbonControl.ColorScheme = RibbonControlColorScheme.DarkBlue;
            this.Controls.Add(ribbonControl);

            // Ribbon Page
            ribbonPageMain = new RibbonPage("🏠 Ana Sayfa");
            ribbonControl.Pages.Add(ribbonPageMain);

            // === RIBBON GROUPS ===
            ribbonGroupAnalysis = new RibbonPageGroup("📊 Analiz");
            ribbonPageMain.Groups.Add(ribbonGroupAnalysis);

            ribbonGroupDatabase = new RibbonPageGroup("💾 Veritabanı");
            ribbonPageMain.Groups.Add(ribbonGroupDatabase);

            // === BUTTONS ===
            btnAnalyze = new BarButtonItem(ribbonControl.Manager, "🔍 Analiz Et");
            btnAnalyze.ItemClick += BtnAnalyze_ItemClick;
            ribbonGroupAnalysis.ItemLinks.Add(btnAnalyze);

            btnClean = new BarButtonItem(ribbonControl.Manager, "🧹 Temizle");
            btnClean.ItemClick += BtnClean_ItemClick;
            ribbonGroupAnalysis.ItemLinks.Add(btnClean);

            btnClearGrid = new BarButtonItem(ribbonControl.Manager, "🗑️ Listeyi Sil");
            btnClearGrid.ItemClick += BtnClearGrid_ItemClick;
            ribbonGroupAnalysis.ItemLinks.Add(btnClearGrid);

            btnSaveToDb = new BarButtonItem(ribbonControl.Manager, "💾 Kaydet");
            btnSaveToDb.ItemClick += BtnSaveToDb_ItemClick;
            ribbonGroupDatabase.ItemLinks.Add(btnSaveToDb);

            // Status Label
            lblStatus = new BarStaticItem();
            lblStatus.Caption = "⏳ Hazırlanıyor...";
            ribbonControl.Items.Add(lblStatus);

            // === MAIN LAYOUT ===
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DarkBg,
                Padding = new Padding(15)
            };
            this.Controls.Add(mainPanel);

            // === HEADER PANEL (Gradient) ===
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };
            panelHeader.Paint += PanelHeader_Paint;
            mainPanel.Controls.Add(panelHeader);

            // Header Label
            var lblTitle = new Label
            {
                Text = "🔮 Türkçe Morfolojik Analiz Sistemi",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            panelHeader.Controls.Add(lblTitle);

            // === INPUT PANEL ===
            var inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = CardBg,
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 0, 10)
            };
            inputPanel.Paint += RoundedPanel_Paint;
            mainPanel.Controls.Add(inputPanel);

            txtInput = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { 
                    NullValuePrompt = "📝 Analiz edilecek Türkçe metni buraya yazın...",
                    Appearance = { 
                        BackColor = Color.FromArgb(55, 55, 80),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 13),
                        Options = { UseBackColor = true, UseForeColor = true, UseFont = true }
                    }
                }
            };
            txtInput.Properties.AppearanceFocused.BackColor = Color.FromArgb(65, 65, 95);
            inputPanel.Controls.Add(txtInput);

            // === STATS PANEL ===
            panelStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
            };
            mainPanel.Controls.Add(panelStats);

            CreateStatCards();

            // === GRID PANEL ===
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Padding = new Padding(10)
            };
            gridPanel.Paint += RoundedPanel_Paint;
            mainPanel.Controls.Add(gridPanel);

            gridControl = new GridControl
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg
            };
            gridView = new GridView(gridControl);
            gridControl.MainView = gridView;
            gridPanel.Controls.Add(gridControl);

            // Grid ayarları
            gridView.OptionsView.ShowGroupPanel = true;
            gridView.OptionsView.ColumnAutoWidth = true;
            gridView.OptionsSelection.MultiSelect = true;
            gridView.Appearance.Row.BackColor = Color.FromArgb(50, 50, 70);
            gridView.Appearance.Row.ForeColor = Color.White;
            gridView.Appearance.EvenRow.BackColor = Color.FromArgb(55, 55, 75);
            gridView.Appearance.OddRow.BackColor = Color.FromArgb(50, 50, 70);
            gridView.Appearance.GroupRow.BackColor = PrimaryColor;
            gridView.Appearance.GroupRow.ForeColor = Color.White;
            gridView.Appearance.FocusedRow.BackColor = SecondaryColor;
            gridView.OptionsView.EnableAppearanceEvenRow = true;
            gridView.OptionsView.EnableAppearanceOddRow = true;

            // Z-Order düzeltme
            mainPanel.Controls.SetChildIndex(gridPanel, 0);
            mainPanel.Controls.SetChildIndex(panelStats, 1);
            mainPanel.Controls.SetChildIndex(inputPanel, 2);
            mainPanel.Controls.SetChildIndex(panelHeader, 3);
        }

        private void CreateStatCards()
        {
            var statData = new[]
            {
                ("Toplam", "📊", PrimaryColor),
                ("Geçerli", "✅", SuccessColor),
                ("İsim", "📝", InfoColor),
                ("Fiil", "🏃", AccentColor),
                ("Sıfat", "🎨", WarningColor),
                ("Zarf", "⚡", SecondaryColor),
                ("Diğer", "📦", Color.FromArgb(100, 100, 120))
            };

            int cardWidth = 150;
            int spacing = 10;
            int startX = 10;

            foreach (var (name, emoji, color) in statData)
            {
                var card = new Panel
                {
                    Size = new Size(cardWidth, 70),
                    Location = new Point(startX, 10),
                    BackColor = color
                };
                card.Paint += (s, e) => {
                    using (var path = GetRoundedRectPath(card.ClientRectangle, 12))
                    using (var brush = new LinearGradientBrush(
                        card.ClientRectangle,
                        color,
                        ControlPaint.Dark(color, 0.2f),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                };

                var lblEmoji = new Label
                {
                    Text = emoji,
                    Font = new Font("Segoe UI Emoji", 16),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblEmoji);

                var lblName = new Label
                {
                    Text = name,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(220, 220, 220),
                    Location = new Point(40, 8),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblName);

                var lblValue = new Label
                {
                    Text = "0",
                    Font = new Font("Segoe UI", 18, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(40, 30),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblValue);
                statLabels[name] = lblValue;

                panelStats.Controls.Add(card);
                startX += cardWidth + spacing;
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void PanelHeader_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            using (var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                PrimaryColor,
                SecondaryColor,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);
            }
        }

        private void RoundedPanel_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            using (var path = GetRoundedRectPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 10))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(CardBg))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
        }

        #endregion

        #region Event Handlers

        private async void MainForm_Load(object sender, EventArgs e)
        {
            lblStatus.Caption = "🔄 API kontrol ediliyor...";
            var isHealthy = await _nlpService.IsHealthyAsync();

            if (isHealthy)
            {
                lblStatus.Caption = "✅ API Hazır - Analiz yapabilirsiniz!";
            }
            else
            {
                lblStatus.Caption = "⚠️ API Bağlantısı Yok";
                XtraMessageBox.Show(
                    "Python NLP API'ye bağlanılamadı.\n\nLütfen önce terminal'den şu komutu çalıştırın:\npython backend/main.py",
                    "🔌 API Bağlantısı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            UpdateDatabaseStats();
        }

        private async void BtnAnalyze_ItemClick(object sender, ItemClickEventArgs e)
        {
            string inputText = txtInput.Text?.Trim();

            if (string.IsNullOrEmpty(inputText))
            {
                XtraMessageBox.Show("📝 Lütfen analiz edilecek bir metin girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.Enabled = false;
                lblStatus.Caption = "🔄 Analiz yapılıyor...";

                var results = await _nlpService.AnalyzeTextAsync(inputText);
                _analyzedWords = WordFactory.CreateMany(results);

                gridControl.DataSource = _analyzedWords;
                SetupGridColumns();

                if (gridView.Columns["GetWordType"] != null)
                {
                    gridView.Columns["GetWordType"].GroupIndex = 0;
                    gridView.ExpandAllGroups();
                }

                UpdateStats();
                lblStatus.Caption = $"✅ {_analyzedWords.Count} kelime analiz edildi!";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"❌ Analiz hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Caption = "❌ Analiz hatası";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.Enabled = true;
            }
        }

        private async void BtnClean_ItemClick(object sender, ItemClickEventArgs e)
        {
            string inputText = txtInput.Text?.Trim();
            if (string.IsNullOrEmpty(inputText)) return;

            try
            {
                lblStatus.Caption = "🧹 Temizleniyor...";
                var cleanedText = await _nlpService.CleanTextAsync(inputText);
                txtInput.Text = cleanedText;
                lblStatus.Caption = "✅ Metin temizlendi";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"❌ Temizleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSaveToDb_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_analyzedWords == null || _analyzedWords.Count == 0)
            {
                XtraMessageBox.Show("📝 Kaydedilecek kelime yok. Önce analiz yapın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.Enabled = false;
                lblStatus.Caption = "💾 Veritabanına kaydediliyor...";

                int savedCount = await _wordRepository.InsertManyAsync(_analyzedWords);
                lblStatus.Caption = $"✅ {savedCount} kelime kaydedildi!";
                UpdateDatabaseStats();

                XtraMessageBox.Show($"🎉 {savedCount} kelime başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"❌ Kaydetme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.Enabled = true;
            }
        }

        private void BtnClearGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            _analyzedWords.Clear();
            gridControl.DataSource = null;
            UpdateStats();
            lblStatus.Caption = "🗑️ Liste temizlendi";
        }

        #endregion

        #region Helper Methods

        private void SetupGridColumns()
        {
            gridView.Columns.Clear();
            gridControl.DataSource = _analyzedWords;

            if (gridView.Columns["Id"] != null)
                gridView.Columns["Id"].Visible = false;

            if (gridView.Columns["Text"] != null)
            {
                gridView.Columns["Text"].Caption = "📝 Kelime";
                gridView.Columns["Text"].VisibleIndex = 0;
            }

            if (gridView.Columns["Root"] != null)
            {
                gridView.Columns["Root"].Caption = "🌱 Kök";
                gridView.Columns["Root"].VisibleIndex = 1;
            }

            if (gridView.Columns["Features"] != null)
            {
                gridView.Columns["Features"].Caption = "⚙️ Özellikler";
                gridView.Columns["Features"].VisibleIndex = 2;
            }

            if (gridView.Columns["IsValid"] != null)
            {
                gridView.Columns["IsValid"].Caption = "✅ Geçerli";
                gridView.Columns["IsValid"].VisibleIndex = 3;
            }
        }

        private void UpdateStats()
        {
            if (_analyzedWords == null || statLabels == null || statLabels.Count == 0)
                return;

            int total = _analyzedWords.Count;
            int valid = _analyzedWords.FindAll(w => w.IsValid).Count;
            int nouns = _analyzedWords.FindAll(w => w is NounRoot).Count;
            int verbs = _analyzedWords.FindAll(w => w is VerbRoot).Count;
            int adjs = _analyzedWords.FindAll(w => w is AdjectiveRoot).Count;
            int advs = _analyzedWords.FindAll(w => w is AdverbRoot).Count;
            int other = total - nouns - verbs - adjs - advs;

            if (statLabels.ContainsKey("Toplam")) statLabels["Toplam"].Text = total.ToString();
            if (statLabels.ContainsKey("Geçerli")) statLabels["Geçerli"].Text = valid.ToString();
            if (statLabels.ContainsKey("İsim")) statLabels["İsim"].Text = nouns.ToString();
            if (statLabels.ContainsKey("Fiil")) statLabels["Fiil"].Text = verbs.ToString();
            if (statLabels.ContainsKey("Sıfat")) statLabels["Sıfat"].Text = adjs.ToString();
            if (statLabels.ContainsKey("Zarf")) statLabels["Zarf"].Text = advs.ToString();
            if (statLabels.ContainsKey("Diğer")) statLabels["Diğer"].Text = other.ToString();
        }

        private void UpdateDatabaseStats()
        {
            try
            {
                int dbCount = _dbContext.GetWordCount();
                this.Text = $"🔮 TurkMorph - Türkçe Morfolojik Analiz | 💾 DB: {dbCount} kelime";
            }
            catch { }
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nlpService?.Dispose();
                _dbContext?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}

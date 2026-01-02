namespace TurkishNLP.Desktop.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // DevExpress Tab Control
            this.tabMain = new DevExpress.XtraTab.XtraTabControl();
            this.tabDashboard = new DevExpress.XtraTab.XtraTabPage();
            this.tabAnalysis = new DevExpress.XtraTab.XtraTabPage();
            this.tabBatch = new DevExpress.XtraTab.XtraTabPage();
            this.tabDatabase = new DevExpress.XtraTab.XtraTabPage();

            // Dashboard Controls (DevExpress Labels)
            this.lblNoun = new DevExpress.XtraEditors.LabelControl();
            this.lblVerb = new DevExpress.XtraEditors.LabelControl();
            this.lblAdj = new DevExpress.XtraEditors.LabelControl();
            this.lblAdv = new DevExpress.XtraEditors.LabelControl();
            this.lblPron = new DevExpress.XtraEditors.LabelControl();
            this.lblConj = new DevExpress.XtraEditors.LabelControl();
            this.lblAdp = new DevExpress.XtraEditors.LabelControl();
            this.lblDet = new DevExpress.XtraEditors.LabelControl();
            this.lblNum = new DevExpress.XtraEditors.LabelControl();
            this.lblTotal = new DevExpress.XtraEditors.LabelControl();
            this.btnRefresh = new DevExpress.XtraEditors.SimpleButton();

            // Analysis Controls
            this.txtWord = new DevExpress.XtraEditors.TextEdit();
            this.btnAnalyze = new DevExpress.XtraEditors.SimpleButton();
            this.memoResult = new DevExpress.XtraEditors.MemoEdit();
            this.btnSaveWord = new DevExpress.XtraEditors.SimpleButton();

            // Batch Controls
            this.btnSelectFile = new DevExpress.XtraEditors.SimpleButton();
            this.progressBatch = new DevExpress.XtraEditors.ProgressBarControl();
            this.lblProgress = new DevExpress.XtraEditors.LabelControl();
            this.gridBatch = new DevExpress.XtraGrid.GridControl();
            this.gridViewBatch = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnSaveBatch = new DevExpress.XtraEditors.SimpleButton();

            // Database Controls
            this.cmbPosFilter = new DevExpress.XtraEditors.ComboBoxEdit();
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.lblDbCount = new DevExpress.XtraEditors.LabelControl();
            this.gridDatabase = new DevExpress.XtraGrid.GridControl();
            this.gridViewDatabase = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnExport = new DevExpress.XtraEditors.SimpleButton();
            this.btnDelete = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.tabMain)).BeginInit();
            this.tabMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtWord.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoResult.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBatch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPosFilter.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridDatabase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewDatabase)).BeginInit();
            this.SuspendLayout();

            // ==================== TAB CONTROL ====================
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedTabPage = this.tabDashboard;
            this.tabMain.Size = new System.Drawing.Size(1100, 700);
            this.tabMain.TabIndex = 0;
            this.tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
                this.tabDashboard, this.tabAnalysis, this.tabBatch, this.tabDatabase
            });

            // ==================== DASHBOARD TAB ====================
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Text = "📊 Dashboard";
            this.tabDashboard.Size = new System.Drawing.Size(1094, 669);

            // POS Labels setup
            var posLabels = new DevExpress.XtraEditors.LabelControl[] { 
                lblNoun, lblVerb, lblAdj, lblAdv, lblPron, lblConj, lblAdp, lblDet, lblNum 
            };
            var posColors = new System.Drawing.Color[] { 
                System.Drawing.Color.FromArgb(52, 152, 219),   // NOUN
                System.Drawing.Color.FromArgb(46, 204, 113),   // VERB
                System.Drawing.Color.FromArgb(230, 126, 34),   // ADJ
                System.Drawing.Color.FromArgb(155, 89, 182),   // ADV
                System.Drawing.Color.FromArgb(241, 196, 15),   // PRON
                System.Drawing.Color.FromArgb(26, 188, 156),   // CONJ
                System.Drawing.Color.FromArgb(231, 76, 60),    // ADP
                System.Drawing.Color.FromArgb(149, 165, 166),  // DET
                System.Drawing.Color.FromArgb(52, 73, 94)      // NUM
            };
            var posNames = new string[] { "NOUN", "VERB", "ADJ", "ADV", "PRON", "CONJ", "ADP", "DET", "NUM" };

            for (int i = 0; i < 9; i++)
            {
                posLabels[i].Location = new System.Drawing.Point(20 + (i % 3) * 200, 20 + (i / 3) * 80);
                posLabels[i].Size = new System.Drawing.Size(180, 70);
                posLabels[i].AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
                posLabels[i].Appearance.BackColor = posColors[i];
                posLabels[i].Appearance.ForeColor = System.Drawing.Color.White;
                posLabels[i].Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
                posLabels[i].Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                posLabels[i].Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                posLabels[i].Text = $"{posNames[i]}: 0";
                this.tabDashboard.Controls.Add(posLabels[i]);
            }

            this.lblTotal.Location = new System.Drawing.Point(20, 280);
            this.lblTotal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Text = "Total Words: 0";
            this.tabDashboard.Controls.Add(this.lblTotal);

            this.btnRefresh.Location = new System.Drawing.Point(20, 330);
            this.btnRefresh.Size = new System.Drawing.Size(150, 40);
            this.btnRefresh.Text = "🔄 Refresh (F5)";
            this.tabDashboard.Controls.Add(this.btnRefresh);

            // ==================== ANALYSIS TAB ====================
            this.tabAnalysis.Name = "tabAnalysis";
            this.tabAnalysis.Text = "🔍 Word Analysis";
            this.tabAnalysis.Size = new System.Drawing.Size(1094, 669);

            this.txtWord.Location = new System.Drawing.Point(20, 20);
            this.txtWord.Size = new System.Drawing.Size(400, 30);
            this.txtWord.Properties.NullValuePrompt = "Enter a Turkish word...";
            this.txtWord.Properties.NullValuePromptShowForEmptyValue = true;
            this.tabAnalysis.Controls.Add(this.txtWord);

            this.btnAnalyze.Location = new System.Drawing.Point(430, 18);
            this.btnAnalyze.Size = new System.Drawing.Size(120, 35);
            this.btnAnalyze.Text = "🔍 Analyze";
            this.tabAnalysis.Controls.Add(this.btnAnalyze);

            this.memoResult.Location = new System.Drawing.Point(20, 65);
            this.memoResult.Size = new System.Drawing.Size(600, 400);
            this.memoResult.Properties.ReadOnly = true;
            this.memoResult.Properties.Appearance.Font = new System.Drawing.Font("Consolas", 11F);
            this.tabAnalysis.Controls.Add(this.memoResult);

            this.btnSaveWord.Location = new System.Drawing.Point(20, 480);
            this.btnSaveWord.Size = new System.Drawing.Size(180, 40);
            this.btnSaveWord.Text = "💾 Save to Database";
            this.btnSaveWord.Enabled = false;
            this.tabAnalysis.Controls.Add(this.btnSaveWord);

            // ==================== BATCH TAB ====================
            this.tabBatch.Name = "tabBatch";
            this.tabBatch.Text = "📦 Batch Processing";
            this.tabBatch.Size = new System.Drawing.Size(1094, 669);

            this.btnSelectFile.Location = new System.Drawing.Point(20, 20);
            this.btnSelectFile.Size = new System.Drawing.Size(180, 40);
            this.btnSelectFile.Text = "📂 Select CSV (Ctrl+O)";
            this.tabBatch.Controls.Add(this.btnSelectFile);

            this.lblProgress.Location = new System.Drawing.Point(220, 28);
            this.lblProgress.Text = "";
            this.tabBatch.Controls.Add(this.lblProgress);

            this.progressBatch.Location = new System.Drawing.Point(20, 70);
            this.progressBatch.Size = new System.Drawing.Size(1050, 25);
            this.progressBatch.Properties.ShowTitle = true;
            this.tabBatch.Controls.Add(this.progressBatch);

            this.gridBatch.Location = new System.Drawing.Point(20, 110);
            this.gridBatch.Size = new System.Drawing.Size(1050, 450);
            this.gridBatch.MainView = this.gridViewBatch;
            this.gridViewBatch.GridControl = this.gridBatch;
            this.gridViewBatch.OptionsBehavior.Editable = false;
            this.gridViewBatch.OptionsSelection.MultiSelect = true;
            this.tabBatch.Controls.Add(this.gridBatch);

            this.btnSaveBatch.Location = new System.Drawing.Point(20, 575);
            this.btnSaveBatch.Size = new System.Drawing.Size(200, 40);
            this.btnSaveBatch.Text = "💾 Save All to Database";
            this.btnSaveBatch.Enabled = false;
            this.tabBatch.Controls.Add(this.btnSaveBatch);

            // ==================== DATABASE TAB ====================
            this.tabDatabase.Name = "tabDatabase";
            this.tabDatabase.Text = "💾 Database";
            this.tabDatabase.Size = new System.Drawing.Size(1094, 669);

            this.cmbPosFilter.Location = new System.Drawing.Point(20, 20);
            this.cmbPosFilter.Size = new System.Drawing.Size(150, 28);
            this.cmbPosFilter.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.tabDatabase.Controls.Add(this.cmbPosFilter);

            this.txtSearch.Location = new System.Drawing.Point(185, 20);
            this.txtSearch.Size = new System.Drawing.Size(250, 28);
            this.txtSearch.Properties.NullValuePrompt = "🔍 Search...";
            this.txtSearch.Properties.NullValuePromptShowForEmptyValue = true;
            this.tabDatabase.Controls.Add(this.txtSearch);

            this.lblDbCount.Location = new System.Drawing.Point(450, 25);
            this.lblDbCount.Text = "";
            this.tabDatabase.Controls.Add(this.lblDbCount);

            this.gridDatabase.Location = new System.Drawing.Point(20, 60);
            this.gridDatabase.Size = new System.Drawing.Size(1050, 500);
            this.gridDatabase.MainView = this.gridViewDatabase;
            this.gridViewDatabase.GridControl = this.gridDatabase;
            this.gridViewDatabase.OptionsBehavior.Editable = false;
            this.gridViewDatabase.OptionsSelection.MultiSelect = true;
            this.tabDatabase.Controls.Add(this.gridDatabase);

            this.btnExport.Location = new System.Drawing.Point(20, 575);
            this.btnExport.Size = new System.Drawing.Size(180, 40);
            this.btnExport.Text = "📤 Export JSON (Ctrl+S)";
            this.tabDatabase.Controls.Add(this.btnExport);

            this.btnDelete.Location = new System.Drawing.Point(220, 575);
            this.btnDelete.Size = new System.Drawing.Size(150, 40);
            this.btnDelete.Text = "🗑️ Delete Selected";
            this.tabDatabase.Controls.Add(this.btnDelete);

            // ==================== MAIN FORM ====================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tabMain);
            this.Name = "MainForm";
            this.Text = "Turkish NLP Analyzer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.MainForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.tabMain)).EndInit();
            this.tabMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtWord.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoResult.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBatch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewBatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbPosFilter.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridDatabase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewDatabase)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        // Tab Control
        private DevExpress.XtraTab.XtraTabControl tabMain;
        private DevExpress.XtraTab.XtraTabPage tabDashboard;
        private DevExpress.XtraTab.XtraTabPage tabAnalysis;
        private DevExpress.XtraTab.XtraTabPage tabBatch;
        private DevExpress.XtraTab.XtraTabPage tabDatabase;

        // Dashboard
        private DevExpress.XtraEditors.LabelControl lblNoun;
        private DevExpress.XtraEditors.LabelControl lblVerb;
        private DevExpress.XtraEditors.LabelControl lblAdj;
        private DevExpress.XtraEditors.LabelControl lblAdv;
        private DevExpress.XtraEditors.LabelControl lblPron;
        private DevExpress.XtraEditors.LabelControl lblConj;
        private DevExpress.XtraEditors.LabelControl lblAdp;
        private DevExpress.XtraEditors.LabelControl lblDet;
        private DevExpress.XtraEditors.LabelControl lblNum;
        private DevExpress.XtraEditors.LabelControl lblTotal;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;

        // Analysis
        private DevExpress.XtraEditors.TextEdit txtWord;
        private DevExpress.XtraEditors.SimpleButton btnAnalyze;
        private DevExpress.XtraEditors.MemoEdit memoResult;
        private DevExpress.XtraEditors.SimpleButton btnSaveWord;

        // Batch
        private DevExpress.XtraEditors.SimpleButton btnSelectFile;
        private DevExpress.XtraEditors.ProgressBarControl progressBatch;
        private DevExpress.XtraEditors.LabelControl lblProgress;
        private DevExpress.XtraGrid.GridControl gridBatch;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewBatch;
        private DevExpress.XtraEditors.SimpleButton btnSaveBatch;

        // Database
        private DevExpress.XtraEditors.ComboBoxEdit cmbPosFilter;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraEditors.LabelControl lblDbCount;
        private DevExpress.XtraGrid.GridControl gridDatabase;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewDatabase;
        private DevExpress.XtraEditors.SimpleButton btnExport;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
    }
}

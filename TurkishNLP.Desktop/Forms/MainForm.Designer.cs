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

            // Tab Control
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.tabAnalysis = new System.Windows.Forms.TabPage();
            this.tabBatch = new System.Windows.Forms.TabPage();
            this.tabDatabase = new System.Windows.Forms.TabPage();

            // Dashboard Controls
            this.lblNoun = new System.Windows.Forms.Label();
            this.lblVerb = new System.Windows.Forms.Label();
            this.lblAdj = new System.Windows.Forms.Label();
            this.lblAdv = new System.Windows.Forms.Label();
            this.lblPron = new System.Windows.Forms.Label();
            this.lblConj = new System.Windows.Forms.Label();
            this.lblAdp = new System.Windows.Forms.Label();
            this.lblDet = new System.Windows.Forms.Label();
            this.lblNum = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();

            // Analysis Controls
            this.txtWord = new System.Windows.Forms.TextBox();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnSaveWord = new System.Windows.Forms.Button();

            // Batch Controls
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.progressBatch = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.gridBatch = new System.Windows.Forms.DataGridView();
            this.btnSaveBatch = new System.Windows.Forms.Button();

            // Database Controls
            this.cmbPosFilter = new System.Windows.Forms.ComboBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblDbCount = new System.Windows.Forms.Label();
            this.gridDatabase = new System.Windows.Forms.DataGridView();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();

            this.tabMain.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.tabAnalysis.SuspendLayout();
            this.tabBatch.SuspendLayout();
            this.tabDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridDatabase)).BeginInit();
            this.SuspendLayout();

            // ==================== TAB CONTROL ====================
            this.tabMain.Controls.Add(this.tabDashboard);
            this.tabMain.Controls.Add(this.tabAnalysis);
            this.tabMain.Controls.Add(this.tabBatch);
            this.tabMain.Controls.Add(this.tabDatabase);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1100, 700);
            this.tabMain.TabIndex = 0;

            // ==================== DASHBOARD TAB ====================
            this.tabDashboard.Controls.Add(this.lblNoun);
            this.tabDashboard.Controls.Add(this.lblVerb);
            this.tabDashboard.Controls.Add(this.lblAdj);
            this.tabDashboard.Controls.Add(this.lblAdv);
            this.tabDashboard.Controls.Add(this.lblPron);
            this.tabDashboard.Controls.Add(this.lblConj);
            this.tabDashboard.Controls.Add(this.lblAdp);
            this.tabDashboard.Controls.Add(this.lblDet);
            this.tabDashboard.Controls.Add(this.lblNum);
            this.tabDashboard.Controls.Add(this.lblTotal);
            this.tabDashboard.Controls.Add(this.btnRefresh);
            this.tabDashboard.Location = new System.Drawing.Point(4, 30);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(10);
            this.tabDashboard.Size = new System.Drawing.Size(1092, 666);
            this.tabDashboard.Text = "📊 Dashboard";

            // POS Labels (3x3 grid)
            var posLabels = new[] { lblNoun, lblVerb, lblAdj, lblAdv, lblPron, lblConj, lblAdp, lblDet, lblNum };
            var posColors = new[] { 
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

            for (int i = 0; i < 9; i++)
            {
                posLabels[i].Location = new System.Drawing.Point(20 + (i % 3) * 200, 20 + (i / 3) * 80);
                posLabels[i].Size = new System.Drawing.Size(180, 70);
                posLabels[i].BackColor = posColors[i];
                posLabels[i].ForeColor = System.Drawing.Color.White;
                posLabels[i].Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
                posLabels[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                posLabels[i].Text = "POS: 0";
            }

            this.lblNoun.Text = "NOUN: 0";
            this.lblVerb.Text = "VERB: 0";
            this.lblAdj.Text = "ADJ: 0";
            this.lblAdv.Text = "ADV: 0";
            this.lblPron.Text = "PRON: 0";
            this.lblConj.Text = "CONJ: 0";
            this.lblAdp.Text = "ADP: 0";
            this.lblDet.Text = "DET: 0";
            this.lblNum.Text = "NUM: 0";

            this.lblTotal.Location = new System.Drawing.Point(20, 280);
            this.lblTotal.Size = new System.Drawing.Size(300, 40);
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotal.Text = "Total Words: 0";

            this.btnRefresh.Location = new System.Drawing.Point(20, 330);
            this.btnRefresh.Size = new System.Drawing.Size(150, 40);
            this.btnRefresh.Text = "🔄 Refresh (F5)";
            this.btnRefresh.UseVisualStyleBackColor = true;

            // ==================== ANALYSIS TAB ====================
            this.tabAnalysis.Controls.Add(this.txtWord);
            this.tabAnalysis.Controls.Add(this.btnAnalyze);
            this.tabAnalysis.Controls.Add(this.txtResult);
            this.tabAnalysis.Controls.Add(this.btnSaveWord);
            this.tabAnalysis.Location = new System.Drawing.Point(4, 30);
            this.tabAnalysis.Name = "tabAnalysis";
            this.tabAnalysis.Padding = new System.Windows.Forms.Padding(10);
            this.tabAnalysis.Size = new System.Drawing.Size(1092, 666);
            this.tabAnalysis.Text = "🔍 Word Analysis";

            this.txtWord.Location = new System.Drawing.Point(20, 20);
            this.txtWord.Size = new System.Drawing.Size(400, 30);
            this.txtWord.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtWord.PlaceholderText = "Enter a Turkish word...";

            this.btnAnalyze.Location = new System.Drawing.Point(430, 18);
            this.btnAnalyze.Size = new System.Drawing.Size(120, 35);
            this.btnAnalyze.Text = "🔍 Analyze";
            this.btnAnalyze.UseVisualStyleBackColor = true;

            this.txtResult.Location = new System.Drawing.Point(20, 65);
            this.txtResult.Size = new System.Drawing.Size(600, 400);
            this.txtResult.Multiline = true;
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Font = new System.Drawing.Font("Consolas", 11F);

            this.btnSaveWord.Location = new System.Drawing.Point(20, 480);
            this.btnSaveWord.Size = new System.Drawing.Size(180, 40);
            this.btnSaveWord.Text = "💾 Save to Database";
            this.btnSaveWord.UseVisualStyleBackColor = true;
            this.btnSaveWord.Enabled = false;

            // ==================== BATCH TAB ====================
            this.tabBatch.Controls.Add(this.btnSelectFile);
            this.tabBatch.Controls.Add(this.progressBatch);
            this.tabBatch.Controls.Add(this.lblProgress);
            this.tabBatch.Controls.Add(this.gridBatch);
            this.tabBatch.Controls.Add(this.btnSaveBatch);
            this.tabBatch.Location = new System.Drawing.Point(4, 30);
            this.tabBatch.Name = "tabBatch";
            this.tabBatch.Padding = new System.Windows.Forms.Padding(10);
            this.tabBatch.Size = new System.Drawing.Size(1092, 666);
            this.tabBatch.Text = "📦 Batch Processing";

            this.btnSelectFile.Location = new System.Drawing.Point(20, 20);
            this.btnSelectFile.Size = new System.Drawing.Size(180, 40);
            this.btnSelectFile.Text = "📂 Select CSV (Ctrl+O)";
            this.btnSelectFile.UseVisualStyleBackColor = true;

            this.lblProgress.Location = new System.Drawing.Point(220, 28);
            this.lblProgress.AutoSize = true;
            this.lblProgress.Text = "";

            this.progressBatch.Location = new System.Drawing.Point(20, 70);
            this.progressBatch.Size = new System.Drawing.Size(1050, 25);

            this.gridBatch.Location = new System.Drawing.Point(20, 110);
            this.gridBatch.Size = new System.Drawing.Size(1050, 450);
            this.gridBatch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridBatch.ReadOnly = true;
            this.gridBatch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridBatch.AllowUserToAddRows = false;

            this.btnSaveBatch.Location = new System.Drawing.Point(20, 575);
            this.btnSaveBatch.Size = new System.Drawing.Size(200, 40);
            this.btnSaveBatch.Text = "💾 Save All to Database";
            this.btnSaveBatch.UseVisualStyleBackColor = true;
            this.btnSaveBatch.Enabled = false;

            // ==================== DATABASE TAB ====================
            this.tabDatabase.Controls.Add(this.cmbPosFilter);
            this.tabDatabase.Controls.Add(this.txtSearch);
            this.tabDatabase.Controls.Add(this.lblDbCount);
            this.tabDatabase.Controls.Add(this.gridDatabase);
            this.tabDatabase.Controls.Add(this.btnExport);
            this.tabDatabase.Controls.Add(this.btnDelete);
            this.tabDatabase.Location = new System.Drawing.Point(4, 30);
            this.tabDatabase.Name = "tabDatabase";
            this.tabDatabase.Padding = new System.Windows.Forms.Padding(10);
            this.tabDatabase.Size = new System.Drawing.Size(1092, 666);
            this.tabDatabase.Text = "💾 Database";

            this.cmbPosFilter.Location = new System.Drawing.Point(20, 20);
            this.cmbPosFilter.Size = new System.Drawing.Size(150, 30);
            this.cmbPosFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.txtSearch.Location = new System.Drawing.Point(185, 20);
            this.txtSearch.Size = new System.Drawing.Size(250, 30);
            this.txtSearch.PlaceholderText = "🔍 Search...";

            this.lblDbCount.Location = new System.Drawing.Point(450, 25);
            this.lblDbCount.AutoSize = true;
            this.lblDbCount.Text = "";

            this.gridDatabase.Location = new System.Drawing.Point(20, 60);
            this.gridDatabase.Size = new System.Drawing.Size(1050, 500);
            this.gridDatabase.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridDatabase.ReadOnly = true;
            this.gridDatabase.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridDatabase.AllowUserToAddRows = false;

            this.btnExport.Location = new System.Drawing.Point(20, 575);
            this.btnExport.Size = new System.Drawing.Size(180, 40);
            this.btnExport.Text = "📤 Export JSON (Ctrl+S)";
            this.btnExport.UseVisualStyleBackColor = true;

            this.btnDelete.Location = new System.Drawing.Point(220, 575);
            this.btnDelete.Size = new System.Drawing.Size(150, 40);
            this.btnDelete.Text = "🗑️ Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = true;

            // ==================== MAIN FORM ====================
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tabMain);
            this.Name = "MainForm";
            this.Text = "Turkish NLP Analyzer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Load += new System.EventHandler(this.MainForm_Load);

            this.tabMain.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.tabAnalysis.ResumeLayout(false);
            this.tabAnalysis.PerformLayout();
            this.tabBatch.ResumeLayout(false);
            this.tabBatch.PerformLayout();
            this.tabDatabase.ResumeLayout(false);
            this.tabDatabase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridDatabase)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TabPage tabAnalysis;
        private System.Windows.Forms.TabPage tabBatch;
        private System.Windows.Forms.TabPage tabDatabase;

        // Dashboard
        private System.Windows.Forms.Label lblNoun;
        private System.Windows.Forms.Label lblVerb;
        private System.Windows.Forms.Label lblAdj;
        private System.Windows.Forms.Label lblAdv;
        private System.Windows.Forms.Label lblPron;
        private System.Windows.Forms.Label lblConj;
        private System.Windows.Forms.Label lblAdp;
        private System.Windows.Forms.Label lblDet;
        private System.Windows.Forms.Label lblNum;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnRefresh;

        // Analysis
        private System.Windows.Forms.TextBox txtWord;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnSaveWord;

        // Batch
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.ProgressBar progressBatch;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.DataGridView gridBatch;
        private System.Windows.Forms.Button btnSaveBatch;

        // Database
        private System.Windows.Forms.ComboBox cmbPosFilter;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblDbCount;
        private System.Windows.Forms.DataGridView gridDatabase;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnDelete;
    }
}

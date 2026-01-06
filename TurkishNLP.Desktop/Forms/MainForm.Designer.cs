using System.Windows.Forms;
using TurkishNLP.Desktop.Controls;

namespace TurkishNLP.Desktop.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Layout Panels
        private Panel pnlSidebar;
        private Panel pnlRightPanel;
        private Panel pnlContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlRightPanel = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = ModernTheme.BackColor;
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.Font = ModernTheme.GetBodyFont(10f);
            this.ForeColor = ModernTheme.TextPrimary;
            this.Text = "NLP Workbench";
            this.StartPosition = FormStartPosition.CenterScreen;

            // 
            // pnlSidebar (Left)
            // 
            this.pnlSidebar.BackColor = ModernTheme.SidebarColor;
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Width = 240;
            this.pnlSidebar.Name = "pnlSidebar";
            
            // 
            // pnlRightPanel (Right)
            // 
            this.pnlRightPanel.BackColor = ModernTheme.SidebarColor;
            this.pnlRightPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRightPanel.Width = 280;
            this.pnlRightPanel.Name = "pnlRightPanel";

            // 
            // pnlContent (Fill)
            // 
            this.pnlContent.BackColor = ModernTheme.BackColor;
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new Padding(20);

            // 
            // Add Controls (Reverse Order for Dock Priority: Top of Z-Order docks first)
            // We want Sidebar and RightPanel to cut their space first.
            // 
            this.Controls.Add(this.pnlRightPanel); // First priority (Index 0)
            this.Controls.Add(this.pnlSidebar);    // Second priority (Index 1) -> Actually works fine either way if they are opposite
            this.Controls.Add(this.pnlContent);    // Fills remaining

            // Fix Docking Z-Order logic in WinForms:
            // Controls added last are at the BOTTOM of Z-order.
            // Controls at the BOTTOM of Z-order are docked LAST (inner-most).
            // Controls at the TOP of Z-order (Index 0) are docked FIRST (outer-most).
            // So:
            // Add RightPanel (Index 0) -> Docks to Right edge of Form.
            // Add Sidebar (Index 0) -> Docks to Left edge of Form.
            // Add Content (Index 0) -> Fills remaining.
            
            // Wait, Add() puts at END of collection?
            // "The Control.ControlCollection class is a zero-based collection."
            // "The Add method adds the specified control to the end of the collection."
            // "The controls are drawn in the reverse order of the collection." (Index 0 is drawn last -> Top).
            // "Docking behavior: The control at the END of the collection (Bottom of Z-order) gets docked FIRST (takes the edge)."
            // RE-INITIALIZING LOGIC:
            // To be safe, use BringToFront/SendToBack or correct Add order.
            // If I perform:
            // Controls.Add(pnlContent); // Index 0
            // Controls.Add(pnlSidebar); // Index 1
            // Controls.Add(pnlRight);   // Index 2
            
            // If standard WinForms Designer:
            // this.Controls.Add(this.pnlContent);
            // this.Controls.Add(this.pnlRightPanel);
            // this.Controls.Add(this.pnlSidebar);
            // And Sidebar (Left) is outer-most? 
            // Usually, Designer puts `pnlSidebar.Dock = Left` and it's added LAST to the collection to be "outermost" in docking logic?
            // Actually, Z-Order determines docking.
            // Top of Z-Order (Front) = Inner-most Docking (Last to claim space).
            // Bottom of Z-Order (Back) = Outer-most Docking (First to claim space).
            // So we want Sidebar and RightPanel to be at the BACK (Bottom).
            // We want Content to be at the FRONT (Top).
            
            // So:
            // Add Sidebar.
            // Add RightPanel.
            // Add Content.
            // Sidebar is Index 0. Right is Index 1. Content is Index 2.
            // Content (Index 2) is at the END/Bottom? No, Add appends.
            // So Sidebar=0, Right=1, Content=2.
            // If 0 is Top/Front... and Front is Inner...
            // Let's just trust explicit z-ordering via ChildIndex if needed, but usually:
            // Add Content. Add Right. Add Sidebar.
            // Sidebar (Last added) is Bottom/Back -> Outermost.
            
            this.Controls.Clear();
            this.Controls.Add(this.pnlContent);   // Will be pushed to fill center
            this.Controls.Add(this.pnlRightPanel); // Will take right edge
            this.Controls.Add(this.pnlSidebar);    // Will take left edge

            this.ResumeLayout(false);
        }
    }
}

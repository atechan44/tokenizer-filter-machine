using System;
using System.Drawing;
using System.Windows.Forms;

namespace TurkishNLP.Desktop.Controls
{
    public class ModernTabControl : TabControl
    {
        public ModernTabControl()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.Appearance = TabAppearance.Normal;
            this.ItemSize = new Size(120, 40);
            this.Padding = new Point(20, 6);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw background
            e.Graphics.Clear(ModernTheme.BackColor);
            
            // We only need to draw the content of the tab control which is handled by child controls usually. 
            // But for TabControl, OnPaint paints the background behind tabs.
            // The actual tabs are drawn in OnDrawItem.
            // However, we want to hide the standard border.
            // Unfortunately standard TabControl is tricky. 
            // We will just rely on DrawItem and cover up borders if needed.
            // For true borderless, we might need to override WndProc or use a Panel based approach, 
            // but OwnerDrawFixed covers most 'looks'.
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var g = e.Graphics;
            var tabRect = this.GetTabRect(e.Index);
            var page = this.TabPages[e.Index];
            bool isSelected = (e.State == DrawItemState.Selected);

            // Background
            g.FillRectangle(new SolidBrush(ModernTheme.BackColor), tabRect);

            // Text
            var textColor = isSelected ? ModernTheme.AccentGreen : ModernTheme.TextSecondary;
            var font = isSelected ? ModernTheme.GetTitleFont(10f) : ModernTheme.GetBodyFont(10f); // Bold if selected
            
            TextRenderer.DrawText(g, page.Text, font, tabRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Selection Indicator (Underline)
            if (isSelected)
            {
                using (var pen = new Pen(ModernTheme.AccentGreen, 3))
                {
                    g.DrawLine(pen, tabRect.Left + 10, tabRect.Bottom - 2, tabRect.Right - 10, tabRect.Bottom - 2);
                }
            }
        }
    }
}

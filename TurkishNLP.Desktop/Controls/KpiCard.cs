using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TurkishNLP.Desktop.Controls
{
    public class KpiCard : Panel
    {
        private bool _isHovered = false;

        public Color ShadowColor { get; set; } = Color.FromArgb(30, 0, 0, 0);
        public Color BorderColor { get; set; } = Color.FromArgb(50, 255, 255, 255);

        public KpiCard()
        {
            this.DoubleBuffered = true;
            this.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
            this.Resize += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw shadow
            using (var shadowBrush = new SolidBrush(ShadowColor))
            {
                var shadowRect = new Rectangle(3, 3, Width - 3, Height - 3);
                e.Graphics.FillRectangle(shadowBrush, shadowRect);
            }

            // Draw card with rounded corners
            var cardRect = new Rectangle(0, 0, Width - 6, Height - 6);
            var radius = 8;

            using (var path = GetRoundedRect(cardRect, radius))
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border
            using (var pen = new Pen(BorderColor, 2))
            using (var path = GetRoundedRect(cardRect, radius))
            {
                e.Graphics.DrawPath(pen, path);
            }

            // Scale effect on hover
            if (_isHovered)
            {
                using (var highlightBrush = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                using (var path = GetRoundedRect(cardRect, radius))
                {
                    e.Graphics.FillPath(highlightBrush, path);
                }
            }

            // We do NOT call base.OnPaint to prevent default background drawing which might overlap
            // But we must raise the Paint event for children if any (though usually Panel draws children automatically)
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TurkishNLP.Desktop.Controls
{
    public class ModernKpiCard : Panel
    {
        private Color _startColor = ModernTheme.CardColor;
        private Color _endColor = ModernTheme.CardColor;
        private string _value = "0";
        private string _title = "TITLE";
        private string _changeText = "+0%";
        private Color _changeColor = ModernTheme.AccentGreen;

        public Color StartColor
        {
            get => _startColor;
            set { _startColor = value; Invalidate(); }
        }

        public Color EndColor
        {
            get => _endColor;
            set { _endColor = value; Invalidate(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }
        
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public string ChangeText
        {
            get => _changeText;
            set { _changeText = value; Invalidate(); }
        }

        public ModernKpiCard()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(240, 100);
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Background
            using (var brush = new LinearGradientBrush(ClientRectangle, _startColor, _endColor, LinearGradientMode.ForwardDiagonal))
            {
                var path = GetRoundedRect(ClientRectangle, 12);
                e.Graphics.FillPath(brush, path);
            }

            // Title
            using (var font = ModernTheme.GetBodyFont(9f))
            {
                e.Graphics.DrawString(_title, font, new SolidBrush(ModernTheme.TextSecondary), new PointF(15, 15));
            }

            // Value
            using (var font = ModernTheme.GetMetricFont(24f))
            {
                e.Graphics.DrawString(_value, font, new SolidBrush(ModernTheme.TextPrimary), new PointF(15, 40));
            }
            
            // Change Badge
            if (!string.IsNullOrEmpty(_changeText))
            {
                // Measure value first to place badge next to it or below
                // Simple implementation: Top right or next to value
                using (var font = ModernTheme.GetBodyFont(9f))
                {
                   // Draw small pill background
                   var size = e.Graphics.MeasureString(_changeText, font);
                   var rect = new Rectangle(120, 15, (int)size.Width + 10, (int)size.Height + 4);
                   
                   using (var brush = new SolidBrush(Color.FromArgb(40, _changeColor)))
                   {
                         var path = GetRoundedRect(rect, 4);
                         e.Graphics.FillPath(brush, path);
                   }
                   
                   e.Graphics.DrawString(_changeText, font, new SolidBrush(_changeColor), rect.X + 5, rect.Y + 2);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TurkishNLP.Desktop.Controls
{
    public class ModernPanel : Panel
    {
        private int _borderRadius = 12;
        private Color _borderColor = Color.Transparent;

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }
        
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public ModernPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = ModernTheme.CardColor;
            this.Padding = new Padding(10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
             // base.OnPaint(e); // default paint
            
             e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
             
             using (var path = GetRoundedRect(ClientRectangle, _borderRadius))
             using (var brush = new SolidBrush(BackColor))
             {
                 e.Graphics.FillPath(brush, path);
                 
                 if (_borderColor != Color.Transparent)
                 {
                     using (var pen = new Pen(_borderColor, 1))
                     {
                         e.Graphics.DrawPath(pen, path);
                     }
                 }
             }
        }
        
        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

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

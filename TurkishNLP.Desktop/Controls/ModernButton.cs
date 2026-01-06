using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TurkishNLP.Desktop.Controls
{
    public class ModernButton : Button
    {
        private Color _primaryColor = ModernTheme.AccentGreen;
        private Color _hoverColor;
        private int _borderRadius = 8;

        public Color PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                _hoverColor = ControlPaint.Light(value, 0.15f);
                this.BackColor = value;
                Invalidate();
            }
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(120, 40);
            this.BackColor = _primaryColor;
            this.ForeColor = Color.White;
            this.Font = ModernTheme.GetBodyFont(10f); // Semibold equivalent usually
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
            
            _hoverColor = ControlPaint.Light(_primaryColor, 0.15f);
            
            this.MouseEnter += (s, e) => this.BackColor = _hoverColor;
            this.MouseLeave += (s, e) => this.BackColor = _primaryColor;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            
            // Apply Rounded Corners Region
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, _borderRadius * 2, _borderRadius * 2, 180, 90);
                path.AddArc(Width - _borderRadius * 2, 0, _borderRadius * 2, _borderRadius * 2, 270, 90);
                path.AddArc(Width - _borderRadius * 2, Height - _borderRadius * 2, _borderRadius * 2, _borderRadius * 2, 0, 90);
                path.AddArc(0, Height - _borderRadius * 2, _borderRadius * 2, _borderRadius * 2, 90, 90);
                path.CloseFigure();
                
                this.Region = new Region(path);
            }
        }
    }
}

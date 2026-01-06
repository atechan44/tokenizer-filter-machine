using System;
using System.Drawing;
using System.Windows.Forms;

namespace TurkishNLP.Desktop.Controls
{
    public class ModernTextBox : Panel
    {
        public TextBox InnerTextBox { get; private set; }

        public override string Text
        {
            get => InnerTextBox.Text;
            set => InnerTextBox.Text = value;
        }
        
        public bool Multiline
        {
            get => InnerTextBox.Multiline;
            set => InnerTextBox.Multiline = value;
        }

        public ModernTextBox()
        {
            this.BackColor = ModernTheme.InputBackColor;
            this.Padding = new Padding(10, 7, 10, 7);
            this.Size = new Size(200, 35);
            
            InnerTextBox = new TextBox();
            InnerTextBox.Dock = DockStyle.Fill;
            InnerTextBox.BorderStyle = BorderStyle.None;
            InnerTextBox.BackColor = ModernTheme.InputBackColor;
            InnerTextBox.ForeColor = ModernTheme.TextPrimary;
            InnerTextBox.Font = ModernTheme.GetBodyFont(10f);
            
            this.Controls.Add(InnerTextBox);
            
            // Forward events
            InnerTextBox.TextChanged += (s, e) => this.OnTextChanged(e);
            InnerTextBox.KeyDown += (s, e) => this.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Optional: Draw border if focused or hover
        }
        
        protected override void OnClick(EventArgs e)
        {
            InnerTextBox.Focus();
            base.OnClick(e);
        }
    }
}

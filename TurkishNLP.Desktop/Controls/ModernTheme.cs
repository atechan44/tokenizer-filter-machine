using System.Drawing;

namespace TurkishNLP.Desktop.Controls
{
    public static class ModernTheme
    {
        // Main Colors from Reference
        public static readonly Color BackColor = Color.FromArgb(18, 18, 18);       // Almost black
        public static readonly Color SidebarColor = Color.FromArgb(25, 25, 25);    // Very dark gray
        public static readonly Color CardColor = Color.FromArgb(30, 30, 30);       // Slightly lighter for cards
        public static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);  // White
        public static readonly Color TextSecondary = Color.FromArgb(156, 163, 175);// Gray (Tailwind gray-400 approx)
        public static readonly Color BorderColor = Color.FromArgb(45, 45, 45);     // Dark gray border
        public static readonly Color InputBackColor = Color.FromArgb(35, 35, 35);  // Input background

        // Accents
        public static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);   // #10b981
        public static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);    // #3b82f6
        public static readonly Color AccentPurple = Color.FromArgb(139, 92, 246);  // #8b5cf6
        public static readonly Color AccentOrange = Color.FromArgb(249, 115, 22);  // #f97316

        // System Status
        public static readonly Color StatusOnline = Color.FromArgb(16, 185, 129);
        public static readonly Color StatusOffline = Color.FromArgb(239, 68, 68);
        public static readonly Color StatusWaiting = Color.FromArgb(245, 158, 11);

        // Fonts
        public static Font GetTitleFont(float size = 14f) => new Font("Segoe UI", size, FontStyle.Bold);
        public static Font GetBodyFont(float size = 10f) => new Font("Segoe UI", size, FontStyle.Regular);
        public static Font GetMetricFont(float size = 24f) => new Font("Segoe UI", size, FontStyle.Bold);
    }
}

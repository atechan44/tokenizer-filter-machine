using System.Drawing;

namespace TurkishNLP.Desktop.Utils
{
    public static class ThemeManager
    {
        public struct ThemeColors
        {
            public Color Background;
            public Color Surface;
            public Color Text;
            public Color TextSecondary;
            public Color Primary;
            public Color Border;
        }

        public static ThemeColors Light = new ThemeColors
        {
            Background = Color.FromArgb(240, 242, 245), // #F0F2F5
            Surface = Color.White,
            Text = Color.FromArgb(51, 51, 51), // #333333
            TextSecondary = Color.FromArgb(102, 102, 102), // #666666
            Primary = Color.FromArgb(0, 122, 204), // #007ACC
            Border = Color.FromArgb(200, 200, 200)
        };

        public static ThemeColors Dark = new ThemeColors
        {
            Background = Color.FromArgb(30, 30, 30), // #1E1E1E
            Surface = Color.FromArgb(45, 45, 48), // #2D2D30
            Text = Color.FromArgb(224, 224, 224), // #E0E0E0
            TextSecondary = Color.FromArgb(170, 170, 170), // #AAAAAA
            Primary = Color.FromArgb(0, 122, 204), // #007ACC
            Border = Color.FromArgb(60, 60, 60)
        };
    }
}

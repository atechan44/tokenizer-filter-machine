using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using TurkMorph.Forms;

namespace TurkMorph
{
    /// <summary>
    /// Program giriş noktası.
    /// DevExpress skinleri ve ana formu başlatır.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Windows Forms ayarları
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DevExpress Skin ayarları
            // Modern WXI teması (Office 2019 Black benzeri)
            UserLookAndFeel.Default.SetSkinStyle("WXI");

            // Ana formu başlat
            Application.Run(new MainForm());
        }
    }
}

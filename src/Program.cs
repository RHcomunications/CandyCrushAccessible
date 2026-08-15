using System;
using System.Windows.Forms;
using CandyCrushAccessible.UI;

namespace CandyCrushAccessible
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
using System;
using System.Windows.Forms;
using ChartShared;

namespace ChartRendererApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var dataPath = SharedPaths.ResolveDataPath(args);
            Application.Run(new ChartRendererForm(dataPath));
        }
    }
}

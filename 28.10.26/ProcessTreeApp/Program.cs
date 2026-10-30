using System;
using System.Windows.Forms;

namespace ProcessTreeApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProcessTreeForm());
        }
    }
}

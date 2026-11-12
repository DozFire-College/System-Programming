using System;
using System.Windows.Forms;

namespace Task1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

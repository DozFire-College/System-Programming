using System;
using System.IO;

namespace ChartShared
{
    public static class SharedPaths
    {
        public static string ResolveDataPath(string[] args)
        {
            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                return args[0];
            }

            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HomeworkCharts");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "chart-data.xml");
        }
    }
}

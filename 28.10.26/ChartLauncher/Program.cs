using System;
using System.IO;
using System.Threading;

namespace ChartLauncher
{
    internal static class Program
    {
        private static void Main()
        {
            var solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            var configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent?.Name ?? "Debug";
            var sharedDataFolder = Path.Combine(solutionRoot, "SharedChartData");
            var dataPath = Path.Combine(sharedDataFolder, "chart-data.xml");

            Directory.CreateDirectory(sharedDataFolder);

            var inputAppPath = Path.Combine(solutionRoot, "ChartInputApp", "bin", configuration, "ChartInputApp.exe");
            var rendererAppPath = Path.Combine(solutionRoot, "ChartRendererApp", "bin", configuration, "ChartRendererApp.exe");

            if (!File.Exists(inputAppPath) || !File.Exists(rendererAppPath))
            {
                Console.WriteLine("Сначала соберите решение, чтобы появились исполняемые файлы ChartInputApp и ChartRendererApp.");
                Console.WriteLine($"Ожидаемый путь первого приложения: {inputAppPath}");
                Console.WriteLine($"Ожидаемый путь второго приложения: {rendererAppPath}");
                Console.ReadLine();
                return;
            }

            var inputThread = CreateDomainThread("InputDomain", inputAppPath, dataPath);
            var rendererThread = CreateDomainThread("RendererDomain", rendererAppPath, dataPath);

            inputThread.Start();
            rendererThread.Start();

            inputThread.Join();
            rendererThread.Join();
        }

        private static Thread CreateDomainThread(string domainName, string assemblyPath, string dataPath)
        {
            var thread = new Thread(() =>
            {
                var setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(assemblyPath)
                };

                var domain = AppDomain.CreateDomain(domainName, null, setup);

                try
                {
                    domain.ExecuteAssembly(assemblyPath, new[] { dataPath });
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            });

            thread.Name = domainName;
            thread.SetApartmentState(ApartmentState.STA);
            return thread;
        }
    }
}

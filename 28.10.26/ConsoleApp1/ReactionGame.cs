using System;
using System.Diagnostics;
using System.Threading;

namespace ConsoleApp1
{
    internal static class ReactionGame
    {
        public static void Run()
        {
            var random = new Random();

            Console.WriteLine("Игра \"Успел, не успел\"");
            Console.WriteLine("После сигнала нажмите любую клавишу как можно быстрее.");
            Console.WriteLine("Нажмите Enter для старта.");
            Console.ReadLine();

            Console.WriteLine("Ждите сигнал...");
            Thread.Sleep(random.Next(2000, 5001));

            Console.WriteLine("СИГНАЛ! Нажмите клавишу.");
            var stopwatch = Stopwatch.StartNew();
            Console.ReadKey(true);
            stopwatch.Stop();

            Console.WriteLine($"Ваша реакция: {stopwatch.ElapsedMilliseconds} мс.");
        }
    }
}

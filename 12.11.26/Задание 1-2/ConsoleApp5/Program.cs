using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    internal class Program
    {
        static List<int> ReadNumbers(string path)
        {
            string text = File.ReadAllText(path);
            string[] parts = text.Split(new char[] { ' ', ' ', ' ', ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> list = new List<int>();
            foreach (string s in parts)
            {
                int x;
                if (int.TryParse(s, out x))
                    list.Add(x);
            }
            return list;
        }

        static void Task1(List<int> list)
        {
            int count = list.AsParallel().Distinct().Count();
            Console.WriteLine("Количество уникальных значений: " + count);
        }

        static void Task2(List<int> list)
        {
            List<int> starts = ParallelEnumerable.Range(0, list.Count)
                .AsOrdered()
                .Where(i => i == 0 || list[i] <= list[i - 1])
                .ToList();
            starts.Add(list.Count);

            var best = ParallelEnumerable.Range(0, starts.Count - 1)
                .AsOrdered()
                .Select(i => new { Start = starts[i], Length = starts[i + 1] - starts[i] })
                .OrderByDescending(p => p.Length)
                .First();

            Console.WriteLine("Длина самой большой возрастающей последовательности: " + best.Length);
            Console.Write("Последовательность: ");
            for (int i = best.Start; i < best.Start + best.Length; i++)
                Console.Write(list[i] + " ");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.Write("Путь к файлу: ");
            string path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден");
                Console.ReadKey();
                return;
            }

            List<int> list = ReadNumbers(path);

            if (list.Count == 0)
            {
                Console.WriteLine("В файле нет чисел");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Числа из файла:");
            foreach (int x in list)
                Console.Write(x + " ");
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Задание 1");
            Task1(list);
            Console.WriteLine();

            Console.WriteLine("Задание 2");
            Task2(list);

            Console.ReadKey();
        }
    }
}

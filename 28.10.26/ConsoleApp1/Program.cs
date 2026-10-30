using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Домашнее задание по многопоточности";
            RunMenu();
        }

        private static void RunMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Домашнее задание по многопоточности");
                Console.WriteLine("1. Печать коллекции через отдельный поток");
                Console.WriteLine("2. Демонстрация класса Bank с записью в файл");
                Console.WriteLine("3. Игра \"Успел, не успел\"");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт: ");

                var choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        RunCollectionPrintingDemo();
                        break;
                    case "2":
                        RunBankDemo();
                        break;
                    case "3":
                        ReactionGame.Run();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестный пункт меню.");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Нажмите Enter, чтобы вернуться в меню...");
                Console.ReadLine();
            }
        }

        private static void RunCollectionPrintingDemo()
        {
            var items = new List<object>
            {
                "строка",
                125,
                DateTime.Now,
                new DemoPerson("Иван", 21),
                45.67m
            };

            Console.WriteLine("Поток начал обработку коллекции:");
            var thread = CollectionPrinter.PrintCollection(items);
            thread.Join();
            Console.WriteLine("Поток завершил работу.");
        }

        private static void RunBankDemo()
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bank_changes.log");
            var bank = new Bank(1000, "Учебный банк", 5, logPath);

            Console.WriteLine("Начальное состояние:");
            Console.WriteLine(bank);
            Console.WriteLine();

            bank.Money = 1500;
            bank.Percent = 7;
            bank.Name = "Новый банк";
            bank.Money = 2200;

            bank.WaitForPendingWrites();

            Console.WriteLine("Изменения выполнены.");
            Console.WriteLine("Текущее состояние:");
            Console.WriteLine(bank);
            Console.WriteLine();
            Console.WriteLine("Лог записан в файл:");
            Console.WriteLine(logPath);
        }
    }

    internal sealed class DemoPerson
    {
        public DemoPerson(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; }

        public int Age { get; }

        public override string ToString()
        {
            return $"Человек: {Name}, возраст: {Age}";
        }
    }
}

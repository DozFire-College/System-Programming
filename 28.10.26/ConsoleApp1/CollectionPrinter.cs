using System;
using System.Collections;
using System.Threading;

namespace ConsoleApp1
{
    internal static class CollectionPrinter
    {
        public static Thread PrintCollection(IEnumerable items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var worker = new Thread(() =>
            {
                foreach (var item in items)
                {
                    Console.WriteLine(item?.ToString() ?? "null");
                }
            });

            worker.IsBackground = true;
            worker.Start();
            return worker;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ConsoleApp1
{
    internal sealed class Bank
    {
        private readonly object _stateLock = new object();
        private readonly object _fileLock = new object();
        private readonly List<Thread> _writeThreads = new List<Thread>();
        private readonly string _logPath;

        private int _money;
        private string _name;
        private int _percent;

        public Bank(int money, string name, int percent, string logPath)
        {
            _money = money;
            _name = name;
            _percent = percent;
            _logPath = logPath;

            var directory = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public int Money
        {
            get
            {
                lock (_stateLock)
                {
                    return _money;
                }
            }
            set
            {
                ChangeProperty(() => _money = value, nameof(Money), value.ToString());
            }
        }

        public string Name
        {
            get
            {
                lock (_stateLock)
                {
                    return _name;
                }
            }
            set
            {
                ChangeProperty(() => _name = value, nameof(Name), value ?? string.Empty);
            }
        }

        public int Percent
        {
            get
            {
                lock (_stateLock)
                {
                    return _percent;
                }
            }
            set
            {
                ChangeProperty(() => _percent = value, nameof(Percent), value.ToString());
            }
        }

        public void WaitForPendingWrites()
        {
            Thread[] snapshot;
            lock (_writeThreads)
            {
                snapshot = _writeThreads.ToArray();
            }

            foreach (var thread in snapshot)
            {
                thread.Join();
            }
        }

        public override string ToString()
        {
            lock (_stateLock)
            {
                return $"Name = {_name}, Money = {_money}, Percent = {_percent}";
            }
        }

        private void ChangeProperty(Action updateAction, string propertyName, string newValue)
        {
            string stateSnapshot;

            lock (_stateLock)
            {
                updateAction();
                stateSnapshot = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff} | {propertyName} = {newValue} | {this}";
            }

            var thread = new Thread(() => WriteStateToFile(stateSnapshot))
            {
                IsBackground = true
            };

            lock (_writeThreads)
            {
                _writeThreads.Add(thread);
            }

            thread.Start();
        }

        private void WriteStateToFile(string line)
        {
            lock (_fileLock)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine);
            }
        }
    }
}

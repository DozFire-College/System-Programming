using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessTreeApp
{
    internal sealed class ProcessTreeForm : Form
    {
        private readonly TreeView _treeView;
        private readonly TextBox _detailsTextBox;
        private readonly Button _refreshButton;
        private readonly Label _statusLabel;

        private CancellationTokenSource _loadCancellation;
        private Dictionary<int, ProcessInfoRecord> _processMap = new Dictionary<int, ProcessInfoRecord>();

        public ProcessTreeForm()
        {
            Text = "Дерево процессов";
            Width = 1100;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 450
            };

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false
            };
            _treeView.AfterSelect += TreeViewAfterSelect;

            _detailsTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10.0f)
            };

            _refreshButton = new Button
            {
                Text = "Обновить",
                Dock = DockStyle.Top,
                Height = 40
            };
            _refreshButton.Click += (sender, args) => LoadProcessesAsync();

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            splitContainer.Panel1.Controls.Add(_treeView);
            splitContainer.Panel1.Controls.Add(_refreshButton);
            splitContainer.Panel2.Controls.Add(_detailsTextBox);

            Controls.Add(splitContainer);
            Controls.Add(_statusLabel);

            Shown += (sender, args) => LoadProcessesAsync();
            FormClosing += OnFormClosing;
        }

        private void LoadProcessesAsync()
        {
            _loadCancellation?.Cancel();
            _loadCancellation = new CancellationTokenSource();
            var token = _loadCancellation.Token;

            _treeView.Nodes.Clear();
            _detailsTextBox.Clear();
            _statusLabel.Text = "Загрузка дерева процессов...";
            _refreshButton.Enabled = false;

            Task.Run(() => BuildTreeOnWorker(token), token)
                .ContinueWith(task =>
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    _refreshButton.Enabled = true;

                    if (task.IsCanceled || token.IsCancellationRequested)
                    {
                        _statusLabel.Text = "Загрузка отменена.";
                        return;
                    }

                    if (task.Exception != null)
                    {
                        _statusLabel.Text = "Ошибка при получении информации о процессах.";
                        MessageBox.Show(task.Exception.GetBaseException().Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _statusLabel.Text = $"Загрузка завершена. Найдено процессов: {_processMap.Count}.";
                    _treeView.ExpandAll();
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void BuildTreeOnWorker(CancellationToken token)
        {
            var records = ReadProcessRecords();
            var lookup = records.ToDictionary(item => item.ProcessId);
            var childMap = records
                .GroupBy(item => item.ParentProcessId)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Name).ToList());

            _processMap = lookup;

            var roots = records
                .Where(item => item.ParentProcessId == 0 || !lookup.ContainsKey(item.ParentProcessId))
                .OrderBy(item => item.Name)
                .ToList();

            foreach (var root in roots)
            {
                token.ThrowIfCancellationRequested();
                var node = BuildNode(root, childMap, token);

                BeginInvoke((Action)(() =>
                {
                    _treeView.Nodes.Add(node);
                }));
            }
        }

        private static TreeNode BuildNode(ProcessInfoRecord record, IReadOnlyDictionary<int, List<ProcessInfoRecord>> childMap, CancellationToken token)
        {
            var node = new TreeNode(record.ToString())
            {
                Tag = record
            };

            if (!childMap.ContainsKey(record.ProcessId))
            {
                return node;
            }

            foreach (var child in childMap[record.ProcessId])
            {
                token.ThrowIfCancellationRequested();
                node.Nodes.Add(BuildNode(child, childMap, token));
            }

            return node;
        }

        private static List<ProcessInfoRecord> ReadProcessRecords()
        {
            var records = new List<ProcessInfoRecord>();
            using (var searcher = new ManagementObjectSearcher("SELECT ProcessId, ParentProcessId, Name, ExecutablePath, ThreadCount, CreationDate FROM Win32_Process"))
            using (var collection = searcher.Get())
            {
                foreach (ManagementObject process in collection)
                {
                    records.Add(new ProcessInfoRecord
                    {
                        ProcessId = Convert.ToInt32(process["ProcessId"] ?? 0),
                        ParentProcessId = Convert.ToInt32(process["ParentProcessId"] ?? 0),
                        Name = Convert.ToString(process["Name"] ?? "unknown"),
                        ExecutablePath = Convert.ToString(process["ExecutablePath"] ?? "недоступно"),
                        ThreadCount = Convert.ToUInt32(process["ThreadCount"] ?? 0),
                        CreationDate = Convert.ToString(process["CreationDate"] ?? "недоступно")
                    });
                }
            }

            return records;
        }

        private void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            var record = e.Node.Tag as ProcessInfoRecord;
            if (record == null)
            {
                return;
            }

            _detailsTextBox.Text =
                $"Имя процесса: {record.Name}{Environment.NewLine}" +
                $"PID: {record.ProcessId}{Environment.NewLine}" +
                $"Родительский PID: {record.ParentProcessId}{Environment.NewLine}" +
                $"Количество потоков: {record.ThreadCount}{Environment.NewLine}" +
                $"Путь: {record.ExecutablePath}{Environment.NewLine}" +
                $"Дата запуска (WMI): {record.CreationDate}";
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
        }
    }
}

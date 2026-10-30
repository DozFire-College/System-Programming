using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ChartShared;

namespace ChartInputApp
{
    internal sealed class ChartInputForm : Form
    {
        private readonly string _dataPath;
        private readonly TextBox _titleTextBox;
        private readonly DataGridView _grid;
        private readonly Label _statusLabel;

        public ChartInputForm(string dataPath)
        {
            _dataPath = dataPath;
            Text = "Ввод данных для графика";
            Width = 850;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };

            var titleLabel = new Label
            {
                Text = "Название графика:",
                AutoSize = true,
                Left = 12,
                Top = 16
            };

            _titleTextBox = new TextBox
            {
                Left = 12,
                Top = 36,
                Width = 420
            };

            var addRowButton = new Button
            {
                Text = "Добавить строку",
                Width = 130,
                Height = 30,
                Left = 460,
                Top = 30
            };
            addRowButton.Click += (sender, args) => _grid.Rows.Add();

            var removeRowButton = new Button
            {
                Text = "Удалить строку",
                Width = 130,
                Height = 30,
                Left = 600,
                Top = 30
            };
            removeRowButton.Click += RemoveSelectedRow;

            var buildButton = new Button
            {
                Text = "Построить график",
                Width = 130,
                Height = 30,
                Left = 740,
                Top = 30
            };
            buildButton.Click += BuildChart;

            topPanel.Controls.Add(titleLabel);
            topPanel.Controls.Add(_titleTextBox);
            topPanel.Controls.Add(addRowButton);
            topPanel.Controls.Add(removeRowButton);
            topPanel.Controls.Add(buildButton);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            _grid.Columns.Add("Label", "Подпись");
            _grid.Columns.Add("Value", "Значение");

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(_grid);
            Controls.Add(topPanel);
            Controls.Add(_statusLabel);

            Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            var data = ChartDataStorage.Load(_dataPath);

            if (data == null)
            {
                _titleTextBox.Text = "Температура воздуха за месяц";
                _grid.Rows.Add("1", "12");
                _grid.Rows.Add("2", "14");
                _grid.Rows.Add("3", "11");
                _grid.Rows.Add("4", "16");
                _statusLabel.Text = "Создан пример данных.";
                return;
            }

            _titleTextBox.Text = data.Title;
            foreach (var point in data.Points)
            {
                _grid.Rows.Add(point.Label, point.Value.ToString(CultureInfo.InvariantCulture));
            }

            _statusLabel.Text = $"Загружен файл: {_dataPath}";
        }

        private void RemoveSelectedRow(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in _grid.SelectedCells)
            {
                if (cell.RowIndex >= 0 && cell.RowIndex < _grid.Rows.Count)
                {
                    _grid.Rows.RemoveAt(cell.RowIndex);
                    break;
                }
            }
        }

        private void BuildChart(object sender, EventArgs e)
        {
            try
            {
                var data = new ChartDataSet
                {
                    Title = string.IsNullOrWhiteSpace(_titleTextBox.Text) ? "График" : _titleTextBox.Text,
                    UpdatedAt = DateTime.Now,
                    Points = _grid.Rows
                        .Cast<DataGridViewRow>()
                        .Where(row => !row.IsNewRow)
                        .Select(row => new ChartPoint
                        {
                            Label = Convert.ToString(row.Cells[0].Value ?? string.Empty),
                            Value = ParseDouble(row.Cells[1].Value)
                        })
                        .Where(point => !string.IsNullOrWhiteSpace(point.Label))
                        .ToList()
                };

                if (data.Points.Count == 0)
                {
                    MessageBox.Show("Введите хотя бы одну строку данных.", "Нет данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ChartDataStorage.Save(_dataPath, data);
                _statusLabel.Text = $"Данные сохранены: {_dataPath}. Второе приложение может сразу построить график.";
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static double ParseDouble(object value)
        {
            var text = Convert.ToString(value ?? string.Empty);
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantResult))
            {
                return invariantResult;
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var localResult))
            {
                return localResult;
            }

            throw new InvalidOperationException($"Не удалось преобразовать значение \"{text}\" в число.");
        }
    }
}

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ChartShared;

namespace ChartRendererApp
{
    internal sealed class ChartRendererForm : Form
    {
        private readonly string _dataPath;
        private readonly Chart _chart;
        private readonly Label _statusLabel;
        private readonly FileSystemWatcher _watcher;

        public ChartRendererForm(string dataPath)
        {
            _dataPath = dataPath;
            Text = "Построение графика";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(8)
            };

            var reloadButton = new Button
            {
                Text = "Обновить",
                AutoSize = true
            };
            reloadButton.Click += (sender, args) => ReloadChart();

            var saveImageButton = new Button
            {
                Text = "Сохранить изображение",
                AutoSize = true
            };
            saveImageButton.Click += SaveChartAsImage;

            toolbar.Controls.Add(reloadButton);
            toolbar.Controls.Add(saveImageButton);

            _chart = new Chart
            {
                Dock = DockStyle.Fill
            };

            var area = new ChartArea("MainArea");
            area.AxisX.Title = "Период";
            area.AxisY.Title = "Значение";
            area.AxisX.Interval = 1;
            _chart.ChartAreas.Add(area);

            var series = new Series("Series")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                XValueType = ChartValueType.String
            };
            _chart.Series.Add(series);

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            Controls.Add(_chart);
            Controls.Add(toolbar);
            Controls.Add(_statusLabel);

            var directory = Path.GetDirectoryName(_dataPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _watcher = new FileSystemWatcher(directory ?? ".", Path.GetFileName(_dataPath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };
            _watcher.Changed += WatcherOnChanged;
            _watcher.Created += WatcherOnChanged;
            _watcher.Renamed += WatcherOnChanged;

            Load += (sender, args) => ReloadChart();
            FormClosing += (sender, args) => _watcher.Dispose();
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            BeginInvoke((Action)ReloadChart);
        }

        private void ReloadChart()
        {
            try
            {
                var data = ChartDataStorage.Load(_dataPath);
                if (data == null || data.Points == null || data.Points.Count == 0)
                {
                    _chart.Titles.Clear();
                    _chart.Series[0].Points.Clear();
                    _statusLabel.Text = "Файл данных пока не содержит точек для построения.";
                    return;
                }

                _chart.Titles.Clear();
                _chart.Titles.Add(string.IsNullOrWhiteSpace(data.Title) ? "График" : data.Title);

                var series = _chart.Series[0];
                series.Points.Clear();

                foreach (var point in data.Points.Where(item => !string.IsNullOrWhiteSpace(item.Label)))
                {
                    series.Points.AddXY(point.Label, point.Value);
                }

                _statusLabel.Text = $"График обновлён: {data.UpdatedAt:dd.MM.yyyy HH:mm:ss}";
            }
            catch (Exception exception)
            {
                _statusLabel.Text = "Не удалось прочитать файл данных.";
                MessageBox.Show(exception.Message, "Ошибка чтения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveChartAsImage(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG image|*.png";
                dialog.FileName = "chart.png";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _chart.SaveImage(dialog.FileName, ChartImageFormat.Png);
                _statusLabel.Text = $"Изображение сохранено: {dialog.FileName}";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Task1
{
    public partial class Form1 : Form
    {
        private delegate void UpdateDel(ProgressBar progressBar, int value, Color color);

        private List<ProgressBar> progressBars = new List<ProgressBar>();
        private Random random = new Random();
        private object locker = new object();
        private int activeThreads;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count;
            if (!int.TryParse(textBox1.Text, out count) || count < 1 || count > 100)
            {
                MessageBox.Show("Введите количество прогресс-баров от 1 до 100");
                return;
            }

            button1.Enabled = false;
            panel1.Controls.Clear();
            progressBars.Clear();

            for (int i = 0; i < count; ++i)
            {
                ProgressBar progressBar = new ProgressBar();
                progressBar.Location = new Point(10, 10 + i * 35);
                progressBar.Size = new Size(panel1.ClientSize.Width - 40, 25);
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Minimum = 0;
                progressBar.Maximum = 100;
                panel1.Controls.Add(progressBar);
                progressBars.Add(progressBar);
            }

            activeThreads = count;
            for (int i = 0; i < count; ++i)
                ThreadPool.QueueUserWorkItem(ThreadMethod, progressBars[i]);
        }

        private void ThreadMethod(object state)
        {
            ProgressBar progressBar = (ProgressBar)state;
            int value = 0;

            while (value < 100)
            {
                int step;
                int delay;
                Color color;
                lock (locker)
                {
                    step = random.Next(1, 11);
                    delay = random.Next(50, 301);
                    color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                }

                value += step;
                if (value > 100)
                    value = 100;

                try
                {
                    Invoke(new UpdateDel(UpdateProgressBar), progressBar, value, color);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (InvalidOperationException)
                {
                    return;
                }

                Thread.Sleep(delay);
            }

            if (Interlocked.Decrement(ref activeThreads) == 0)
            {
                try
                {
                    Invoke(new MethodInvoker(delegate { button1.Enabled = true; }));
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void UpdateProgressBar(ProgressBar progressBar, int value, Color color)
        {
            if (progressBar.IsDisposed)
                return;
            progressBar.ForeColor = color;
            progressBar.Value = value;
        }
    }
}

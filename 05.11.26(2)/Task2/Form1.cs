using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Task2
{
    public partial class Form1 : Form
    {
        private delegate void UpdateDel(ProgressBar progressBar, int value);

        private class Horse
        {
            public int Number;
            public ProgressBar Bar;
            public TimeSpan Time;
        }

        private const int horseCount = 5;
        private ProgressBar[] progressBars;
        private List<Horse> results = new List<Horse>();
        private Random random = new Random();
        private object locker = new object();
        private Stopwatch stopwatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            progressBars = new ProgressBar[] { progressBar1, progressBar2, progressBar3, progressBar4, progressBar5 };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            listView1.Items.Clear();
            results.Clear();

            for (int i = 0; i < horseCount; ++i)
                progressBars[i].Value = 0;

            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < horseCount; ++i)
            {
                Horse horse = new Horse();
                horse.Number = i + 1;
                horse.Bar = progressBars[i];
                Thread thread = new Thread(ThreadMethod);
                thread.IsBackground = true;
                thread.Start(horse);
            }
        }

        private void ThreadMethod(object state)
        {
            Horse horse = (Horse)state;
            int value = 0;

            while (value < horse.Bar.Maximum)
            {
                int step;
                lock (locker)
                {
                    step = random.Next(1, 6);
                }

                value += step;
                if (value > horse.Bar.Maximum)
                    value = horse.Bar.Maximum;

                try
                {
                    Invoke(new UpdateDel(UpdateProgressBar), horse.Bar, value);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (InvalidOperationException)
                {
                    return;
                }

                Thread.Sleep(100);
            }

            bool finished;
            lock (locker)
            {
                horse.Time = stopwatch.Elapsed;
                results.Add(horse);
                finished = results.Count == horseCount;
            }

            if (finished)
            {
                try
                {
                    Invoke(new MethodInvoker(ShowResults));
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void UpdateProgressBar(ProgressBar progressBar, int value)
        {
            progressBar.Value = value;
        }

        private void ShowResults()
        {
            stopwatch.Stop();
            for (int i = 0; i < results.Count; ++i)
            {
                ListViewItem item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add("Лошадь " + results[i].Number);
                item.SubItems.Add(results[i].Time.TotalSeconds.ToString("F2") + " с");
                listView1.Items.Add(item);
            }
            button1.Enabled = true;
        }
    }
}

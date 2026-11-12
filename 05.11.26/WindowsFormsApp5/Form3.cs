using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp5
{
    public partial class Form3 : Form
    {
        const int busNumber = 175;
        const int busCount = 3;
        const int busSize = 30;

        ManualResetEvent mre = new ManualResetEvent(false);
        AutoResetEvent are = new AutoResetEvent(true);
        object locker = new object();
        Random rnd = new Random();
        int people = 0;
        int total = 0;
        int time = 360;

        public Form3()
        {
            InitializeComponent();
            label1.Text = "Автобус №" + busNumber + ", автобусов на маршруте: " + busCount + ", мест в автобусе: " + busSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            listBox1.Items.Clear();
            people = 0;
            total = 0;
            time = 360;
            label3.Text = "Людей на остановке: 0";
            mre.Reset();

            Thread[] threads = new Thread[busCount + 1];
            threads[0] = new Thread(People);
            threads[0].IsBackground = true;
            threads[0].Start();
            for (int i = 1; i <= busCount; i++)
            {
                threads[i] = new Thread(Bus);
                threads[i].IsBackground = true;
                threads[i].Start(i);
            }

            Thread clock = new Thread(Clock);
            clock.IsBackground = true;
            clock.Start(threads);
        }

        void People()
        {
            while (!mre.WaitOne(Next(90, 300)))
            {
                int count = Next(1, 16);
                lock (locker)
                {
                    people += count;
                    Print("На остановку пришло " + count + " чел., всего ждут " + people);
                }
            }
        }

        void Bus(object obj)
        {
            int n = (int)obj;
            while (!mre.WaitOne(Next(1200, 2400)))
            {
                are.WaitOne();
                lock (locker)
                {
                    int count = people;
                    if (count > busSize)
                        count = busSize;
                    people -= count;
                    total += count;
                    Print("Автобус №" + busNumber + " (" + n + "-й) забрал " + count + " чел., на остановке осталось " + people);
                }
                Thread.Sleep(200);
                are.Set();
            }
        }

        void Clock(object obj)
        {
            Thread[] threads = obj as Thread[];
            while (time < 1380)
            {
                ShowTime();
                if (mre.WaitOne(30))
                    return;
                time++;
            }
            ShowTime();
            mre.Set();
            for (int i = 0; i < threads.Length; i++)
                threads[i].Join();
            Print("День закончен. Перевезено " + total + " чел., на остановке осталось " + people);
            try
            {
                Invoke(new Action(() => button1.Enabled = true));
            }
            catch (InvalidOperationException)
            {
            }
        }

        int Next(int min, int max)
        {
            lock (rnd)
            {
                return rnd.Next(min, max);
            }
        }

        string Time()
        {
            return (time / 60).ToString("00") + ":" + (time % 60).ToString("00");
        }

        void ShowTime()
        {
            string t = Time();
            try
            {
                Invoke(new Action(() => label2.Text = "Время: " + t));
            }
            catch (InvalidOperationException)
            {
            }
        }

        void Print(string s)
        {
            string t = Time();
            int n = people;
            try
            {
                Invoke(new Action(() =>
                {
                    listBox1.Items.Add(t + "  " + s);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    label3.Text = "Людей на остановке: " + n;
                }));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            mre.Set();
        }
    }
}

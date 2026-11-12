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
    public partial class Form4 : Form
    {
        const int busCount = 2;
        int[] numbers = { 175, 28, 46 };
        int[] sizes = { 30, 20, 25 };

        ManualResetEvent mre = new ManualResetEvent(false);
        AutoResetEvent are = new AutoResetEvent(true);
        object locker = new object();
        Random rnd = new Random();
        List<int> people = new List<int>();
        int total = 0;
        int time = 360;

        public Form4()
        {
            InitializeComponent();
            string s = "Автобусов на каждом маршруте: " + busCount + ", мест:";
            for (int i = 0; i < numbers.Length; i++)
                s += "  №" + numbers[i] + " - " + sizes[i];
            label1.Text = s;
            label3.Text = Waiting();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            listBox1.Items.Clear();
            people.Clear();
            total = 0;
            time = 360;
            label3.Text = Waiting();
            mre.Reset();

            Thread[] threads = new Thread[numbers.Length * busCount + 1];
            threads[0] = new Thread(People);
            threads[0].IsBackground = true;
            threads[0].Start();
            int n = 1;
            for (int i = 0; i < numbers.Length; i++)
            {
                for (int j = 0; j < busCount; j++)
                {
                    threads[n] = new Thread(Bus);
                    threads[n].IsBackground = true;
                    threads[n].Start(i);
                    n++;
                }
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
                    for (int i = 0; i < count; i++)
                        people.Add(numbers[Next(0, numbers.Length)]);
                    Print("На остановку пришло " + count + " чел., всего ждут " + people.Count);
                }
            }
        }

        void Bus(object obj)
        {
            int k = (int)obj;
            while (!mre.WaitOne(Next(1800, 3600)))
            {
                are.WaitOne();
                lock (locker)
                {
                    int count = 0;
                    for (int i = 0; i < people.Count && count < sizes[k]; i++)
                    {
                        if (people[i] == numbers[k])
                        {
                            people.RemoveAt(i);
                            i--;
                            count++;
                        }
                    }
                    total += count;
                    Print("Автобус №" + numbers[k] + " забрал " + count + " чел., этот номер еще ждут " + CountPeople(numbers[k]));
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
            Print("День закончен. Перевезено " + total + " чел., на остановке осталось " + people.Count);
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

        int CountPeople(int number)
        {
            int c = 0;
            for (int i = 0; i < people.Count; i++)
            {
                if (people[i] == number)
                    c++;
            }
            return c;
        }

        string Waiting()
        {
            string s = "Людей на остановке: " + people.Count;
            for (int i = 0; i < numbers.Length; i++)
                s += "ждут №" + numbers[i] + ": " + CountPeople(numbers[i]);
            return s;
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
            string w = Waiting();
            try
            {
                Invoke(new Action(() =>
                {
                    listBox1.Items.Add(t + "  " + s);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    label3.Text = w;
                }));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            mre.Set();
        }
    }
}

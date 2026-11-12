using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp5
{
    public partial class Form2 : Form
    {
        ManualResetEvent mre = new ManualResetEvent(false);
        ManualResetEvent[] finish = new ManualResetEvent[2];
        int count;
        bool closed = false;

        public Form2()
        {
            InitializeComponent();
            finish[0] = new ManualResetEvent(false);
            finish[1] = new ManualResetEvent(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out count) || count <= 0)
            {
                MessageBox.Show("Введите количество пар");
                return;
            }

            button1.Enabled = false;
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            label5.Text = "Идет генерация, потоки 2 и 3 ждут";

            mre.Reset();
            finish[0].Reset();
            finish[1].Reset();

            Thread t1 = new Thread(Generate);
            Thread t2 = new Thread(Sum);
            Thread t3 = new Thread(Mult);
            Thread t4 = new Thread(Wait);
            t1.IsBackground = true;
            t2.IsBackground = true;
            t3.IsBackground = true;
            t4.IsBackground = true;
            t2.Start();
            t3.Start();
            t4.Start();
            t1.Start();
        }

        void Generate()
        {
            Random r = new Random();
            StreamWriter sw = new StreamWriter("pairs.txt");
            for (int i = 0; i < count; i++)
            {
                if (closed)
                    break;
                int a = r.Next(1, 100);
                int b = r.Next(1, 100);
                sw.WriteLine(a + " " + b);
                Print(listBox1, a + " " + b);
                Thread.Sleep(100);
            }
            sw.Close();
            Status("Генерация завершена, идет подсчет");
            mre.Set();
        }

        void Sum()
        {
            mre.WaitOne();
            string[] lines = File.ReadAllLines("pairs.txt");
            StreamWriter sw = new StreamWriter("sum.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] s = lines[i].Split(' ');
                int a = int.Parse(s[0]);
                int b = int.Parse(s[1]);
                string res = a + " + " + b + " = " + (a + b);
                sw.WriteLine(res);
                Print(listBox2, res);
                Thread.Sleep(100);
            }
            sw.Close();
            finish[0].Set();
        }

        void Mult()
        {
            mre.WaitOne();
            string[] lines = File.ReadAllLines("pairs.txt");
            StreamWriter sw = new StreamWriter("mult.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] s = lines[i].Split(' ');
                int a = int.Parse(s[0]);
                int b = int.Parse(s[1]);
                string res = a + " * " + b + " = " + (a * b);
                sw.WriteLine(res);
                Print(listBox3, res);
                Thread.Sleep(100);
            }
            sw.Close();
            finish[1].Set();
        }

        void Wait()
        {
            WaitHandle.WaitAll(finish);
            Status("Готово");
            try
            {
                Invoke(new Action(() => button1.Enabled = true));
            }
            catch (InvalidOperationException)
            {
            }
        }

        void Print(ListBox lb, string s)
        {
            try
            {
                Invoke(new Action(() => lb.Items.Add(s)));
            }
            catch (InvalidOperationException)
            {
            }
        }

        void Status(string s)
        {
            try
            {
                Invoke(new Action(() => label5.Text = s));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            closed = true;
        }
    }
}

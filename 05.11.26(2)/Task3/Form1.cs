using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Task3
{
    public partial class Form1 : Form
    {
        private delegate List<ulong> AsyncFibonacciDel(ulong limit);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ulong limit;
            if (!ulong.TryParse(textBox1.Text, out limit))
            {
                MessageBox.Show("Введите целое неотрицательное число");
                return;
            }

            button1.Enabled = false;
            listBox1.Items.Clear();
            label2.Text = "Идет подсчет...";

            AsyncFibonacciDel del = Fibonacci;
            del.BeginInvoke(limit, EndFibonacci, del);
        }

        private List<ulong> Fibonacci(ulong limit)
        {
            List<ulong> numbers = new List<ulong>();
            ulong a = 0;
            ulong b = 1;

            while (a <= limit)
            {
                numbers.Add(a);
                if (ulong.MaxValue - a < b)
                    break;
                ulong c = a + b;
                a = b;
                b = c;
            }

            return numbers;
        }

        private void EndFibonacci(IAsyncResult ar)
        {
            AsyncFibonacciDel del = (AsyncFibonacciDel)ar.AsyncState;
            List<ulong> numbers = del.EndInvoke(ar);

            try
            {
                Invoke(new MethodInvoker(delegate { ShowResult(numbers); }));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void ShowResult(List<ulong> numbers)
        {
            listBox1.BeginUpdate();
            for (int i = 0; i < numbers.Count; ++i)
                listBox1.Items.Add(numbers[i]);
            listBox1.EndUpdate();

            label2.Text = "Найдено чисел: " + numbers.Count;
            button1.Enabled = true;
        }
    }
}

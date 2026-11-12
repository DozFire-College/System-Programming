using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Task4
{
    public partial class Form1 : Form
    {
        private delegate int AsyncSearchDel(string path, string word);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string word = textBox1.Text.Trim();
            string path = textBox2.Text.Trim();

            if (word.Length == 0)
            {
                MessageBox.Show("Введите слово");
                return;
            }
            if (!File.Exists(path))
            {
                MessageBox.Show("Файл не найден");
                return;
            }

            button2.Enabled = false;
            label3_result.Text = "Идет поиск...";

            AsyncSearchDel del = CountWord;
            del.BeginInvoke(path, word, EndSearch, del);
        }

        private int CountWord(string path, string word)
        {
            return CountInText(ReadText(path), word);
        }

        private static string ReadText(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            try
            {
                return new UTF8Encoding(false, true).GetString(data);
            }
            catch (DecoderFallbackException)
            {
                return Encoding.Default.GetString(data);
            }
        }

        private static int CountInText(string text, string word)
        {
            int count = 0;
            int index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
            while (index != -1)
            {
                ++count;
                index = text.IndexOf(word, index + word.Length, StringComparison.OrdinalIgnoreCase);
            }
            return count;
        }

        private void EndSearch(IAsyncResult ar)
        {
            AsyncSearchDel del = (AsyncSearchDel)ar.AsyncState;
            string message;

            try
            {
                int count = del.EndInvoke(ar);
                message = "Количество вхождений слова: " + count;
            }
            catch (Exception ex)
            {
                message = "Ошибка: " + ex.Message;
            }

            try
            {
                Invoke(new MethodInvoker(delegate
                {
                    label3_result.Text = message;
                    button2.Enabled = true;
                }));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}

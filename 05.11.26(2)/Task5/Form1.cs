using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Task5
{
    public partial class Form1 : Form
    {
        private class SearchResult
        {
            public string FileName;
            public string FilePath;
            public int Count;
        }

        private delegate List<SearchResult> AsyncSearchDel(string directory, string word);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string word = textBox1.Text.Trim();
            string directory = textBox2.Text.Trim();

            if (word.Length == 0)
            {
                MessageBox.Show("Введите слово");
                return;
            }
            if (!Directory.Exists(directory))
            {
                MessageBox.Show("Директория не найдена");
                return;
            }

            button2.Enabled = false;
            textBox3_report.Clear();
            label3_result.Text = "Идет поиск...";

            AsyncSearchDel del = SearchInDirectory;
            del.BeginInvoke(directory, word, EndSearch, del);
        }

        private List<SearchResult> SearchInDirectory(string directory, string word)
        {
            List<SearchResult> results = new List<SearchResult>();
            SearchRecursive(directory, word, results);
            return results;
        }

        private void SearchRecursive(string directory, string word, List<SearchResult> results)
        {
            string[] files;
            string[] directories;

            try
            {
                files = Directory.GetFiles(directory);
                directories = Directory.GetDirectories(directory);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (IOException)
            {
                return;
            }

            for (int i = 0; i < files.Length; ++i)
            {
                try
                {
                    int count = CountInText(ReadText(files[i]), word);
                    if (count > 0)
                    {
                        SearchResult result = new SearchResult();
                        result.FileName = Path.GetFileName(files[i]);
                        result.FilePath = files[i];
                        result.Count = count;
                        results.Add(result);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }
            }

            for (int i = 0; i < directories.Length; ++i)
                SearchRecursive(directories[i], word, results);
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
            StringBuilder report = new StringBuilder();

            try
            {
                List<SearchResult> results = del.EndInvoke(ar);
                for (int i = 0; i < results.Count; ++i)
                {
                    report.AppendLine("Название файла: " + results[i].FileName);
                    report.AppendLine("Путь к файлу: " + results[i].FilePath);
                    report.AppendLine("Количество вхождений слова: " + results[i].Count);
                    report.AppendLine();
                }
                message = "Найдено файлов: " + results.Count;
            }
            catch (Exception ex)
            {
                message = "Ошибка: " + ex.Message;
            }

            try
            {
                Invoke(new MethodInvoker(delegate
                {
                    textBox3_report.Text = report.ToString();
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

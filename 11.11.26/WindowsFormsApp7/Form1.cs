using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp7
{
    public partial class Form1 : Form
    {
        CancellationTokenSource cts;
        Task tmain;

        public Form1()
        {
            InitializeComponent();
        }

        int sent(string s, char c, CancellationToken token)
        {
            int n = 0;
            bool end = false;
            bool has = false;
            bool text = false;
            for (int i = 0; i < s.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                if (s[i] == '.' || s[i] == '!' || s[i] == '?')
                {
                    end = true;
                    if (s[i] == c) has = true;
                }
                else
                {
                    if (end)
                    {
                        if (text && (c == ' ' || has)) n++;
                        text = false;
                        end = false;
                        has = false;
                    }
                    if (char.IsLetterOrDigit(s[i])) text = true;
                }
                if (i % 10 == 0) Thread.Sleep(5);
            }
            if (text && (c == ' ' || has)) n++;
            return n;
        }

        int symbols(string s, CancellationToken token)
        {
            int n = 0;
            for (int i = 0; i < s.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                if (s[i] != '\r' && s[i] != '\n') n++;
                if (i % 10 == 0) Thread.Sleep(5);
            }
            return n;
        }

        int words(string s, CancellationToken token)
        {
            string[] m = s.Split(new char[] { ' ', ' ', ' ', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int n = 0;
            for (int i = 0; i < m.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                if (m[i].Any(x => char.IsLetterOrDigit(x))) n++;
                if (i % 2 == 0) Thread.Sleep(5);
            }
            return n;
        }

        async Task start()
        {
            if (textBox1.Text.Trim().Length == 0)
            {
                MessageBox.Show("Введите текст");
                return;
            }
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked && !checkBox4.Checked && !checkBox5.Checked)
            {
                MessageBox.Show("Выберите что выводить в отчёт");
                return;
            }
            string s = textBox1.Text;
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            button1.Enabled = false;
            button2.Enabled = true;
            textBox2.Text = "";
            label3_status.Text = "Идёт анализ...";

            Task<int> t1 = null;
            Task<int> t2 = null;
            Task<int> t3 = null;
            Task<int> t4 = null;
            Task<int> t5 = null;
            List<Task> tasks = new List<Task>();
            if (checkBox1.Checked)
            {
                t1 = Task.Run(() => sent(s, ' ', token), token);
                tasks.Add(t1);
            }
            if (checkBox2.Checked)
            {
                t2 = Task.Run(() => symbols(s, token), token);
                tasks.Add(t2);
            }
            if (checkBox3.Checked)
            {
                t3 = Task.Run(() => words(s, token), token);
                tasks.Add(t3);
            }
            if (checkBox4.Checked)
            {
                t4 = Task.Run(() => sent(s, '?', token), token);
                tasks.Add(t4);
            }
            if (checkBox5.Checked)
            {
                t5 = Task.Run(() => sent(s, '!', token), token);
                tasks.Add(t5);
            }

            try
            {
                await Task.WhenAll(tasks);
                string res = "";
                if (t1 != null) res += "Предложений: " + t1.Result + "\n";
                if (t2 != null) res += "Символов: " + t2.Result + "\n";
                if (t3 != null) res += "Слов: " + t3.Result + "\n";
                if (t4 != null) res += "Вопросительных предложений: " + t4.Result + "\n";
                if (t5 != null) res += "Восклицательных предложений: " + t5.Result + "\n";
                if (radioButton1.Checked)
                {
                    textBox2.Text = res;
                    label3_status.Text = "Готово";
                }
                else
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog1.FileName, res);
                        label3_status.Text = "Сохранено в файл";
                    }
                    else
                    {
                        label3_status.Text = "Файл не выбран";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                label3_status.Text = "Остановлено";
            }
            catch (Exception ex)
            {
                label3_status.Text = "Ошибка";
                MessageBox.Show(ex.Message);
            }
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            tmain = start();
            await tmain;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cts != null) cts.Cancel();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            if (cts != null) cts.Cancel();
            if (tmain != null) await tmain;
            button3.Enabled = true;
            tmain = start();
            await tmain;
        }

        string hash(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(fs));
                }
            }
        }

        string move(string src, string dst)
        {
            string[] files = Directory.GetFiles(src);
            Array.Sort(files);
            Task<string>[] tasks = new Task<string>[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string f = files[i];
                tasks[i] = Task.Run(() => hash(f));
            }
            Task.WaitAll(tasks);

            string log = "Отчёт от " + DateTime.Now + "\r\n";
            log += "Источник: " + src + "\r\n";
            log += "Приёмник: " + dst + "\r\n";
            log += "Файлов в источнике: " + files.Length + "\r\n\r\n";
            Dictionary<string, string> orig = new Dictionary<string, string>();
            int moved = 0;
            int copies = 0;
            int skipped = 0;
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                string h = tasks[i].Result;
                if (orig.ContainsKey(h))
                {
                    copies++;
                    log += "Двойник: " + name + " (совпадает с " + orig[h] + ") ";
                    continue;
                }
                orig.Add(h, name);
                string path = Path.Combine(dst, name);
                if (File.Exists(path))
                {
                    skipped++;
                    log += "Пропущен: " + name + " (уже есть в приёмнике) ";
                    continue;
                }
                File.Move(files[i], path);
                moved++;
                log += "Перенесён: " + name + "";
            }
            log += "Перенесено: " + moved + " ";
            log += "Двойников: " + copies + " ";
            log += "Пропущено: " + skipped + " ";
            return log;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            string src = textBox3.Text.Trim();
            string dst = textBox4.Text.Trim();
            if (!Directory.Exists(src))
            {
                MessageBox.Show("Папка источник не найдена");
                return;
            }
            if (dst.Length == 0)
            {
                MessageBox.Show("Введите путь к приёмнику");
                return;
            }
            if (Path.GetFullPath(src).TrimEnd('\\').ToLower() == Path.GetFullPath(dst).TrimEnd('\\').ToLower())
            {
                MessageBox.Show("Источник и приёмник совпадают");
                return;
            }
            button6.Enabled = false;
            textBox5.Text = "";
            label6_status.Text = "Идёт перенос...";
            try
            {
                Directory.CreateDirectory(dst);
                Task<string> t = Task.Run(() => move(src, dst));
                string log = await t;
                textBox5.Text = log;
                if (checkBox6.Checked)
                {
                    File.WriteAllText(Path.Combine(dst, "report.txt"), log);
                    label6_status.Text = "Готово, отчёт сохранён в report.txt";
                }
                else
                {
                    label6_status.Text = "Готово";
                }
            }
            catch (Exception ex)
            {
                label6_status.Text = "Ошибка";
                MessageBox.Show(ex.Message);
            }
            button6.Enabled = true;
        }
    }
}

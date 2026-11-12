using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp8
{
    public partial class Form1 : Form
    {
        List<Resume> resumes = new List<Resume>();

        public Form1()
        {
            InitializeComponent();
        }

        private async Task LoadFiles(string[] files)
        {
            if (files.Length == 0)
            {
                MessageBox.Show("Файлы не найдены");
                return;
            }

            groupBox1.Enabled = false;

            List<Resume> loaded = await Task.Run(() =>
                files.AsParallel()
                    .AsOrdered()
                    .Select(f => Resume.Load(f))
                    .Where(r => r != null)
                    .ToList());

            foreach (Resume r in loaded)
            {
                if (!resumes.Any(x => x.FileName == r.FileName && x.Name == r.Name))
                {
                    resumes.Add(r);
                    listBox1.Items.Add(r);
                }
            }

            if (loaded.Count < files.Length)
                MessageBox.Show("Не удалось прочитать файлов: " + (files.Length - loaded.Count));

            groupBox1.Enabled = true;
        }

        private string Format(Resume r)
        {
            return "ФИО: " + r.Name + Environment.NewLine +
                   "Город: " + r.City + Environment.NewLine +
                   "Опыт: " + r.Experience + " лет" + Environment.NewLine +
                   "Зарплата: " + r.Salary + " руб." + Environment.NewLine +
                   "Файл: " + r.FileName + Environment.NewLine;
        }

        private string FormatCity(List<Resume> group)
        {
            string s = "Город: " + group[0].City + " (кандидатов: " + group.Count + ")" + Environment.NewLine;
            foreach (Resume r in group.OrderBy(x => x.Name))
                s += "   " + r.Name + ", опыт " + r.Experience + " лет, зарплата " + r.Salary + " руб." + Environment.NewLine;
            return s;
        }

        private bool Check()
        {
            if (resumes.Count == 0)
            {
                MessageBox.Show("Сначала загрузите резюме");
                return false;
            }
            return true;
        }

        private async Task ShowReport(string title, List<Resume> items)
        {
            List<string> lines = await Task.Run(() =>
                items.AsParallel().AsOrdered().Select(r => Format(r)).ToList());
            string text = title + Environment.NewLine + Environment.NewLine;
            foreach (string s in lines)
                text += s + Environment.NewLine;
            textBox1.Text = text;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                await LoadFiles(openFileDialog1.FileNames);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                await LoadFiles(Directory.GetFiles(folderBrowserDialog1.SelectedPath, "*.txt"));
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (!Check())
                return;
            int max = resumes.AsParallel().Max(r => r.Experience);
            List<Resume> items = resumes.AsParallel().AsOrdered().Where(r => r.Experience == max).ToList();
            await ShowReport("Самый опытный кандидат", items);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (!Check())
                return;
            int min = resumes.AsParallel().Min(r => r.Experience);
            List<Resume> items = resumes.AsParallel().AsOrdered().Where(r => r.Experience == min).ToList();
            await ShowReport("Самый неопытный кандидат", items);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if (!Check())
                return;
            List<string> lines = await Task.Run(() =>
                resumes.AsParallel()
                    .GroupBy(r => r.City.ToLower())
                    .OrderBy(g => g.Key)
                    .Select(g => FormatCity(g.ToList()))
                    .ToList());
            string text = "Кандидаты из одного города" + Environment.NewLine + Environment.NewLine;
            foreach (string s in lines)
                text += s + Environment.NewLine;
            textBox1.Text = text;
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            if (!Check())
                return;
            decimal min = resumes.AsParallel().Min(r => r.Salary);
            List<Resume> items = resumes.AsParallel().AsOrdered().Where(r => r.Salary == min).ToList();
            await ShowReport("Кандидат с самыми низкими зарплатными требованиями", items);
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            if (!Check())
                return;
            decimal max = resumes.AsParallel().Max(r => r.Salary);
            List<Resume> items = resumes.AsParallel().AsOrdered().Where(r => r.Salary == max).ToList();
            await ShowReport("Кандидат с самыми высокими зарплатными требованиями", items);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            resumes.Clear();
            listBox1.Items.Clear();
            textBox1.Clear();
        }
    }
}

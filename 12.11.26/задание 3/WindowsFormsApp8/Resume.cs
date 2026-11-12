using System;
using System.IO;

namespace WindowsFormsApp8
{
    public class Resume
    {
        public string Name { get; set; }
        public string City { get; set; }
        public int Experience { get; set; }
        public decimal Salary { get; set; }
        public string FileName { get; set; }

        public static Resume Load(string path)
        {
            try
            {
                Resume r = new Resume();
                r.FileName = Path.GetFileName(path);
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    int pos = line.IndexOf(':');
                    if (pos < 0)
                        continue;
                    string key = line.Substring(0, pos).Trim().ToLower();
                    string value = line.Substring(pos + 1).Trim();
                    switch (key)
                    {
                        case "фио":
                            r.Name = value;
                            break;
                        case "город":
                            r.City = value;
                            break;
                        case "опыт":
                            r.Experience = int.Parse(value);
                            break;
                        case "зарплата":
                            r.Salary = decimal.Parse(value);
                            break;
                    }
                }
                if (string.IsNullOrEmpty(r.Name) || string.IsNullOrEmpty(r.City))
                    return null;
                return r;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override string ToString()
        {
            return Name + " (" + City + ")";
        }
    }
}

using System.IO;
using System.Xml.Serialization;

namespace ChartShared
{
    public static class ChartDataStorage
    {
        public static void Save(string filePath, ChartDataSet data)
        {
            var serializer = new XmlSerializer(typeof(ChartDataSet));
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                serializer.Serialize(stream, data);
            }
        }

        public static ChartDataSet Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(ChartDataSet));
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return serializer.Deserialize(stream) as ChartDataSet;
            }
        }
    }
}

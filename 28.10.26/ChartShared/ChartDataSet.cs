using System;
using System.Collections.Generic;

namespace ChartShared
{
    [Serializable]
    public class ChartDataSet
    {
        public ChartDataSet()
        {
            Points = new List<ChartPoint>();
            UpdatedAt = DateTime.Now;
        }

        public string Title { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<ChartPoint> Points { get; set; }
    }
}

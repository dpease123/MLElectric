using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyUsageMachine.POCO
{
    public class EnergyUsage
    {
        [LoadColumn(0)]
        public string Center;

        [LoadColumn(1)]
        public int DayOfWeek;

        [LoadColumn(2)]
        public int Hour;

        [LoadColumn(3)]
        public double AvgTemp;

        [LoadColumn(4)]
        public float kWH;

    }

    public class EnergyUsagePrediction
    {
        [ColumnName("Score")]
        public float kWH;
    }
}

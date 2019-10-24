using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.ViewModels
{
    public class DataSummary
    {
       public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public string Center { get; set; }
        public int JoinedCount { get; set; }
        public int TempRecords { get; set; }
        public int kWHRecords { get; set; }
    }
}

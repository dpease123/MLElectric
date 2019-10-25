using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.ViewModels
{
    public class MLModelDataSummary
    {
        public DateTime DataStartDate { get; set; }
        public DateTime DataEndDate { get; set; }
        public string Center { get; set; }
        public int JoinedCount { get; set; }
        public int TemperatureRecordCount { get; set; }
        public int DemandRecordCount { get; set; }
    }
}

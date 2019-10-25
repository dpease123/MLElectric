using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.ViewModels
{
    public class MLModelDataSummary
    {
        public string DataStartDate { get; set; }
        public string DataEndDate { get; set; }
        public string Center { get; set; }
        public string JoinedCount { get; set; }
        public string TemperatureRecordCount { get; set; }
        public string DemandRecordCount { get; set; }
        public EvaluateModel EvaluateModel {get; set;}
    }
}

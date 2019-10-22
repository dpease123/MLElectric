
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Data
{
    public class CenterkWhUsage_History
    {
        public string Tag { get; set; }
        public string CenterAbbr { get; set; }
        public DateTime CurrentTimeStamp { get; set; }
        public float CurrentAvgValue { get; set; }
      

    }
}

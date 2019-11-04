
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Data
{
    public class CenterkWhUsage_History
    {
        public string FullTag { get; set; }
        public string CenterAbbr { get; set; }
        public DateTime TimeStamp { get; set; }
        public float AvgValue { get; set; }
        public Guid BatchId { get; set; }

        



    }
}

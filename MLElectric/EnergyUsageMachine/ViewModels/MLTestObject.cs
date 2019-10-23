using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.ViewModels
{
    public class MLTestObject
    {
        public string CenterAbbr { get; set; }
        public int DayOfWeek { get; set; }
        public int Hour { get; set; }
        public float Temperature { get; set; }
    }
}

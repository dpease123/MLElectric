using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Models
{
    public class MLSetting
    {
        public string Region { get; set; }
        public string CenterName { get; set; }
        [Key]
        public string CenterAbbr { get; set; }
        public string WeatherURL { get; set; }
        public DateTime? DateLastRecord { get; set; }
    }
}

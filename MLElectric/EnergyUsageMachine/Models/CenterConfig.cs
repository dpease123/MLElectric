using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Models
{
    [Table("ML_CenterConfig")]
    public class CenterConfig
    {
        [Key]
        public string CenterAbbr { get; set; }
        public string Region { get; set; }
        public string CenterName { get; set; }
        public string WeatherURL { get; set; }
        public DateTime? DateLastRecord { get; set; }
    }
}

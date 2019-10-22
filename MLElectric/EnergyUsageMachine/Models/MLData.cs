using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Models
{
    [Table("MLHistoricalData")]
    public class MLData
    {
        [Key]
        public int Id { get; set; }
        public string Fulltag { get; set; }

        public DateTime TimeStamp { get; set; }

        public string  CenterAbbr { get; set; }

        public float AvgValue { get; set; }
    }
}

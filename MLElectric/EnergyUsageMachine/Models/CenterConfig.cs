using EnergyUsageMachine.Enums;
using EnergyUsageMachine.POCO;
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
        public DateTime? DataStartDate { get; set; }
        public DateTime? DataEndDate { get; set; }
        public int? TemperatureRecordCount { get; set; }

        public int? DemandRecordCount { get; set; }

        public int? JoinedRecordCount { get; set; }

        public decimal? RSquaredScore { get; set; }

        public decimal? RootMeanSquaredError { get; set; }

        public string ModelGrade { get; set; }

        public DateTime? DateUpdated { get; set; }
        public  string BestTrainer { get; set; }

        public IEnumerable<EnergyUsage> EnergyUSage {get; set;}
    }
}

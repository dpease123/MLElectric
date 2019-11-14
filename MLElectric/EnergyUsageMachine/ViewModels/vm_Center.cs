using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.ViewModels
{
    public class vm_Center
    {
        public string CenterAbbr { get; set; }
        public string Region { get; set; }
        public string CenterName { get; set; }
        public string WeatherURL { get; set; }
        public string DataStartDate { get; set; }
        public string DataEndDate { get; set; }
        public int? TemperatureRecordCount { get; set; }

        public int? DemandRecordCount { get; set; }

        public int? JoinedRecordCount { get; set; }

        public decimal? RSquaredScore { get; set; }

        public decimal? RootMeanSquaredError { get; set; }

        public string ModelGrade { get; set; }

        public string DateUpdated { get; set; }
        public string BestTrainer { get; set; }

       
}
}

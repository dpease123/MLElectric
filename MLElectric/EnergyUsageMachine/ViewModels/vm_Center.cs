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
        public string DataUpdatedDate { get; set; }
        public string TemperatureRecordCount { get; set; }

        public string DemandRecordCount { get; set; }

        public string JoinedRecordCount { get; set; }

        //public string MatchQuality { get; set; }

        public decimal? RSquaredScore { get; set; }

        public decimal? RootMeanSquaredError { get; set; }

        public string ModelGrade { get; set; }

       
        public string BestTrainer { get; set; }

       
}
}

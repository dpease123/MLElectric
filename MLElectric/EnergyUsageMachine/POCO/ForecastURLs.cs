using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.POCO
{
    public class ForecastURLs
    {
        [JsonProperty(PropertyName = "forecast")]
        public string ThreeDay { get; set; }
        [JsonProperty(PropertyName = "forecastHourly")]
        public string Hourly { get; set; }

    }
}

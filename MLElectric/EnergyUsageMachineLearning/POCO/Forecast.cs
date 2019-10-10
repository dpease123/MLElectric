using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLElectric.POCO
{
    public class Forecast
    {
        [JsonProperty("properties")]
        public ForecastURLs ForecastURLs { get; set; }

        public List<Period> Next24Hours { get; set; }
    }
}

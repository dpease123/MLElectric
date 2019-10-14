using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Enums
{
    public class Center
    {
        public string _centerAbbr;
        public string _name;
        public string _region;
        public string _weatherURL;

        public Center(string CenterAbbr, string Name, string Region, string WeatherURL)
        {
            _centerAbbr = CenterAbbr;
            _name = Name;
            _region = Region;
            _weatherURL = WeatherURL;

        }
     

    }
}

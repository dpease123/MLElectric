using EnergyUsageMachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Enums
{
    public static class CenterEnum
    {
        public static Center Find(string Abbr)
        {
            var Centers = new List<Center>
            {
                new Center("TVO", "Twelve Oaks", "Region 33", "https://api.weather.gov/points/42.4923,-83.4698"),
                new Center("BEV", "Beverly Center", "Region 34", "https://api.weather.gov/points/34.0735,-118.3777"),
                new Center("CCK", "Cherry Creek", "Region 34", "https://api.weather.gov/points/39.7206,-104.9461"),
                new Center("DOL", "DOLPHIN", "Region 34", "https://api.weather.gov/points/25.7879,-80.3808"),
                new Center("GLC", "Great Lakes Crossing", "Region 34", "https://api.weather.gov/points/42.6877,-83.2399"),
                new Center("MGH", "Green Hills", "Region 34", "https://api.weather.gov/points/36.1057,-86.8123"),
                new Center("MSJ", "San Juan", "Region 34", "https://api.weather.gov/points/18.4124,-66.0255"),
                new Center("SHH", "Short Hills", "Region 34", "https://api.weather.gov/points/40.7399,-74.3645"),
                new Center("FAO", "Fair Oaks", "Region 35", "https://api.weather.gov/points/38.8626,-77.3591"),
                new Center("IMP", "International Marketplace", "Region 35", "https://api.weather.gov/points/21.2776,-157.827"),
                new Center("IPL", "International Plaza", "Region 35", "https://api.weather.gov/points/27.9647,-82.521"),
                new Center("STC", "Stamford", "Region 35", "https://api.weather.gov/points/41.053,-73.5371"),
                new Center("SUN", "Sunvalley", "Region 35", "https://api.weather.gov/points/37.9679,-122.0612"),
                new Center("UTC", "University Town CenterEnum", "Region 35", "https://api.weather.gov/points/27.385,-82.453"),
                new Center("WFS", "Westfarms", "Region 35", "https://api.weather.gov/points/41.7233,-72.7637")
            };

            return Centers.Find(x=> x._centerAbbr == Abbr);
        }
        

       
    }
}

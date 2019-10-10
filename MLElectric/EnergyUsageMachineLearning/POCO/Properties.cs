using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLElectric.POCO
{
    public class Properties
    {
        public DateTime updated { get; set; }
        public string units { get; set; }
        public string forecastGenerator { get; set; }
        public DateTime generatedAt { get; set; }
        public DateTime updateTime { get; set; }
        public string validTimes { get; set; }
        public Period[] periods { get; set; }
    }
}

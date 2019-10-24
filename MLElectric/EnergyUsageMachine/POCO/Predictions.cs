using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnergyUsageMachine.POCO
{
    [Serializable()]
   
    public class Predictions
    {
        [XmlElement("PreditedKWH")]
        public float kWH_Usage { get; set; }
        [XmlElement("HourOfDay")]
        public int Hour { get; set; }
    }
}

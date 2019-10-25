using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnergyUsageMachine.POCO
{
    [Serializable(), DataContract]

    public class Predictions
    {
        [DataMember]
        [XmlElement("PreditedKWH")]
        public float kWH_Usage { get; set; }
        [DataMember]
        [XmlElement("HourOfDay")]
        public int Hour { get; set; }
        [DataMember]
        public EvaluateModel ModelQuality { get; set; }
        [DataMember]
        public string Center { get; set; }
    }
}

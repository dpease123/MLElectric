using Newtonsoft.Json;
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
        [JsonProperty(Order = 2)]
        [XmlElement("PreditedKWH")]
        public float kWH_Usage { get; set; }
        [DataMember]
        [JsonProperty(Order = 3)]
        [XmlElement("HourOfDay")]
        public int Hour { get; set; }
        [DataMember]
        [JsonProperty(Order = 4)]
        public EvaluateModel ModelQuality { get; set; }
        [DataMember]
        [JsonProperty(Order = 1)]
        public string Center { get; set; }

        [DataMember]
        [JsonProperty(Order = 0)]
        public string TrainerUsed{ get; set; }
    }
}

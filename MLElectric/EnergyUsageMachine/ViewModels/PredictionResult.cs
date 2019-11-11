using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnergyUsageMachine.ViewModels
{
    [Serializable()]
    [XmlRoot("EnergyDemands")]
    public class PredictionResult
    {
        public PredictionResult()
        {
            this.Predictions = new List<Predictions>();
        }
        [XmlElement("Demand")]
        public List<Predictions> Predictions { get; set; }
        [XmlAttribute("Center")]
        public string Center { get; set; }
        [XmlAttribute("ModelUsed")]
        public string ModelUsed { get; set; }


        
    }
}

using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnergyUsageMachinex
{
    public class XMLHandler
    {
        public void GenerateXML(List<PredictionResult> list)
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(List<PredictionResult>));

            TextWriter filestream = new StreamWriter(@"C:\temp\output.xml");

            serialiser.Serialize(filestream, list);

            filestream.Close();
        }
    }
}

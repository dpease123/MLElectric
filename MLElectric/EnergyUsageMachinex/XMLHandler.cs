using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnergyUsageMachine
{
    public static class XMLHandler
    {
        private static readonly List<PredictionResult> _results;
        public static void GenerateXML(List<PredictionResult> results)
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(List<PredictionResult>));

            TextWriter filestream = new StreamWriter(@"C:\temp\output.xml");

            serialiser.Serialize(filestream, _results);

            filestream.Close();
        }
    }
}

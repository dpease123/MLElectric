using EnergyUsageMachine.Enums;
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
        public static void GenerateXML(List<PredictionResult> results, Center c)
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(List<PredictionResult>));

            TextWriter filestream = new StreamWriter(@"C:\temp\ML\output.xml".Replace("output","output_" + c._centerAbbr));

            serialiser.Serialize(filestream, results);

            filestream.Close();
        }
    }
}

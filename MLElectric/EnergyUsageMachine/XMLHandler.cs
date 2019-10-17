using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace EnergyUsageMachine
{
    public static class XMLHandler
    {
        public static void GenerateXMLFile(List<PredictionResult> results, MLSetting c)
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(List<PredictionResult>));

            TextWriter filestream = new StreamWriter(@"C:\temp\ML\output.xml".Replace("output","output_" + c.CenterAbbr));

            serialiser.Serialize(filestream, results);

            filestream.Close();
        }

        public static string SerializeXml<T>(T config)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            string xml = "";
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = false
            };

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, config);
                    xml = sww.ToString();
                }
            }

            return xml;
        }


    }
}

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
using EnergyUsageMachine.ViewModels;
using System.Web.Configuration;

namespace EnergyUsageMachine
{
    public static class XMLHandler
    {
        public static void GenerateXMLFile(PredictionResult results, CenterConfig c)
        {
            var filePath = Path.Combine(WebConfigurationManager.AppSettings["XMLFilePath"], "output.xml");
            XmlSerializer serialiser = new XmlSerializer(typeof(PredictionResult));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            TextWriter filestream = new StreamWriter(filePath.Replace("output","output_" + c.CenterAbbr));

            serialiser.Serialize(filestream, results, ns);

            filestream.Close();
        }

        public static string SerializeXml<T>(T config)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            string xml = "";
         

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, config, ns);
                    xml = sww.ToString();
                }
            }

            return xml;
        }


    }
}

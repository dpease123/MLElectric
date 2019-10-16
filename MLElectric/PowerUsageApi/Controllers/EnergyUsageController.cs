using EnergyUsageMachine.Services;
using System.Threading.Tasks;
using System.Web.Http;
using PowerUsageApi.SwaggerFilters;
using System.IO;
using Microsoft.ML;
using EnergyUsageMachine;
using EnergyUsageMachine.POCO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace PowerUsageApi.Controllers
{
    public class EnergyUsageController : ApiController
    {
        static string modelSavePath = Path.Combine(@"C:\Temp\ML", "Model.zip");
        [SwaggerImplementationNotes("Returns the predicted next 24hrs. of energy usage for a building. i.e. BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/{BldgId}")]
        public IHttpActionResult EnergyUsage(string BldgId)
        {
            var ws = new WeatherService();
            var ds = new DataService();
            var center = ds.GetSetting(BldgId);
            IEnumerable<EnergyUsage> modelData;
            ITransformer trainedModel;
            var startDate = "01/01/2018";
            var endDate = "01/01/2020";
            var results = new List<PredictionResult>();
            modelSavePath = modelSavePath.Replace("Model", "Model_" + center.CenterAbbr);
            try
            {
                var mlContext = new MLContext();
                var foreCast = Task.Run(async () => await ws.Get24HrForecast(center)).Result;


                if (!File.Exists(modelSavePath))
                {
                    modelData = ds.GetTrainingData(center, startDate, endDate);
                    var mlModel = new MLModel(modelData, modelSavePath);
                    trainedModel = mlModel.Train();
                }
                else
                    trainedModel = mlContext.Model.Load(modelSavePath, out DataViewSchema modelSchema);

                var p = new Prediction(trainedModel, foreCast);
                results = p.Predict();

                //XMLHandler.GenerateXML(results, center);
                //XmlSerializer serialiser = new XmlSerializer(typeof(List<PredictionResult>));



            }
            catch (Exception ex)
            {

            }
            return Ok(results);

        }
    }
}
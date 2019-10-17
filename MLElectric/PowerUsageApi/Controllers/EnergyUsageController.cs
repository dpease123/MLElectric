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

                var usagePredictions = new Prediction(trainedModel, foreCast);
                return Ok(XMLHandler.SerializeXml<List<PredictionResult>>(usagePredictions.Predict()));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [SwaggerImplementationNotes("Train the machine learning model for a center. Parameters: BEV,UTC,CCK: Begin Date: 01/01/2018; End Date: 06/01/2020")]
        [HttpGet]
        [Route("api/EnergyUsage/Train{BldgId}")]
        public IHttpActionResult Trainmodel(string BldgId, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                var ds = new DataService();
                var center = ds.GetSetting(BldgId);
                IEnumerable<EnergyUsage> modelData;
                modelData = ds.GetTrainingData(center, StartDate.ToShortDateString(), StartDate.ToShortDateString());
                var mlModel = new MLModel(modelData, modelSavePath);
                mlModel.Train();
                return Ok($"{center.CenterAbbr} mechine learning model trained");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
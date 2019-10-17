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
using System.Web.Configuration;

namespace PowerUsageApi.Controllers
{
    public class EnergyUsageController : ApiController
    {
       
        [SwaggerImplementationNotes("Returns the predicted next 24hrs. of energy usage for a building. Parameters: BldgId= BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/Predict/{BldgId}")]
        public IHttpActionResult EnergyUsage(string BldgId)
        {
            if (string.IsNullOrEmpty(BldgId))
                return BadRequest("3 character building abbreviation required");

            var ds = new DataService();
            var center = ds.GetSetting(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            ITransformer trainedModel;
            var results = new List<PredictionResult>();
            var ws = new WeatherService();
            try
            {
                var mlContext = new MLContext();
                if (!File.Exists(GetPath(center)))
                    return BadRequest($"No machine learning model found for {center.CenterAbbr}");

                trainedModel = mlContext.Model.Load(GetPath(center), out DataViewSchema modelSchema);

                var weatherForeCast = Task.Run(async () => await ws.Get24HrForecast(center)).Result;

                var usagePredictions = new Prediction(trainedModel, weatherForeCast);
                return Ok(XMLHandler.SerializeXml<List<PredictionResult>>(usagePredictions.Predict()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [SwaggerImplementationNotes("Train the machine learning model for a center. Parameters: BldgId: BEV,UTC,CCK; Begin Date: 01/01/2018; End Date: 06/01/2020")]
        [HttpGet]
        [Route("api/EnergyUsage/Train/{BldgId}")]
        public IHttpActionResult Trainmodel(string BldgId, string StartDate, string EndDate)
        {
            try
            {
                if (string.IsNullOrEmpty(BldgId))
                    return BadRequest("3 character building abbreviation required i.e. BEV,CCK,TVO");

                var ds = new DataService();
                var center = ds.GetSetting(BldgId.Substring(0, 3).ToUpper());

                if (center == null)
                    return BadRequest("Building not found");

             

                if (!IsValidDate(StartDate))
                    return BadRequest("Required or invalid start date");

                if (!IsValidDate(EndDate))
                    return BadRequest("Required or invalid end date");

                if (DateTime.Parse(StartDate) > DateTime.Parse(EndDate) || (DateTime.Parse(EndDate) < DateTime.Parse(StartDate)))
                    return BadRequest("Start date must be prior to end date");

                IEnumerable<EnergyUsage> modelData;
                modelData = ds.GetTrainingData(center, StartDate, EndDate);
                var mlModel = new MLModel(modelData, GetPath(center));
                mlModel.Train();
                return Ok($"{center.CenterAbbr} machine learning model trained using data spanning {StartDate} - {EndDate}");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool IsValidDate(string date)
        {

            return DateTime.TryParse(date, out DateTime dDate);
        }

        private static string GetPath(EnergyUsageMachine.Models.MLSetting center)
        {
            return Path.Combine(WebConfigurationManager.AppSettings["MLModelPath"], "Model_" + center.CenterAbbr.ToUpper() + ".zip");
        }
    }
}
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
using EnergyUsageMachine.Data;

namespace PowerUsageApi.Controllers
{
    public class EnergyUsageController : ApiController
    {
        readonly List<string> LoadDates = new List<string>
        {
            "01/01/2017, 05/30/2017",
            "06/01/2017, 12/31/2017",
            "01/01/2018, 05/30/2018",
            "06/01/2018, 12/31/2018",
            "01/01/2019, 04/23/2019",
            "05/015/2019, " + DateTime.Now.ToShortDateString()
        };

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

        [SwaggerImplementationNotes("Train ALL center machine learning models using previously staged data. Parameters: Begin Date: 01/01/2018 or '?' for all; End Date: 06/01/2020 or '?' for all")]
        [HttpGet]
        [Route("api/EnergyUsage/Train/All")]
        public IHttpActionResult TrainAllModels(string StartDate, string EndDate)
        {
            try
            {
                if (StartDate == "?" || EndDate == "?")
                {
                    StartDate = "01/01/2017";
                    EndDate = DateTime.Now.ToShortDateString();

                }
                else
                {
                    if (!IsValidDate(StartDate))
                        return BadRequest("Required or invalid start date");

                    if (!IsValidDate(EndDate))
                        return BadRequest("Required or invalid end date");

                    if (DateTime.Parse(StartDate) > DateTime.Parse(EndDate) || (DateTime.Parse(EndDate) < DateTime.Parse(StartDate)))
                        return BadRequest("Start date must be prior to end date");
                }

                var ds = new DataService();
                var centers = ds.GetAllSettings();

                foreach (var center in centers)
                {
                    IEnumerable<EnergyUsage> modelData;
                    modelData = ds.GetTrainingData(center, StartDate, EndDate);
                    var mlModel = new MLModel(modelData, GetPath(center));
                    mlModel.Train();
                }
                return Ok($"All center machine learning models trained using data spanning {StartDate} - {EndDate}");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [SwaggerImplementationNotes("Train the machine learning model using previously staged data for a center. Parameters: BldgId: BEV,UTC,CCK; Begin Date: 01/01/2018 or '?' for all; End Date: 06/01/2020 or '?' for all")]
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

                if (StartDate == "?" || EndDate == "?")
                {
                    StartDate = "01/01/2017";
                    EndDate = DateTime.Now.ToShortDateString();
                }
                else
                {
                    if (!IsValidDate(StartDate))
                        return BadRequest("Required or invalid start date");

                    if (!IsValidDate(EndDate))
                        return BadRequest("Required or invalid end date");

                    if (DateTime.Parse(StartDate) > DateTime.Parse(EndDate) || (DateTime.Parse(EndDate) < DateTime.Parse(StartDate)))
                        return BadRequest("Start date must be prior to end date");
                }

            

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

        [SwaggerImplementationNotes("Load and train the machine learning model for a center appending the latest data to the model. Parameters: BldgId: BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/Train/NewData/{BldgId}")]
        public IHttpActionResult TrainNow(string BldgId)
        {
            var ds = new DataService();
            var center = ds.GetSetting(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            var a = ds.GetMaxLoadDate(center);
            var z = DateTime.Now;
            try
            {
                IEnumerable<EnergyUsage> modelData;

                ds.StageTrainingData(center, a.ToShortTimeString(), z.ToShortDateString());
                modelData = ds.GetTrainingData(center, "01/01/2017", z.ToShortTimeString());
                var mlModel = new MLModel(modelData, GetPath(center));
                mlModel.Train();
                center = ds.UpdateSetting(center);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok($"{center.CenterAbbr} machine learning model trained with latest data starting {a.ToShortDateString()} to {z.ToShortDateString()}");

        }

        [SwaggerImplementationNotes("CAUTION: Will Delete and refresh ALL center data from Jan 2017 to now. Parameters: None")]
        [HttpGet]
        [Route("api/EnergyUsage/RefreshData/All")]
        public IHttpActionResult RefreshAllData()
        {
            var repo = new HyperHistorianRepository();
            var ds = new DataService();
            var centers = ds.GetAllSettings();
          
            foreach (var cen in centers)
            {
                try
                {
                    ds.DeleteCenterData(cen.CenterAbbr);

                    foreach (var d in LoadDates)
                    {
                        var a = d.Split(',')[0];
                        var z = d.Split(',')[1];
                        IEnumerable<EnergyUsage> modelData;
                        modelData = ds.StageTrainingData(cen, a, z);
                    }
                  
                    repo.UpdateSetting(cen);


                }
            
                catch (Exception ex)
                {
                    continue;
                }

            }

            return Ok($"All data refreshed.");

        }

        [SwaggerImplementationNotes("CAUTION: Will Delete and refresh center data from Jan 2017 to Now. Parameters: BldgId: BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/RefreshData/{BldgId}")]
        public IHttpActionResult RefreshDataForCenter(string BldgId)
        {
            var ds = new DataService();
            var center = ds.GetSetting(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            ds.DeleteCenterData(center.CenterAbbr);
           
            try
            {
                foreach (var d in LoadDates)
                {
                    var a = d.Split(',')[0];
                    var z = d.Split(',')[1];
                    IEnumerable<EnergyUsage> modelData;
                    modelData = ds.StageTrainingData(center, a, z);
                };

                center = ds.UpdateSetting(center);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok($"Data refreshed for {center.CenterAbbr}");

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
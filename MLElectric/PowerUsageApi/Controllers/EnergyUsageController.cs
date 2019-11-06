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
using EnergyUsageMachine.ViewModels;
using System.Linq;
using System.Text;
using EnergyUsageMachine.Enums;

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
            "01/01/2019, 05/30/2019",
            "06/01/2019, " + DateTime.Now.ToShortDateString()
        };

        [SwaggerImplementationNotes("Returns a single energy usage prediction for testing purposes.")]
        [HttpPost]
        [Route("api/EnergyUsage/Predict/Single")]
        public IHttpActionResult PredictSingle(MLTestObject testObj)
        {
            if (string.IsNullOrEmpty(testObj.CenterAbbr))
                return BadRequest("3 character building abbreviation required");

            var ds = new DataService();
            var center = ds.GetCenterConfig((testObj.CenterAbbr.Substring(0, 3).ToUpper()));

            if (center == null)
                return BadRequest("Building not found");

            ITransformer trainedModel;
            var result = new Predictions();
           
            try
            {
                var mlContext = new MLContext();
                if (!File.Exists(GetPath(center)))
                    return BadRequest($"No machine learning model found for {center.CenterAbbr}");

                trainedModel = mlContext.Model.Load(GetPath(center), out DataViewSchema modelSchema);

                var usagePrediction = new Prediction(trainedModel, testObj, center);
                var predictions = usagePrediction.PredictSingle();
                var eval = new Evaluate(mlContext, GetPath(center), center).EvaluateModel();
                predictions.ModelQuality = eval;
                predictions.Center = center.CenterAbbr;

                return Ok(predictions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [SwaggerImplementationNotes("Best model contest...")]
        [HttpPost]
        [Route("api/EnergyUsage/Predict/BestModel")]
        public IHttpActionResult BestModel(MLTestObject testObj)
        {
            List<Obj> trainedModels = new List<Obj>();
            
            if (string.IsNullOrEmpty(testObj.CenterAbbr))
                return BadRequest("3 character building abbreviation required");

            var ds = new DataService();
            var center = ds.GetCenterConfig((testObj.CenterAbbr.Substring(0, 3).ToUpper()));

            if (center == null)
                return BadRequest("Building not found");

            var a = WebConfigurationManager.AppSettings["MLDataStartDate"];
            var z = DateTime.Now.ToString();

            try
            {
                var mlContext = new MLContext();
                var modelData = ds.GetTrainingData(center, a, z);
                IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
                var o = new Obj()
                {
                    TypeName = RegressionTrainer.FastTree,
                    TrainedModel = new PredictionEngine(modelData, GetPath2(center, RegressionTrainer.FastTree)).FastTree()

                };
                trainedModels.Add(o);

                o = new Obj()
                {
                    TypeName = RegressionTrainer.FastTreeTweedie,
                    TrainedModel = new PredictionEngine(modelData, GetPath2(center, RegressionTrainer.FastTreeTweedie)).FastTreeTweedie()
                };
                trainedModels.Add(o);

                o = new Obj()
                {
                    TypeName = RegressionTrainer.FastForest,
                    TrainedModel = new PredictionEngine(modelData, GetPath2(center, RegressionTrainer.FastForest)).FastForest()
                };
                trainedModels.Add(o);

                o = new Obj()
                {
                    TypeName = RegressionTrainer.PoissonRegression,
                    TrainedModel = new PredictionEngine(modelData, GetPath2(center, RegressionTrainer.PoissonRegression)).PoissonRegression()
                };
                trainedModels.Add(o);
                o = new Obj()
                {
                    TypeName = RegressionTrainer.OnlineGradientDescent,
                    TrainedModel = new PredictionEngine(modelData, GetPath2(center, RegressionTrainer.OnlineGradientDescent)).OnlineGradientDescent()
                };
                trainedModels.Add(o);

                //o = new Obj()
                //{
                //    TypeName = RegressionTrainer.Sdca,
                //    TrainedModel = new MLModel(modelData, GetPath2(center, RegressionTrainer.Sdca)).Sdca()
                //};
                //trainedModels.Add(o);


                var predictionsList = new List<Predictions>();
                foreach (var tm in trainedModels)
                {
                    var usagePrediction = new Prediction(tm.TrainedModel, testObj, center);
                    var predictions = usagePrediction.PredictSingle();
                    var eval = new Evaluate(mlContext, GetPath2(center, tm.TypeName), center).EvaluateModel();
                    predictions.ModelQuality = eval;
                    predictions.Center = center.CenterAbbr;
                    predictions.TrainerUsed = tm.TypeName;
                    predictionsList.Add(predictions);
                }
                return Ok(predictionsList.OrderByDescending(x => x.ModelQuality.RSquaredScore));
}
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [SwaggerImplementationNotes("Returns the predicted next 24hrs. of energy usage for a building. Parameters: BldgId= BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/Predict/{BldgId}")]
        public IHttpActionResult EnergyUsage(string BldgId)
        {
            if (string.IsNullOrEmpty(BldgId))
                return BadRequest("3 character building abbreviation required");

            var ds = new DataService();
            var center = ds.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            ITransformer trainedModel;
            var ws = new WeatherService();
            try
            {
                var mlContext = new MLContext();
                if (!File.Exists(GetPath(center)))
                    return BadRequest($"No machine learning model found for {center.CenterAbbr}");

                trainedModel = mlContext.Model.Load(GetPath(center), out DataViewSchema modelSchema);

                var weatherForeCast = Task.Run(async () => await ws.Get24HrForecast(center)).Result;

                var usagePredictions = new Prediction(trainedModel, weatherForeCast, center);

                var results = usagePredictions.Predict();

                XMLHandler.GenerateXMLFile(results, center);

                return Ok(XMLHandler.SerializeXml<PredictionResult>(usagePredictions.Predict()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        //[SwaggerImplementationNotes("Train ALL center machine learning models using previously staged data. Parameters: Begin Date: 01/01/2018 or '?' for all; End Date: 06/01/2020 or '?' for all")]
        //[HttpGet]
        //[Route("api/EnergyUsage/Train/All")]
        //public IHttpActionResult TrainAllModels(string StartDate, string EndDate)
        //{
        //    try
        //    {
        //        if (StartDate == "?" || EndDate == "?")
        //        {
        //            StartDate = WebConfigurationManager.AppSettings["MLDataStartDate"]; 
        //            EndDate = DateTime.Now.ToShortDateString();

        //        }
        //        else
        //        {
        //            if (!IsValidDate(StartDate))
        //                return BadRequest("Required or invalid start date");

        //            if (!IsValidDate(EndDate))
        //                return BadRequest("Required or invalid end date");

        //            if (DateTime.Parse(StartDate) > DateTime.Parse(EndDate) || (DateTime.Parse(EndDate) < DateTime.Parse(StartDate)))
        //                return BadRequest("Start date must be prior to end date");
        //        }

        //        var ds = new DataService();
        //        var centers = ds.GetAllCenterConfigs();

        //        foreach (var center in centers)
        //        {
        //            IEnumerable<EnergyUsage> modelData;
        //            modelData = ds.GetTrainingData(center, StartDate, EndDate);
        //            var mlModel = new MLModel(modelData, GetPath(center));
        //            mlModel.Train();
        //        }
        //        return Ok($"All center machine learning models successfully trained.");

        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.InnerException.ToString());
        //    }
        //}

        //[SwaggerImplementationNotes("Train the machine learning model using previously staged data for a center. Parameters: BldgId: BEV,UTC,CCK; Begin Date: 01/01/2018 or '?' for all; End Date: 06/01/2020 or '?' for all")]
        //[HttpGet]
        //[Route("api/EnergyUsage/Train/{BldgId}")]
        //public IHttpActionResult Trainmodel(string BldgId, string StartDate, string EndDate)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(BldgId))
        //            return BadRequest("3 character building abbreviation required i.e. BEV,CCK,TVO");

        //        var ds = new DataService();
        //        var center = ds.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

        //        if (center == null)
        //            return BadRequest("Building not found");

        //        if (StartDate == "?" || EndDate == "?")
        //        {
        //            StartDate = WebConfigurationManager.AppSettings["MLDataStartDate"];
        //            EndDate = DateTime.Now.ToShortDateString();
        //        }
        //        else
        //        {
        //            if (!IsValidDate(StartDate))
        //                return BadRequest("Required or invalid start date");

        //            if (!IsValidDate(EndDate))
        //                return BadRequest("Required or invalid end date");

        //            if (DateTime.Parse(StartDate) > DateTime.Parse(EndDate) || (DateTime.Parse(EndDate) < DateTime.Parse(StartDate)))
        //                return BadRequest("Start date must be prior to end date");
        //        }

        //        IEnumerable<EnergyUsage> modelData;
        //        modelData = ds.GetTrainingData(center, StartDate, EndDate);
        //        var mlModel = new MLModel(modelData, GetPath(center));
        //        mlModel.Train();
        //        return Ok($"{center.CenterAbbr} machine learning succesfully model trained.");
        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest(ex.InnerException.ToString());
        //    }
        //}

        [SwaggerImplementationNotes("Load and train the machine learning model for a center appending the latest Iconics data to the model. Parameters: BldgId: BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/Train/NewData/{BldgId}")]
        public IHttpActionResult TrainNow(string BldgId)
        {
            var ds = new DataService();
            var center = ds.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            var a = ds.GetMaxLoadDate(center);
            var z = DateTime.Now.ToString();
            try
            {
                IEnumerable<EnergyUsage> modelData;

                ds.StageTrainingData(center, a.ToString(), z.ToString());
                modelData = ds.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"], z);
                var mlModel = new PredictionEngine(modelData, GetPath(center));
                mlModel.FastTree();
             
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok($"{center.CenterAbbr} machine learning model trained with latest data starting {a.ToShortDateString()} to {z}");

        }

        [SwaggerImplementationNotes("Load and train the machine learning model for all centers appending the latest Iconics data to each model. Parameters: None")]
        [HttpGet]
        [Route("api/EnergyUsage/Train/NewData/All")]
        public IHttpActionResult TrainAllNow()
        {
            var ds = new DataService();
            var centers = ds.GetAllCenterConfigs();

            foreach (var center in centers)
            {
                var a = ds.GetMaxLoadDate(center);
                var z = DateTime.Now.ToString();
                try
                {
                    IEnumerable<EnergyUsage> modelData;

                    ds.StageTrainingData(center, a.ToString(), z);
                    modelData = ds.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"],z);
                    var mlModel = new PredictionEngine(modelData, GetPath(center));
                    mlModel.FastTree();

                }
                catch (Exception ex)
                {
                    return BadRequest(ex.InnerException.ToString());
                }
            }


            return Ok($"All center machine learning models successfully trained.");

        }

        [SwaggerImplementationNotes("Returns a summary of the Iconics data loaded for use in the enrgy prediction models.")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/SummaryReport")]
        public IHttpActionResult DataSummary()
        {
            var ds = new DataService();
            var mlContext = new MLContext();
            try
            {
                var centers = ds.GetAllCenterConfigs();
                var list = new List<MLModelDataSummary>();
                foreach (var c in centers)
                {
                    list.Add(ds.GetDataSummary(mlContext, GetPath(c), c));
                }

                return Ok(list.OrderBy(x => x.Center).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException.ToString());
            }

        }

        [SwaggerImplementationNotes("CAUTION: Will delete then reload Iconics data for each center. Parameters: None")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/All")]
        public IHttpActionResult IconicsAllCenters()
        {
            return Ok("Please contact IT to have all center data reloaded from Iconics.");

            var ds = new DataService();
            var centers = ds.GetAllCenterConfigs();
            var mlContext = new MLContext();
            var errorsList = new List<string>();

            foreach (var center in centers)
            {
                try
                {
                    ds.DeleteCenterData(center.CenterAbbr);

                    foreach (var d in LoadDates)
                    {
                        var a = d.Split(',')[0];
                        var z = d.Split(',')[1];
                        ds.StageTrainingData(center, a, z);
                    }

                    IEnumerable<EnergyUsage> modelData;
                    modelData = ds.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"], DateTime.Now.ToShortDateString());
                    ds.UpdateCenterConfig(center);
                    var mlModel = new PredictionEngine(modelData, GetPath(center));
                    mlModel.FastTree();

                    //var sum = ds.GetDataSummary(mlContext, GetPath(center), center);
                    //center.DataStartDate = DateTime.Parse("01/01/2017");
                    //center.DataEndDate = DateTime.Parse(sum.DataEndDate);
                    //center.DemandRecordCount = int.Parse(sum.DemandRecordCount);
                    //center.JoinedRecordCount = int.Parse(sum.JoinedCount);
                    //center.TemperatureRecordCount = int.Parse(sum.TemperatureRecordCount);
                    //center.RootMeanSquaredError = decimal.Parse(sum.ModelQuality.RootMeanSquaredError);
                    //center.RSquaredScore = decimal.Parse(sum.ModelQuality.RSquaredScore);
                    //center.ModelGrade = sum.ModelQuality.Grade;
                    //ds.UpdateCenterConfig(center);

                }

                catch (Exception ex)
                {
                    var msg = new StringBuilder();
                    msg.Append($"{center} - Message: { ex.Message}");
                    if (ex.InnerException != null)
                        msg.Append($"Inner Ex: { ex.InnerException.ToString()}");
                    errorsList.Add(msg.ToString());
                    continue;
                }

            }
            if (errorsList.Any())
                return Ok(errorsList);

            return Ok($"All data refreshed.");

        }

        [SwaggerImplementationNotes("CAUTION: Will delete then reload Iconics data for center provided. Parameters: BldgId: BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/{BldgId}")]
        public IHttpActionResult IconicsDataForCenter(string BldgId)
        {
            
            var ds = new DataService();
            var center = ds.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");
            return Ok($"Please contact IT to have {BldgId} center data reloaded from Iconics.");
            var dataList = new List<EnergyUsage>();

            ds.DeleteCenterData(center.CenterAbbr);

            try
            {
                foreach (var d in LoadDates)
                {
                    var a = d.Split(',')[0];
                    var z = d.Split(',')[1];
                    ds.StageTrainingData(center, a, z);
                };

                IEnumerable<EnergyUsage> modelData;
                modelData = ds.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"], DateTime.Now.ToShortDateString());
                center = ds.UpdateCenterConfig(center);
                var mlModel = new PredictionEngine(modelData, GetPath(center));
                mlModel.FastTree();

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

        private static string GetPath(EnergyUsageMachine.Models.CenterConfig center)
        {
            return Path.Combine(WebConfigurationManager.AppSettings["MLModelPath"], "Model_" + center.CenterAbbr.ToUpper() + ".zip");
        }

        private static string GetPath2(EnergyUsageMachine.Models.CenterConfig center, string trainer)
        {
            return Path.Combine(WebConfigurationManager.AppSettings["MLModelPath"], "Model_" + center.CenterAbbr.ToUpper() +"_"+ trainer + ".zip");
        }

    }

public class Obj
{
    public string TypeName { get; set; }
    public ITransformer TrainedModel { get; set; }
}
    
}


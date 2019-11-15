using EnergyUsageMachine.Services;
using System.Threading.Tasks;
using System.Web.Http;
using PowerUsageApi.SwaggerFilters;
using System.IO;
using Microsoft.ML;
using EnergyUsageMachine;
using EnergyUsageMachine.POCO;
using System.Collections.Generic;
using System;
using System.Web.Configuration;
using EnergyUsageMachine.ViewModels;
using System.Linq;
using System.Text;
using EnergyUsageMachine.Enums;
using EnergyUsageMachine.Models;


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
        readonly MLContext mlContext = new MLContext();
        readonly DataService dataService = new DataService();

        [SwaggerImplementationNotes("Returns a single energy usage prediction using the highest ranking model.")]
        [HttpPost]
        [Route("api/EnergyUsage/Predict/Single")]
        public IHttpActionResult PredictSingle(MLTestObject testObj)
        {
            if (string.IsNullOrEmpty(testObj.CenterAbbr))
                return BadRequest("3 character building abbreviation required");

            var center = dataService.GetCenterById((testObj.CenterAbbr.Substring(0, 3).ToUpper()));

            if (center == null)
                return BadRequest("Building not found");

            ITransformer trainedModel;
            var errorsList = new List<string>();

            try
            {
                var mlContext = new MLContext();
                if (!File.Exists(GetPath(center, center.BestTrainer)))
                    return BadRequest($"No machine learning model found for {center.CenterAbbr}");

                trainedModel = mlContext.Model.Load(GetPath(center, center.BestTrainer), out DataViewSchema modelSchema);

                var usagePrediction = new Prediction(trainedModel, testObj, center);
                var predictions = usagePrediction.PredictSingle();
                var eval = new Evaluate(mlContext, GetPath(center, center.BestTrainer), center).EvaluateModel();
                predictions.ModelQuality = eval;
                predictions.Center = center.CenterAbbr;
                predictions.TrainerUsed = center.BestTrainer;

                return Ok(predictions);
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append($"{center} - Message: { ex.Message}");
                if (ex.InnerException != null)
                    msg.Append($"Inner Ex: { ex.InnerException.ToString()}");
                errorsList.Add(msg.ToString());
                return Ok(errorsList);
            }

        }

        [SwaggerImplementationNotes("Returns the predicted next 24hrs. of energy usage for a center using the highest ranking model. Parameters: BldgId= BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/Predict/{BldgId}")]
        public IHttpActionResult EnergyUsage(string BldgId)
        {
            if (string.IsNullOrEmpty(BldgId))
                return BadRequest("3 character building abbreviation required");
          
            var center = dataService.GetCenterById(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            ITransformer trainedModel;
            var ws = new WeatherService();
            try
            {
                if (!File.Exists(GetPath(center, center.BestTrainer)))
                    return BadRequest($"No machine learning model found for {center.CenterAbbr}");

                trainedModel = mlContext.Model.Load(GetPath(center, center.BestTrainer), out DataViewSchema modelSchema);

                var weatherForeCast = Task.Run(async () => await ws.Get24HrForecast(center)).Result;
                if (!weatherForeCast.Periods.Any())
                    return BadRequest($"No weather data found for {center.CenterAbbr}");

                var usagePredictions = new Prediction(trainedModel, weatherForeCast, center);

                var results = usagePredictions.Predict();
                results.ModelUsed = center.BestTrainer;
                XMLHandler.GenerateXMLFile(results, center);

                return Ok(XMLHandler.SerializeXml<PredictionResult>(usagePredictions.Predict()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [SwaggerImplementationNotes("Returns an evaluation of each machine learning model ranked by quality.")]
        [HttpPost]
        [Route("api/EnergyUsage/EvaluateModels/")]
        public IHttpActionResult EvaluateModels(MLTestObject testObj)
        {
            List<Obj> trainedModels = new List<Obj>();

            if (string.IsNullOrEmpty(testObj.CenterAbbr))
                return BadRequest("3 character building abbreviation required");

            var center = dataService.GetCenterById((testObj.CenterAbbr.Substring(0, 3).ToUpper()));

            if (center == null)
                return BadRequest("Building not found");

            var errorsList = new List<string>();

            try
            {
                var modelData = dataService.GetTrainingDataForCenter(center);
                IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
                var o = new Obj()
                {
                    TypeName = RegressionTrainer.FastTree,
                    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastTree)).FastTree()

                };
                trainedModels.Add(o);

                o = new Obj()
                {
                    TypeName = RegressionTrainer.FastTreeTweedie,
                    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastTreeTweedie)).FastTreeTweedie()
                };
                trainedModels.Add(o);

                o = new Obj()
                {
                    TypeName = RegressionTrainer.FastForest,
                    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastForest)).FastForest()
                };
                trainedModels.Add(o);

                o = new Obj()
                {
                    TypeName = RegressionTrainer.PoissonRegression,
                    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.PoissonRegression)).PoissonRegression()
                };
                trainedModels.Add(o);
                o = new Obj()
                {
                    TypeName = RegressionTrainer.OnlineGradientDescent,
                    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.OnlineGradientDescent)).OnlineGradientDescent()
                };
                trainedModels.Add(o);
                o = new Obj()
                {
                    TypeName = RegressionTrainer.Gam,
                    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.Gam)).Gam()
                };
                trainedModels.Add(o);

                //o = new Obj()
                //{
                //    TypeName = RegressionTrainer.Sdca,
                //    TrainedModel = new PredictionEngine(modelData, GetPath(center, RegressionTrainer.Sdca)).Sdca()
                //};
                //trainedModels.Add(o);


                var predictionsList = new List<Predictions>();
                foreach (var tm in trainedModels)
                {
                    var usagePrediction = new Prediction(tm.TrainedModel, testObj, center);
                    var predictions = usagePrediction.PredictSingle();
                    var eval = new Evaluate(mlContext, GetPath(center, tm.TypeName), center).EvaluateModel();
                    predictions.ModelQuality = eval;
                    predictions.Center = center.CenterAbbr;
                    predictions.TrainerUsed = tm.TypeName;
                    predictionsList.Add(predictions);
                }
                return Ok(predictionsList.OrderByDescending(x => x.ModelQuality.RSquaredScore));
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append($"{center} - Message: { ex.Message}");
                if (ex.InnerException != null)
                    msg.Append($"Inner Ex: { ex.InnerException.ToString()}");
                errorsList.Add(msg.ToString());
                return Ok(errorsList);
            }

        }

        [SwaggerImplementationNotes("Load and train all machine learning models with the latest Iconics data. Parameters: None")]
        [HttpGet]
        [Route("api/EnergyUsage/Train/NewData/All")]
        public IHttpActionResult TrainAllNow()
        {
            var errorsList = new List<string>();
            var centers = dataService.GetAllCenters();

            foreach (var center in centers.OrderBy(x=> x.CenterAbbr))
            {
                try
                {
                    var a = GetStartDate(center);
                    var z = DateTime.Now;
                    dataService.StageTrainingData(center, a, z);
                    LoadTrainEvaluatePredictSave(center);
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

            return Ok(dataService.GetAllCenters());

        }

        [SwaggerImplementationNotes("Returns center settings and related Iconics data load metrics.")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/Metrics")]
        public IHttpActionResult Metrics()
        {
            var list = new List<vm_Center>();
            var data = dataService.GetAllCenters();
            foreach (var d in data)
            {
                var c = new vm_Center()
                {
                    BestTrainer = d.BestTrainer,
                    CenterAbbr = d.CenterAbbr,
                    CenterName = d.CenterName,
                    DataEndDate = d.DataEndDate.HasValue ? d.DataEndDate.Value.ToString() : null,
                    DataStartDate = d.DataStartDate.HasValue ? d.DataStartDate.Value.ToString() : null,
                    DataUpdatedDate = d.DateUpdated.HasValue ? d.DateUpdated.Value.ToString() : null,
                    DemandRecordCount = d.DemandRecordCount.HasValue ? d.DemandRecordCount.Value.ToString("#,##0") : null,
                    JoinedRecordCount = d.JoinedRecordCount.HasValue ? d.JoinedRecordCount.Value.ToString("#,##0") : null,
                    TemperatureRecordCount = d.TemperatureRecordCount.HasValue ? d.TemperatureRecordCount.Value.ToString("#,##0") : null,
                    //MatchQuality = GetMatchPercent(d),
                    ModelGrade = d.ModelGrade,
                    Region = d.Region,
                    RootMeanSquaredError = d.RootMeanSquaredError,
                    RSquaredScore = d.RSquaredScore,
                    WeatherURL = d.WeatherURL
                };
                list.Add(c);
            }
            return Ok(list);

            //var errorsList = new List<string>();
            //var list = new List<MLModelDataSummary>();
            //try
            //{
            //    foreach (var c in centers)
            //    {
            //        list.Add(dataService.GetDataSummary(mlContext, GetPath(c), c));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var msg = new StringBuilder();
            //    msg.Append($"Message: { ex.Message}");
            //    if (ex.InnerException != null)
            //        msg.Append($"Inner Ex: { ex.InnerException.ToString()}");
            //    errorsList.Add(msg.ToString());

            //}

            //if (errorsList.Any())
            //    return Ok(errorsList);

            //return Ok(list.OrderBy(x => x.Center).ToList());
        }

        [SwaggerImplementationNotes("CAUTION: Will wipe and reload ALL Iconics data since the beginning of time. Parameters: None")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/WipeReload")]
        public IHttpActionResult LoadAllIconicsData()
        {
            return Ok("Please contact IT to have all center data reloaded from Iconics.");

            var errorsList = new List<string>();
            var centers = dataService.GetAllCenters();

            foreach (var center in centers)
            {
                try
                {
                    dataService.DeleteCenterData(center.CenterAbbr);

                    foreach (var d in LoadDates)
                    {
                        var a = d.Split(',')[0];
                        var z = d.Split(',')[1];
                        dataService.StageTrainingData(center, DateTime.Parse(a), DateTime.Parse(z));
                    }
                    LoadTrainEvaluatePredictSave(center);
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

            return Ok(dataService.GetAllCenters());
        }

        #region Private  

        private void LoadTrainEvaluatePredictSave(Center center)
        {
            var predictionsList = new List<Predictions>();
            IEnumerable<EnergyUsage> modelData;

            modelData = dataService.GetTrainingDataForCenter(center);
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
            List<Obj> trainedModels = new List<Obj>();

            var o = new Obj()
            {
                DataView = dataView,
                TypeName = RegressionTrainer.FastTree,
                TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastTree)).FastTree()
            };
            trainedModels.Add(o);

            o = new Obj()
            {
                DataView = dataView,
                TypeName = RegressionTrainer.FastTreeTweedie,
                TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastTreeTweedie)).FastTreeTweedie()
            };
            trainedModels.Add(o);

            o = new Obj()
            {
                DataView = dataView,
                TypeName = RegressionTrainer.FastForest,
                TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastForest)).FastForest()
            };
            trainedModels.Add(o);

            o = new Obj()
            {
                DataView = dataView,
                TypeName = RegressionTrainer.PoissonRegression,
                TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.PoissonRegression)).PoissonRegression()
            };
            trainedModels.Add(o);
            o = new Obj()
            {
                DataView = dataView,
                TypeName = RegressionTrainer.OnlineGradientDescent,
                TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.OnlineGradientDescent)).OnlineGradientDescent()
            };
            trainedModels.Add(o);

            o = new Obj()
            {
                DataView = dataView,
                TypeName = RegressionTrainer.Gam,
                TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.Gam)).Gam()
            };
            trainedModels.Add(o);

            //o = new Obj()
            //{
            //    DataView = dataView,
            //    TypeName = RegressionTrainer.Sdca,
            //    TrainedModel = new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.Sdca)).Sdca()
            //};
            //trainedModels.Add(o);

            var rnd = new Random();
            var dayOfWeek = rnd.Next(1, 7);
            var hour = rnd.Next(0, 23);
            var minTemp = (int)modelData.Where(x => x.DayOfWeek == dayOfWeek && x.Hour == hour).Min(x => x.AvgTemp);
            var maxtemp = (int)modelData.Where(x => x.DayOfWeek == dayOfWeek && x.Hour == hour).Max(x => x.AvgTemp);

            MLTestObject testObj = new MLTestObject()
            {
                CenterAbbr = center.CenterAbbr,
                DayOfWeek = dayOfWeek,
                Hour = hour,
                Temperature = rnd.Next(minTemp, maxtemp)
            };

            foreach (var tm in trainedModels)
            {
                //Predict using current model
                var usagePrediction = new Prediction(tm.TrainedModel, testObj, center);
                var predictions = usagePrediction.PredictSingle();
                predictions.Center = center.CenterAbbr;
                predictions.TrainerUsed = tm.TypeName;
                predictionsList.Add(predictions);

                //Evaluate model
                var eval = new Evaluate(mlContext, GetPath(center, tm.TypeName), center).EvaluateModel(tm.DataView);
                predictions.ModelQuality = eval;
            }

            SelectBestModelThenSaveChanges(predictionsList, center);
        }
        
        private static string GetPath(Center center, string trainer)
        {
            return Path.Combine(WebConfigurationManager.AppSettings["MLModelPath"], "Model_" + center.CenterAbbr.ToUpper() + "_" + trainer + ".zip");
        }

        private void SelectBestModelThenSaveChanges(List<Predictions> list, Center center)
        {
            var bestModelForCenterList = new List<Predictions>();
           
            foreach (var grp in list.GroupBy(x=> x.Center))
            {
                var highestRank = grp.OrderByDescending(c => c.ModelQuality.RSquaredScore).First();
                bestModelForCenterList.Add(highestRank);
                center.BestTrainer = highestRank.TrainerUsed;
                center.ModelGrade = highestRank.ModelQuality.Grade;
                center.RootMeanSquaredError = decimalparse(highestRank.ModelQuality.RootMeanSquaredError.ToString());
                center.RSquaredScore = decimalparse(highestRank.ModelQuality.RSquaredScore.ToString());

                //record data load stats
                var rpt = dataService.GetDataSummary(mlContext, GetPath(center, center.BestTrainer), center);
                center.JoinedRecordCount = int.Parse(rpt.JoinedCount.Replace(",", ""));
                center.DemandRecordCount = int.Parse(rpt.DemandRecordCount.Replace(",", ""));
                center.TemperatureRecordCount = int.Parse(rpt.TemperatureRecordCount.Replace(",", ""));
                center.DataStartDate = DateTime.Parse(rpt.DataStartDate);
                center.DataEndDate = DateTime.Parse(rpt.DataEndDate);
                dataService.SaveChanges(center);
            }
        }

        private decimal decimalparse(string value)
        {
            decimal x;
            if (decimal.TryParse(value, out x))
                return x;
            return int.Parse(value);

        }

        private DateTime GetStartDate(Center center)
        {
            if (center.DataStartDate == null)
                return dataService.GetMaxDataLoadDate(center);
            else
                return center.DataStartDate.Value;
            
        }

        private string GetMatchPercent(Center d)
        {
            var drc = d.DemandRecordCount;
            var jrc = d.JoinedRecordCount;
            var trc = d.TemperatureRecordCount;

            if (drc.HasValue && jrc.HasValue && trc.HasValue)
            {

                var calc = decimal.Parse(jrc.ToString()) / (decimal.Parse(drc.ToString() + decimal.Parse(trc.ToString())));
                return calc.ToString("P1");
            }
            return "null";
        }

        #endregion

    }

    public class Obj
    {
        public string TypeName { get; set; }
        public ITransformer TrainedModel { get; set; }
        public IDataView DataView { get; set; }
    }
    
}


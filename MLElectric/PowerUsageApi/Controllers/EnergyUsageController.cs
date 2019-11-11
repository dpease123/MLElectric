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

        [SwaggerImplementationNotes("Returns a single energy usage prediction for testing purposes.")]
        [HttpPost]
        [Route("api/EnergyUsage/Predict/Single")]
        public IHttpActionResult PredictSingle(MLTestObject testObj)
        {
            if (string.IsNullOrEmpty(testObj.CenterAbbr))
                return BadRequest("3 character building abbreviation required");

            var center = dataService.GetCenterConfig((testObj.CenterAbbr.Substring(0, 3).ToUpper()));

            if (center == null)
                return BadRequest("Building not found");

            ITransformer trainedModel;
            var errorsList = new List<string>();

            try
            {
                var mlContext = new MLContext();
                if (!File.Exists(GetPath(center)))
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

        [SwaggerImplementationNotes("Best model contest...")]
        [HttpPost]
        [Route("api/EnergyUsage/Predict/BestModel")]
        public IHttpActionResult BestModel(MLTestObject testObj)
        {
            List<Obj> trainedModels = new List<Obj>();
            
            if (string.IsNullOrEmpty(testObj.CenterAbbr))
                return BadRequest("3 character building abbreviation required");

            var center = dataService.GetCenterConfig((testObj.CenterAbbr.Substring(0, 3).ToUpper()));

            if (center == null)
                return BadRequest("Building not found");

            var errorsList = new List<string>();
            var a = WebConfigurationManager.AppSettings["MLDataStartDate"];
            var z = DateTime.Now.ToString();

            try
            {
                var modelData = dataService.GetTrainingData(center, a, z);
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


        [SwaggerImplementationNotes("Best model contest...")]
        [HttpGet]
        [Route("api/EnergyUsage/Predict/BestModelForCenter")]
        public IHttpActionResult BestModelForCenter()
        {
           

            var centers = dataService.GetAllCenterConfigs();
            var errorsList = new List<string>();
            var predictionsList = new List<Predictions>();

            foreach (var center in centers)
            {
                List<Obj> trainedModels = new List<Obj>();
                try
                {
                    var modelData = dataService.GetTrainingDataForCenter(center);
                    IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
                    var o = new Obj()
                    {

                        DataView = dataView,
                        TypeName = RegressionTrainer.FastTree,
                        TrainedModel = ModelExists(GetPath(center, RegressionTrainer.FastTree)) ? 
                                                    mlContext.Model.Load(GetPath(center, RegressionTrainer.FastTree), out DataViewSchema modelSchema) 
                                                    : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastTree)).FastTree()
                        

                    };
                    trainedModels.Add(o);

                    o = new Obj()
                    {
                        DataView = dataView,
                        TypeName = RegressionTrainer.FastTreeTweedie,
                        TrainedModel = ModelExists(GetPath(center, RegressionTrainer.FastTreeTweedie)) ?
                                                    mlContext.Model.Load(GetPath(center, RegressionTrainer.FastTreeTweedie), out DataViewSchema modelSchema1)
                                                    : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastTreeTweedie)).FastTreeTweedie()
                    };
                    trainedModels.Add(o);

                    o = new Obj()
                    {
                        DataView = dataView,
                        TypeName = RegressionTrainer.FastForest,
                        TrainedModel = ModelExists(GetPath(center, RegressionTrainer.FastForest)) ?
                                                    mlContext.Model.Load(GetPath(center, RegressionTrainer.FastForest), out DataViewSchema modelSchema2)
                                                    : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastForest)).FastForest()
                    };
                    trainedModels.Add(o);

                    o = new Obj()
                    {
                        DataView = dataView,
                        TypeName = RegressionTrainer.PoissonRegression,
                        TrainedModel = ModelExists(GetPath(center, RegressionTrainer.PoissonRegression)) ?
                                                    mlContext.Model.Load(GetPath(center, RegressionTrainer.PoissonRegression), out DataViewSchema modelSchema3)
                                                    : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.PoissonRegression)).PoissonRegression()
                    };
                    trainedModels.Add(o);
                    o = new Obj()
                    {
                        DataView = dataView,
                        TypeName = RegressionTrainer.OnlineGradientDescent,
                        TrainedModel = ModelExists(GetPath(center, RegressionTrainer.OnlineGradientDescent)) ?
                                                    mlContext.Model.Load(GetPath(center, RegressionTrainer.OnlineGradientDescent), out DataViewSchema modelSchema4)
                                                    : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.OnlineGradientDescent)).OnlineGradientDescent()
                    };
                    trainedModels.Add(o);
                    o = new Obj()
                    {
                        DataView = dataView,
                        TypeName = RegressionTrainer.Gam,
                        TrainedModel = ModelExists(GetPath(center, RegressionTrainer.Gam)) ?
                                                    mlContext.Model.Load(GetPath(center, RegressionTrainer.Gam), out DataViewSchema modelSchema5)
                                                    : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.Gam)).Gam()
                    };
                    trainedModels.Add(o);

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
                        var usagePrediction = new Prediction(tm.TrainedModel, testObj, center);
                        var predictions = usagePrediction.PredictSingle();
                        var eval = new Evaluate(mlContext, GetPath(center, tm.TypeName), center).EvaluateModel(tm.DataView);
                        predictions.ModelQuality = eval;
                        predictions.Center = center.CenterAbbr;
                        predictions.TrainerUsed = tm.TypeName;
                        predictionsList.Add(predictions);
                    }
                   
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

            return Ok(SelectBestModelForCenters(predictionsList));
        }

        [SwaggerImplementationNotes("Returns the predicted next 24hrs. of energy usage for a building. Parameters: BldgId= BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/EnergyUsage/Predict/{BldgId}")]
        public IHttpActionResult EnergyUsage(string BldgId)
        {
            if (string.IsNullOrEmpty(BldgId))
                return BadRequest("3 character building abbreviation required");
          
            var center = dataService.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

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

        //        
        //        var centers = dataService.GetAllCenterConfigs();

        //        foreach (var center in centers)
        //        {
        //            IEnumerable<EnergyUsage> modelData;
        //            modelData = dataService.GetTrainingData(center, StartDate, EndDate);
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

        //        var dataService.= new DataService();
        //        var center = dataService.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

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
        //        modelData = dataService.GetTrainingData(center, StartDate, EndDate);
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
         
            var center = dataService.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            var a = dataService.GetMaxLoadDate(center);
            var z = DateTime.Now.ToString();
          
            try
            {
                dataService.StageTrainingData(center, a.ToString(), z.ToString());
                var modelData = dataService.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"], z);
                IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
                var mlModel = new PredictionEngine(dataView, mlContext, GetPath(center));
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
            var predictionsList = new List<Predictions>();
            var centers = dataService.GetAllCenterConfigs();
            IEnumerable<EnergyUsage> modelData;
            var errorsList = new List<string>();

            foreach (var center in centers)
            {
                var a = center.DataEndDate.ToString();
                var z = DateTime.Now.ToString();
                
                try
                {
                  
                    dataService.StageTrainingData(center, a.ToString(), z.ToString());

                    //record data load stats
                    var rpt = dataService.GetDataSummary(mlContext, GetPath(center), center);
                    center.JoinedRecordCount = int.Parse(rpt.JoinedCount.Replace(",", ""));
                    center.DemandRecordCount = int.Parse(rpt.DemandRecordCount.Replace(",", ""));
                    center.TemperatureRecordCount = int.Parse(rpt.TemperatureRecordCount.Replace(",", ""));
                    center.DataStartDate = DateTime.Parse(rpt.DataStartDate);
                    center.DataEndDate = DateTime.Parse(rpt.DataEndDate);
                    dataService.UpdateCenterConfig(center);

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
                            TrainedModel = ModelExists(GetPath(center, RegressionTrainer.FastForest)) ?
                                                        mlContext.Model.Load(GetPath(center, RegressionTrainer.FastForest), out DataViewSchema modelSchema2)
                                                        : new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.FastForest)).FastForest()
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
                            TrainedModel =  new PredictionEngine(dataView, mlContext, GetPath(center, RegressionTrainer.Gam)).Gam()
                        };
                        trainedModels.Add(o);

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
                            var usagePrediction = new Prediction(tm.TrainedModel, testObj, center);
                            var predictions = usagePrediction.PredictSingle();
                            var eval = new Evaluate(mlContext, GetPath(center, tm.TypeName), center).EvaluateModel(tm.DataView);
                            predictions.ModelQuality = eval;
                            predictions.Center = center.CenterAbbr;
                            predictions.TrainerUsed = tm.TypeName;
                            predictionsList.Add(predictions);
                        }

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

            SelectBestModelForCenters(predictionsList);
            return Ok($"All center machine learning models successfully trained.");

        }

        [SwaggerImplementationNotes("Returns a summary of the Iconics data loaded for use in the enrgy prediction models.")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/SummaryReport")]
        public IHttpActionResult DataSummary()
        {
            var errorsList = new List<string>();
            var centers = dataService.GetAllCenterConfigs();
                var list = new List<MLModelDataSummary>();
            try
            {
                
                foreach (var c in centers)
                {
                    var rpt = dataService.GetDataSummary(mlContext, GetPath(c), c);
                    list.Add(rpt);
                    c.JoinedRecordCount = int.Parse(rpt.JoinedCount.Replace(",",""));
                    c.DemandRecordCount = int.Parse(rpt.DemandRecordCount.Replace(",", ""));
                    c.TemperatureRecordCount = int.Parse(rpt.TemperatureRecordCount.Replace(",", ""));
                    c.DataStartDate = DateTime.Parse(rpt.DataStartDate);
                    c.DataEndDate = DateTime.Parse(rpt.DataEndDate);

                    dataService.UpdateCenterConfig(c);
                }

               
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.Append($"Message: { ex.Message}");
                if (ex.InnerException != null)
                    msg.Append($"Inner Ex: { ex.InnerException.ToString()}");
                errorsList.Add(msg.ToString());
                
            }

            if (errorsList.Any())
                return Ok(errorsList);

            return Ok(list.OrderBy(x => x.Center).ToList());

        }

        [SwaggerImplementationNotes("CAUTION: Will delete then reload Iconics data for each center. Parameters: None")]
        [HttpGet]
        [Route("api/EnergyUsage/IconicsData/All")]
        public IHttpActionResult IconicsAllCenters()
        {
            return Ok("Please contact IT to have all center data reloaded from Iconics.");

           
            var centers = dataService.GetAllCenterConfigs();
            var errorsList = new List<string>();

            foreach (var center in centers)
            {
                try
                {
                    dataService.DeleteCenterData(center.CenterAbbr);

                    foreach (var d in LoadDates)
                    {
                        var a = d.Split(',')[0];
                        var z = d.Split(',')[1];
                        dataService.StageTrainingData(center, a, z);
                    }

                    var modelData = dataService.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"], DateTime.Now.ToShortDateString());
                    IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
                    var mlModel = new PredictionEngine(dataView, mlContext, GetPath(center));
                    dataService.UpdateCenterConfig(center);
                    mlModel.FastTree();

                    //var sum = dataService.GetDataSummary(mlContext, GetPath(center), center);
                    //center.DataStartDate = DateTime.Parse("01/01/2017");
                    //center.DataEndDate = DateTime.Parse(sum.DataEndDate);
                    //center.DemandRecordCount = int.Parse(sum.DemandRecordCount);
                    //center.JoinedRecordCount = int.Parse(sum.JoinedCount);
                    //center.TemperatureRecordCount = int.Parse(sum.TemperatureRecordCount);
                    //center.RootMeanSquaredError = decimal.Parse(sum.ModelQuality.RootMeanSquaredError);
                    //center.RSquaredScore = decimal.Parse(sum.ModelQuality.RSquaredScore);
                    //center.ModelGrade = sum.ModelQuality.Grade;
                    //dataService.UpdateCenterConfig(center);

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
            
            var center = dataService.GetCenterConfig(BldgId.Substring(0, 3).ToUpper());

            if (center == null)
                return BadRequest("Building not found");

            return Ok($"Please contact IT to have {BldgId} center data reloaded from Iconics.");

            dataService.DeleteCenterData(center.CenterAbbr);

            try
            {
                foreach (var d in LoadDates)
                {
                    var a = d.Split(',')[0];
                    var z = d.Split(',')[1];
                    dataService.StageTrainingData(center, a, z);
                };

                var modelData = dataService.GetTrainingData(center, WebConfigurationManager.AppSettings["MLDataStartDate"], DateTime.Now.ToShortDateString());
                IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
                var mlModel = new PredictionEngine(dataView, mlContext, GetPath(center));
                dataService.UpdateCenterConfig(center);
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

        private static string GetPath(EnergyUsageMachine.Models.CenterConfig center, string trainer)
        {
            return Path.Combine(WebConfigurationManager.AppSettings["MLModelPath"], "Model_" + center.CenterAbbr.ToUpper() +"_"+ trainer + ".zip");
        }

        private List<Predictions> SelectBestModelForCenters(List<Predictions> list)
        {
            var bestModelForCenterList = new List<Predictions>();
           
            foreach (var grp in list.GroupBy(x=> x.Center))
            {
                var center = dataService.GetCenterConfig(grp.Key);
                var highestRank = grp.OrderByDescending(c => c.ModelQuality.RSquaredScore).First();
                bestModelForCenterList.Add(highestRank);
                center.BestTrainer = highestRank.TrainerUsed;
                center.ModelGrade = highestRank.ModelQuality.Grade;
                center.RootMeanSquaredError = decimalparse(highestRank.ModelQuality.RootMeanSquaredError.ToString());
                center.RSquaredScore = decimalparse(highestRank.ModelQuality.RSquaredScore.ToString());
                dataService.UpdateCenterConfig(center);
            }
            return bestModelForCenterList;
        }

        private bool ModelExists(string modelPath)
        {
            return (File.Exists(modelPath)) ? true : false;
        }

        private decimal decimalparse(string value)
        {
            decimal x;
            if (decimal.TryParse(value, out x))
                return x;
            return int.Parse(value);

        }
     
    }

    public class Obj
    {
        public string TypeName { get; set; }
        public ITransformer TrainedModel { get; set; }
        public IDataView DataView { get; set; }
    }
    
}


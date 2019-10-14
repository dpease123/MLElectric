using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ML;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Services;
using EnergyUsageMachine;
using EnergyUsageMachine.Data;

namespace MLElectric
{
    class Program
    {

        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");
        static readonly string BEV = "https://api.weather.gov/points/34.0735,-118.3777";
        static readonly WeatherService ws = new WeatherService();
        static readonly HyperHistorianRepository _repo = new HyperHistorianRepository();

        static IEnumerable<EnergyUsage> modelData = new List<EnergyUsage>();
        static List<Forecast> next24hourForecast = new List<Forecast>();

        static void Main(string[] args)
        {
            var MLContext = new MLContext();
            var foreCast = Task.Run(async () => await ws.Get24HrForecast(BEV)).Result;
            var modelData = _repo.GetTrainingData("BEV");


            Console.WriteLine();
            Console.WriteLine($"*****STARTING TRAINING");
           
            var MLModel = new EnergyUsageMachine.Model();
            var trainedModel = MLModel.Train(MLContext);

            Console.WriteLine();
            Console.WriteLine($"*****STARTING EVALUATE");
            var ev = new Evaluate(MLContext, trainedModel, modelData);

            Console.WriteLine();
            Console.WriteLine($"*****STARTING PREDICT");
            TestSinglePrediction(MLContext, trainedModel, foreCast.Next24Hours);


            Console.Read();

        }

        public static ITransformer Train(MLContext mlContext)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);

            return model;
        }


        private static void Evaluate(MLContext mlContext, ITransformer model)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
            var predictions = model.Transform(dataView);
            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");
            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");

        }

        private static void TestSinglePrediction(MLContext mlContext, ITransformer loadedModel, List<Period> foreCast)
        {
            var predictionList = new List<PredictionResult>();
            var predictionFunction = mlContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(loadedModel);
            var xml = new XMLHandler();

          
            Console.WriteLine($"Prediction Date: {foreCast.First().startTime.ToShortDateString()}");
            foreach (var fc in foreCast)
            {
                var test = new EnergyUsage()
                {

                    Center = "Taubman/Region 34/Beverly Center/Temperature_F",
                    DayOfWeek = (int)fc.startTime.DayOfWeek,
                    Hour = fc.startTime.Hour,
                    AvgTemp = fc.temperature,
                    kWH = 0 // To
                };

                var prediction = predictionFunction.Predict(test);
                Console.WriteLine($"**********************************************************************");
                Console.WriteLine($"Predicted kWH: {prediction.kWH:0.####}, actual temp: {test.AvgTemp}, Hour: {test.Hour}");
                Console.WriteLine($"**********************************************************************");
                var pr = new PredictionResult()
                {
                    kWH_Usage = prediction.kWH,
                    Hour = test.Hour

                };
                predictionList.Add(pr);
            }

            xml.GenerateXML(predictionList);

            Console.ReadLine();

        }

    } 

}


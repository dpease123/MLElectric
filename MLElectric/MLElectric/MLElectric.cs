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
            var prediction = new Prediction(MLContext, trainedModel, foreCast.Next24Hours);
            var predictions = prediction.Predict();


            XMLHandler.GenerateXML(predictions);
            Console.Read();

        }
    }

}


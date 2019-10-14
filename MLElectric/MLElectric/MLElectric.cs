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
using System.Configuration;
using EnergyUsageMachine.Enums;

namespace MLElectric
{
    class Program
    {

        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");
               static readonly WeatherService ws = new WeatherService();
        static readonly HyperHistorianRepository _repo = new HyperHistorianRepository();

        static readonly IEnumerable<EnergyUsage> modelData = new List<EnergyUsage>();
        static readonly List<Forecast> next24hourForecast = new List<Forecast>();

        static void Main(string[] args)
        {
            var center = CenterEnum.Find("BEV");
            var startDate = "01/01/2018";
            var endDate = "01/01/2020";

            try
            {
                var MLContext = new MLContext();
                var foreCast = Task.Run(async () => await ws.Get24HrForecast(center._weatherURL)).Result;
                var modelData = _repo.GetTrainingData(center, startDate, endDate);

                Console.WriteLine($"*****STARTING TRAINING");

                var MLModel = new MLModel(MLContext, modelData);
                var trainedModel = MLModel.Train();

                //Console.WriteLine($"*****STARTING EVALUATE");
                //var ev = new Evaluate(MLContext, trainedModel, modelData);

                Console.WriteLine($"*****STARTING PREDICT");
                var prediction = new Prediction(MLContext, trainedModel, foreCast.Next24Hours);
                var predictions = prediction.Predict();

                XMLHandler.GenerateXML(predictions);
                Console.Read();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();

        }
    }

}


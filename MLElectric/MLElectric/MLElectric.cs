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

        static string modelSavePath = Path.Combine(@"C:\Temp\ML", "Model.zip");
        static readonly WeatherService ws = new WeatherService();
        static readonly HyperHistorianRepository _repo = new HyperHistorianRepository();
        static readonly IEnumerable<EnergyUsage> modelData = new List<EnergyUsage>();
        static readonly List<Forecast> next24hourForecast = new List<Forecast>();

        static void Main(string[] args)
        {
            //var input = "CCK";
            //var center = CenterEnum.Find(input);
            //var startDate = "01/01/2018";
            //var endDate = "01/01/2020";
            //modelSavePath = modelSavePath.Replace("Model", "Model_" + input);
            //try
            //{
            //    Console.WriteLine($"Running predictions for: {center._centerAbbr}");
            //    var mlContext = new MLContext();
            //    var foreCast = Task.Run(async () => await ws.Get24HrForecast(center._weatherURL)).Result;
            //    ITransformer trainedModel;
            //    if(!File.Exists(modelSavePath))
            //        Train(input, center, startDate, endDate, out mlContext, out trainedModel);
            //    else
            //        Console.WriteLine("Skip training..already have a trained model");


            //    //Console.WriteLine($"*****STARTING EVALUATE");
            //    //var ev = new Evaluate(MLContext, trainedModel, modelData);
            //    Console.WriteLine($"Predicting 24hr usage for:  {center._centerAbbr}");
            //    trainedModel = Predict(mlContext, foreCast, center);
            //    Console.WriteLine("Generated predcitions in XML.");
            //    Console.WriteLine("Done");
            //    Console.Read();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            Console.ReadLine();

        }

        //private static void Train(string input, Center center, string startDate, string endDate, out MLContext mlContext, out ITransformer trainedModel)
        //{
           
        //    mlContext = new MLContext();
           
        //    var modelData = _repo.GetTrainingData(center, startDate, endDate);

        //    Console.WriteLine($"Training model for: {center._centerAbbr}");

        //    var MLModel = new MLModel(mlContext, modelData, modelSavePath);
        //    trainedModel = MLModel.Train();
        //}

        //private static ITransformer Predict(MLContext mlContext, Forecast foreCast, Center c)
        //{
        //    ITransformer trainedModel;
        //    trainedModel = mlContext.Model.Load(modelSavePath, out DataViewSchema modelSchema);
           
        //    var prediction = new Prediction(mlContext, trainedModel, foreCast.Next24Hours);
        //    var predictions = prediction.Predict();

        //    XMLHandler.GenerateXML(predictions, c);
        //    return trainedModel;
        //}
    }

}


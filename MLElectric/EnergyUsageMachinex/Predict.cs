using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;
using Newtonsoft.Json;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Data;
using EnergyUsageMachinex;

namespace EnergyUsageMachine
{
    public class Predict
    {
        private static void TestSinglePrediction(MLContext mlContext, ITransformer loadedModel, List<Period> foreCast)
        {
            var predictionList = new List<PredictionResult>();
            var predictionFunction = mlContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(loadedModel);



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

                //Console.WriteLine($"Predicted kWH: {prediction.kWH:0.####}, actual temp: {test.AvgTemp}, Hour: {test.Hour}");

                var pr = new PredictionResult()
                {
                    kWH_Usage = prediction.kWH,
                    Hour = test.Hour

                };
                predictionList.Add(pr);
            }

            var xml = new XMLHandler();
            xml.GenerateXML(predictionList);
        }
    }
}

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
    public class Prediction
    {

        private readonly MLContext _MLContext;
        private readonly ITransformer _model;
        private readonly List<Period> _forecast;

        public Prediction(MLContext mlContext, ITransformer model, List<Period> foreCast)
        {
            _model = model;
            _MLContext = mlContext;
            _forecast = foreCast;
        }
        public List<PredictionResult> Predict()
        {
            var PredictionResultList = new List<PredictionResult>();
            var predictionFunction = _MLContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(_model);



            foreach (var fc in _forecast)
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
                PredictionResultList.Add(pr);
            }

            return PredictionResultList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;
using Newtonsoft.Json;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Data;
using EnergyUsageMachine;

namespace EnergyUsageMachine
{
    public class Prediction
    {
        private readonly ITransformer _trainedModel;
        private readonly Forecast _forecast;

        public Prediction(ITransformer trainedModel, Forecast forecast)
        {
            _trainedModel = trainedModel;
            _forecast = forecast;
        }

        public List<PredictionResult> Predict()
        {
            var _MLContext = new MLContext();
            var PredictionResultList = new List<PredictionResult>();
            var predictionFunction = _MLContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(_trainedModel);



            foreach (var fc in _forecast.Periods)
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

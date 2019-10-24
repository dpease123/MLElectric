using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;
using Newtonsoft.Json;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Data;
using EnergyUsageMachine;
using EnergyUsageMachine.Models;
using EnergyUsageMachine.ViewModels;

namespace EnergyUsageMachine
{
    public class Prediction
    {
        private readonly ITransformer _trainedModel;
        private readonly Forecast _forecast;
        private readonly CenterConfig _center;
        private readonly MLTestObject _testObj;

        public Prediction(ITransformer trainedModel, Forecast forecast, CenterConfig center)
        {
            _trainedModel = trainedModel;
            _forecast = forecast;
            _center = center;
        }

        public Prediction(ITransformer trainedModel, MLTestObject testObj, CenterConfig center)
        {
            _trainedModel = trainedModel;
            _center = center;
            _testObj = testObj;
        }

        public PredictionResult Predict()
        {
            var _MLContext = new MLContext();
            var PredictionResultList = new List<Predictions>();
            var predictionFunction = _MLContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(_trainedModel);
            var results = new PredictionResult
            {
                Center = _center.CenterAbbr
            };


            foreach (var fc in _forecast.Periods)
            {
                var test = new EnergyUsage()
                {

                    Center = _center.CenterAbbr,
                    DayOfWeek = (int)fc.startTime.DayOfWeek,
                    Hour = fc.startTime.Hour,
                    AvgTemp = fc.temperature,
                    kWH = 0 
                };

                var prediction = predictionFunction.Predict(test);
                var pr = new Predictions()
                {
                    kWH_Usage = prediction.kWH,
                    Hour = test.Hour

                };
                results.Predictions.Add(pr);
            }

            return results;
        }

        public Predictions PredictSingle()
        {
            var _MLContext = new MLContext();
            var PredictionResultList = new List<Predictions>();
            var predictionFunction = _MLContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(_trainedModel);

                var test = new EnergyUsage()
                {
                    Center = _center.CenterAbbr,
                    DayOfWeek = (int)_testObj.DayOfWeek,
                    Hour = _testObj.Hour,
                    AvgTemp = _testObj.Temperature,
                    kWH = 0 // To
                };

                var prediction = predictionFunction.Predict(test);
                var predictionResult = new Predictions()
                {
                    kWH_Usage = prediction.kWH,
                    Hour = test.Hour
                };

            return predictionResult;
        }
    }
}

using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;
using EnergyUsageMachine.ViewModels;
using EnergyUsageMachine.Services;
using EnergyUsageMachine.Models;

namespace EnergyUsageMachine
{
    public class Evaluate
    {
        private readonly MLContext _MLContext;
        public readonly string _modelPath;
        private readonly CenterConfig _centerConfig;
        private readonly Predictions _prediction;
        public Evaluate(MLContext mlContext, string modelPath, Predictions prediction, CenterConfig center)
        {
            //_model = model;
            _MLContext = mlContext;
            _centerConfig = center;
            _modelPath = modelPath;
            _prediction = prediction;
        }

        public Predictions EvaluateModel()
        {
            var ds = new DataService();
            var data = ds.GetTrainingData(_centerConfig, "01/01/2017", DateTime.Now.ToString());
            IDataView dataView = _MLContext.Data.LoadFromEnumerable<EnergyUsage>(data);
            var trainedModel = _MLContext.Model.Load(_modelPath, out DataViewSchema modelSchema);
            var predictions = trainedModel.Transform(dataView);
            var metrics = _MLContext.Regression.Evaluate(predictions, "Label", "Score");
            //Model quality metrics evaluation    
            _prediction.RSquaredScore = ($"{metrics.RSquared:0.##}");
            _prediction.RootMeanSquaredError = ($"{metrics.RootMeanSquaredError:#.##}");
            return _prediction;

        }
    }
}

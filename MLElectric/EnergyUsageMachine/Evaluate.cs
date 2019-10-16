using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;


namespace EnergyUsageMachine
{
    public class Evaluate
    {
        private readonly MLContext _MLContext;
        private readonly ITransformer _model;
        private readonly IEnumerable<EnergyUsage> _modelData;
        public Evaluate(MLContext mlContext, ITransformer model, IEnumerable<EnergyUsage> modelData)
        {
            _model = model;
            _MLContext = mlContext;
            _modelData = modelData;
        }

        public void EvaluateModel()
        {
            IDataView dataView = _MLContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var predictions = _model.Transform(dataView);
            var metrics = _MLContext.Regression.Evaluate(predictions, "Label", "Score");
            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");

        }
    }
}

using EnergyUsageMachine.POCO;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;


namespace EnergyUsageMachine
{
    public class Evaluate
    {
        static IEnumerable<EnergyUsage> modelData = new List<EnergyUsage>();
        private static void EvaluateModel(MLContext mlContext, ITransformer model)
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
    }
}

using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Data;

using System;

namespace EnergyUsageMachine
{
    public class Model
    {
        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");
        static readonly string BEV = "https://api.weather.gov/points/34.0735,-118.3777";
        static IEnumerable<EnergyUsage> modelData = new List<EnergyUsage>();
        static List<Forecast> next24hourForecast = new List<Forecast>();
        public ITransformer Train(MLContext mlContext)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.Sdca());

            var model = pipeline.Fit(dataView);

            return model;
        }


      

      

       

      

       

        
    }
}


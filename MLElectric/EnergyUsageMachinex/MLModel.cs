using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Data;

using System;

namespace EnergyUsageMachine
{
    public class MLModel
    {
        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");
        private readonly MLContext _mlContext;
        private readonly IEnumerable<EnergyUsage> _modelData;
       
        public MLModel(MLContext mlContext, IEnumerable<EnergyUsage> modelData)
        {
            _mlContext = mlContext;
            _modelData = modelData;
        }
        public ITransformer Train()
        {
            IDataView dataView = _mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.Sdca());

            var model = pipeline.Fit(dataView);

            return model;
        }


      

      

       

      

       

        
    }
}


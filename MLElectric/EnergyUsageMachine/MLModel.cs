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
        
        private readonly MLContext _mlContext;
        private readonly IEnumerable<EnergyUsage> _modelData;
        private readonly string _modelSavePath;
       
        public MLModel(MLContext mlContext, IEnumerable<EnergyUsage> modelData, string modelSavePath)
        {
            _mlContext = mlContext;
            _modelData = modelData;
            _modelSavePath = modelSavePath;
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
                .Append(_mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            _mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }


      

      

       

      

       

        
    }
}


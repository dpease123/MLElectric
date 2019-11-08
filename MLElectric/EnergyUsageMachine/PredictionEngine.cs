using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Data;


using System;

namespace EnergyUsageMachine
{
    public class PredictionEngine
    {
        
        private readonly IEnumerable<EnergyUsage> _modelData;
        private readonly string _modelSavePath;
        private readonly MLContext _mlContext;
        private IDataView _dataView;
        public PredictionEngine(IDataView dataView, MLContext mlContext, string modelSavePath)
        {
            _dataView = dataView;
            _mlContext = mlContext;
            _modelSavePath = modelSavePath;
        }
       
        public ITransformer FastTree()
        {
           
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);
           
            return model;
        }

        public ITransformer FastTreeTweedie()
        {
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.FastTreeTweedie());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer Sdca()
        {
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.Sdca());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer FastForest()
        {
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.FastForest());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);
           

            return model;
        }

        public ITransformer PoissonRegression()
        {
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.LbfgsPoissonRegression());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer OnlineGradientDescent()
        {
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.OnlineGradientDescent());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer Gam()
        {
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(_mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(_mlContext.Regression.Trainers.Gam());

            var model = pipeline.Fit(_dataView);
            _mlContext.Model.Save(model, _dataView.Schema, _modelSavePath);


            return model;
        }















    }
}


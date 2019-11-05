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
        
        private readonly IEnumerable<EnergyUsage> _modelData;
        private readonly string _modelSavePath;
       
        public MLModel(IEnumerable<EnergyUsage> modelData, string modelSavePath)
        {
            _modelData = modelData;
            _modelSavePath = modelSavePath;
        }
        public ITransformer FastTree()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer FastTreeTweedie()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.FastTreeTweedie());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer Sdca()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.Sdca());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer FastForest()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.FastForest());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer PoissonRegression()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer OnlineGradientDescent()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.OnlineGradientDescent());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }

        public ITransformer Gam()
        {
            var mlContext = new MLContext();
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(_modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.Gam());

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelSavePath);


            return model;
        }















    }
}


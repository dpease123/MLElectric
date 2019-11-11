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
        private readonly Center _centerConfig;

        public Evaluate(MLContext mlContext, string modelPath, Center center)
        {
            //_model = model;
            _MLContext = mlContext;
            _centerConfig = center;
            _modelPath = modelPath;
       
        }


        public EvaluateModel EvaluateModel()
        {
            var ds = new DataService();
            var data = ds.GetTrainingDataForCenter(_centerConfig);
            IDataView dataView = _MLContext.Data.LoadFromEnumerable<EnergyUsage>(data);
            var trainedModel = _MLContext.Model.Load(_modelPath, out DataViewSchema modelSchema);
            var predictions = trainedModel.Transform(dataView);
            var metrics = _MLContext.Regression.Evaluate(predictions, "Label", "Score");

            /*<<RMSE - Root mean squared erro>>
            * The RMSE is the square root of the variance of the residuals. It indicates the absolute fit of the model 
            to the data–how close the observed data points are to the model's predicted values. RMSE can be interpreted as 
            the standard deviation of the unexplained variance, and has the useful property of being in the same units 
            as the response variable. Lower values of RMSE indicate better fit. 
            RMSE is a good measure of how accurately the model predicts the response, and it is the most important criterion 
            for fit if the main purpose of the model is prediction.*/

            /*<<RSquared>>
            * R-squared is a relative measure of fit, RMSE is an absolute measure of fit. R-squared has the useful 
            property that its scale is intuitive: it ranges from zero to one, with zero indicating that the proposed model 
            does not improve prediction over the mean model, and one indicating perfect prediction. Improvement in the 
            regression model results in proportional increases in R-squared.*/


            var ev = new EvaluateModel()
            {
                RSquaredScore = ($"{metrics.RSquared: 0.##}"),
                RootMeanSquaredError = ($"{metrics.RootMeanSquaredError: #.##}"),
                Grade = string.Empty
            };
            ev.Grade = GradeModel(ev.RSquaredScore);
            return ev;
        }

        public EvaluateModel EvaluateModel(IDataView dataView)
        {
            var trainedModel = _MLContext.Model.Load(_modelPath, out DataViewSchema modelSchema);
            var predictions = trainedModel.Transform(dataView);
            var metrics = _MLContext.Regression.Evaluate(predictions, "Label", "Score");

            /*<<RMSE - Root mean squared erro>>
            * The RMSE is the square root of the variance of the residuals. It indicates the absolute fit of the model 
            to the data–how close the observed data points are to the model's predicted values. RMSE can be interpreted as 
            the standard deviation of the unexplained variance, and has the useful property of being in the same units 
            as the response variable. Lower values of RMSE indicate better fit. 
            RMSE is a good measure of how accurately the model predicts the response, and it is the most important criterion 
            for fit if the main purpose of the model is prediction.*/

            /*<<RSquared>>
            * R-squared is a relative measure of fit, RMSE is an absolute measure of fit. R-squared has the useful 
            property that its scale is intuitive: it ranges from zero to one, with zero indicating that the proposed model 
            does not improve prediction over the mean model, and one indicating perfect prediction. Improvement in the 
            regression model results in proportional increases in R-squared.*/


            var ev = new EvaluateModel()
            {
                RSquaredScore = ($"{metrics.RSquared: 0.##}"),
                RootMeanSquaredError = ($"{metrics.RootMeanSquaredError: #.##}"),
                Grade = string.Empty
            };
            ev.Grade = GradeModel(ev.RSquaredScore);
            return ev;
        }



        private string GradeModel(string score)
        {
            var grade = "Unknown";
            double x;
            if (double.TryParse(score, out x))
            {
                if (x > 1 || x < 0)
                    return "?";
                if (x >= .81)
                    grade = "A";
                if (x >= .61 && x <= .80)
                    grade = "B";
                if (x >= .41 && x <= .60)
                    grade = "C";
                if (x >= .21 && x <= .40)
                    grade = "D";
                if (x <= .20)
                    grade = "F";
                
            }
            return grade;


        }

    }
}
          
   

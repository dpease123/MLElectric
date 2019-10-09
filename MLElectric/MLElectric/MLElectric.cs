using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ML;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using MLElectric.POCO;
using MLElectric.Data;
using System.Data.SqlClient;
using System.Data;

namespace MLElectric
{
    class Program
    {

        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");
        static readonly string BEV = "https://api.weather.gov/points/34.0735,-118.3777";
        static List<CenterTemp_History> centerTempData =  new List<CenterTemp_History>();
        static List<CenterkWhUsage_History> centerEnergyUsageData = new List<CenterkWhUsage_History>();
        static IEnumerable<EnergyUsage> modelData = new List<EnergyUsage>();
        static List<Forecast> next24hourForecast = new List<Forecast>();
        static void Main(string[] args)
        {
            var foreCast = Task.Run(async () => await Get24HourWeatherForecast(BEV)).Result;
            //next24hourForecast = ; 
            centerTempData = GetTemperatureData();
            centerEnergyUsageData = GetEnergyUsageData();
            modelData = MergeData(centerTempData, centerEnergyUsageData);
            Console.WriteLine();
            Console.WriteLine($"*****STARTING TRAINING");
            MLContext mlContext = new MLContext(seed: 0);
            var model = Train(mlContext);

            Console.WriteLine();
            Console.WriteLine($"*****STARTING EVALUATE");
            Evaluate(mlContext, model);

            Console.WriteLine();
            Console.WriteLine($"*****STARTING PREDICT");
            TestSinglePrediction(mlContext, model, foreCast.Next24Hours);


            Console.Read();

        }

        public static ITransformer Train(MLContext mlContext)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<EnergyUsage>(modelData);
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "kWH")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CenterEncoded", inputColumnName: "Center"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DayOfWeekEncoded", inputColumnName: "DayOfWeek"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HourEncoded", inputColumnName: "Hour"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "AvgTempEncoded", inputColumnName: "AvgTemp"))
                .Append(mlContext.Transforms.Concatenate("Features", "CenterEncoded", "DayOfWeekEncoded", "HourEncoded", "AvgTempEncoded"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);

            return model;
        }


        private static void Evaluate(MLContext mlContext, ITransformer model)
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

        private static void TestSinglePrediction(MLContext mlContext, ITransformer loadedModel, List<Period> foreCast)
        {

            var predictionFunction = mlContext.Model.CreatePredictionEngine<EnergyUsage, EnergyUsagePrediction>(loadedModel);

          
            Console.WriteLine($"Prediction Date: {foreCast.First().startTime.ToShortDateString()}");
            foreach (var fc in foreCast)
            {
                var test = new EnergyUsage()
                {

                    Center = "Taubman/Region 34/Beverly Center/Temperature_F",
                    DayOfWeek = (int)fc.startTime.DayOfWeek,
                    Hour = fc.startTime.Hour,
                    AvgTemp = fc.temperature,
                    kWH = 0 // To
                };

                var prediction = predictionFunction.Predict(test);
                Console.WriteLine($"**********************************************************************");
                Console.WriteLine($"Predicted kWH: {prediction.kWH:0.####}, actual temp: {test.AvgTemp}, Hour: {test.Hour}");
                Console.WriteLine($"**********************************************************************");
            }
           


            Console.WriteLine();
            Console.WriteLine($"*** PRESS <ENTER> TO CLOSE  ***");
            Console.ReadLine();

        }

        static async Task<Forecast> Get24HourWeatherForecast(string path)
        {

            Forecast Forecast = new Forecast();
            Weather weather = new Weather();
            var client = new HttpClient();
            try
            {
               
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Taubman/1.0");
                var centerWeatherDetails = await client.GetStringAsync(path);
                Forecast = JsonConvert.DeserializeObject<Forecast>(centerWeatherDetails);
           
                var WeatherJson = await client.GetStringAsync(Forecast.ForecastURLs.forecastHourly);
                weather = JsonConvert.DeserializeObject<Weather>(WeatherJson);
                Forecast.Next24Hours = weather.properties.periods.Take(25).ToList();

                return Forecast;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Forecast;
        }

        static List<CenterTemp_History> GetTemperatureData()
        {
            var db = new HyperHistorianContext();
            var temperatureList = new List<CenterTemp_History>();
          
            object[] xparams = {
            new SqlParameter("@level",3),
            new SqlParameter("@regionName", "Region 34"),
            new SqlParameter("@mallName", "Beverly Center"),
            new SqlParameter("@startTime", "01/01/2019"),
            new SqlParameter("@endTime", "10/01/2019"),
            new SqlParameter("@sampleInterval", 1),
            new SqlParameter("@tagName", "Temperature_F")
            };

            try
            {

            temperatureList = db.Database.SqlQuery<CenterTemp_History>(
            "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @level, @regionName, @mallName, @startTime, @endTime, @sampleInterval, @tagName",
             xparams).ToList();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return temperatureList;
            
        }

        static List<CenterkWhUsage_History> GetEnergyUsageData()
        {
            var db = new HyperHistorianContext();
            var kWHUsageList = new List<CenterkWhUsage_History>();
            object[] xparams = {
            new SqlParameter("@level",3),
            new SqlParameter("@regionName", "Region 34"),
            new SqlParameter("@mallName", "Beverly Center"),
            new SqlParameter("@startTime", "01/01/2019"),
            new SqlParameter("@endTime", "10/01/2019"),
            new SqlParameter("@sampleInterval", 1),
            new SqlParameter("@tagName", "=Peak_DEM")
            };

            try
            {

                kWHUsageList = db.Database.SqlQuery<CenterkWhUsage_History>(
                "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @level, @regionName, @mallName, @startTime, @endTime, @sampleInterval, @tagName",
                 xparams).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            return kWHUsageList;

        }

        static IEnumerable<EnergyUsage> MergeData(List<CenterTemp_History> tempData, List<CenterkWhUsage_History> energyData)
        {

            var merged = from temp in tempData
                         join energy in energyData
                            on temp.CurrentTimeStamp equals energy.CurrentTimeStamp
                         select new EnergyUsage
                         {
                             Center = temp.Tag,
                             AvgTemp = Math.Round(temp.CurrentAvgValue, 6, MidpointRounding.ToEven),
                             kWH = float.Parse(Math.Round(energy.CurrentAvgValue, 6, MidpointRounding.ToEven).ToString()),
                             DayOfWeek = (int)temp.CurrentTimeStamp.DayOfWeek,
                             Hour = temp.CurrentTimeStamp.Hour
                         };

            return merged.AsEnumerable();               
        }
    } 

}


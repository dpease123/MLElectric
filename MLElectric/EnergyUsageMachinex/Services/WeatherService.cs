﻿using EnergyUsageMachine.POCO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnergyUsageMachinex.Services
{
    class WeatherService
    {
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
                Forecast.Next24Hours = weather.properties.periods.Take(24).ToList();

                return Forecast;

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            return Forecast;
        }
    }
}

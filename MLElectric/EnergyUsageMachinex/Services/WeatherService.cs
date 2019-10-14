﻿using EnergyUsageMachine.POCO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace EnergyUsageMachine.Services
{
    public class WeatherService
    {
        public async Task<Forecast> Get24HrForecast(string path)
        {

            Forecast Forecast = new Forecast();
            Weather weather = new Weather();
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            try
            {

                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Taubman/1.0");
                var centerWeatherDetails = await client.GetAsync(path);
                Forecast = JsonConvert.DeserializeObject<Forecast>(await centerWeatherDetails.Content.ReadAsStringAsync());

                var WeatherJson = await client.GetStringAsync(Forecast.ForecastURLs.forecastHourly);
                weather = JsonConvert.DeserializeObject<Weather>(WeatherJson);
                Forecast.Next24Hours = weather.properties.periods.Take(24).ToList();

                return Forecast;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Forecast;
        }

        public async Task<Forecast> Get3DayForecast(string path)
        {
            throw new NotImplementedException("No 3 day wetaher wired yet");
        }
    }
}

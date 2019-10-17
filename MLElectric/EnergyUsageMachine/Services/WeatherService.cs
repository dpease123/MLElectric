using EnergyUsageMachine.POCO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EnergyUsageMachine.Models;
using System.Collections.Generic;

namespace EnergyUsageMachine.Services
{
    public class WeatherService
    {
        System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        public async Task<Forecast> Get24HrForecast(MLSetting mls)
        {

            var Forecast = new Forecast
            {
                CenterName = mls.CenterName,
                URL = mls.WeatherURL,
            };
          
            try
            {
                Forecast = await GetForecastURLs(mls, Forecast);

                var periods = await GetWeather(Forecast.ForecastURLs.Hourly, 24);

                Forecast.CenterName = mls.CenterName;
                Forecast.URL = mls.WeatherURL;
                Forecast.Periods = periods.ToList();
                return Forecast;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Forecast;
        }

        private async Task<Forecast> GetForecastURLs(MLSetting mls, Forecast Forecast)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Taubman/1.1");
            var centerWeatherDetails = await client.GetAsync(mls.WeatherURL);
            Forecast = JsonConvert.DeserializeObject<Forecast>(await centerWeatherDetails.Content.ReadAsStringAsync());
            return Forecast;
        }

        public async Task<Forecast> Get3DayForecast(MLSetting mls)
        {

            var Forecast = new Forecast
            {
                CenterName = mls.CenterName,
                URL = mls.WeatherURL,
            };
            try
            {
                Forecast = await GetForecastURLs(mls, Forecast);

                Forecast.Periods = await GetWeather(Forecast.ForecastURLs.ThreeDay, 6);

                Forecast.Periods.Take(24);
                return Forecast;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Forecast;
        }

        private async Task<List<Period>> GetWeather(string URL, int rows)
        {
            Forecast Forecast = new Forecast();
            try
            {
                var WeatherJson = await client.GetStringAsync(URL);
                var w = JsonConvert.DeserializeObject<Weather>(WeatherJson);
                var periods = w.properties.periods.Take(rows);
               
                return periods.ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new List<Period>();
        }
    }
}

using EnergyUsageMachine.POCO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EnergyUsageMachine.Models;

namespace EnergyUsageMachine.Services
{
    public class WeatherService
    {
        System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
       
        public async Task<Forecast> Get24HrForecast(MLSetting mls)
        {

            Forecast Forecast = new Forecast();
            Weather weather = new Weather();
          
            try
            {

                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Taubman/1.1");
                var centerWeatherDetails = await client.GetAsync(mls.WeatherURL);
                Forecast = JsonConvert.DeserializeObject<Forecast>(await centerWeatherDetails.Content.ReadAsStringAsync());

                var WeatherJson = await client.GetStringAsync(Forecast.ForecastURLs.Hourly);
                weather = JsonConvert.DeserializeObject<Weather>(WeatherJson);
                Forecast.Periods = weather.properties.periods.Take(24).ToList();

                Forecast.CenterName = mls.CenterName;
                Forecast.URL = mls.WeatherURL;
                return Forecast;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Forecast;
        }

        public async Task<Forecast> Get3DayForecast(MLSetting mls)
        {
            Forecast Forecast = new Forecast();
            Weather weather = new Weather();
            try
            {

                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Taubman/1.1");
                var centerWeatherDetails = await client.GetAsync(mls.WeatherURL);
                Forecast = JsonConvert.DeserializeObject<Forecast>(await centerWeatherDetails.Content.ReadAsStringAsync());

                var WeatherJson = await client.GetStringAsync(Forecast.ForecastURLs.ThreeDay);
                weather = JsonConvert.DeserializeObject<Weather>(WeatherJson);
                Forecast.Periods = weather.properties.periods.Take(24).ToList();

                Forecast.CenterName = mls.CenterName;
                Forecast.URL = mls.WeatherURL;
                return Forecast;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Forecast;
        }
    }
}

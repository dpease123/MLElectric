using EnergyUsageMachine.Services;
using System.Threading.Tasks;
using System.Web.Http;
using PowerUsageApi.SwaggerFilters;

namespace PowerUsageApi.Controllers
{
    public class WeatherController : ApiController
    {
        [SwaggerImplementationNotes("Returns next 24hrs weather forecast for building specified i.e. BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/Weather/24Hour/{BldgId}")]
        public IHttpActionResult Get24Hour(string BldgId)
        {
            var ws = new WeatherService();
            var ds = new DataService();
            var setting = ds.GetSetting(BldgId);
            var foreCast = Task.Run(async () => await ws.Get24HrForecast(setting)).Result;
            return Ok(foreCast);
        }

        [SwaggerImplementationNotes("Returns 3 day weather forecast for building specified i.e. BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/Weather/3Day/{BldgId}")]
        public IHttpActionResult Get3Day(string BldgId)
        {
            var ws = new WeatherService();
            var ds = new DataService();
            var setting = ds.GetSetting(BldgId);
            var foreCast = Task.Run(async () => await ws.Get3DayForecast(setting)).Result;
            return Ok(foreCast);
        }

    }
}

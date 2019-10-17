using EnergyUsageMachine.Services;
using System.Threading.Tasks;
using System.Web.Http;
using PowerUsageApi.SwaggerFilters;
using System;

namespace PowerUsageApi.Controllers
{
    public class WeatherController : ApiController
    {
        [SwaggerImplementationNotes("Returns next 24hrs weather forecast for building specified i.e. BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/Weather/24Hour/{BldgId}")]
        public IHttpActionResult Get24Hour(string BldgId)
        {
            try
            {
                var ws = new WeatherService();
                var ds = new DataService();

                var center = ds.GetSetting(BldgId);
                if (center == null)
                    return BadRequest("Building not found");

                var foreCast = Task.Run(async () => await ws.Get24HrForecast(center)).Result;
                return Ok(foreCast);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
        }

        [SwaggerImplementationNotes("Returns 3 day weather forecast for building specified i.e. BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/Weather/3Day/{BldgId}")]
        public IHttpActionResult Get3Day(string BldgId)
        {
            try
            {
                var ws = new WeatherService();
                var ds = new DataService();
                var center = ds.GetSetting(BldgId);
                if (center == null)
                    return BadRequest("Building not found");

                var foreCast = Task.Run(async () => await ws.Get3DayForecast(center)).Result;
                return Ok(foreCast);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }

    }
}

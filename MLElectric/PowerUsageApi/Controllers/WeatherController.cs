using EnergyUsageMachine.Services;
using System.Threading.Tasks;
using System.Web.Http;
using PowerUsageApi.SwaggerFilters;
using System;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine;

namespace PowerUsageApi.Controllers
{
    public class WeatherController : ApiController
    {
        [SwaggerImplementationNotes("Returns next 24hrs weather forecast for building specified. Parameters: BldgId= BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/Weather/24Hour/{BldgId}")]
        public IHttpActionResult Get24Hour(string BldgId)
        {
            if (string.IsNullOrEmpty(BldgId))
                return BadRequest("3 character building abbreviation required");

            try
            {
                var ws = new WeatherService();
                var ds = new DataService();

                var center = ds.GetSetting(BldgId.Substring(0, 3).ToUpper());

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

        [SwaggerImplementationNotes("Returns 3 day weather forecast for building specified. Parameters: BldgId= BEV,UTC,CCK")]
        [HttpGet]
        [Route("api/Weather/3Day/{BldgId}")]
        public IHttpActionResult Get3Day(string BldgId)
        {
            if (string.IsNullOrEmpty(BldgId))
                return BadRequest("3 character building abbreviation required");

            try
            {
                var ws = new WeatherService();
                var ds = new DataService();
                var center = ds.GetSetting(BldgId.Substring(0, 3).ToUpper());
                if (center == null)
                    return BadRequest("Building not found");

                var foreCast = Task.Run(async () => await ws.Get3DayForecast(center)).Result;
                return Ok(XMLHandler.SerializeXml<Forecast>(foreCast));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }

    }
}

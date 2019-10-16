using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using EnergyUsageMachine.Services;

namespace PowerUsageApi.Controllers
{
    public class WeatherController : ApiController
    {
        // GET api/24hour/BEV
        public IHttpActionResult Get24Hour(string Id)
        {
            var ws = new WeatherService();
            var ds = new DataService();
            var setting = ds.GetSetting(Id);
            var foreCast = Task.Run(async () => await ws.Get24HrForecast(setting)).Result;
            return Ok(foreCast);
        }

    }
}

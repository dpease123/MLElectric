using EnergyUsageMachine.Data;
using EnergyUsageMachine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Services
{
    public class DataService
    {
        HyperHistorianRepository hhr = new HyperHistorianRepository();

        public MLSetting GetSetting(string Id)
        {
            return hhr.GetMLSetting(Id);
        }
    }
}

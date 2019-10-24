using EnergyUsageMachine.Data;
using EnergyUsageMachine.Models;
using EnergyUsageMachine.POCO;
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

        public CenterConfig GetSetting(string Id)
        {
            return hhr.GetMLSetting(Id);
        }

        public List<CenterConfig>GetAllSettings()
        {
            return hhr.GetAllMLSettings();
        }

        public IEnumerable<EnergyUsage> GetTrainingData(CenterConfig center, string startDate, string endDate)
        {
            return hhr.GetTrainingData(center, startDate, endDate).ToList();
        }

        public IEnumerable<EnergyUsage> StageTrainingData(CenterConfig center, string startDate, string endDate)
        {
            return hhr.StageTrainingData(center, startDate, endDate).ToList();
        }

        public bool DeleteCenterData(string BldgId)
        {

            return hhr.DeleteCenterData(BldgId);
        }

        public CenterConfig UpdateSetting(CenterConfig m)
        {
            return hhr.UpdateSetting(m);
        }

        public DateTime GetMaxLoadDate(CenterConfig center)
        {
            return hhr.GetMaxLoadDate(center);
        }
    }
}

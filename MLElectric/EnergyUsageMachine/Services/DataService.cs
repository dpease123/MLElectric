using EnergyUsageMachine.Data;
using EnergyUsageMachine.Models;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.ViewModels;
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

        public CenterConfig GetCenterConfig(string Id)
        {
            return hhr.GetMLSetting(Id);
        }

        public List<CenterConfig>GetAllCenterConfigs()
        {
            return hhr.GetAllMLSettings();
        }

        public IEnumerable<EnergyUsage> GetTrainingData(CenterConfig center, string startDate, string endDate)
        {
            return hhr.GetTrainingData(center, startDate, endDate).ToList();
        }

        public void StageTrainingData(CenterConfig center, string startDate, string endDate)
        {
            hhr.StageTrainingData(center, startDate, endDate);
        }

        public bool DeleteCenterData(string BldgId)
        {

            return hhr.DeleteCenterData(BldgId);
        }

        public CenterConfig UpdateCenterConfig(CenterConfig m)
        {
            return hhr.UpdateConfig(m);
        }

        public DateTime GetMaxLoadDate(CenterConfig center)
        {
            return hhr.GetTrainingDataSummary(center).DataEndDate;
        }

        public List<MLModelDataSummary> GetDataSummary()
        {
            var centers = this.GetAllCenterConfigs();
            var list = new List<MLModelDataSummary>();
            foreach(var c in centers)
            {
                list.Add(hhr.GetTrainingDataSummary(c));
            }
            return list.OrderBy(x=> x.Center).ToList();
        }
    }
}

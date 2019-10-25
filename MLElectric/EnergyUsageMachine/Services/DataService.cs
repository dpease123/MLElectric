using EnergyUsageMachine.Data;
using EnergyUsageMachine.Models;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.ViewModels;
using Microsoft.ML;
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
            return DateTime.Parse(hhr.GetTrainingDataSummary(center).DataEndDate);
        }

        public MLModelDataSummary GetDataSummary(MLContext ctx, string modelPath, CenterConfig center)
        {
            var eval = new Evaluate(ctx, modelPath,  center);
            var summary = hhr.GetTrainingDataSummary(center);
            summary.EvaluateModel = new Evaluate(ctx, modelPath, center).EvaluateModel();
            return summary;
         }
            
        
    }
}

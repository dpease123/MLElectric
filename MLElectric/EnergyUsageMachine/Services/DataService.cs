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

        public Center GetCenterById(string Id)
        {
            return hhr.GetCenterById(Id);
        }

        public DateTime GetMaxDataLoadDate(Center center)
        {
            return hhr.GetMaxDataLoadDate(center);
        }

        public List<Center>GetAllCenters()
        {
            return hhr.GetAllCenters();
        }

        public IEnumerable<EnergyUsage> GetTrainingData(Center center, string startDate, string endDate)
        {
            return hhr.GetTrainingData(center, startDate, endDate).ToList();
        }

        public IEnumerable<EnergyUsage> GetTrainingDataForCenter(Center center)
        {
            return hhr.GetTrainingData(center).ToList();
        }

        public void StageTrainingData(Center center, DateTime startDate, DateTime endDate)
        {
            hhr.StageTrainingData(center, startDate.ToString(), endDate.ToString());
        }

        public bool DeleteCenterData(string BldgId)
        {

            return hhr.DeleteCenterData(BldgId);
        }

        public Center SaveChanges(Center c)
        {
            return hhr.SaveChanges(c);
        }

        public MLModelDataSummary GetDataSummary(MLContext ctx, string modelPath, Center center)
        {
            var eval = new Evaluate(ctx, modelPath,  center);
            var summary = hhr.GetTrainingDataSummary(center);
            summary.ModelQuality= new Evaluate(ctx, modelPath, center).EvaluateModel();
            return summary;
         }
            
        
    }
}

using EnergyUsageMachine.Data;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyUsageMachine.ViewModels;

namespace EnergyUsageMachine.Data
{
    public class HyperHistorianRepository
    {
        HyperHistorianContext ctx = new HyperHistorianContext();
        private List<CenterTemp_History> GetTemperatureData(Center c, string startDate, string endDate)
        {

            object[] xparams = {
            new SqlParameter("@regionName", c.Region),
            new SqlParameter("@mallName",c.CenterName),
            new SqlParameter("@startTime", startDate),
            new SqlParameter("@endTime", endDate),
            new SqlParameter("@tagName", "Temperature_F"),
            new SqlParameter("@CenterAbbr", c.CenterAbbr)
            };

            return ctx.Database.SqlQuery<CenterTemp_History>(
                   "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @regionName, @mallName, @startTime, @endTime, @CenterAbbr,  @tagName",
                    xparams).ToList();
        }

        private List<CenterkWhUsage_History> GetEnergyDemandData(Center c, string startDate, string endDate)
        {
            object[] xparams = {
            new SqlParameter("@regionName", c.Region),
            new SqlParameter("@mallName",c.CenterName),
            new SqlParameter("@startTime", startDate),
            new SqlParameter("@endTime", endDate),
            new SqlParameter("@tagName", "=Peak_DEM"),
            new SqlParameter("@CenterAbbr", c.CenterAbbr)
            };

            return ctx.Database.SqlQuery<CenterkWhUsage_History>(
                    "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @regionName, @mallName, @startTime, @endTime, @CenterAbbr,  @tagName",
                    xparams).ToList();

        }

        public void StageTrainingData(Center center, string startDate, string endDate)
        {
            var tempData = GetTemperatureData(center, startDate, endDate);
            var energyData = GetEnergyDemandData(center, startDate, endDate);

            //var merged = from temp in tempData
            //             join energy in energyData
            //                on temp.CurrentTimeStamp equals energy.CurrentTimeStamp
            //             select new EnergyUsage
            //             {
            //                 Center = temp.CenterAbbr,
            //                 AvgTemp = Math.Round(temp.CurrentAvgValue, 6, MidpointRounding.ToEven),
            //                 kWH = float.Parse(Math.Round(energy.CurrentAvgValue, 6, MidpointRounding.ToEven).ToString()),
            //                 DayOfWeek = (int)temp.CurrentTimeStamp.DayOfWeek,
            //                 Hour = temp.CurrentTimeStamp.Hour
            //             };

            //  return merged.AsEnumerable();
        }

        public IEnumerable<EnergyUsage> GetTrainingData(Center center, string startDate, string endDate)
        {
            var a = DateTime.Parse(startDate);
            var z = DateTime.Parse(endDate);

            var tempData = (ctx.IconicsData.Where(x => x.CenterAbbr == center.CenterAbbr
                             && x.TimeStamp >= a && x.TimeStamp <= z && x.Fulltag.Contains("Temperature_F")).ToList());


            var energyData = (ctx.IconicsData.Where(x => x.CenterAbbr == center.CenterAbbr
                              && x.TimeStamp >= a && x.TimeStamp <= z && x.Fulltag.Contains("Peak_DEM")).ToList());
                             

            var merged = (from temp in tempData
                         join energy in energyData
                            on temp.TimeStamp equals energy.TimeStamp
                         select new 
                         {
                             Center = temp.CenterAbbr,
                             AvgTemp = temp.AvgValue,
                             kWH = energy.AvgValue,
                             Date = temp.TimeStamp,
                         }).ToList();

            var usage = new List<EnergyUsage>();
            foreach (var m in merged)
            {
                var e = new EnergyUsage()
                {
                    Center = m.Center,
                    AvgTemp = Math.Round(m.AvgTemp, 6, MidpointRounding.ToEven),
                    kWH = float.Parse(Math.Round(m.kWH, 6, MidpointRounding.ToEven).ToString()),
                    DayOfWeek = (int)m.Date.DayOfWeek,
                    Hour = m.Date.Hour
                };
                usage.Add(e);
            }


            return usage.AsEnumerable();
        }

        public IEnumerable<EnergyUsage> GetTrainingData(Center center)
        {
            var tempData = (ctx.IconicsData.Where(x => x.CenterAbbr == center.CenterAbbr && x.Fulltag.Contains("Temperature_F")).ToList());


            var energyData = (ctx.IconicsData.Where(x => x.CenterAbbr == center.CenterAbbr && x.Fulltag.Contains("Peak_DEM")).ToList());


            var merged = (from temp in tempData
                          join energy in energyData
                             on temp.TimeStamp equals energy.TimeStamp
                          select new
                          {
                              Center = temp.CenterAbbr,
                              AvgTemp = temp.AvgValue,
                              kWH = energy.AvgValue,
                              Date = temp.TimeStamp,
                          }).ToList();

            var usage = new List<EnergyUsage>();
            foreach (var m in merged)
            {
                var e = new EnergyUsage()
                {
                    Center = m.Center,
                    AvgTemp = Math.Round(m.AvgTemp, 6, MidpointRounding.ToEven),
                    kWH = float.Parse(Math.Round(m.kWH, 6, MidpointRounding.ToEven).ToString()),
                    DayOfWeek = (int)m.Date.DayOfWeek,
                    Hour = m.Date.Hour
                };
                usage.Add(e);
            }


            return usage.AsEnumerable();
        }


        public MLModelDataSummary GetTrainingDataSummary(Center center)
        {
            var dsList = new List<MLModelDataSummary>();
            var tempData = (ctx.IconicsData.Where(x => x.Fulltag.Contains("Temperature_F") && x.CenterAbbr == center.CenterAbbr).ToList());
            var energyData = (ctx.IconicsData.Where(x => x.Fulltag.Contains("Peak_DEM") && x.CenterAbbr == center.CenterAbbr).ToList());

            var merged = (from temp in tempData
                          join energy in energyData
                             on temp.TimeStamp equals energy.TimeStamp
                          select new
                          {
                              Center = temp.CenterAbbr,
                              Date = temp.TimeStamp,
                          }).ToList();


            var ds = new MLModelDataSummary()
            {
                Center = center.CenterAbbr,
                DataStartDate = merged.Min(a => a.Date).ToString(),
                DataEndDate = merged.Max(a => a.Date).ToString(),
                TemperatureRecordCount = String.Format("{0:#,##0}", tempData.Count()),
                DemandRecordCount = String.Format("{0:#,##0}", energyData.Count()),
                JoinedCount = String.Format("{0:#,##0}", merged.Count())
                    
             };
            return ds;
           
        }

        public Center GetCenterById(string Id)
        {
            return ctx.CenterConfig.Find(Id);
        }

        public List<Center> GetAllCenters()
        {
            return ctx.CenterConfig.ToList();
        }

        public Center SaveChanges(Center cc)
        {
            var row = ctx.CenterConfig.Find(cc.CenterAbbr);
            row.DateUpdated = DateTime.Now;
            row.BestTrainer = cc.BestTrainer;
            row.ModelGrade = cc.ModelGrade;
            row.RSquaredScore = cc.RSquaredScore;
            row.RootMeanSquaredError = cc.RootMeanSquaredError;
            row.JoinedRecordCount = cc.JoinedRecordCount;
            row.DemandRecordCount = cc.DemandRecordCount;
            row.TemperatureRecordCount = cc.TemperatureRecordCount;
            ctx.SaveChanges();

            return row;
        }

        public bool DeleteCenterData(string BldgId)
        {

            var ret = ctx.Database.ExecuteSqlCommand(
                   "DELETE FROM [ML_IconicsData] WHERE [CenterAbbr] = @BldgId",
                    new SqlParameter("@BldgId", BldgId));
            return ret > 0;
        }

        public DateTime GetMaxDataLoadDate(Center center)
        {
            return ctx.IconicsData.Where(x => x.CenterAbbr == center.CenterAbbr).Max(t => t.TimeStamp);
        }

        
    }
    
}

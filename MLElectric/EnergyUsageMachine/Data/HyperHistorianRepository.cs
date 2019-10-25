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
        private List<CenterTemp_History> GetTemperatureData(CenterConfig c, string startDate, string endDate)
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
                   "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @regionName, @mallName, @startTime, @endTime, @tagName, @CenterAbbr",
                    xparams).ToList();
        }

        private List<CenterkWhUsage_History> GetEnergyDemandData(CenterConfig c, string startDate, string endDate)
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
                    "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @regionName, @mallName, @startTime, @endTime, @tagName, @CenterAbbr",
                    xparams).ToList();

        }

        public void StageTrainingData(CenterConfig center, string startDate, string endDate)
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

        public IEnumerable<EnergyUsage> GetTrainingData(CenterConfig center, string startDate, string endDate)
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

        public MLModelDataSummary GetTrainingDataSummary(CenterConfig center)
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

        public CenterConfig GetMLSetting(string Id)
        {
            return ctx.CenterConfig.Find(Id);
        }

        public List<CenterConfig> GetAllMLSettings()
        {
            return ctx.CenterConfig.ToList();
        }

        public CenterConfig UpdateConfig(CenterConfig m)
        {
            var row = ctx.CenterConfig.Find(m.CenterAbbr);
            row.DateLastRecord = DateTime.Now;
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

        
    }
    
}

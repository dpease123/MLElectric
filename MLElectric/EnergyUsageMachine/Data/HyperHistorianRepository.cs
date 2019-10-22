using EnergyUsageMachine.Data;
using EnergyUsageMachine.POCO;
using EnergyUsageMachine.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyUsageMachine.Data
{
    public class HyperHistorianRepository
    {
        HyperHistorianContext ctx = new HyperHistorianContext();
        private List<CenterTemp_History> GetTemperatureData(MLSetting c, string startDate, string endDate)
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

        private List<CenterkWhUsage_History> GetEnergyUsageData(MLSetting c, string startDate, string endDate)
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

        public IEnumerable<EnergyUsage> StageTrainingData(MLSetting center, string startDate, string endDate)
        {
            var tempData = GetTemperatureData(center, startDate, endDate);
            var energyData = GetEnergyUsageData(center, startDate, endDate);

            var merged = from temp in tempData
                         join energy in energyData
                            on temp.CurrentTimeStamp equals energy.CurrentTimeStamp
                         select new EnergyUsage
                         {
                             Center = temp.CenterAbbr,
                             AvgTemp = Math.Round(temp.CurrentAvgValue, 6, MidpointRounding.ToEven),
                             kWH = float.Parse(Math.Round(energy.CurrentAvgValue, 6, MidpointRounding.ToEven).ToString()),
                             DayOfWeek = (int)temp.CurrentTimeStamp.DayOfWeek,
                             Hour = temp.CurrentTimeStamp.Hour
                         };

              return merged.AsEnumerable();
        }

        public IEnumerable<EnergyUsage> GetTrainingData(MLSetting center, string startDate, string endDate)
        {
            var a = DateTime.Parse(startDate);
            var z = DateTime.Parse(endDate);

            var tempData = from data in ctx.MLData
                            where data.CenterAbbr == center.CenterAbbr
                             && data.TimeStamp >= a && data.TimeStamp <= z && data.Fulltag.Contains("_TEMP")
                            select data;

            var energyData = from data in ctx.MLData
                             where data.CenterAbbr == center.CenterAbbr
                              && data.TimeStamp >= a && data.TimeStamp <= z && data.Fulltag.Contains("_DEM")
                             select data;

            var merged = from temp in tempData
                         join energy in energyData
                            on temp.TimeStamp equals energy.TimeStamp
                         select new EnergyUsage
                         {
                             Center = temp.CenterAbbr,
                             AvgTemp = Math.Round(temp.AvgValue, 6, MidpointRounding.ToEven),
                             kWH = float.Parse(Math.Round(energy.AvgValue, 6, MidpointRounding.ToEven).ToString()),
                             DayOfWeek = (int)temp.TimeStamp.DayOfWeek,
                             Hour = temp.TimeStamp.Hour
                         };

                 

            return merged.AsEnumerable();
        }

        public MLSetting GetMLSetting(string Id)
        {
            return ctx.MLSettings.Find(Id);
        }

        public List<MLSetting> GetAllMLSettings()
        {
            return ctx.MLSettings.ToList();
        }

        public MLSetting UpdateSetting(MLSetting m)
        {
            var row = ctx.MLSettings.Find(m.CenterAbbr);
            row.DateLastRecord = DateTime.Now;
            ctx.SaveChanges();

            return row;
        }

        public bool DeleteCenterData(string BldgId)
        {

            var ret = ctx.Database.ExecuteSqlCommand(
                   "DELETE FROM [MLHistoricalData_BackUp] WHERE [CenterAbbr] = @BldgId",
                    new SqlParameter("@BldgId", BldgId));
            return ret > 0;
        }
    }
    
}

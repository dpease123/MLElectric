using EnergyUsageMachine.Data;
using EnergyUsageMachine.Enums;
using EnergyUsageMachine.POCO;
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
        private List<CenterTemp_History> GetTemperatureData(Center c, string startDate, string endDate)
        {

            object[] xparams = {
            new SqlParameter("@regionName", c._region),
            new SqlParameter("@mallName",c._name),
            new SqlParameter("@startTime", startDate),
            new SqlParameter("@endTime", endDate),
            new SqlParameter("@tagName", "Temperature_F")
            };

         return ctx.Database.SqlQuery<CenterTemp_History>(
                "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @regionName, @mallName, @startTime, @endTime, @tagName",
                 xparams).ToList();
        }

        private List<CenterkWhUsage_History> GetEnergyUsageData(Center c, string startDate, string endDate)
        {
            object[] xparams = {
            new SqlParameter("@regionName", c._region),
            new SqlParameter("@mallName",c._name),
            new SqlParameter("@startTime", startDate),
            new SqlParameter("@endTime", endDate),
            new SqlParameter("@tagName", "=Peak_DEM")
            };

            return ctx.Database.SqlQuery<CenterkWhUsage_History>(
                    "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @regionName, @mallName, @startTime, @endTime, @tagName",
                    xparams).ToList();

        }

        public IEnumerable<EnergyUsage> GetTrainingData(Center center, string startDate, string endDate)
        {
            var tempData = GetTemperatureData(center, startDate, endDate);
            var energyData = GetEnergyUsageData(center, startDate, endDate);

            var merged = from temp in tempData
                         join energy in energyData
                            on temp.CurrentTimeStamp equals energy.CurrentTimeStamp
                         select new EnergyUsage
                         {
                             Center = temp.Tag,
                             AvgTemp = Math.Round(temp.CurrentAvgValue, 6, MidpointRounding.ToEven),
                             kWH = float.Parse(Math.Round(energy.CurrentAvgValue, 6, MidpointRounding.ToEven).ToString()),
                             DayOfWeek = (int)temp.CurrentTimeStamp.DayOfWeek,
                             Hour = temp.CurrentTimeStamp.Hour
                         };

            return merged.AsEnumerable();
        }
    }
}

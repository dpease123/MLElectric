using EnergyUsageMachine.Data;
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
        private List<CenterTemp_History> GetTemperatureData(string CenterAbbr)
        {
            var temperatureList = new List<CenterTemp_History>();
            var db = new HyperHistorianContext();


            object[] xparams = {
            new SqlParameter("@level",3),
            new SqlParameter("@regionName", "Region 34"),
            new SqlParameter("@mallName", "Beverly Center"),
            new SqlParameter("@startTime", "01/01/2019"),
            new SqlParameter("@endTime", "10/01/2019"),
            new SqlParameter("@sampleInterval", 1),
            new SqlParameter("@tagName", "Temperature_F")
            };

            try
            {

                temperatureList = db.Database.SqlQuery<CenterTemp_History>(
                "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @level, @regionName, @mallName, @startTime, @endTime, @sampleInterval, @tagName",
                 xparams).ToList();

            }
            catch (Exception ex)
            {
               
            }

            return temperatureList;

        }

        private List<CenterkWhUsage_History> GetEnergyUsageData(string CenterAbbr)
        {
            var kWHUsageList = new List<CenterkWhUsage_History>();
            var db = new HyperHistorianContext();

            object[] xparams = {
            new SqlParameter("@level",3),
            new SqlParameter("@regionName", "Region 34"),
            new SqlParameter("@mallName", "Beverly Center"),
            new SqlParameter("@startTime", "01/01/2019"),
            new SqlParameter("@endTime", "10/01/2019"),
            new SqlParameter("@sampleInterval", 1),
            new SqlParameter("@tagName", "=Peak_DEM")
            };

            try
            {

                kWHUsageList = db.Database.SqlQuery<CenterkWhUsage_History>(
                "exec [dbo].[sp_GetAggregatesForTimeRangeForLevel_Dev] @level, @regionName, @mallName, @startTime, @endTime, @sampleInterval, @tagName",
                 xparams).ToList();

            }
            catch (Exception ex)
            {
                
            }



            return kWHUsageList;

        }

        public IEnumerable<EnergyUsage> GetTrainingData(string CenterAbbr)
        {
            var tempData = GetTemperatureData(CenterAbbr);
            var energyData = GetEnergyUsageData(CenterAbbr);

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

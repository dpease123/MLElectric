namespace EnergyUsageMachine.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.Infrastructure;
    using EnergyUsageMachine.Models;

    public partial class HyperHistorianContext : DbContext
    {

        public DbSet<MLSetting> MLSettings { get; set; }
        public DbSet<MLData> MLData { get; set; }

        public HyperHistorianContext()
            : base("name=HyperHistorianContext")
        {
            this.SetCommandTimeOut(1800);
        }

        public void SetCommandTimeOut(int Timeout)
        {
            var objectContext = (this as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = Timeout;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}

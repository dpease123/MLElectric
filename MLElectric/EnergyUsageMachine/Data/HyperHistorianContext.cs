namespace EnergyUsageMachine.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.Infrastructure;
    using EnergyUsageMachine.Models;
    using System.Data.Entity.ModelConfiguration.Conventions;

    public partial class HyperHistorianContext : DbContext
    {

        public DbSet<CenterConfig> CenterConfig { get; set; }
       
        public DbSet<IconicsData> IconicsData { get; set; }

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
            Database.SetInitializer<HyperHistorianContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}

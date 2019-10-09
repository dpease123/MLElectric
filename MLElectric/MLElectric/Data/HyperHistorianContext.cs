namespace MLElectric.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.Infrastructure;

    public partial class HyperHistorianContext : DbContext
    {
        public HyperHistorianContext()
            : base("name=HyperHistorianContext")
        {
            this.SetCommandTimeOut(300);
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

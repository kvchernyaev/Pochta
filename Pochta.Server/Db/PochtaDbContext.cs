#region usings
using System.Data.Entity;
#endregion

namespace Pochta.Server
{
    internal class PochtaDbContext : DbContext
    {
        public PochtaDbContext() : base("DbConnection")
        {
            this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<IncomingRequest> IncomingRequests { get; set; }
    }
}   

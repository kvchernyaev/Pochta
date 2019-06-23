#region usings
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Client
{
    class ClientDbContext : DbContext
    {
        public ClientDbContext() : base("DbConnection")
        {
            this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<QueueItem> QueueItems { get; set; }
    }
}

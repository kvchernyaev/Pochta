#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Server
{
    internal class Db
    {
        public int InsertIncomingRequest(string str, string clientAddress)
        {
            using (var db = new PochtaDbContext())
            {
                IncomingRequest rInserted = db.IncomingRequests.Add(new IncomingRequest { ClientAddress = clientAddress, Str = str, IncomedDate = DateTime.Now });
                db.SaveChanges();

                return rInserted.Id;
            }
        }


        public List<IncomingRequest> GetIncomingRequests(int? top)
        {
            using (var db = new PochtaDbContext())
            {
                var i = db.IncomingRequests.OrderByDescending(x => x.Id);
                return (top.HasValue ? i.Take(top.Value) : i).ToList();
            }
        }

    }
}

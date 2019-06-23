#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Client
{
    class Db
    {
        public int InsertQueueItem(string message)
        {
            using (var db = new ClientDbContext())
            {
                QueueItem qi = db.QueueItems.Add(new QueueItem { Message = message, CreatedDate = DateTime.Now });
                db.SaveChanges();

                return qi.Id;
            }
        }


        public List<QueueItem> GetNextQueueItem()
        {
            using (var db = new ClientDbContext())
            {
                List<QueueItem> l = db.Database.SqlQuery<QueueItem>("GetNextQueueItem").ToList();
                return l;
            }
        }


        public void DeleteQueueItem(int queueItemId)
        {
            using (var db = new ClientDbContext())
            {
                var qi = new QueueItem { Id = queueItemId, SuccessfullySent = true };
                db.QueueItems.Attach(qi);
                db.Entry(qi).Property(x => x.SuccessfullySent).IsModified = true;
                db.SaveChanges();
            }
        }
    }
}

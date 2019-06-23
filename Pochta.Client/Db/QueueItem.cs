#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Client
{
    public class QueueItem
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        
        
        /// <summary>
        /// When this record was got to sending to server
        /// </summary>
        public DateTime? LastRetryDate { get; set; }
        public int? TakedCount { get; set; }

        public bool? SuccessfullySent { get; set; }
    }
}

#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Common
{
    public class DispatchersHandler
    {
        readonly List<DispatcherCommon> _dispatchers = new List<DispatcherCommon>();


        public void Start(DispatcherCommon d)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));

            lock (_dispatchers)
            {
                _dispatchers.Add(d);
                d.Start();
            }
        }


        public void Stop()
        {
            lock (_dispatchers)
            {
                foreach (DispatcherCommon d in _dispatchers) d.Stop();

                _dispatchers.Clear();
            }
        }
    }
}
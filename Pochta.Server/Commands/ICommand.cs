#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Server
{
    interface ICommand
    {
        void Execute();
    }
}

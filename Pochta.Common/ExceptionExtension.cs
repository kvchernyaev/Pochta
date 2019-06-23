#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Common
{
    public static class ExceptionExtension
    {
        public static string GetWholeMessage(this Exception ex)
        {
            StringBuilder sb = new StringBuilder(ex.Message);

            while(ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.Append(" -> ");
                sb.Append(ex.Message);
            }
            return sb.ToString();
        }
    }
}

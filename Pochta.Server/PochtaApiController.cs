#region usings
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
#endregion

namespace Pochta.Server
{
    public class PochtaApiController : ApiController
    {
        protected string GetClientAddress()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Request.UserHostAddress;
            else if (Request.Properties.ContainsKey("MS_OwinContext"))
            {
                OwinContext context = (OwinContext)Request.Properties["MS_OwinContext"];
                int? port = context.Request.RemotePort;
                string ip = context.Request.RemoteIpAddress;
                return $"{ip}:{port}";
            }
            else return null;
        }
    }
}

#region usings
using Owin;
using System.Web.Http;
#endregion

namespace Pochta.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{str}",
                defaults: new { str = RouteParameter.Optional }
            );

            app.UseWebApi(config);
        }
    }
}   

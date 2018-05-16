using LNF.Web;
using Microsoft.Owin;
using System.Web.Routing;

[assembly: OwinStartup(typeof(sselData.Startup))]

namespace sselData
{
    public class Startup : OwinStartup
    {
        public override void ConfigureRoutes(RouteCollection routes)
        {
            // nothing to do here...
        }
    }
}
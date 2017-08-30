using LNF.Impl;
using System.Web.Routing;
using Microsoft.Owin;

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
using LNF.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(sselData.Startup))]

namespace sselData
{
    public class Startup
    { 
        public void Configuration(IAppBuilder app)
        { 
            app.UseDataAccess(Global.ContainerContext);
        }
    }
}
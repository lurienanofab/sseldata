using LNF;
using LNF.DependencyInjection;
using LNF.Impl.DependencyInjection;
using LNF.Web;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;

namespace sselData
{
    public class Global : HttpApplication
    {
        private static WebApp webapp;

        public static IContainerContext ContainerContext => webapp.Context;

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();

            webapp = new WebApp();

            // setup up dependency injection container
            var wcc = webapp.GetConfiguration();
            wcc.Context.EnablePropertyInjection();
            wcc.RegisterAllTypes();

            // setup web dependency injection
            webapp.Bootstrap(assemblies);

            if (Configuration.Current.Production)
                Application["AppServer"] = $"http://{Environment.MachineName}.eecs.umich.edu/";
            else
                Application["AppServer"] = "/";
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                FormsIdentity ident = (FormsIdentity)User.Identity;
                string[] roles = ident.Ticket.UserData.Split('|');
                Context.User = new GenericPrincipal(ident, roles);
            }
        }
    }
}

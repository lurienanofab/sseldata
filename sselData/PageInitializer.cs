using LNF.Web;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(PageInitializer), "Initialize")]

public class PageInitializer : PageInitializerModule
{
    public static void Initialize()
    {
        RegisterModule(typeof(PageInitializer));
    }
}
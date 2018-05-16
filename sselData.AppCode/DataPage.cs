using LNF;
using LNF.Data;
using LNF.Web.Content;
using StructureMap.Attributes;

namespace sselData.AppCode
{
    public class DataPage : LNFPage
    {
        [SetterProperty]
        public IAccountManager AccountManager { get; set; }

        [SetterProperty]
        public IDryBoxManager DryBoxManager { get; set; }

        public DataPage()
        {
            ServiceProvider.Current.Resolver.BuildUp(this);
        }
    }
}

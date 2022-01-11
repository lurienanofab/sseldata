using LNF;
using LNF.Data;
using LNF.Web.Content;

namespace sselData.AppCode
{
    public class DataPage : OnlineServicesPage
    {
        [Inject] public IAccountRepository AccountRepository { get; set; }


        [Inject] public IDryBoxRepository DryBoxRepository { get; set; }
    }
}

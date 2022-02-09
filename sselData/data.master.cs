using System;
using System.Configuration;
using LNF.Web.Content;

namespace sselData
{
    public partial class DataMaster : OnlineServicesMasterPage
    {
        public override bool ShowMenu
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["ShowMenu"]) || Request.QueryString["menu"] == "1";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}
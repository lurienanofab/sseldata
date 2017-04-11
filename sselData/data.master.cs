using System;
using System.Configuration;

namespace sselData
{
    public partial class data : LNF.Web.Content.LNFMasterPage
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
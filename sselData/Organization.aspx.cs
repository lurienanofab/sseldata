using System;
using System.Web.UI;

namespace sselData
{
    public partial class Organization : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect(hidNavigateUrl.Value);
        }
    }
}
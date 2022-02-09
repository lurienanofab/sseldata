using LNF;
using LNF.CommonTools;
using LNF.Data;
using LNF.Web;
using LNF.Web.Content;
using sselData.AppCode;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class Index : OnlineServicesPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Request.QueryString["AbandonSession"] == "1")
                {
                    Session.Abandon();
                    Response.Redirect("~");
                }

                if (int.TryParse(Request.QueryString["OrgID"], out int orgId))
                {
                    Session["OrgID"] = orgId;
                }

                // check to see if session is valid
                if (!string.IsNullOrEmpty(Request.QueryString["ClientID"])) //probably coming from sselonline
                {
                    if (int.TryParse(Request.QueryString["ClientID"].Trim(), out int clientId))
                    {
                        if (CurrentUser.ClientID != clientId)
                        {
                            Session.Abandon();
                            Response.Redirect("~");
                        }
                    }
                }

                // populate site dropdown - preselect using site linked in from
                var dt = DataCommand().Param("Action", "CurrentlyActive").FillDataTable("sselData.dbo.Org_Select");
                ddlOrg.DataSource = dt;
                ddlOrg.DataValueField = "OrgID";
                ddlOrg.DataTextField = "OrgName";
                ddlOrg.DataBind();

                ddlOrg.SelectedValue = GetSelectedOrgID(dt).ToString();

                ButtonControl();
            }
        }

        private int GetSelectedOrgID(DataTable dt)
        {
            if (Session["OrgID"] != null)
                return Convert.ToInt32(Session["OrgID"]);

            var rows = dt.Select("PrimaryOrg = 1");

            if (rows.Length > 0)
            { 
                var orgId = Convert.ToInt32(rows[0]["OrgID"]);
                Session["OrgID"] = orgId;
                return orgId;
            }

            throw new Exception("Cannot determine selected OrgID (no primary org found).");
        }

        protected void DdlOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["OrgID"] = Convert.ToInt32(ddlOrg.SelectedValue);
            ButtonControl();
        }

        private void ButtonControl()
        {
            var orgId = Convert.ToInt32(Session["OrgID"]);
            var canAddClient = DataCommand().Param("Action", "Status").Param("OrgID", orgId).ExecuteScalar<int>("sselData.dbo.Org_Select").Value;

            // need to have an organization w/ a department to add a client
            // need to have a client to add an account

            // add/modify controls
            if (canAddClient > 1) btnAddModClient.Enabled = true;
            if (canAddClient == 3) btnAddModAccount.Enabled = true;
        }

        protected void Button_Command(object sender, CommandEventArgs e)
        {
            string page = e.CommandArgument.ToString();
            string url;
            switch (e.CommandName)
            {
                case "navigate":
                    Response.Redirect(page);
                    break;
                case "navigate-client":
                    NavigateClient(page);
                    break;
                case "navigate-global":
                    url = string.Format("GlobalConfig.aspx?ConfigItem={0}", page);
                    Response.Redirect(url);
                    break;
                case "navigate-client-acct-assign":
                    url = page.Replace("{OrgID}", Session["OrgID"].ToString());
                    Response.Redirect(url);
                    break;
                case "navigate-logout":
                    Cache.Remove(DataUtility.CacheID);
                    Session.Abandon();
                    Response.Redirect(Session["Logout"].ToString());
                    break;
            }
        }

        private void NavigateClient(string page)
        {
            int orgId = Convert.ToInt32(Session["OrgID"]);
            var reader = DataCommand().Param("Action", "ByOrg").Param("OrgID", orgId).ExecuteReader("sselData.dbo.Department_Select");
            bool noDept = !reader.Read();

            if (noDept)
                ServerJScript.JSAlert(Page, "Please add a department to the organization before adding clients.");
            else
                Response.Redirect(page);

        }

        protected void BtnFoobar_Click(object sender, EventArgs e)
        {
            DataCommand().Param("Action", "foobar").Param("Password", Encryption.SHA256.EncryptText("foobar")).ExecuteNonQuery("sselData.dbo.Client_Update");
        }

        protected string GetLogoutUrl()
        {
            if (Session["Logout"] == null)
                Session["Logout"] = Configuration.Current.Context.LoginUrl;
            return Session["Logout"].ToString();
        }
    }
}
using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Web;
using LNF.Web.Content;
using sselData.AppCode;
using System;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class Index : LNFPage
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
                        if (CacheManager.Current.CurrentUser.ClientID != clientId)
                        {
                            Session.Abandon();
                            Response.Redirect("~");
                        }
                    }
                }

                // populate site dropdown - preselect using site linked in from
                using (var dba = DA.Current.GetAdapter())
                {
                    var dt = dba.ApplyParameters(new { Action = "CurrentlyActive" }).FillDataTable("Org_Select");
                    ddlOrg.DataSource = dt;
                    ddlOrg.DataValueField = "OrgID";
                    ddlOrg.DataTextField = "OrgName";
                    ddlOrg.DataBind();
                }

                ddlOrg.SelectedValue = Session["OrgID"].ToString();

                ButtonControl();
            }
        }

        protected void ddlOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["OrgID"] = Convert.ToInt32(ddlOrg.SelectedValue);
            ButtonControl();
        }

        private void ButtonControl()
        {
            using (var dba = DA.Current.GetAdapter())
            {
                var orgId = Convert.ToInt32(Session["OrgID"]);
                var canAddClient = dba.ApplyParameters(new { Action = "Status", OrgID = orgId }).ExecuteScalar<int>("Org_Select");

                // need to have an organization w/ a department to add a client
                // need to have a client to add an account

                // add/modify controls
                if (canAddClient > 1) btnAddModClient.Enabled = true;
                if (canAddClient == 3) btnAddModAccount.Enabled = true;
            }
        }

        protected void Button_Command(object sender, CommandEventArgs e)
        {
            string page = e.CommandArgument.ToString();
            string url = string.Empty;
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
            using (var dba = DA.Current.GetAdapter())
            {
                int orgId = Convert.ToInt32(Session["OrgID"]);
                var reader = dba.ApplyParameters(new { Action = "ByOrg", OrgID = orgId }).ExecuteReader("Department_Select");
                bool noDept = !reader.Read();

                if (noDept)
                    ServerJScript.JSAlert(Page, "Please add a department to the organization before adding clients.");
                else
                    Response.Redirect(page);
            }
        }

        protected void btnFoobar_Click(object sender, EventArgs e)
        {
            using (var dba = DA.Current.GetAdapter())
            {
                var enc = new Encryption();
                dba.ApplyParameters(new { Action = "foobar", Password = enc.EncryptText("foobar") }).ExecuteNonQuery("Client_Update");
            }
        }
    }
}
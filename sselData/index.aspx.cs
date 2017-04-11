using LNF.Cache;
using LNF.Models.Data;
using sselData.AppCode;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class index : LNF.Web.Content.LNFPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return 0; }
        }

        protected override void OnInit(EventArgs e)
        {
            CacheManager.Current.CheckSession();

            if (!User.IsInRole("Administrator"))
                Response.Redirect(Session["Logout"].ToString());

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                // check to see if session is valid
                if (Request.QueryString.Count > 0) // probably coming from sselOnLine
                {
                    string strClientID = Request.QueryString["ClientID"];
                    int cid;
                    if (int.TryParse(strClientID.Trim(), out cid) && Session["ClientID"] != null)
                    {
                        if (Convert.ToInt32(Session["ClientID"]) != cid)
                        {
                            Session.Abandon();
                            Response.Redirect("~");
                        }
                    }
                }

                SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                // populate site dropdown - preselect using site linked in from
                SqlCommand cmdListOrgs = new SqlCommand("Org_Select", cnSselData);
                cmdListOrgs.CommandType = CommandType.StoredProcedure;
                cmdListOrgs.Parameters.AddWithValue("@Action", "CurrentlyActive");

                cnSselData.Open();
                ddlOrg.DataSource = cmdListOrgs.ExecuteReader(CommandBehavior.CloseConnection);
                ddlOrg.DataValueField = "OrgID";
                ddlOrg.DataTextField = "OrgName";
                ddlOrg.DataBind();

                ddlOrg.SelectedValue = Session["OrgID"].ToString();
                ButtonControl();
            }
        }

        protected void ddlOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["OrgID"] = ddlOrg.SelectedValue;
            ButtonControl();
        }

        private void ButtonControl()
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            SqlCommand cmdOrgStat = new SqlCommand("Org_Select", cnSselData);
            cmdOrgStat.CommandType = CommandType.StoredProcedure;
            cmdOrgStat.Parameters.AddWithValue("@Action", "Status");
            cmdOrgStat.Parameters.AddWithValue("@OrgID", Session["OrgID"]);

            cnSselData.Open();
            int CanAddClient = Convert.ToInt32(cmdOrgStat.ExecuteScalar());
            cnSselData.Close();

            // need to have an organization w/ a department to add a client
            // need to have a client to add an account

            // add/modify controls
            if (CanAddClient > 1) btnAddModClient.Enabled = true;
            if (CanAddClient == 3) btnAddModAccount.Enabled = true;
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
            bool noDept;

            using (SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            using (SqlCommand cmdDepartment = new SqlCommand("Department_Select", cnSselData))
            {
                cmdDepartment.CommandType = CommandType.StoredProcedure;
                cmdDepartment.Parameters.AddWithValue("@Action", "ByOrg");
                cmdDepartment.Parameters.AddWithValue("@OrgID", Session["OrgID"]);
                cnSselData.Open();
                SqlDataReader rdr = cmdDepartment.ExecuteReader(CommandBehavior.CloseConnection);
                noDept = !rdr.Read();
                cnSselData.Close();
            }

            if (noDept)
                LNF.Web.ServerJScript.JSAlert(Page, "Please add a department to the organization before adding clients.");
            else
                Response.Redirect(page);
        }

        protected void btnFoobar_Click(object sender, EventArgs e)
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);
            LNF.CommonTools.Encryption myEncryption = new LNF.CommonTools.Encryption();

            SqlCommand cmdUpdatePwd = new SqlCommand("Client_Update", cnSselData);
            cmdUpdatePwd.CommandType = CommandType.StoredProcedure;
            cmdUpdatePwd.Parameters.AddWithValue("@Action", "foobar");
            cmdUpdatePwd.Parameters.AddWithValue("@Password", myEncryption.EncryptText("foobar"));

            cnSselData.Open();
            cmdUpdatePwd.ExecuteNonQuery();
            cnSselData.Close();
        }
    }
}
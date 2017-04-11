using LNF;
using LNF.CommonTools;
using LNF.Data;
using LNF.Models.Data;
using LNF.PhysicalAccess;
using LNF.Repository;
using LNF.Web;
using LNF.Web.Content;
using sselData.AppCode;
using sselData.AppCode.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using repo = LNF.Repository.Data;

namespace sselData
{
    public partial class Client : LNFPage
    {
        struct AlertInfo
        {
            public string Comment;
            public Control FormField;
        }

        private SqlConnection cnSselData;
        private DataSet dsClient;
        private int selectedClientOrgId; //holds the ClientOrgID that the current user is trying to modify
        private int oldClientID;
        private int oldAddressID;
        private int oldClientOrgID;

        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        private int GetSessionOrgID()
        {
            if (Session["OrgID"] == null)
            {
                Response.Redirect("~", true);
                return 0;
            }
            else
                return Convert.ToInt32(Session["OrgID"]);
        }

        private void FillDemographicsRadioButtonList(SQLDBAccess dba, RadioButtonList rbl, string demType)
        {
            string textField = demType;
            string valueField = string.Format("{0}ID", demType);

            dba.SelectCommand.SetParameterValue("@DemType", demType);
            using (var reader = dba.ExecuteReader("Dem_Select"))
            {
                rbl.DataSource = reader;
                rbl.DataTextField = textField;
                rbl.DataValueField = valueField;
                rbl.DataBind();
                reader.Close();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var cookie = Request.Cookies["lnf-download.lastLocation"];
            if (cookie != null)
                Response.Cookies.Add(new HttpCookie("lnf-download.lastLocation", "") { Expires = DateTime.Now.AddYears(1) });

            if (!User.IsInRole("Administrator"))
            {
                Session.Abandon();
                Response.Redirect(Session["Logout"] + "?Action=Exit");
            }

            if (Page.IsPostBack)
            {
                dsClient = (DataSet)Cache.Get(DataUtility.CacheID);
                if (dsClient == null)
                    Response.Redirect("~");
                else if (dsClient.DataSetName != "Client")
                    Response.Redirect("~");
            }
            else
            {
                Cache.Remove(DataUtility.CacheID); // remove anything left in cache

                int orgId = GetSessionOrgID();

                dsClient = DataUtility.GetClientDataSet(orgId);

                // populate Org ddl
                DataView dvOrg = dsClient.Tables["Org"].DefaultView;
                dvOrg.Sort = "OrgName ASC";
                ddlOrg.DataSource = dvOrg;
                ddlOrg.DataValueField = "OrgID";
                ddlOrg.DataTextField = "OrgName";
                ddlOrg.DataBind();
                ddlOrg.Items.Insert(0, new ListItem("All organizations", "0"));
                ddlOrg.ClearSelection();

                // populate the Privs radio list
                cblPriv.Items.LoadPrivs();

                using (var dba = new SQLDBAccess("cnSselData"))
                {
                    // fill in ddl for department
                    using (var reader = dba.ApplyParameters(new { Action = "ByOrg", OrgID = orgId }).ExecuteReader("Department_Select"))
                    {
                        ddlDepartment.DataSource = reader;
                        ddlDepartment.DataTextField = "Department";
                        ddlDepartment.DataValueField = "DepartmentID";
                        ddlDepartment.DataBind();
                        reader.Close();
                    }

                    // fill in ddl for role
                    using (var reader = dba.ApplyParameters(new { TableName = "Role" }).ExecuteReader("Global_Select"))
                    {
                        ddlRole.DataSource = reader;
                        ddlRole.DataTextField = "Role";
                        ddlRole.DataValueField = "RoleID";
                        ddlRole.DataBind();
                        reader.Close();
                    }

                    // fill in ddl for usertype
                    using (var reader = dba.ApplyParameters(new { TableName = "Community" }).ExecuteReader("Global_Select"))
                    {
                        cblCommunities.DataSource = reader;
                        cblCommunities.DataTextField = "Community";
                        cblCommunities.DataValueField = "CommunityFlag";
                        cblCommunities.DataBind();
                        reader.Close();
                    }

                    // fill in ddl for technical interest
                    using (var reader = dba.ApplyParameters(new { TableName = "TechnicalField" }).ExecuteReader("Global_Select"))
                    {
                        ddlTechnicalInterest.DataSource = reader;
                        ddlTechnicalInterest.DataTextField = "TechnicalField";
                        ddlTechnicalInterest.DataValueField = "TechnicalFieldID";
                        ddlTechnicalInterest.DataBind();
                        reader.Close();
                    }

                    // fill in demographics RBL 
                    dba.SelectCommand.ClearParameters();
                    dba.SelectCommand.AddParameter("@Action", "All");
                    dba.SelectCommand.AddParameter("@DemType", DbType.String, 30);

                    FillDemographicsRadioButtonList(dba, rblCitizen, "DemCitizen");
                    FillDemographicsRadioButtonList(dba, rblEthnic, "DemEthnic");
                    FillDemographicsRadioButtonList(dba, rblRace, "DemRace");
                    FillDemographicsRadioButtonList(dba, rblGender, "DemGender");
                    FillDemographicsRadioButtonList(dba, rblDisability, "DemDisability");
                }

                //Declare default sort parameter and sort direction
                ViewState["dgClientSortDir"] = " ASC";

                Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                SetPageControlsAndBind(false, false, false, false, true);
            }
        }

        protected void DialogDuplicateName_Command(object sender, CommandEventArgs e)
        {
            panDialogDuplicateName.Visible = false;
            panDisplay.Visible = true;

            switch (e.CommandName)
            {
                case "yes":
                    StoreClientInfo();
                    SetPageControlsAndBind(false, false, false, false, true);
                    break;
                case "no":
                    SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    break;
            }
        }

        protected void DialogError_Command(object sender, CommandEventArgs e)
        {
            panDialogError.Visible = false;
            panDisplay.Visible = true;

            switch (e.CommandName)
            {
                case "client":
                    Response.Redirect("~/Client.aspx");
                    break;
                case "home":
                    Response.Redirect("~");
                    break;
            }
        }

        private void SetPageControlsAndBind(bool showAddEditPanel, bool showAddExistingPanel, bool showClientOrgPanel, bool inlineAddressEdit, bool recalcDisplayInfo)
        {
            int sessionOrgId = GetSessionOrgID();

            bool panelVis = showAddEditPanel || showAddExistingPanel;

            // determine which panel to display
            pClientList.Visible = !panelVis;
            pAddEdit.Visible = showAddEditPanel;
            pExisting.Visible = showAddExistingPanel;
            pClientOrg.Visible = showClientOrgPanel;

            if (recalcDisplayInfo)
                SetDisplayInfo();

            // set page header label
            if (pClientList.Visible)
            {
                DataRow odr = dsClient.Tables["Org"].Rows.Find(sessionOrgId);
                lblHeader.Text = string.Format("Configure Clients for {0}", odr["OrgName"]);

                DataView dv = dsClient.Tables["Client"].DefaultView;
                dv.Sort = "DisplayName" + ViewState["dgClientSortDir"].ToString();
                dv.RowFilter = "DisplayInfo = ''";
                dgClient.DataSource = dv;
                dgClient.DataBind();

                // now, add to ddlPager for paging
                ddlPager.Items.Clear();
                int pSize = dgClient.PageSize;
                for (int i = 0; i < dv.Count; i += pSize)
                {
                    ListItem pagerItem = new ListItem();
                    pagerItem.Value = (i / pSize).ToString();
                    pagerItem.Text = dv[i].Row["LName"] + " ... " + dv[(i + (pSize - 1) >= dv.Count ? dv.Count - 1 : i + (pSize - 1))].Row["LName"];
                    ddlPager.Items.Add(pagerItem);
                }
                ddlPager.SelectedValue = dgClient.CurrentPageIndex.ToString();
            }
            else if (pAddEdit.Visible)
            {
                SetFocus(txtFName);
                if (btnClientSave.Text.ToLower().Contains("new"))
                {
                    lblHeader.Text = "Add new Client";
                    txtUsername.Enabled = true;
                    panLDAPLookup.Visible = true;
                }
                else
                {
                    int clientId = Convert.ToInt32(btnClientSave.CommandArgument);
                    DataRow cdr = dsClient.Tables["Client"].Rows.Find(clientId);
                    lblHeader.Text = string.Format("Configure Client {0}", cdr["DisplayName"]);
                    txtUsername.Enabled = false;
                }
            }
            else if (pExisting.Visible)
            {
                lblHeader.Text = "Add existing Client";
                btnExistingQuit.Visible = true;
            }

            // set other controls
            if (pClientOrg.Visible)
            {
                btnExistingQuit.Visible = false; // only valid prior to selection

                // turn off add new and add existing buttons during data edit
                btnClientSave.Enabled = !inlineAddressEdit;
                btnClientQuit.Enabled = !inlineAddressEdit;
                dgAddress.ShowFooter = !inlineAddressEdit;

                DataRow odr = dsClient.Tables["Org"].Rows.Find(sessionOrgId);
                lblClientOrg.Text = string.Format("for {0}", odr["OrgName"]);

                int clientId = Convert.ToInt32(btnClientSave.CommandArgument);
                DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));

                // only set the fields when the client has just been selected
                if (pExisting.Visible && Request.Form["__EVENTTARGET"] == "ddlClient")
                {
                    if (codrs.Length > 0)
                        FillClientOrgPanel(codrs[0]);
                    else
                        ClearClientOrgPanel();
                }

                // filter the addresses
                DataView dva = dsClient.Tables["Address"].DefaultView;
                dva.RowFilter = "AddDelete = 1"; // pick up any newly added addresses
                if (codrs.Length == 1)
                {
                    // if affiliated with this org, this row must exist
                    dva.RowFilter += string.Format(" OR AddressID = {0}", codrs[0]["ClientAddressID"]);
                }

                dgAddress.DataSource = dva;
                dgAddress.DataBind();

                // filter the managers
                DataView dvm = dsClient.Tables["ClientManager"].DefaultView;
                if (clientId == 0)
                    dvm.RowFilter = "ClientOrgID = 0";
                else
                {
                    if (codrs.Length == 1)
                    {
                        // if affiliated with this org, this row must exist
                        dvm.RowFilter = string.Format("Active = 1 AND ClientOrgID = {0}", codrs[0]["ClientOrgID"]);
                    }
                    else
                    {
                        // no manager at this org - filter returns nothing unless manager just added
                        dvm.RowFilter = "Active = 1 AND ClientOrgID = 0";
                    }
                }

                dvm.Sort = "DisplayName";
                dgClientManager.DataSource = dvm;
                dgClientManager.DataBind();

                UpdateAccountList();

                // it will be one if it's edit, else if add new user, codrs has no data
                if (codrs.Length > 0)
                {
                    //This is needed for billing types feature
                    selectedClientOrgId = Convert.ToInt32(codrs[0]["ClientOrgID"]);
                }

                FillPhysicalAccess(clientId);
            }
            else
            {
                rptPhysicalAccess.Visible = false;
                phPhysicalAccessNoData.Visible = false;
            }
        }

        private void FillPhysicalAccess(int clientId)
        {
            phPhysicalAccessNoData.Visible = true;
            rptPhysicalAccess.Visible = false;

            if (clientId > 0)
            {
                repo.Client c = DA.Current.Single<repo.Client>(clientId);

                var badges = Providers.PhysicalAccess.GetBadge(c);

                if (badges.Count() > 0)
                {
                    phPhysicalAccessNoData.Visible = false;
                    rptPhysicalAccess.Visible = true;
                    rptPhysicalAccess.DataSource = badges;
                    rptPhysicalAccess.DataBind();
                }
            }
        }

        private void SetDisplayInfo()
        {
            int sessionOrgId = GetSessionOrgID();

            // for each client:
            // if it is active at the current org, DisplayInfo = ""
            // else, loop through all orgs and show status
            foreach (DataRow cdr in dsClient.Tables["Client"].Rows)
            {
                DataRowState rowState = cdr.RowState;

                DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1} AND Active = 1", cdr["ClientID"], sessionOrgId));
                if (codrs.Length == 1) // associated with current org and active
                    cdr["DisplayInfo"] = string.Empty;
                else
                {
                    cdr["DisplayInfo"] = cdr["DisplayName"].ToString() + ": ";
                    codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0}", cdr["ClientID"]));
                    for (int i = 0; i < codrs.Length; i++) // loop through all orgs
                    {
                        DataRow[] odrs = dsClient.Tables["Org"].Select(string.Format("OrgID = {0}", codrs[i]["OrgID"]));
                        if (i > 0) cdr["DisplayInfo"] = cdr["DisplayInfo"].ToString() + ", ";
                        cdr["DisplayInfo"] = cdr["DisplayInfo"].ToString() + odrs[0]["OrgName"].ToString();
                        if (Convert.ToBoolean(codrs[i]["Active"]))
                            cdr["DisplayInfo"] = cdr["DisplayInfo"].ToString() + " (A)";
                        else
                            cdr["DisplayInfo"] = cdr["DisplayInfo"].ToString() + " (I)";
                    }
                }

                if (rowState == DataRowState.Unchanged)
                    cdr.AcceptChanges(); // this is so adding DisplayInfo doesn't cause the row to be marked as modified
            }
        }

        protected void dgClient_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();

            int clientId;
            DataRow[] codrs;
            DataRow cdr;

            switch (e.CommandName)
            {
                case "AddNew":
                    txtFName.Text = string.Empty;
                    txtMName.Text = string.Empty;
                    txtLName.Text = string.Empty;
                    txtUsername.Text = string.Empty;

                    pp1.SelectedPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    pp2.SelectedPeriod = DateTime.Parse("1/1/2007");

                    rblCitizen.ClearSelection();
                    rblEthnic.ClearSelection();
                    rblRace.ClearSelection();
                    rblGender.ClearSelection();
                    rblDisability.ClearSelection();
                    cblPriv.ClearSelection();
                    cblCommunities.ClearSelection();
                    ddlTechnicalInterest.ClearSelection();
                    rdolistBillingType.ClearSelection();
                    //by default, we select the regular billing type
                    foreach (ListItem btn in rdolistBillingType.Items)
                    {
                        if (btn.Text == "Regular")
                        {
                            btn.Selected = true;
                            break;
                        }
                    }

                    ClearClientOrgPanel();

                    btnClientSave.CommandArgument = "0";
                    btnClientSave.Text = "Store New Client";

                    // if org has DefClientAddr, add row to addr table with this info
                    DataRow odr = dsClient.Tables["Org"].Rows.Find(sessionOrgId);
                    if (Convert.ToInt32(odr["DefClientAddressID"]) != 0)
                    {
                        DataRow[] rows = dsClient.Tables["Address"].Select("AddDelete = 1");
                        if (rows.Length == 0)
                        {
                            // need to copy element by element - is there a better way?
                            DataRow sdr = dsClient.Tables["Address"].Rows.Find(odr["DefClientAddressID"]);
                            DataRow dr = dsClient.Tables["Address"].NewRow();
                            for (int i = 1; i < dsClient.Tables["Address"].Columns.Count; i++) // skip the AddressID column
                                dr[i] = sdr[i];
                            dr["AddDelete"] = true;
                            dsClient.Tables["Address"].Rows.Add(dr);
                        }
                    }

                    Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    SetPageControlsAndBind(true, false, true, false, false);
                    break;
                case "Edit":
                    clientId = Convert.ToInt32(dgClient.DataKeys[e.Item.ItemIndex]);
                    cdr = dsClient.Tables["Client"].Rows.Find(clientId);

                    txtFName.Text = cdr["FName"].ToString();
                    txtMName.Text = cdr["MName"].ToString();
                    txtLName.Text = cdr["LName"].ToString();
                    txtUsername.Text = cdr["UserName"].ToString();

                    rblCitizen.SelectedValue = cdr["DemCitizenID"].ToString();
                    rblEthnic.SelectedValue = cdr["DemEthnicID"].ToString();
                    rblRace.SelectedValue = cdr["DemRaceID"].ToString();
                    rblGender.SelectedValue = cdr["DemGenderID"].ToString();
                    rblDisability.SelectedValue = cdr["DemDisabilityID"].ToString();

                    if (Convert.ToInt32(cdr["Communities"]) > 0)
                    {
                        foreach (ListItem li in cblCommunities.Items)
                        {
                            if ((Convert.ToInt32(li.Value) & Convert.ToInt32(cdr["Communities"])) > 0)
                                li.Selected = true;
                        }
                    }
                    WebUtility.FillDropDownLIst(ddlTechnicalInterest, cdr["TechnicalInterestID"]);

                    // select proper Privs's
                    foreach (ListItem privItem in cblPriv.Items)
                    {
                        int pVal = int.Parse(privItem.Value);
                        privItem.Selected = (Convert.ToInt32(cdr["Privs"]) & pVal) == pVal;
                    }

                    // again, exactly one row must be returned
                    codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", cdr["ClientID"], sessionOrgId));
                    FillClientOrgPanel(codrs[0]);

                    btnClientSave.CommandArgument = clientId.ToString();
                    btnClientSave.Text = "Store modified data";

                    Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    SetPageControlsAndBind(true, false, true, false, false);

                    //need to bind again to evoke the data bound event of rdolistBillingType so correct billingtype can be set
                    rdolistBillingType.DataBind();
                    break;
                case "AddExisting":
                    ddlClient.Enabled = true;
                    SetClientDDL();
                    btnClientSave.CommandArgument = "-1";
                    btnClientSave.Text = "Add Client to Organization";
                    SetPageControlsAndBind(false, true, false, false, false);
                    break;
                case "Delete":
                    // check if managing accounts or people first
                    string cannotDelete = string.Empty;

                    clientId = Convert.ToInt32(dgClient.DataKeys[e.Item.ItemIndex]);
                    codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));

                    if (codrs.Length == 0)
                    {
                        string script = @"<script type=""text/javascript"" id=""showWarning""> ";
                        script += "alert('Serious error - No ClientOrgID for selected client');";
                        script += "</script>";

                        if (!Page.ClientScript.IsStartupScriptRegistered("showWarning"))
                            Page.ClientScript.RegisterStartupScript(typeof(Page), "showWarning", script);

                        return;
                    }

                    int clientOrgId = codrs[0].Field<int>("ClientOrgID");
                    string userName = string.Empty;
                    DataRow[] cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("ManagerOrgID = {0} AND Active = 1", clientOrgId));
                    for (int i = 0; i < cmdrs.Length; i++)
                    {
                        if (CanDeleteClientManager(cmdrs[i].Field<int>("ClientOrgID")))
                            continue;

                        if (string.IsNullOrEmpty(cannotDelete))
                        {
                            cannotDelete = "Cannot disable selected client---.\\n";
                            cannotDelete += "Curently managing the following clients: ";
                        }
                        else
                            cannotDelete += "; ";

                        DataRow codr = dsClient.Tables["ClientOrg"].Rows.Find(cmdrs[i].Field<int>("ClientOrgID"));
                        cdr = dsClient.Tables["Client"].Rows.Find(codr.Field<int>("ClientID"));

                        //2008-01-15 The code below is to handle user name that has apostrophe
                        userName = cdr.Field<string>("DisplayName");
                        if (userName.Contains("'"))
                            userName = userName.Replace("'", " ");
                        cannotDelete += userName;
                    }

                    if (!string.IsNullOrEmpty(cannotDelete))
                        cannotDelete += "\\n";

                    DataRow[] madrs = dsClient.Tables["ClientAccount"].Select(string.Format("ClientOrgID = {0} AND Active = 1 AND Manager = 1", clientOrgId));
                    for (int i = 0; i < madrs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(cannotDelete))
                        {
                            cannotDelete = "Cannot disable selected client.\\n";
                            cannotDelete += "Curently managing the following accounts: ";
                        }
                        else if (cannotDelete.EndsWith("\n"))
                            cannotDelete += "Curently managing the following accounts: ";
                        else
                            cannotDelete += "; ";
                        cannotDelete += madrs[i].Field<string>(rblAcctDisplay.SelectedValue);
                    }


                    // if problems exist, show message, otherwise disable
                    if (cannotDelete.Length > 0)
                    {
                        string strScript = @"<script type=""text/javascript"" id=""showWarning""> ";
                        strScript += string.Format("alert('{0}');", cannotDelete);
                        strScript += "</script>";

                        if (!Page.ClientScript.IsStartupScriptRegistered("showWarning"))
                            Page.ClientScript.RegisterStartupScript(typeof(Page), "showWarning", strScript);
                    }
                    else
                    {
                        cdr = dsClient.Tables["Client"].Rows.Find(clientId);

                        DataRow[] sdrs = dsClient.Tables["Address"].Select(string.Format("AddressID = {0}", codrs[0].Field<int>("ClientAddressID")));
                        if (sdrs.Length > 0)
                        {
                            if (cdr.RowState == DataRowState.Added)
                                dsClient.Tables["Address"].Rows.Remove(sdrs[0]); // if it is a new client, we don't need its address
                        }

                        cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("Active = 1 AND ClientOrgID = {0}", clientOrgId));
                        for (int j = 0; j < cmdrs.Length; j++)
                        {
                            if (cdr.RowState == DataRowState.Added) // if it is a new client, we don't need its manager
                                dsClient.Tables["ClientManager"].Rows.Remove(cmdrs[j]);
                            else if (DataUtility.IsReactivated(cmdrs[j])) // if reactivated then deleted, change nothing
                                cmdrs[j].RejectChanges();
                            else
                                DataUtility.SetActiveFalse(cmdrs[j]);
                        }

                        DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("Active = 1 AND ClientOrgID = {0}", clientOrgId));
                        for (int j = 0; j < cadrs.Length; j++)
                        {
                            if (cdr.RowState == DataRowState.Added) // if it is a new client, we don't need its account
                                dsClient.Tables["ClientAccount"].Rows.Remove(cadrs[j]);
                            else if (DataUtility.IsReactivated(cadrs[j])) // if reactivated then deleted, change nothing
                                cadrs[j].RejectChanges();
                            else
                                DataUtility.SetActiveFalse(cadrs[j]);
                        }

                        // possible to still be active but not have any active accounts
                        bool hasActiveAcct = DataUtility.HasActiveAccount(cdr, dsClient.Tables["ClientOrg"], dsClient.Tables["ClientAccount"]);
                        if (!hasActiveAcct)
                            cdr.SetField("EnableAccess", false);

                        if (cdr.RowState == DataRowState.Added) // if it is a new client, we don't need its org assoc
                            dsClient.Tables["ClientOrg"].Rows.Remove(codrs[0]);
                        else if (DataUtility.IsReactivated(codrs[0])) // if reactivated then deleted, change nothing
                            codrs[0].RejectChanges();
                        else
                            DataUtility.SetActiveFalse(codrs[0]);

                        if (cdr.RowState == DataRowState.Added)
                            dsClient.Tables["Client"].Rows.Remove(cdr);
                        else if (DataUtility.IsReactivated(cdr)) // if reactivated then deleted, change nothing
                            cdr.RejectChanges();
                        else
                        {
                            codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND Active = 1", cdr["ClientID"]));
                            //[2014-08-13 jg] should be codrs.Length == 0 !!! otherwise we deactivate the client even when they still have one active org
                            if (codrs.Length == 0) // no active organizations for Client
                                DataUtility.SetActiveFalse(cdr);
                        }

                        SetPageControlsAndBind(false, false, false, false, true);
                        Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    }
                    break;
            }
        }

        private bool CanDeleteClientManager(int ClientOrgID)
        {
            repo.ClientOrg co = DA.Current.Single<repo.ClientOrg>(ClientOrgID);
            if (!co.Client.Active)
                return true;
            if (!co.Active)
                return true;
            return false;
        }

        private void ClearClientOrgPanel()
        {
            // this needs to be done from two places
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            ddlRole.ClearSelection();
            ddlDepartment.ClearSelection();
            chkManager.Checked = false;
            chkFinManager.Checked = false;
        }

        private void FillClientOrgPanel(DataRow codr)
        {
            // this needs to be done from two places
            txtEmail.Text = codr["Email"].ToString();
            txtPhone.Text = WebUtility.FillField(codr["Phone"], string.Empty);
            ddlRole.SelectedValue = codr["RoleID"].ToString();
            ddlDepartment.SelectedValue = codr["DepartmentID"].ToString();
            chkManager.Checked = Convert.ToBoolean(codr["IsManager"]);
            chkFinManager.Checked = Convert.ToBoolean(codr["IsFinManager"]);
            try
            {
                DateTime SubsidyStartDate = Convert.ToDateTime(codr["SubsidyStartDate"]);
                pp1.SelectedPeriod = new DateTime(SubsidyStartDate.Year, SubsidyStartDate.Month, 1);
            }
            catch
            {
                pp1.SelectedPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            DateTime NewFacultyStartDate = Convert.ToDateTime(codr["NewFacultyStartDate"]);
            pp2.SelectedPeriod = new DateTime(NewFacultyStartDate.Year, NewFacultyStartDate.Month, 1);
        }

        protected void dgClient_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((Label)e.Item.FindControl("lblClientName")).Text = drv["DisplayName"].ToString();

                Literal lit = (Literal)e.Item.FindControl("litDryBoxMessage");
                int clientId = Convert.ToInt32(drv["ClientID"]);

                DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));

                if (codrs.Length > 0)
                {
                    int ClientOrgID = Convert.ToInt32(codrs[0]["ClientOrgID"]);
                    if (ClientOrgID > 0) //If the ClientOrg was just added, and changes haven't been saved yet, then ClientOrgID = -1
                    {
                        repo.ClientOrg co = DA.Current.Single<repo.ClientOrg>(ClientOrgID);
                        if (co != null) //Just in case...
                        {
                            if (co.HasDryBox())
                                lit.Text = string.Format(@"<img src=""images/im_drybox.gif"" title=""DryBox reserved with account: {0}"" />", co.GetDryBoxClientAccount().Account.Name);
                        }
                    }
                }
            }
        }

        // serves to page dgClient
        protected void ddlPager_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgClient.CurrentPageIndex = Convert.ToInt32(ddlPager.SelectedValue); // selectedItem should also work
            SetPageControlsAndBind(false, false, false, false, false);
        }

        protected void dgClient_SortCommand(object source, DataGridSortCommandEventArgs e)
        {
            //Flip-flop sort direction 
            if (ViewState["dgClientSortDir"].ToString() == " ASC")
                ViewState["dgClientSortDir"] = " DESC";
            else
                ViewState["dgClientSortDir"] = " ASC";

            SetPageControlsAndBind(false, false, false, false, false);
        }

        protected void ddlOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetClientDDL();
        }

        protected void txtName_TextChanged(object sender, EventArgs e)
        {
            SetClientDDL(); // name filter 
        }

        private void SetClientDDL()
        {
            DataView dv = dsClient.Tables["Client"].DefaultView;
            dv.Sort = "DisplayInfo ASC";
            dv.RowFilter = "DisplayInfo <> ''";

            ddlClient.DataSource = dv;
            ddlClient.DataTextField = "DisplayInfo";
            ddlClient.DataValueField = "ClientID";
            ddlClient.DataBind();

            // now, go through and remove items that do not meet the filters
            for (int i = ddlClient.Items.Count - 1; i >= 0; i--)
            {
                // remove items whose DisplayInfo does not contain the org of interest
                bool noskip = true;
                if (Convert.ToInt32(ddlOrg.SelectedValue) > 0)
                {
                    if (!ddlClient.Items[i].Text.Contains(ddlOrg.SelectedItem.Text))
                    {
                        ddlClient.Items.Remove(ddlClient.Items[i]);
                        noskip = false;
                    }
                }

                // only match specified text
                if (noskip)
                {
                    if (txtName.Text.Trim().Length > 0)
                    {
                        if (!ddlClient.Items[i].Text.ToLower().Contains(txtName.Text.Trim().ToLower()))
                            ddlClient.Items.Remove(ddlClient.Items[i]);
                    }
                }
            }

            // now add a blank item so SelectedIndexChnaged works for the first name in the list
            ListItem blankItem = new ListItem();
            blankItem.Value = "0";
            blankItem.Text = string.Empty;
            ddlClient.Items.Insert(0, blankItem);
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();

            // if client is currently inactive, reactivate
            DataRow cdr = dsClient.Tables["Client"].Rows.Find(ddlClient.SelectedValue);
            if (cdr.RowState == DataRowState.Modified) // must have just been disabled
                cdr.RejectChanges();
            else
            {
                if (!Convert.ToBoolean(cdr["Active"]))
                {
                    cdr["Active"] = true;
                    cdr["Reactivated"] = true;
                }
            }

            // check if Client has previous affiliation with org
            DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", ddlClient.SelectedValue, sessionOrgId));
            if (codrs.Length == 1)
            {
                if (codrs[0].RowState == DataRowState.Modified) // must have just been disabled
                {
                    codrs[0].RejectChanges();

                    // reject all changes made to ClientManager and ClientAccount records
                    DataRow[] cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("ClientOrgID = {0}", codrs[0]["ClientOrgID"]));
                    for (int i = 0; i < cmdrs.Length; i++)
                    {
                        if (cmdrs[i].RowState == DataRowState.Modified)
                            cmdrs[i].RejectChanges();
                    }

                    DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("ClientOrgID = {0}", codrs[0]["ClientOrgID"]));
                    for (int i = 0; i < cadrs.Length; i++)
                    {
                        if (cadrs[i].RowState == DataRowState.Modified)
                            cadrs[i].RejectChanges();
                    }
                }
                else
                {
                    codrs[0]["Active"] = true;
                    codrs[0]["Reactivated"] = true;

                    // was affiliated - restore prior managers and accounts
                    // this requires ActiveLog since C and CO are unique records and must be turned back on
                    // these others may have been disabled at an earlier date, so we cannot simply turn all back on
                    // also need to check for specific mgr/acct if several orgs assocs disabled at same time
                    // also need to check that the others are still valid
                    cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                    SqlDataAdapter daLogRecordByRecord = new SqlDataAdapter("ActiveLog_Select", cnSselData);
                    daLogRecordByRecord.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daLogRecordByRecord.SelectCommand.Parameters.AddWithValue("@Action", "ByRecord");
                    daLogRecordByRecord.SelectCommand.Parameters.AddWithValue("@TableName", "ClientOrg");
                    daLogRecordByRecord.SelectCommand.Parameters.AddWithValue("@Record", codrs[0]["ClientOrgID"]);

                    DataTable dtLogRecByRec = new DataTable();
                    daLogRecordByRecord.Fill(dtLogRecByRec); // should be exactly one row

                    if (dtLogRecByRec.Rows.Count == 1)
                    {
                        //Dim dr, adr, codr, cmdr, cmdrs(), cadr, madrs() As DataRow
                        SqlDataAdapter daLogRecordByDate = new SqlDataAdapter("ActiveLog_Select", cnSselData);

                        // NOTE: This can potentially return multipel records
                        daLogRecordByDate.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daLogRecordByDate.SelectCommand.Parameters.AddWithValue("@Action", "ByDate");
                        daLogRecordByDate.SelectCommand.Parameters.AddWithValue("@DisableDate", dtLogRecByRec.Rows[0]["DisableDate"]);
                        daLogRecordByDate.SelectCommand.Parameters.Add("@TableName", SqlDbType.NVarChar, 50);

                        // get records for ClientManager
                        daLogRecordByDate.SelectCommand.Parameters["@TableName"].Value = "ClientManager";
                        DataTable dtClientMgrRec = new DataTable();
                        daLogRecordByDate.Fill(dtClientMgrRec);

                        // get records for ClientAccount
                        daLogRecordByDate.SelectCommand.Parameters["@TableName"].Value = "ClientAccount";
                        DataTable dtClientAcctRec = new DataTable();
                        daLogRecordByDate.Fill(dtClientAcctRec);

                        foreach (DataRow dr in dtClientMgrRec.Rows)
                        {
                            // check that the manager is in the same org and is active
                            DataRow cmdr = dsClient.Tables["ClientManager"].Rows.Find(dr["Record"]);
                            // this culls any spurious rows that have the same disable time
                            if (cmdr != null && cmdr["ClientOrgID"] == codrs[0]["ClientOrgID"])
                            {
                                DataRow codr = dsClient.Tables["ClientOrg"].Rows.Find(cmdr["ManagerOrgID"]);
                                if (codr.Field<int>("OrgID") == sessionOrgId && codr.Field<bool>("Active"))
                                {
                                    cmdr["Active"] = true;
                                    cmdr["Reactivated"] = true;
                                }
                            }
                        }

                        foreach (DataRow dr in dtClientAcctRec.Rows)
                        {
                            // check that the account is in the same org and is active
                            DataRow cadr = dsClient.Tables["ClientAccount"].Rows.Find(dr["Record"]);
                            // this culls any spurious rows that have the same disable time
                            if (cadr["ClientOrgID"] == codrs[0]["ClientOrgID"])
                            {
                                DataRow adr = dsClient.Tables["Account"].Rows.Find(cadr["AccountID"]);
                                if (adr.Field<int>("OrgID") == sessionOrgId && adr.Field<bool>("Active"))
                                {
                                    // now check that one of this clients managers is managing this account
                                    DataRow[] madrs = dsClient.Tables["ClientAccount"].Select(string.Format("AccountID = {0} AND Manager = 1", cadr["AccountID"]));
                                    for (int i = 0; i < madrs.Length; i++)
                                    {
                                        DataRow[] cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("ClientOrgID = {0} AND ManagerOrgID = {1} AND Active = 1", codrs[0]["ClientOrgID"], madrs[i]["ClientOrgID"]));
                                        if (cmdrs.Length > 0)
                                        {
                                            cadr["Active"] = true;
                                            cadr["Reactivated"] = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (Convert.ToBoolean(codrs[0]["IsManager"]))
                        {
                            foreach (DataRow dr in dtClientAcctRec.Rows)
                            {
                                // check that the account is in the same org and is active and reactivated person is manager
                                DataRow cadr = dsClient.Tables["ClientAccount"].Rows.Find(dr["Record"]);
                                // this culls any spurious rows that have the same disable time
                                if (cadr["ClientOrgID"] == codrs[0]["ClientOrgID"])
                                {
                                    DataRow adr = dsClient.Tables["Account"].Rows.Find(cadr["AccountID"]);
                                    if (adr.Field<int>("OrgID") == sessionOrgId && adr.Field<bool>("Active") && cadr.Field<bool>("Manager"))
                                    {
                                        cadr["Active"] = true;
                                        cadr["Reactivated"] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else // no previous affiliation - set default address
            {
                // if org has DefClientAddr, add row to addr table with this info
                DataRow odr = dsClient.Tables["Org"].Rows.Find(sessionOrgId);
                if (Convert.ToInt32(odr["DefClientAddressID"]) != 0)
                {
                    // need to copy element by element - is there a better way?
                    DataRow sdr = dsClient.Tables["Address"].Rows.Find(odr["DefClientAddressID"]);
                    DataRow dr = dsClient.Tables["Address"].NewRow();
                    for (int i = 1; i < dsClient.Tables["Address"].Columns.Count; i++) // skip the AddressID column
                        dr[i] = sdr[i];
                    dr["AddDelete"] = true;
                    dsClient.Tables["Address"].Rows.Add(dr);
                }
            }

            ddlClient.Enabled = false;
            btnClientSave.CommandArgument = ddlClient.SelectedValue;
            SetPageControlsAndBind(false, true, true, false, false);
        }

        // This function is the single user save function - it save to the dataset instead of back to database
        protected void btnClientSave_Click(Object sender, EventArgs e)
        {
            panLDAPLookup.Visible = false;

            // instead of using field validators, check for values programmatically and display errors using alert
            AlertInfo[] strValidate;

            if (pAddEdit.Visible)
            {
                if (cblPriv.Items[0].Selected) // has lab user Privs
                    strValidate = new AlertInfo[9];
                else
                    strValidate = new AlertInfo[4];

                strValidate[0].FormField = txtFName;
                strValidate[0].Comment = "a first name.";
                strValidate[1].FormField = txtLName;
                strValidate[1].Comment = "a last name.";
                strValidate[2].FormField = txtUsername;
                strValidate[2].Comment = "a username.";
                strValidate[3].FormField = txtEmail;
                strValidate[3].Comment = "an email address.";
                if (cblPriv.Items[0].Selected) // has lab user Privs
                {
                    strValidate[4].FormField = rblCitizen;
                    strValidate[4].Comment = "citizenship status.";
                    strValidate[5].FormField = rblGender;
                    strValidate[5].Comment = "gender.";
                    strValidate[6].FormField = rblRace;
                    strValidate[6].Comment = "race.";
                    strValidate[7].FormField = rblEthnic;
                    strValidate[7].Comment = "ethnicity.";
                    strValidate[8].FormField = rblDisability;
                    strValidate[8].Comment = "disability status.";
                }
            }
            else
            {
                strValidate = new AlertInfo[1];
                strValidate[0].FormField = txtEmail;
                strValidate[0].Comment = "an email address.";
            }

            if (ValidateField(strValidate))
            {
                bool doStore = true;

                if (btnClientSave.Text.ToLower().Contains("new"))
                {
                    // first check that username is 20 char or less
                    if (txtUsername.Text.Trim().Length > 20)
                    {
                        ServerJScript.JSAlert(Page, "Username must be 20 characters or less.");
                        return;
                    }

                    // second check that username is unique
                    DataRow[] fdr = dsClient.Tables["Client"].Select(string.Format("UserName = '{0}'", txtUsername.Text.Trim()));
                    if (fdr.Length > 0)
                    {
                        ServerJScript.JSAlert(Page, "Username already exists. Please choose another username.");
                        return;
                    }

                    // now see if 
                    string FName = txtFName.Text.Trim();
                    string LName = txtLName.Text.Trim();
                    fdr = dsClient.Tables["Client"].Select(string.Format("FName LIKE '%{0}%' AND LName LIKE '%{1}%'", FName, LName));
                    if (fdr.Length > 0)
                    {
                        string strAlert = "<ul>";
                        for (int i = 0; i < fdr.Length; i++)
                            strAlert += string.Format("<li>{0} {1}</li>", fdr[i]["LName"], fdr[i]["FName"]);
                        strAlert += "</ul>";

                        litDuplicateNameMsg.Text = string.Empty;
                        litDuplicateNameMsg.Text = "The following users have a similar name as the newly entered user:";
                        litDuplicateNameMsg.Text += strAlert;
                        litDuplicateNameMsg.Text += "Do you still want to add the new user?";

                        panDialogDuplicateName.Visible = true;
                        panDisplay.Visible = false;
                        doStore = false;
                    }
                }

                //if the client has no active accounts at this point, as determined
                //by DataUtility.HasActiveAccount, then a new record will not be created in Prowatch
                if (doStore) StoreClientInfo();

                //2007-04-09 Attention
                //The whole Client page is based on disconnected data set - changes are only pushed back to server when user click the main save button
                //However, the implmentation of this mechanism in this page still has bugs - that things might still get saved even users click cancel button
                //To debug this problem takes too much time, so users are warned to save everything right after they made changes
                //Due to this bug, the newly added controls here will use the direct save methond - changes are pushed to server immediately
                if (btnClientSave.Text.ToLower().Contains("new") || true)
                {
                    try
                    {
                        BillingTypeDA.SetBillingTypeID(selectedClientOrgId, Convert.ToInt32(rdolistBillingType.SelectedItem.Value));
                    }
                    catch { }
                }
            }
        }

        private void StoreClientInfo()
        {
            int sessionOrgId = GetSessionOrgID();

            // add rows to Client, ClientSite and ClientOrg for new entries
            bool isNewClientEntry = btnClientSave.Text.ToLower().Contains("new");
            bool addExistingClient = btnClientSave.Text.ToLower().Contains("add");
            int clientId = isNewClientEntry ? 0 : Convert.ToInt32(btnClientSave.CommandArgument);
            bool enableAccessError;
            string alertMsg;

            DataUtility.StoreClientInfo(
                dtClient: dsClient.Tables["Client"],
                dtClientOrg: dsClient.Tables["ClientOrg"],
                dtClientAccount: dsClient.Tables["ClientAccount"],
                dtClientManager: dsClient.Tables["ClientManager"],
                dtAddress: dsClient.Tables["Address"],
                isNewClientEntry: isNewClientEntry,
                addExistingClient: addExistingClient,
                orgId: sessionOrgId,
                username: txtUsername.Text.Trim(),
                lname: txtLName.Text.Trim(),
                fname: txtFName.Text.Trim(),
                mname: txtMName.Text.Trim(),
                demCitizenId: NullCheck(rblCitizen),
                demEthnicId: NullCheck(rblEthnic),
                demRaceId: NullCheck(rblRace),
                demGenderId: NullCheck(rblGender),
                demDisabilityId: NullCheck(rblDisability),
                privs: (int)cblPriv.Items.CalculatePriv(),
                communities: cblCommunities.Items.CalculateCommunities(),
                technicalInterestId: Convert.ToInt32(ddlTechnicalInterest.SelectedValue),
                roleId: Convert.ToInt32(ddlRole.SelectedValue),
                departmentId: Convert.ToInt32(ddlDepartment.SelectedValue),
                email: txtEmail.Text,
                phone: txtPhone.Text,
                isManager: chkManager.Checked,
                isFinManager: chkFinManager.Checked,
                subsidyStartDate: pp1.SelectedPeriod,
                newFacultyStartDate: pp2.SelectedPeriod,
                clientId: ref clientId,
                clientOrgId: ref selectedClientOrgId,
                alertMsg: out alertMsg,
                enableAccessError: out enableAccessError);

            if (enableAccessError)
            {
                litError.Text = string.Format(@"<div class=""error"">Re-enable access error:<br />{0}</div>", alertMsg);
                panDialogError.Visible = true;
                panDisplay.Visible = false;
                return;
            }

            if (alertMsg.Length > 0)
                ServerJScript.JSAlert(Page, alertMsg);

            SetPageControlsAndBind(false, false, false, false, true);
        }

        private int NullCheck(RadioButtonList rbl)
        {
            if (rbl.SelectedIndex == -1)
                return 1;
            else
                return Convert.ToInt32(rbl.SelectedValue);
        }

        protected void btnClientQuit_Click(object sender, EventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();

            panLDAPLookup.Visible = false;

            using (SQLDBAccess dba = new SQLDBAccess("cnSselData"))
                DataUtility.GetClientManagerData(dba, dsClient, sessionOrgId);

            // remove any addresses that were added
            DataRow[] sdrs = dsClient.Tables["Address"].Select("AddDelete = 1");
            if (sdrs.Length == 1)
                sdrs[0].Delete();

            // unmark any addresses that were removed - only need to check for non-new entries
            int clientId = Convert.ToInt32(btnClientSave.CommandArgument);
            if (clientId != 0)
            {
                sdrs = dsClient.Tables["Address"].Select("AddDelete = 0");
                if (sdrs.Length == 1)
                {
                    int AddressID = Convert.ToInt32(sdrs[0]["AddressID"]);
                    sdrs[0]["AddDelete"] = DBNull.Value;
                    DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));
                    codrs[0]["ClientAddressID"] = AddressID;
                }
            }

            Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
            SetPageControlsAndBind(false, false, false, false, false);
        }

        protected void btnExistingQuit_Click(object sender, EventArgs e)
        {
            SetPageControlsAndBind(false, false, false, false, false);
        }

        protected void dgAddress_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            AlertInfo[] strValidate = new AlertInfo[3];

            // use AddDelete to mark new and deleted rows only
            // changes that are saved cannot be undone by quiting at the org layer

            int AddressID;
            DataRow sdr;

            switch (e.CommandName)
            {
                case "AddNewRow":
                    strValidate[0].FormField = (TextBox)e.Item.FindControl("txtStrAddress1F");
                    strValidate[0].Comment = "a street address.";
                    strValidate[1].FormField = (TextBox)e.Item.FindControl("txtCityF");
                    strValidate[1].Comment = "a city.";
                    strValidate[2].FormField = (TextBox)e.Item.FindControl("txtStateF");
                    strValidate[2].Comment = "a state.";
                    if (ValidateField(strValidate))
                    {
                        sdr = dsClient.Tables["Address"].NewRow();
                        sdr["InternalAddress"] = ((TextBox)e.Item.FindControl("txtInternalAddressF")).Text;
                        sdr["StrAddress1"] = ((TextBox)e.Item.FindControl("txtStrAddress1F")).Text;
                        sdr["StrAddress2"] = ((TextBox)e.Item.FindControl("txtStrAddress2F")).Text;
                        sdr["City"] = ((TextBox)e.Item.FindControl("txtCityF")).Text;
                        sdr["State"] = ((TextBox)e.Item.FindControl("txtStateF")).Text;
                        sdr["Zip"] = ((TextBox)e.Item.FindControl("txtZipF")).Text;
                        sdr["Country"] = ((TextBox)e.Item.FindControl("txtCountryF")).Text;
                        sdr["AddDelete"] = true;
                        dsClient.Tables["Address"].Rows.Add(sdr);

                        Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    }
                    break;
                case "Edit":
                    //Datagrid in edit mode, hide footer section 
                    dgAddress.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
                    SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, true, false);
                    break;
                case "Cancel":
                    //Datagrid leaving edit mode 
                    dgAddress.EditItemIndex = -1;
                    SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    break;
                case "Update":
                    strValidate[0].FormField = (TextBox)e.Item.FindControl("txtStrAddress1");
                    strValidate[0].Comment = "a street address.";
                    strValidate[1].FormField = (TextBox)e.Item.FindControl("txtCity");
                    strValidate[1].Comment = "a city.";
                    strValidate[2].FormField = (TextBox)e.Item.FindControl("txtState");
                    strValidate[2].Comment = "a state.";
                    if (ValidateField(strValidate))
                    {
                        AddressID = Convert.ToInt32(dgAddress.DataKeys[e.Item.ItemIndex]);
                        sdr = dsClient.Tables["Address"].Rows.Find(AddressID);
                        sdr["InternalAddress"] = ((TextBox)e.Item.FindControl("txtInternalAddress")).Text;
                        sdr["StrAddress1"] = ((TextBox)e.Item.FindControl("txtStrAddress1")).Text;
                        sdr["StrAddress2"] = ((TextBox)e.Item.FindControl("txtStrAddress2")).Text;
                        sdr["City"] = ((TextBox)e.Item.FindControl("txtCity")).Text;
                        sdr["State"] = ((TextBox)e.Item.FindControl("txtState")).Text;
                        sdr["Zip"] = ((TextBox)e.Item.FindControl("txtZip")).Text;
                        sdr["Country"] = ((TextBox)e.Item.FindControl("txtCountry")).Text;

                        Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        dgAddress.EditItemIndex = -1;
                        SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    }
                    break;
                case "Delete":
                    AddressID = Convert.ToInt32(dgAddress.DataKeys[e.Item.ItemIndex]);
                    sdr = dsClient.Tables["Address"].Rows.Find(AddressID);
                    if (sdr["AddDelete"] == DBNull.Value) // untouched - mark for deletion
                    {
                        sdr["AddDelete"] = false; // will set rowstate to modified

                        // set addrID in client to 0 - one such row must exist
                        DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientAddressID = {0}", AddressID));
                        codrs[0]["ClientAddressID"] = 0;
                    }
                    else
                    {
                        if (Convert.ToBoolean(sdr["AddDelete"])) // was just added, so simply remove it
                            sdr.Delete();
                    }

                    Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    break;
            }
        }

        protected void dgAddress_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;

            if (e.Item.ItemType == ListItemType.EditItem)
            {
                ((TextBox)e.Item.FindControl("txtInternalAddress")).Text = drv["InternalAddress"].ToString();
                ((TextBox)e.Item.FindControl("txtStrAddress1")).Text = drv["StrAddress1"].ToString();
                ((TextBox)e.Item.FindControl("txtStrAddress2")).Text = drv["StrAddress2"].ToString();
                ((TextBox)e.Item.FindControl("txtCity")).Text = drv["City"].ToString();
                ((TextBox)e.Item.FindControl("txtState")).Text = drv["State"].ToString();
                ((TextBox)e.Item.FindControl("txtZip")).Text = drv["Zip"].ToString();
                ((TextBox)e.Item.FindControl("txtCountry")).Text = drv["Country"].ToString();
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                // show unused items
                if (dgAddress.Items.Count == 1)
                    dgAddress.ShowFooter = false;
                else
                {
                    if (dgAddress.EditItemIndex == -1)
                        dgAddress.ShowFooter = true;
                }
                ((TextBox)e.Item.FindControl("txtCountryF")).Text = "US";
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                ((Label)e.Item.FindControl("lblInternalAddress")).Text = drv["InternalAddress"].ToString();
                ((Label)e.Item.FindControl("lblStrAddress1")).Text = drv["StrAddress1"].ToString();
                ((Label)e.Item.FindControl("lblStrAddress2")).Text = drv["StrAddress2"].ToString();
                ((Label)e.Item.FindControl("lblCity")).Text = drv["City"].ToString();
                ((Label)e.Item.FindControl("lblState")).Text = drv["State"].ToString();
                ((Label)e.Item.FindControl("lblZip")).Text = drv["Zip"].ToString();
                ((Label)e.Item.FindControl("lblCountry")).Text = drv["Country"].ToString();
            }
        }

        private bool ValidateField(AlertInfo[] strVal)
        {
            bool bError;
            string strAlert = string.Empty;
            for (int i = 0; i < strVal.Length; i++)
            {
                bError = false;
                if (strVal[i].FormField.ID.ToString().StartsWith("txt"))
                    bError = ((TextBox)strVal[i].FormField).Text.Trim().Length == 0;
                else // must be rbl
                    bError = ((RadioButtonList)strVal[i].FormField).SelectedIndex == -1;

                if (bError)
                {
                    if (strAlert.Length > 0)
                        strAlert += "\n";
                    strAlert += "Please specify " + strVal[i].Comment;
                }
            }

            if (strAlert.Length > 0)
            {
                ServerJScript.JSAlert(Page, strAlert);
                return false;
            }
            else
                return true;
        }

        protected void dgClientManager_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();

            // check if client manager relationship exists
            int clientId = Convert.ToInt32(btnClientSave.CommandArgument);
            DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));

            int clientOrgId;
            if (codrs.Length == 1) // not a new client
                clientOrgId = codrs[0].Field<int>("ClientOrgID");
            else
                clientOrgId = 0;

            DataRow[] cmdrs;

            switch (e.CommandName)
            {
                case "AddNew":
                    DropDownList ddlMgr = (DropDownList)e.Item.FindControl("ddlMgr");
                    if (ddlMgr.SelectedItem != null)
                    {
                        cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("ClientOrgID = {0} AND ManagerOrgID = {1}", clientOrgId, ddlMgr.SelectedValue));

                        if (cmdrs.Length == 1)
                            cmdrs[0].SetField("Active", true);
                        else
                        {
                            DataRow cmdr = dsClient.Tables["ClientManager"].NewRow();
                            cmdr.SetField("ClientOrgID", clientOrgId);
                            cmdr.SetField("ManagerOrgID", Convert.ToInt32(ddlMgr.SelectedValue));
                            cmdr.SetField("Active", true);
                            cmdr.SetField("DisplayName", ddlMgr.SelectedItem.Text);
                            dsClient.Tables["ClientManager"].Rows.Add(cmdr);
                        }

                        Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    }
                    break;
                case "Delete":
                    int key = Convert.ToInt32(dgClientManager.DataKeys[e.Item.ItemIndex]);
                    //Dim cadrs() As DataRow
                    DataRow[] madrs = dsClient.Tables["ClientAccount"].Select(string.Format("Active = 1 AND Manager = 1 AND ClientOrgID = {0}", key));

                    //Removes client/account associations that exist because of this manager (which is being deleted).
                    //What if the user is associated with another manager who also manages this account?
                    for (int i = 0; i < madrs.Length; i++)
                    {
                        int AccountID = Convert.ToInt32(madrs[i]["AccountID"]);
                        DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("Active = 1 AND ClientOrgID = {0} AND AccountID = {1}", clientOrgId, AccountID));
                        if (cadrs.Length > 0)
                        {
                            if (cadrs[0].RowState == DataRowState.Added)
                                cadrs[0].Delete();
                            else
                                DataUtility.SetActiveFalse(cadrs[0]);
                        }
                    }

                    //Remove the client/manager association
                    cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("Active = 1 AND ClientOrgID = {0} AND ManagerOrgID = {1}", clientOrgId, key));
                    if (cmdrs[0].RowState == DataRowState.Added)
                        cmdrs[0].Delete();
                    else
                        DataUtility.SetActiveFalse(cmdrs[0]);

                    Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    SetPageControlsAndBind(pAddEdit.Visible, pExisting.Visible, true, false, false);
                    break;
            }
        }

        protected void dgClientManager_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();

            if (e.Item.ItemType == ListItemType.Footer)
            {
                // populate ddlMgr
                DropDownList ddlMgr = (DropDownList)e.Item.FindControl("ddlMgr");
                DataView dv = dsClient.Tables["ClientOrg"].DefaultView;

                dv.RowFilter = string.Format("(IsManager = 1 OR IsFinManager = 1) AND Active = 1 AND OrgID = {0}", sessionOrgId);
                dv.Sort = "DisplayName";
                ddlMgr.DataSource = dv;
                ddlMgr.DataTextField = "DisplayName";
                ddlMgr.DataValueField = "ClientOrgID";
                ddlMgr.DataBind();

                // remove managers from ddl that have alrady been selected
                for (int i = 0; i < dgClientManager.DataKeys.Count; i++)
                    ddlMgr.Items.Remove(ddlMgr.Items.FindByValue(dgClientManager.DataKeys[i].ToString()));
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                // instead of adding columns, just get the needed value
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((Label)e.Item.FindControl("lblMgr")).Text = drv["DisplayName"].ToString();
            }
        }

        protected void rblAcctDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAccountList();
        }

        private void UpdateAccountList()
        {
            int sessionOrgId = GetSessionOrgID();

            // get list of client's managers
            int clientId = Convert.ToInt32(btnClientSave.CommandArgument);
            DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));

            int clientOrgId;
            if (codrs.Length == 1) // not a new client
                clientOrgId = Convert.ToInt32(codrs[0]["ClientOrgID"]);
            else
                clientOrgId = 0; // means that cmdrs will have no rows
            DataRow[] cmdrs = dsClient.Tables["ClientManager"].Select(string.Format("Active = 1 AND ClientOrgID = {0}", clientOrgId));

            List<ListItem> items = new List<ListItem>();

            // loop through list of managers and find all of their accounts
            for (int i = 0; i < cmdrs.Length; i++)
            {
                DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("Active = 1 AND Manager = 1 AND ClientOrgID = {0}", cmdrs[i]["ManagerOrgID"]));
                for (int j = 0; j < cadrs.Length; j++)
                {
                    if (items.FirstOrDefault(x => x.Value == cadrs[j]["AccountID"].ToString()) == null)
                    {
                        ListItem mgrAcct = new ListItem();
                        mgrAcct.Text = MakeAcctNumber(cadrs[j][rblAcctDisplay.SelectedValue].ToString());
                        mgrAcct.Value = cadrs[j]["AccountID"].ToString();
                        //ddlAccount.Items.Add(mgrAcct);
                        items.Add(mgrAcct);
                    }
                }
            }

            // if the client is a manager, add the accounts he manages
            if (clientOrgId != 0 && Convert.ToBoolean(codrs[0]["IsManager"]))
            {
                DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("Active = 1 AND Manager = 1 AND ClientOrgID = {0}", clientOrgId));
                for (int j = 0; j < cadrs.Length; j++)
                {
                    if (items.FirstOrDefault(x => x.Value == cadrs[j]["AccountID"].ToString()) == null)
                    {
                        ListItem mgrAcct = new ListItem();
                        mgrAcct.Text = MakeAcctNumber(cadrs[j][rblAcctDisplay.SelectedValue].ToString());
                        mgrAcct.Value = cadrs[j]["AccountID"].ToString();
                        //ddlAccount.Items.Add(mgrAcct);
                        items.Add(mgrAcct);
                    }
                }
            }

            if (items.Count == 0)
                ddlAccount.Enabled = false;
            else
            {
                var ordered = items.OrderBy(x => x.Text).ToList();

                ordered.Insert(0, new ListItem("*** Remove all account access for this client ***", "0"));
                // this field now shows one of the client's accounts

                ListItem clientAcct = null;
                DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("Active = 1 AND ClientOrgID = {0}", clientOrgId));

                ddlAccount.DataSource = ordered;
                ddlAccount.DataBind();

                for (int j = 0; j < cadrs.Length; j++)
                {
                    clientAcct = ddlAccount.Items.FindByValue(cadrs[j]["AccountID"].ToString());
                    if (clientAcct != null)
                    {
                        clientAcct.Selected = true;
                        break;
                    }
                }

                ddlAccount.Enabled = true;
            }
        }

        private string MakeAcctNumber(string strBase)
        {
            string strNumber;
            if (rblAcctDisplay.SelectedValue == "Number")
            {
                strNumber = strBase.Substring(0, 6) + "-";
                strNumber += strBase.Substring(6, 5) + "-";
                strNumber += strBase.Substring(11, 6) + "-";
                strNumber += strBase.Substring(17, 5) + "-";
                strNumber += strBase.Substring(22, 5) + "-";
                strNumber += strBase.Substring(27, 7);
            }
            else
                strNumber = strBase;
            return strNumber;
        }

        protected void ddlAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sessionOrgId = GetSessionOrgID();
            int clientId = Convert.ToInt32(btnClientSave.CommandArgument);
            DataRow cdr = dsClient.Tables["Client"].Rows.Find(clientId);
            DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, sessionOrgId));

            int ClientOrgID;
            if (codrs.Length == 1) // not a new client
                ClientOrgID = Convert.ToInt32(codrs[0]["ClientOrgID"]);
            else
                ClientOrgID = 0; // means that cadr will have no rows

            // upate row in clientAccount as needed
            if (ddlAccount.SelectedIndex > 0)
            {
                // see if user is associated with selected account
                DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("ClientOrgID = {0} AND AccountID = {1}", ClientOrgID, ddlAccount.SelectedValue));
                if (cadrs.Length == 0) // this account is not currently assigned
                {
                    DataRow cadr = dsClient.Tables["ClientAccount"].NewRow();
                    cadr["ClientOrgID"] = ClientOrgID;
                    cadr["AccountID"] = ddlAccount.SelectedValue;
                    cadr["Manager"] = false;
                    cadr["Active"] = true;
                    dsClient.Tables["ClientAccount"].Rows.Add(cadr);
                }
                else
                    cadrs[0]["Active"] = true; // row is already changed - do this to be certain

                // this does not remove previously associated accounts, it only adds the new one
                //   and removes any newly added ones
                cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("ClientOrgID = {0} AND AccountID <> {1}", ClientOrgID, ddlAccount.SelectedValue));
                for (int i = 0; i < cadrs.Length; i++)
                {
                    if (cadrs[i].RowState == DataRowState.Added)
                        cadrs[i].Delete();
                }
            }
            else
            {
                DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(string.Format("ClientOrgID = {0}", ClientOrgID));
                for (int i = 0; i < cadrs.Length; i++)
                {
                    if (cadrs[i].RowState == DataRowState.Added)
                        cadrs[i].Delete();
                    else
                        DataUtility.SetActiveFalse(cadrs[i]);
                }
            }

            Cache.Insert(DataUtility.CacheID, dsClient, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            litError.Text = string.Empty;

            cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            // update the Client table - use handler to update ClientOrg table
            SqlDataAdapter daClient = new SqlDataAdapter();
            daClient.RowUpdating += daClient_RowUpdating;
            daClient.RowUpdated += daClient_RowUpdated;
            //AddHandler daClient.RowUpdating, New SqlRowUpdatingEventHandler(AddressOf daClientRowUpdating)
            //AddHandler daClient.RowUpdated, New SqlRowUpdatedEventHandler(AddressOf daClientRowUpdated)

            daClient.InsertCommand = new SqlCommand("Client_Insert", cnSselData);
            daClient.InsertCommand.CommandType = CommandType.StoredProcedure;
            daClient.InsertCommand.Parameters.Add("@FName", SqlDbType.NVarChar, 20, "FName");
            daClient.InsertCommand.Parameters.Add("@MName", SqlDbType.NVarChar, 20, "MName");
            daClient.InsertCommand.Parameters.Add("@LName", SqlDbType.NVarChar, 30, "LName");
            daClient.InsertCommand.Parameters.Add("@UserName", SqlDbType.NVarChar, 50, "UserName");
            daClient.InsertCommand.Parameters.Add("@Password", SqlDbType.NVarChar, 50, "Password");
            daClient.InsertCommand.Parameters.Add("@DemCitizenID", SqlDbType.Int, 4, "DemCitizenID");
            daClient.InsertCommand.Parameters.Add("@DemGenderID", SqlDbType.Int, 4, "DemGenderID");
            daClient.InsertCommand.Parameters.Add("@DemRaceID", SqlDbType.Int, 4, "DemRaceID");
            daClient.InsertCommand.Parameters.Add("@DemEthnicID", SqlDbType.Int, 4, "DemEthnicID");
            daClient.InsertCommand.Parameters.Add("@DemDisabilityID", SqlDbType.Int, 4, "DemDisabilityID");
            daClient.InsertCommand.Parameters.Add("@Privs", SqlDbType.Int, 4, "Privs");
            daClient.InsertCommand.Parameters.Add("@Communities", SqlDbType.Int, 4, "Communities");
            daClient.InsertCommand.Parameters.Add("@TechnicalInterestID", SqlDbType.Int, 4, "TechnicalInterestID");
            daClient.InsertCommand.Parameters.Add("@EnableAccess", SqlDbType.Bit, 1, "EnableAccess");

            daClient.UpdateCommand = new SqlCommand("Client_Update", cnSselData);
            daClient.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClient.UpdateCommand.Parameters.AddWithValue("@Action", "Update");
            daClient.UpdateCommand.Parameters.Add("@ClientID", SqlDbType.Int, 4, "ClientID");
            daClient.UpdateCommand.Parameters.Add("@FName", SqlDbType.NVarChar, 20, "FName");
            daClient.UpdateCommand.Parameters.Add("@MName", SqlDbType.NVarChar, 20, "MName");
            daClient.UpdateCommand.Parameters.Add("@LName", SqlDbType.NVarChar, 30, "LName");
            daClient.UpdateCommand.Parameters.Add("@DemCitizenID", SqlDbType.Int, 4, "DemCitizenID");
            daClient.UpdateCommand.Parameters.Add("@DemGenderID", SqlDbType.Int, 4, "DemGenderID");
            daClient.UpdateCommand.Parameters.Add("@DemRaceID", SqlDbType.Int, 4, "DemRaceID");
            daClient.UpdateCommand.Parameters.Add("@DemEthnicID", SqlDbType.Int, 4, "DemEthnicID");
            daClient.UpdateCommand.Parameters.Add("@DemDisabilityID", SqlDbType.Int, 4, "DemDisabilityID");
            daClient.UpdateCommand.Parameters.Add("@Privs", SqlDbType.Int, 4, "Privs");
            daClient.UpdateCommand.Parameters.Add("@Communities", SqlDbType.Int, 4, "Communities");
            daClient.UpdateCommand.Parameters.Add("@TechnicalInterestID", SqlDbType.Int, 4, "TechnicalInterestID");
            daClient.UpdateCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");
            daClient.UpdateCommand.Parameters.Add("@EnableAccess", SqlDbType.Bit, 1, "EnableAccess");

            try
            {
                daClient.Update(dsClient, "Client");
            }
            catch (Exception ex)
            {
                litError.Text += string.Format(@"<div class=""error"">Client update error:<br />{0}</div>", ex.Message);
            }

            // update the address table - use handler to update ClientOrg table

            SqlDataAdapter daAddress = new SqlDataAdapter();
            daAddress.RowUpdating += daAddress_RowUpdating;
            daAddress.RowUpdated += daAddress_RowUpdated;
            //AddHandler daAddress.RowUpdating, New SqlRowUpdatingEventHandler(AddressOf daAddressRowUpdating)
            //AddHandler daAddress.RowUpdated, New SqlRowUpdatedEventHandler(AddressOf daAddressRowUpdated)

            daAddress.InsertCommand = new SqlCommand("Address_Insert", cnSselData);
            daAddress.InsertCommand.CommandType = CommandType.StoredProcedure;
            daAddress.InsertCommand.Parameters.Add("@InternalAddress", SqlDbType.NVarChar, 50, "InternalAddress");
            daAddress.InsertCommand.Parameters.Add("@StrAddress1", SqlDbType.NVarChar, 50, "StrAddress1");
            daAddress.InsertCommand.Parameters.Add("@StrAddress2", SqlDbType.NVarChar, 50, "StrAddress2");
            daAddress.InsertCommand.Parameters.Add("@City", SqlDbType.NVarChar, 50, "City");
            daAddress.InsertCommand.Parameters.Add("@State", SqlDbType.NVarChar, 50, "State");
            daAddress.InsertCommand.Parameters.Add("@Zip", SqlDbType.NVarChar, 12, "Zip");
            daAddress.InsertCommand.Parameters.Add("@Country", SqlDbType.NVarChar, 50, "Country");

            daAddress.UpdateCommand = new SqlCommand("Address_Update", cnSselData);
            daAddress.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daAddress.UpdateCommand.Parameters.Add("@AddressID", SqlDbType.Int, 4, "AddressID");
            daAddress.UpdateCommand.Parameters.Add("@InternalAddress", SqlDbType.NVarChar, 50, "InternalAddress");
            daAddress.UpdateCommand.Parameters.Add("@StrAddress1", SqlDbType.NVarChar, 50, "StrAddress1");
            daAddress.UpdateCommand.Parameters.Add("@StrAddress2", SqlDbType.NVarChar, 50, "StrAddress2");
            daAddress.UpdateCommand.Parameters.Add("@City", SqlDbType.NVarChar, 50, "City");
            daAddress.UpdateCommand.Parameters.Add("@State", SqlDbType.NVarChar, 50, "State");
            daAddress.UpdateCommand.Parameters.Add("@Zip", SqlDbType.NVarChar, 12, "Zip");
            daAddress.UpdateCommand.Parameters.Add("@Country", SqlDbType.NVarChar, 50, "Country");

            daAddress.DeleteCommand = new SqlCommand("Address_Delete", cnSselData);
            daAddress.DeleteCommand.CommandType = CommandType.StoredProcedure;
            daAddress.DeleteCommand.Parameters.Add("@AddressID", SqlDbType.Int, 4, "AddressID");

            try
            {
                daAddress.Update(dsClient, "Address");
            }
            catch (Exception ex)
            {
                litError.Text += string.Format(@"<div class=""error"">Address update error:<br />{0}</div>", ex.Message);
            }

            // update the ClientOrg table
            SqlDataAdapter daClientOrg = new SqlDataAdapter();
            daClientOrg.RowUpdating += daClientOrg_RowUpdating;
            daClientOrg.RowUpdated += daClientOrg_RowUpdated;
            //AddHandler daClientOrg.RowUpdating, New SqlRowUpdatingEventHandler(AddressOf daClientOrgRowUpdating)
            //AddHandler daClientOrg.RowUpdated, New SqlRowUpdatedEventHandler(AddressOf daClientOrgRowUpdated)

            daClientOrg.InsertCommand = new SqlCommand("ClientOrg_Insert", cnSselData);
            daClientOrg.InsertCommand.CommandType = CommandType.StoredProcedure;
            daClientOrg.InsertCommand.Parameters.Add("@ClientID", SqlDbType.Int, 4, "ClientID");
            daClientOrg.InsertCommand.Parameters.Add("@OrgID", SqlDbType.Int, 4, "OrgID");
            daClientOrg.InsertCommand.Parameters.Add("@DepartmentID", SqlDbType.Int, 4, "DepartmentID");
            daClientOrg.InsertCommand.Parameters.Add("@RoleID", SqlDbType.Int, 4, "RoleID");
            daClientOrg.InsertCommand.Parameters.Add("@SubsidyStartDate", SqlDbType.DateTime2, 7, "SubsidyStartDate");
            daClientOrg.InsertCommand.Parameters.Add("@NewFacultyStartDate", SqlDbType.DateTime2, 7, "NewFacultyStartDate");
            daClientOrg.InsertCommand.Parameters.Add("@ClientAddressID", SqlDbType.Int, 4, "ClientAddressID");
            daClientOrg.InsertCommand.Parameters.Add("@Phone", SqlDbType.NVarChar, 40, "Phone");
            daClientOrg.InsertCommand.Parameters.Add("@Email", SqlDbType.NVarChar, 50, "Email");
            daClientOrg.InsertCommand.Parameters.Add("@IsManager", SqlDbType.Bit, 1, "IsManager");
            daClientOrg.InsertCommand.Parameters.Add("@IsFinManager", SqlDbType.Bit, 1, "IsFinManager");
            daClientOrg.InsertCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            daClientOrg.UpdateCommand = new SqlCommand("ClientOrg_Update", cnSselData);
            daClientOrg.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClientOrg.UpdateCommand.Parameters.Add("@ClientOrgID", SqlDbType.Int, 4, "ClientOrgID");
            daClientOrg.UpdateCommand.Parameters.Add("@DepartmentID", SqlDbType.Int, 4, "DepartmentID");
            daClientOrg.UpdateCommand.Parameters.Add("@RoleID", SqlDbType.Int, 4, "RoleID");
            daClientOrg.UpdateCommand.Parameters.Add("@SubsidyStartDate", SqlDbType.DateTime2, 7, "SubsidyStartDate");
            daClientOrg.UpdateCommand.Parameters.Add("@NewFacultyStartDate", SqlDbType.DateTime2, 7, "NewFacultyStartDate");
            daClientOrg.UpdateCommand.Parameters.Add("@ClientAddressID", SqlDbType.Int, 4, "ClientAddressID");
            daClientOrg.UpdateCommand.Parameters.Add("@Phone", SqlDbType.NVarChar, 40, "Phone");
            daClientOrg.UpdateCommand.Parameters.Add("@Email", SqlDbType.NVarChar, 50, "Email");
            daClientOrg.UpdateCommand.Parameters.Add("@IsManager", SqlDbType.Bit, 1, "IsManager");
            daClientOrg.UpdateCommand.Parameters.Add("@IsFinManager", SqlDbType.Bit, 1, "IsFinManager");
            daClientOrg.UpdateCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            try
            {
                daClientOrg.Update(dsClient, "ClientOrg");
            }
            catch (Exception ex)
            {
                litError.Text += string.Format(@"<div class=""error"">ClientOrg update error:<br />{0}</div>", ex.Message);
            }

            // update the ClientManager table
            SqlDataAdapter daClientManager = new SqlDataAdapter();

            daClientManager.InsertCommand = new SqlCommand("ClientManager_Insert", cnSselData);
            daClientManager.InsertCommand.CommandType = CommandType.StoredProcedure;
            daClientManager.InsertCommand.Parameters.Add("@ClientOrgID", SqlDbType.Int, 4, "ClientOrgID");
            daClientManager.InsertCommand.Parameters.Add("@ManagerOrgID", SqlDbType.Int, 4, "ManagerOrgID");

            daClientManager.UpdateCommand = new SqlCommand("ClientManager_Update", cnSselData);
            daClientManager.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClientManager.UpdateCommand.Parameters.Add("@ClientManagerID", SqlDbType.Int, 4, "ClientManagerID");
            daClientManager.UpdateCommand.Parameters.Add("@Active", SqlDbType.Int, 4, "Active");

            try
            {
                daClientManager.Update(dsClient, "ClientManager");
            }
            catch (Exception ex)
            {
                litError.Text += string.Format(@"<div class=""error"">ClientManager update error:<br />{0}</div>", ex.Message);
            }

            // update the ClientAccount table
            SqlDataAdapter daClientAccount = new SqlDataAdapter();

            daClientAccount.InsertCommand = new SqlCommand("ClientAccount_Insert", cnSselData);
            daClientAccount.InsertCommand.CommandType = CommandType.StoredProcedure;
            daClientAccount.InsertCommand.Parameters.Add("@ClientOrgID", SqlDbType.Int, 4, "ClientOrgID");
            daClientAccount.InsertCommand.Parameters.Add("@AccountID", SqlDbType.Int, 4, "AccountID");
            daClientAccount.InsertCommand.Parameters.Add("@Manager", SqlDbType.Bit, 1, "Manager");
            daClientAccount.InsertCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            daClientAccount.UpdateCommand = new SqlCommand("ClientAccount_Update", cnSselData);
            daClientAccount.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClientAccount.UpdateCommand.Parameters.Add("@ClientAccountID", SqlDbType.Int, 4, "ClientAccountID");
            daClientAccount.UpdateCommand.Parameters.Add("@Manager", SqlDbType.Bit, 1, "Manager");
            daClientAccount.UpdateCommand.Parameters.Add("@Active", SqlDbType.Int, 4, "Active");

            try
            {
                daClientAccount.Update(dsClient, "ClientAccount");
            }
            catch (Exception ex)
            {
                litError.Text += string.Format(@"<div class=""error"">ClientAccount update error:<br />{0}</div>", ex.Message);
            }

            if (string.IsNullOrEmpty(litError.Text))
                btnDiscard_Click(sender, e);
            else
            {
                litError.Text += @"<div style=""padding-top: 10px;"">Some data may have been saved.</div>";
                panDialogError.Visible = true;
                panDisplay.Visible = false;
            }
        }

        private void daClient_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
                oldClientID = Convert.ToInt32(e.Row["ClientID"]);
        }

        private void daClient_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                // update ClientOrg, exactly one row will match
                DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientID = {0}", oldClientID));
                codrs[0]["ClientID"] = e.Row["ClientID"];
            }
        }

        private void daAddress_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
                oldAddressID = Convert.ToInt32(e.Row["AddressID"]);
        }

        private void daAddress_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                // exactly one row will match
                DataRow[] codrs = dsClient.Tables["ClientOrg"].Select(string.Format("ClientAddressID = {0}", oldAddressID));
                codrs[0]["ClientAddressID"] = e.Row["AddressID"];
            }
        }

        private void daClientOrg_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
                oldClientOrgID = Convert.ToInt32(e.Row["ClientOrgID"]);
        }

        private void daClientOrg_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                string strSelect = string.Format("ClientOrgID = {0}", oldClientOrgID);

                // update ClientManager, exactly one row may match
                DataRow[] cmdrs = dsClient.Tables["ClientManager"].Select(strSelect);
                for (int i = 0; i < cmdrs.Length; i++)
                    cmdrs[i]["ClientOrgID"] = e.Row["ClientOrgID"];

                // update ClientAccount, exactly one row may match
                DataRow[] cadrs = dsClient.Tables["ClientAccount"].Select(strSelect);
                if (cadrs.Length == 1)
                    cadrs[0]["ClientOrgID"] = e.Row["ClientOrgID"];

                //Save the billing type permanently in database, since we alreayd know the newly created ClientOrgID
                BillingTypeDA.SetBillingTypeID(Convert.ToInt32(e.Row["ClientOrgID"]), Convert.ToInt32(rdolistBillingType.SelectedItem.Value));
            }
        }

        protected void btnDiscard_Click(object sender, EventArgs e)
        {
            Cache.Remove(DataUtility.CacheID); // remove anything left in cache
            Response.Redirect("~");
        }

        //This event will select the current billing type of this selected user
        protected void rdolistBillingType_DataBound(object sender, EventArgs e)
        {
            //2007-04-09
            if (!string.IsNullOrEmpty(txtUsername.Text)) //Make sure it's in Edit mode
            {
                //The radio button list's selected item must be set after data is bound
                int BillingID = BillingTypeDA.GetBillingTypeID(selectedClientOrgId);
                foreach (ListItem obj in rdolistBillingType.Items)
                {
                    if (Convert.ToInt32(obj.Value) == BillingID)
                        obj.Selected = true;
                }
            }
            else
            {
                //by default, we select the regular billing type
                foreach (ListItem btn in rdolistBillingType.Items)
                {
                    if (btn.Text == "Regular")
                    {
                        btn.Selected = true;
                        break;
                    }
                }
            }
        }

        protected void btnUniqueIDSearch_Click(object sender, EventArgs e)
        {
            lblLDAPMsg.Text = string.Empty;

            if (!string.IsNullOrEmpty(txtUniqueID.Text))
            {
                LDAPUserInfo user = LDAPLookup.GetUser(txtUniqueID.Text);

                string[] name = user.DisplayName;
                txtFName.Text = name[0];
                txtMName.Text = name[1];
                txtLName.Text = name[2];
                txtUsername.Text = user.UID;
                txtEmail.Text = user.Email;
                txtPhone.Text = user.Phone;

                if (user.LDAPException != null)
                {
                    lblLDAPMsg.Text = "LDAP lookup failed: " + user.LDAPException.Message;
                    litDebug.Text = user.Debug;
                }
            }
            else
                lblLDAPMsg.Text = "You can fill in the form using a U of M Unique ID.<br />Please enter a valid Unique ID and click the Search button.";
        }

        private List<T> ConvertCheckBoxListToDataItemList<T>(CheckBoxList cbl) where T : IDataItem
        {
            List<T> result = new List<T>();
            foreach (ListItem i in cbl.Items)
            {
                if (i.Selected)
                    result.Add(DA.Current.Single<T>(i.Value));
            }
            return result;
        }

        protected void rptPhysicalAccess_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var badge = (Badge)e.Item.DataItem;
                var cards = badge.GetCards();

                if (cards.Count() > 0)
                {
                    Repeater rptCards = (Repeater)e.Item.FindControl("rptCards");
                    rptCards.DataSource = badge.GetCards();
                    rptCards.DataBind();
                }
                else
                {
                    PlaceHolder phCardsNoData = (PlaceHolder)e.Item.FindControl("phCardsNoData");
                    phCardsNoData.Visible = true;
                }
            }
        }
    }
}
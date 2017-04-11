using LNF.Models.Data;
using sselData.AppCode;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class Organization : LNF.Web.Content.LNFPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        struct AlertInfo
        {
            public string Field;
            public string Data;
        }

        private DataSet dsOrg;
        private SortedList addressTypes = new SortedList();
        private string strAlert = string.Empty;
        private int oldAddressID;
        private int oldOrgID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.IsInRole("Administrator"))
            {
                Session.Abandon();
                Response.Redirect(Session["Logout"].ToString() + "?Action=Exit");
            }

            // needs to be done each time
            addressTypes.Add("DefClientAddressID", "Client");
            addressTypes.Add("DefBillAddressID", "Billing");
            addressTypes.Add("DefShipAddressID", "Shipping");

            if (Page.IsPostBack)
            {
                dsOrg = (DataSet)Cache.Get(DataUtility.CacheID);
                if (dsOrg == null)
                    Response.Redirect("~");
                else if (dsOrg.DataSetName != "Org")
                    Response.Redirect("~");
            }
            else
            {
                Cache.Remove(DataUtility.CacheID); // remove anything left in cache

                SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                dsOrg = new DataSet("Org");

                // get Org info
                SqlDataAdapter daOrg = new SqlDataAdapter("Org_Select", cnSselData);
                daOrg.SelectCommand.CommandType = CommandType.StoredProcedure;
                daOrg.SelectCommand.Parameters.AddWithValue("@Action", "All");
                daOrg.FillSchema(dsOrg, SchemaType.Mapped, "Org");
                daOrg.Fill(dsOrg, "Org");

                // Get the OrgTypes
                SqlDataAdapter daOrgType = new SqlDataAdapter("Global_Select", cnSselData);
                daOrgType.SelectCommand.CommandType = CommandType.StoredProcedure;
                daOrgType.SelectCommand.Parameters.AddWithValue("@TableName", "OrgType");
                daOrgType.Fill(dsOrg, "OrgType");

                DropDownList ddlOrgType = (DropDownList)pAddEdit.FindControl("ddlOrgType");
                ddlOrgType.DataSource = dsOrg.Tables["OrgType"];
                ddlOrgType.DataTextField = "OrgType";
                ddlOrgType.DataValueField = "OrgTypeID";
                ddlOrgType.DataBind();

                // Get the Address
                SqlDataAdapter daAddress = new SqlDataAdapter("Address_Select", cnSselData);
                daAddress.SelectCommand.CommandType = CommandType.StoredProcedure;
                daAddress.SelectCommand.Parameters.AddWithValue("@Action", "All");
                daAddress.FillSchema(dsOrg, SchemaType.Mapped, "Address");
                daAddress.Fill(dsOrg, "Address");
                dsOrg.Tables["Address"].Columns.Add("AddressType", typeof(string));
                dsOrg.Tables["Address"].Columns.Add("AddDelete", typeof(bool));

                // grab all departments
                SqlDataAdapter daDepartment = new SqlDataAdapter("Department_Select", cnSselData);
                daDepartment.SelectCommand.CommandType = CommandType.StoredProcedure;
                daDepartment.SelectCommand.Parameters.AddWithValue("@Action", "All");
                daDepartment.FillSchema(dsOrg, SchemaType.Mapped, "Department");
                daDepartment.Fill(dsOrg, "Department");

                // these are needed for checking Org disable
                SqlDataAdapter daAccount = new SqlDataAdapter("Account_Select", cnSselData);
                daAccount.SelectCommand.CommandType = CommandType.StoredProcedure;
                daAccount.SelectCommand.Parameters.AddWithValue("@Action", "All");
                daAccount.Fill(dsOrg, "Account");

                SqlDataAdapter daClientOrg = new SqlDataAdapter("ClientOrg_Select", cnSselData);
                daClientOrg.SelectCommand.CommandType = CommandType.StoredProcedure;
                daClientOrg.SelectCommand.Parameters.AddWithValue("@Action", "All");
                daClientOrg.Fill(dsOrg, "ClientOrg");

                //Declare default sort parameter and sort direction
                ViewState["dgOrgSortDir"] = " ASC";
                ViewState["dgDeptSortDir"] = " ASC";

                Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                SetPageControlsAndBind(false, false, false, false);
            }
        }

        private void SetPageControlsAndBind(bool showAddEditPanel, bool showAddExistingPanel, bool inlineAddressEdit, bool inlineDepartmentEdit)
        {
            bool panelVis = showAddEditPanel | showAddExistingPanel;

            // determine which panel to display
            pOrgList.Visible = !panelVis;
            pAddEdit.Visible = showAddEditPanel;
            pExisting.Visible = showAddExistingPanel;

            DataView dv;

            // set page header label
            if (pOrgList.Visible)
            {
                lblHeader.Text = "Configure Organizations";

                dv = dsOrg.Tables["Org"].DefaultView;
                dv.Sort = "OrgName" + ViewState["dgOrgSortDir"].ToString();
                dv.RowFilter = "Active = 1";
                dgOrg.DataSource = dv;
                dgOrg.DataBind();

                // now, add to ddlPager for paging
                ddlPager.Items.Clear();
                int pSize = dgOrg.PageSize;
                for (int i = 0; i < dv.Count; i += pSize)
                {
                    ListItem pagerItem = new ListItem();
                    pagerItem.Value = (i / pSize).ToString();
                    pagerItem.Text = dv[i].Row["OrgName"].ToString() + " ... " + dv[((i + (pSize - 1) >= dv.Count) ? dv.Count - 1 : i + (pSize - 1))].Row["OrgName"].ToString();
                    ddlPager.Items.Add(pagerItem);
                }
                ddlPager.SelectedValue = dgOrg.CurrentPageIndex.ToString();
            }
            else if (pAddEdit.Visible)
            {
                SetFocus(txtOrgName);
                if (btnOrgStore.Text.ToLower().Contains("new"))
                    lblHeader.Text = "Add New Organization";
                else
                {
                    int OrgID = Convert.ToInt32(btnOrgStore.CommandArgument);
                    DataRow dr = dsOrg.Tables["Org"].Rows.Find(OrgID);
                    lblHeader.Text = string.Format("Configure Organization {0}", dr["OrgName"]);
                }
            }
            else
                lblHeader.Text = "Reactivate existing Organization";

            if (pAddEdit.Visible)
            {
                // turn off save button and paging during any edits
                bool bNotEditing = !(inlineAddressEdit | inlineDepartmentEdit);

                // turn off add new and add existing buttons during data edit
                btnOrgStore.Enabled = bNotEditing;
                btnOrgStoreQuit.Enabled = bNotEditing;

                int OrgID = Convert.ToInt32(btnOrgStore.CommandArgument);
                DataRow dr = dsOrg.Tables["Org"].Rows.Find(OrgID);

                // turn off dgAddress control when editing an address or when editing a department
                if (inlineAddressEdit || inlineDepartmentEdit || dgAddress.Items.Count == 3)
                    dgAddress.ShowFooter = false;
                else
                    dgAddress.ShowFooter = true;

                // filter the addresses
                DataView dva = dsOrg.Tables["Address"].DefaultView;
                if (OrgID == 0)
                {
                    // a new client must have a newly added address, if any
                    dva.RowFilter = "AddDelete = 1";
                }
                else
                {
                    // set addresstype and create row filter
                    string filter = string.Empty;
                    MakeAddrFilter(ref filter, dr, "DefClientAddressID");
                    MakeAddrFilter(ref filter, dr, "DefBillAddressID");
                    MakeAddrFilter(ref filter, dr, "DefShipAddressID");

                    // if affiliated with this org, this row must exist
                    dva.RowFilter = string.Format("AddDelete = 1{0}", filter);
                }

                dgAddress.DataSource = dva;
                dgAddress.DataBind();

                dv = dsOrg.Tables["Department"].DefaultView;
                dv.RowFilter = string.Format("OrgID = {0}", btnOrgStore.CommandArgument);
                dv.Sort = "Department" + ViewState["dgDeptSortDir"].ToString();
                dgDepartment.DataSource = dv;
                dgDepartment.DataBind();

                // turn off sorting
                dgDepartment.AllowSorting = bNotEditing;
                // turn off dgDepartment control when editing a department or when editing an address
                dgDepartment.ShowFooter = bNotEditing;
            }
        }

        private bool MakeAddrFilter(ref string filter, DataRow dr, string addrType)
        {
            if (Convert.ToInt32(dr[addrType]) != 0)
            {
                DataRow adr = dsOrg.Tables["Address"].Rows.Find(dr[addrType]);
                if (adr != null)
                {
                    DataRowState myRowState = adr.RowState; // to prevent unnecessary writes to the DB

                    adr["AddressType"] = addrType;
                    if (myRowState == DataRowState.Unchanged)
                        adr.AcceptChanges();

                    filter += string.Format(" OR AddressID = {0}", dr[addrType]);

                    return true;
                }
            }

            return false;
        }

        protected void dgOrg_SortCommand(object sender, DataGridSortCommandEventArgs e)
        {
            //Flip-flop sort direction 
            if (ViewState["dgOrgSortDir"].ToString() == " ASC")
                ViewState["dgOrgSortDir"] = " DESC";
            else
                ViewState["dgOrgSortDir"] = " ASC";

            SetPageControlsAndBind(false, false, false, false);
        }

        protected void dgOrg_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            int OrgID;
            DataRow dr;

            switch (e.CommandName)
            {
                case "AddNew":
                    txtOrgName.Text = string.Empty;
                    ddlOrgType.ClearSelection();
                    chkNninOrg.Checked = false;

                    // since the Org is being added, the address table must be empty

                    btnOrgStore.CommandArgument = "0";
                    btnOrgStore.Text = "Store New Organization";

                    Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    SetPageControlsAndBind(true, false, false, false);
                    break;
                case "Edit":
                    OrgID = Convert.ToInt32(dgOrg.DataKeys[e.Item.ItemIndex]);
                    dr = dsOrg.Tables["Org"].Rows.Find(OrgID);

                    txtOrgName.Text = dr["OrgName"].ToString();
                    ddlOrgType.SelectedValue = dr["OrgTypeID"].ToString();
                    chkNninOrg.Checked = Convert.ToBoolean(dr["NNINOrg"]);

                    btnOrgStore.Text = "Store Modified Data";
                    btnOrgStore.CommandArgument = OrgID.ToString();

                    SetPageControlsAndBind(true, false, false, false);
                    break;
                case "AddExisting":
                    SetOrgDDL();
                    SetPageControlsAndBind(false, true, false, false);
                    break;
                case "Delete":
                    OrgID = Convert.ToInt32(dgOrg.DataKeys[e.Item.ItemIndex]);
                    dr = dsOrg.Tables["Org"].Rows.Find(OrgID);
                    string strCannotDelete = string.Empty;

                    // first check that there are no active clients or accounts
                    DataRow[] codr = dsOrg.Tables["ClientOrg"].Select(string.Format("Active = 1 AND OrgID = {0}", OrgID));
                    if (codr.Length > 0)
                    {
                        if (string.IsNullOrEmpty(strCannotDelete))
                            strCannotDelete = "Cannot disable selected organization.\n";
                        strCannotDelete += string.Format("Organization contains {0} active clients", codr.Length);
                    }

                    DataRow[] tdr = dsOrg.Tables["Account"].Select(string.Format("Active = 1 AND OrgID = {0}", OrgID));
                    if (tdr.Length > 0)
                    {
                        if (string.IsNullOrEmpty(strCannotDelete))
                            strCannotDelete = "Cannot disable selected organization.\n";
                        else
                            strCannotDelete += "\n";
                        strCannotDelete += string.Format("Organization contains {0} active accounts", tdr.Length);
                    }

                    // if problems exist, show message, otherwise disable
                    if (strCannotDelete.Length > 0)
                    {
                        string strScript = @"<script type=""text/javascript"" id=""showWarning""> ";
                        strScript += string.Format("alert('{0}');", strCannotDelete);
                        strScript += "</script>";

                        if (!Page.ClientScript.IsStartupScriptRegistered("showWarning"))
                            Page.ClientScript.RegisterStartupScript(typeof(Page), "showWarning", strScript);
                    }
                    else
                    {
                        // now, remove stuff associated with new orgs
                        DataRow[] adr = dsOrg.Tables["Address"].Select(string.Format("AddressID = {0} OR AddressID = {1} OR AddressID = {2}", dr["DefClientAddressID"], dr["DefBillAddressID"], dr["DefShipAddressID"]));
                        if (dr.RowState == DataRowState.Added)
                        {
                            for (int i = 0; i < adr.Length; i++)
                                dsOrg.Tables["Address"].Rows.Remove(adr[i]); // if it is a new org, we don't need its addresses
                        }

                        DataRow[] ddr = dsOrg.Tables["Department"].Select(string.Format("OrgID = {0}", dr["OrgID"]));
                        if (dr.RowState == DataRowState.Added)
                        {
                            for (int i = 0; i < ddr.Length; i++)
                                dsOrg.Tables["Department"].Rows.Remove(ddr[i]); // if it is a new org, we don't need its departments
                        }

                        if (dr.RowState == DataRowState.Added)
                            dsOrg.Tables["Org"].Rows.Remove(dr);
                        else
                            DataUtility.SetActiveFalse(dr);

                        Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        SetPageControlsAndBind(false, false, false, false);
                    }
                    break;
            }
        }

        protected void txtName_TextChanged(object sender, EventArgs e)
        {
            SetOrgDDL();
        }

        private void SetOrgDDL()
        {
            DataView dv = dsOrg.Tables["Org"].DefaultView;
            dv.Sort = "OrgName ASC";
            dv.RowFilter = "Active = 0";

            ddlOrg.DataSource = dv;
            ddlOrg.DataTextField = "OrgName";
            ddlOrg.DataValueField = "OrgID";
            ddlOrg.DataBind();

            // now, go through and remove items that do not meet the filters
            for (int i = ddlOrg.Items.Count - 1; i >= 0; i--)
            {
                if (txtName.Text.Trim().Length > 0)
                {
                    if (!ddlOrg.Items[i].Text.ToLower().Contains(txtName.Text.Trim().ToLower()))
                        ddlOrg.Items.Remove(ddlOrg.Items[i]);
                }
            }
        }

        protected void btnOrgStore_Click(object sender, EventArgs e)
        {
            AlertInfo[] strValidate = new AlertInfo[1];
            strValidate[0].Data = txtOrgName.Text;
            strValidate[0].Field = "an organization name.";

            if (ValidateField(strValidate))
            {
                bool IsNewEntry = btnOrgStore.Text.ToLower().Contains("new");

                DataRow dr;
                int OrgID;

                if (IsNewEntry)
                {
                    dr = dsOrg.Tables["Org"].NewRow();
                    OrgID = Convert.ToInt32(dr["OrgID"]);
                }
                else
                {
                    OrgID = Convert.ToInt32(btnOrgStore.CommandArgument);
                    dr = dsOrg.Tables["Org"].Rows.Find(OrgID);
                }

                dr["OrgName"] = txtOrgName.Text;
                dr["OrgTypeID"] = ddlOrgType.SelectedValue;
                dr["NNINOrg"] = chkNninOrg.Checked;
                dr["Active"] = true;
                dr["PrimaryOrg"] = false;

                // find any address that need to be dealt with
                DataRow[] fdr = dsOrg.Tables["Address"].Select("AddDelete IS NOT NULL");
                for (int i = 0; i < fdr.Length; i++)
                {
                    if (Convert.ToBoolean(fdr[i]["AddDelete"])) // addr was added
                    {
                        dr[fdr[i]["AddressType"].ToString()] = fdr[i]["AddressID"];
                        fdr[i]["AddDelete"] = DBNull.Value;
                    }
                    else
                    {
                        dr[fdr[i]["AddressType"].ToString()] = 0;
                        fdr[i].Delete();
                    }
                }

                // set unused AddressID's to 0
                foreach (DictionaryEntry addrType in addressTypes)
                {
                    if (dr[addrType.Key.ToString()] == DBNull.Value)
                        dr[addrType.Key.ToString()] = 0;
                }

                // update OrgID as needed
                fdr = dsOrg.Tables["Department"].Select("OrgID = 0");
                for (int i = 0; i < fdr.Length; i++) // new or added address
                    fdr[i]["OrgID"] = dr["OrgID"];

                if (IsNewEntry)
                {
                    // add an entry to the Org table
                    dsOrg.Tables["Org"].Rows.Add(dr);
                }

                Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                SetPageControlsAndBind(false, false, false, false);
            }
        }

        protected void btnOrgStoreQuit_Click(object sender, EventArgs e)
        {
            // remove any addresses that were added
            DataRow[] fdr = dsOrg.Tables["Address"].Select("AddDelete = 1");
            if (fdr.Length == 1)
                fdr[0].Delete();

            // unmark any addresses that were removed - only need to check for non-new entries
            int OrgID = Convert.ToInt32(btnOrgStore.CommandArgument);

            if (OrgID != 0)
            {
                DataRow dr = dsOrg.Tables["Org"].Rows.Find(OrgID);
                fdr = dsOrg.Tables["Address"].Select("AddDelete = 0");

                for (int i = 0; i < fdr.Length; i++)
                {
                    DataRowState drs = fdr[i].RowState;
                    dr[fdr[i]["AddressType"].ToString()] = fdr[i]["AddressID"];
                    fdr[i]["AddDelete"] = DBNull.Value;
                    fdr[i]["AddressType"] = DBNull.Value;

                    if (drs == DataRowState.Unchanged)
                        fdr[i].AcceptChanges();
                }
            }

            SetPageControlsAndBind(false, false, false, false);
        }

        protected void btnOrgReenable_Click(object sender, EventArgs e)
        {
            int OrgID = Convert.ToInt32(ddlOrg.SelectedValue);
            DataRow dr = dsOrg.Tables["Org"].Rows.Find(OrgID);
            if (dr.RowState == DataRowState.Modified)
                dr.RejectChanges();
            else
                dr["Active"] = true;

            Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
            SetPageControlsAndBind(false, false, false, false);
        }

        protected void btnOrgReenableQuit_Click(object sender, EventArgs e)
        {
            SetPageControlsAndBind(false, false, false, false);
        }

        // serves to page dgOrg
        protected void ddlPager_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgOrg.CurrentPageIndex = Convert.ToInt32(ddlPager.SelectedValue); // selectedItem should also work
            SetPageControlsAndBind(false, false, false, false);
        }

        protected void dgOrg_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((Label)e.Item.FindControl("lblOrgName")).Text = drv["OrgName"].ToString();
            }
        }

        protected void dgAddress_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            AlertInfo[] strValidate = new AlertInfo[3];

            // changes that are saved cannot be undone by quiting at the org layer

            int AddressID;
            DataRow dr;

            switch (e.CommandName)
            {
                case "AddNewRow":
                    strValidate[0].Data = ((TextBox)e.Item.FindControl("txtStrAddress1F")).Text;
                    strValidate[0].Field = "a street address.";
                    strValidate[1].Data = ((TextBox)e.Item.FindControl("txtCityF")).Text;
                    strValidate[1].Field = "a city.";
                    strValidate[2].Data = ((TextBox)e.Item.FindControl("txtStateF")).Text;
                    strValidate[2].Field = "a state.";
                    if (ValidateField(strValidate))
                    {
                        dr = dsOrg.Tables["Address"].NewRow();
                        dr["AddressType"] = ((DropDownList)e.Item.FindControl("ddlTypeF")).SelectedValue;
                        dr["InternalAddress"] = ((TextBox)e.Item.FindControl("txtInternalAddressF")).Text;
                        dr["StrAddress1"] = ((TextBox)e.Item.FindControl("txtStrAddress1F")).Text;
                        dr["StrAddress2"] = ((TextBox)e.Item.FindControl("txtStrAddress2F")).Text;
                        dr["City"] = ((TextBox)e.Item.FindControl("txtCityF")).Text;
                        dr["State"] = ((TextBox)e.Item.FindControl("txtStateF")).Text;
                        dr["Zip"] = ((TextBox)e.Item.FindControl("txtZipF")).Text;
                        dr["Country"] = ((TextBox)e.Item.FindControl("txtCountryF")).Text;
                        dr["AddDelete"] = true;
                        dsOrg.Tables["Address"].Rows.Add(dr);

                        Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        SetPageControlsAndBind(true, false, false, false);
                    }
                    break;
                case "Edit":
                    //Datagrid in edit mode, hide footer section 
                    dgAddress.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
                    SetPageControlsAndBind(true, false, true, false);
                    break;
                case "Cancel":
                    //Datagrid leaving edit mode 
                    dgAddress.EditItemIndex = -1;
                    SetPageControlsAndBind(true, false, false, false);
                    break;
                case "Update":
                    strValidate[0].Data = ((TextBox)e.Item.FindControl("txtStrAddress1")).Text;
                    strValidate[0].Field = "a street address.";
                    strValidate[1].Data = ((TextBox)e.Item.FindControl("txtCity")).Text;
                    strValidate[1].Field = "a city.";
                    strValidate[2].Data = ((TextBox)e.Item.FindControl("txtState")).Text;
                    strValidate[2].Field = "a state.";
                    if (ValidateField(strValidate))
                    {
                        AddressID = Convert.ToInt32(dgAddress.DataKeys[e.Item.ItemIndex]);
                        dr = dsOrg.Tables["Address"].Rows.Find(AddressID);
                        dr["AddressType"] = ((DropDownList)e.Item.FindControl("ddlType")).SelectedValue;
                        dr["InternalAddress"] = ((TextBox)e.Item.FindControl("txtInternalAddress")).Text;
                        dr["StrAddress1"] = ((TextBox)e.Item.FindControl("txtStrAddress1")).Text;
                        dr["StrAddress2"] = ((TextBox)e.Item.FindControl("txtStrAddress2")).Text;
                        dr["City"] = ((TextBox)e.Item.FindControl("txtCity")).Text;
                        dr["State"] = ((TextBox)e.Item.FindControl("txtState")).Text;
                        dr["Zip"] = ((TextBox)e.Item.FindControl("txtZip")).Text;
                        dr["Country"] = ((TextBox)e.Item.FindControl("txtCountry")).Text;

                        Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        dgAddress.EditItemIndex = -1;
                        SetPageControlsAndBind(true, false, false, false);
                    }
                    break;
                case "Delete":
                    AddressID = Convert.ToInt32(dgAddress.DataKeys[e.Item.ItemIndex]);
                    dr = dsOrg.Tables["Address"].Rows.Find(AddressID);

                    if (dr["AddDelete"] == DBNull.Value) // untouched - mark for deletion
                    {
                        dr["AddDelete"] = false; // will set rowstate to modified

                        int OrgID = Convert.ToInt32(dgOrg.DataKeys[e.Item.ItemIndex]);
                        DataRow adr = dsOrg.Tables["Org"].Rows.Find(OrgID);
                        adr[dr["AddressType"].ToString()] = 0;
                    }
                    else
                    {
                        if (Convert.ToBoolean(dr["AddDelete"])) // was just added, so simply remove it
                            dr.Delete();
                    }

                    Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                    SetPageControlsAndBind(true, false, false, false);
                    break;
            }
        }

        protected void dgAddress_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;

            if (e.Item.ItemType == ListItemType.EditItem)
            {
                // show current item and all unused items
                int OrgID = Convert.ToInt32(btnOrgStore.CommandArgument);
                DataRow dr = dsOrg.Tables["Org"].Rows.Find(OrgID);
                DropDownList ddlType = (DropDownList)e.Item.FindControl("ddlType");

                foreach (DictionaryEntry addrType in addressTypes)
                {
                    if (Convert.ToInt32(dr[addrType.Key.ToString()]) != 0 && addrType.Key.ToString() != drv["AddressType"].ToString())
                    {
                        ListItem fItem = ddlType.Items.FindByValue(addrType.Key.ToString());
                        ddlType.Items.Remove(fItem);
                    }
                }

                ((DropDownList)e.Item.FindControl("ddlType")).SelectedValue = drv["AddressType"].ToString();
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
                if (dgAddress.ShowFooter)
                {
                    // show unused items
                    if (dgAddress.Items.Count == 3)
                        dgAddress.ShowFooter = false;
                    else
                    {
                        DropDownList ddlType = (DropDownList)e.Item.FindControl("ddlTypeF");

                        foreach (DataGridItem dgi in dgAddress.Items)
                        {
                            foreach (DictionaryEntry addrType in addressTypes)
                            {
                                if (((Label)dgi.FindControl("lblType")).Text == addrType.Value.ToString())
                                {
                                    ListItem fItem = ddlType.Items.FindByValue(addrType.Key.ToString());
                                    ddlType.Items.Remove(fItem);
                                    break;
                                }
                            }
                        }
                        ((TextBox)e.Item.FindControl("txtCountryF")).Text = "US";
                    }
                }
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                ((Label)e.Item.FindControl("lblType")).Text = addressTypes[drv["AddressType"]].ToString();
                ((Label)e.Item.FindControl("lblInternalAddress")).Text = drv["InternalAddress"].ToString();
                ((Label)e.Item.FindControl("lblStrAddress1")).Text = drv["StrAddress1"].ToString();
                ((Label)e.Item.FindControl("lblStrAddress2")).Text = drv["StrAddress2"].ToString();
                ((Label)e.Item.FindControl("lblCity")).Text = drv["City"].ToString();
                ((Label)e.Item.FindControl("lblState")).Text = drv["State"].ToString();
                ((Label)e.Item.FindControl("lblZip")).Text = drv["Zip"].ToString();
                ((Label)e.Item.FindControl("lblCountry")).Text = drv["Country"].ToString();
            }
        }

        protected void dgDepartment_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            AlertInfo[] strValidate = new AlertInfo[1];

            DataRow dr;

            switch (e.CommandName)
            {
                case "AddNewRow":
                    strValidate[0].Data = ((TextBox)e.Item.FindControl("txtDepartmentF")).Text;
                    strValidate[0].Field = "a department.";
                    if (ValidateField(strValidate))
                    {
                        dr = dsOrg.Tables["Department"].NewRow();
                        dr["Department"] = ((TextBox)e.Item.FindControl("txtDepartmentF")).Text;
                        dr["OrgID"] = Convert.ToInt32(btnOrgStore.CommandArgument);
                        dsOrg.Tables["Department"].Rows.Add(dr);
                        Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        SetPageControlsAndBind(true, false, false, false);
                    }
                    break;
                case "Edit":
                    //Datagrid in edit mode, hide footer section 
                    dgDepartment.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
                    SetPageControlsAndBind(true, false, false, true);
                    break;
                case "Cancel":
                    //Datagrid leaving edit mode 
                    dgDepartment.EditItemIndex = -1;
                    SetPageControlsAndBind(true, false, false, false);
                    break;
                case "Update":
                    int DepartmentID = Convert.ToInt32(dgDepartment.DataKeys[e.Item.ItemIndex]);
                    dr = dsOrg.Tables["Department"].Rows.Find(DepartmentID);
                    strValidate[0].Data = ((TextBox)e.Item.FindControl("txtDepartment")).Text;
                    strValidate[0].Field = "a department.";
                    if (ValidateField(strValidate))
                    {
                        dr["Department"] = ((TextBox)e.Item.FindControl("txtDepartment")).Text;
                        Cache.Insert(DataUtility.CacheID, dsOrg, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
                        SetPageControlsAndBind(true, false, false, false);
                    }
                    break;
            }
        }

        protected void dgDepartment_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;

            if (e.Item.ItemType == ListItemType.EditItem)
                ((TextBox)e.Item.FindControl("txtDepartment")).Text = drv["Department"].ToString();
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
                ((Label)e.Item.FindControl("lblDepartment")).Text = drv["Department"].ToString();
        }

        private bool ValidateField(AlertInfo[] strVal)
        {
            string strAlert = string.Empty;
            for (int i = 0; i < strVal.Length; i++)
            {
                if (strAlert.Length > 0)
                    strAlert += "\n";
                if (strVal[i].Data.Length == 0)
                    strAlert += "Please specify " + strVal[i].Field;
            }

            if (strAlert.Length > 0)
            {
                LNF.Web.ServerJScript.JSAlert(Page, strAlert);
                return false;
            }
            else
                return true;
        }

        protected void dgDepartment_SortCommand(object source, DataGridSortCommandEventArgs e)
        {
            //Flip-flop sort direction 
            if (ViewState["dgDeptSortDir"].ToString() == " ASC")
                ViewState["dgDeptSortDir"] = " DESC";
            else
                ViewState["dgDeptSortDir"] = " ASC";

            SetPageControlsAndBind(true, false, false, false);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            // update the address table - use handler to update Org table
            SqlDataAdapter daAddress = new SqlDataAdapter();
            daAddress.RowUpdating += daAddress_RowUpdating;
            daAddress.RowUpdated += daAddress_RowUpdated;

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

            daAddress.Update(dsOrg, "Address");

            // update the Org table - use handler to update department table
            SqlDataAdapter daOrg = new SqlDataAdapter();
            daOrg.RowUpdating += daOrg_RowUpdating;
            daOrg.RowUpdated += daOrg_RowUpdated;
            //AddHandler daOrg.RowUpdating, New SqlRowUpdatingEventHandler(AddressOf daOrgRowUpdating)
            //AddHandler daOrg.RowUpdated, New SqlRowUpdatedEventHandler(AddressOf daOrgRowUpdated)

            daOrg.InsertCommand = new SqlCommand("Org_Insert", cnSselData);
            daOrg.InsertCommand.CommandType = CommandType.StoredProcedure;
            daOrg.InsertCommand.Parameters.Add("@OrgName", SqlDbType.NVarChar, 50, "OrgName");
            daOrg.InsertCommand.Parameters.Add("@OrgTypeID", SqlDbType.Int, 4, "OrgTypeID");
            daOrg.InsertCommand.Parameters.Add("@DefClientAddressID", SqlDbType.Int, 4, "DefClientAddressID");
            daOrg.InsertCommand.Parameters.Add("@DefBillAddressID", SqlDbType.Int, 4, "DefBillAddressID");
            daOrg.InsertCommand.Parameters.Add("@DefShipAddressID", SqlDbType.Int, 4, "DefShipAddressID");
            daOrg.InsertCommand.Parameters.Add("@NNINOrg", SqlDbType.Bit, 1, "NNINOrg");

            daOrg.UpdateCommand = new SqlCommand("Org_Update", cnSselData);
            daOrg.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daOrg.UpdateCommand.Parameters.Add("@OrgID", SqlDbType.Int, 4, "OrgID");
            daOrg.UpdateCommand.Parameters.Add("@OrgName", SqlDbType.NVarChar, 50, "OrgName");
            daOrg.UpdateCommand.Parameters.Add("@OrgTypeID", SqlDbType.Int, 4, "OrgTypeID");
            daOrg.UpdateCommand.Parameters.Add("@DefClientAddressID", SqlDbType.Int, 4, "DefClientAddressID");
            daOrg.UpdateCommand.Parameters.Add("@DefBillAddressID", SqlDbType.Int, 4, "DefBillAddressID");
            daOrg.UpdateCommand.Parameters.Add("@DefShipAddressID", SqlDbType.Int, 4, "DefShipAddressID");
            daOrg.UpdateCommand.Parameters.Add("@NNINOrg", SqlDbType.Bit, 1, "NNINOrg");
            daOrg.UpdateCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            daOrg.Update(dsOrg, "Org");

            // update the department table
            SqlDataAdapter daDepartment = new SqlDataAdapter();

            daDepartment.InsertCommand = new SqlCommand("Department_Insert", cnSselData);
            daDepartment.InsertCommand.CommandType = CommandType.StoredProcedure;
            daDepartment.InsertCommand.Parameters.Add("@Department", SqlDbType.NVarChar, 50, "Department");
            daDepartment.InsertCommand.Parameters.Add("@OrgID", SqlDbType.Int, 4, "OrgID");

            daDepartment.UpdateCommand = new SqlCommand("Department_Update", cnSselData);
            daDepartment.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daDepartment.UpdateCommand.Parameters.Add("@DepartmentID", SqlDbType.Int, 4, "DepartmentID");
            daDepartment.UpdateCommand.Parameters.Add("@Department", SqlDbType.NVarChar, 50, "Department");

            daDepartment.Update(dsOrg, "Department");

            // add warning for Orgs with no departments

            if (strAlert.Length > 0)
            {
                btnSave.Enabled = false;
                SetPageControlsAndBind(false, false, false, false);
                LNF.Web.ServerJScript.JSAlert(Page, "The data you entered have been saved.\nHowever, the following Organizations do not have Departments,\nand you will not be able to add clients until at least one Department is defined.");
            }
            else
                btnDiscard_Click(sender, e);
        }

        private void daAddress_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
                oldAddressID = e.Row.Field<int>("AddressID");
            else if (e.StatementType == StatementType.Delete)
                oldAddressID = e.Row.Field<int>("AddressID", DataRowVersion.Original);
        }

        private void daAddress_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            int addrId = 0;
            string addrType = string.Empty;
            DataRow[] fdr = null;

            if (e.StatementType == StatementType.Insert)
            {
                addrType = e.Row.Field<string>("AddressType");
                addrId = e.Row.Field<int>("AddressID");
            }
            else if (e.StatementType == StatementType.Delete)
            {
                //must use DataRowVersion.Original to get a value from a deleted row
                addrType = e.Row.Field<string>("AddressType", DataRowVersion.Original);
            }
            else
            {
                throw new NotImplementedException();
            }

            // exactly one row will match
            fdr = dsOrg.Tables["Org"].Select(string.Format("{0} = {1}", addrType, oldAddressID));

            //Aparently when an address is deleted the row in the Org table has already been updated (AddressID set to 0) so this is unncessary now
            if (fdr.Length > 0)
            {

                fdr.First().SetField<int>(addrType, addrId);
            }
        }

        private void daOrg_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
                oldOrgID = Convert.ToInt32(e.Row["OrgID"]);
        }

        private void daOrg_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                // update Department, possibly multiple matches
                DataRow[] fdr = dsOrg.Tables["Department"].Select(string.Format("OrgID = {0}", oldOrgID));
                if (fdr.Length == 0)
                    strAlert += e.Row["OrgName"].ToString() + "\n";
                else
                {
                    for (int i = 0; i < fdr.Length; i++)
                        fdr[0]["OrgID"] = e.Row["OrgID"];
                }
            }
        }

        protected void btnDiscard_Click(object sender, EventArgs e)
        {
            Cache.Remove(DataUtility.CacheID); // remove anything left in cache
            Response.Redirect("~");
        }
    }
}
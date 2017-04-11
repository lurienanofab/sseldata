using LNF.Cache;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Web;
using LNF.Web.Content;
using sselData.AppCode;
using sselData.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using repo = LNF.Repository.Data;

namespace sselData
{
    public partial class Account : LNFPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        public enum RequiredDataType
        {
            NotUsed,
            NumberOnly,
            AlphaNumeric
        }

        struct AlertInfo
        {
            public string Field;
            public string Data;
            public int RequiredLength;
            public RequiredDataType RequiredType;
        }

        private DataSet dsAccount;

        private int oldAddressID;
        private int oldAccountID;

        public int AccountID
        {
            get { return int.Parse(hidAccountID.Value); }
            set { hidAccountID.Value = value.ToString(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.IsInRole("Administrator"))
            {
                Session.Abandon();
                Response.Redirect(Session["Logout"].ToString() + "?Action=Exit");
            }

            if (Page.IsPostBack)
            {
                dsAccount = CacheManager.Current.CacheData();

                if (dsAccount == null)
                    Response.Redirect("~");
                else if (dsAccount.DataSetName != "Account")
                    Response.Redirect("~");
            }
            else
            {
                CacheManager.Current.RemoveCacheData(); // remove anything left in cache

                using (SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
                {
                    cnSselData.Open();

                    dsAccount = new DataSet("Account");

                    // get account and clientAccount info
                    using (SqlDataAdapter daAccount = new SqlDataAdapter("Account_Select", cnSselData))
                    {
                        daAccount.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daAccount.SelectCommand.Parameters.AddWithValue("@Action", "AllByOrg");
                        daAccount.SelectCommand.Parameters.AddWithValue("@OrgID", Session["OrgID"]);
                        daAccount.Fill(dsAccount, "Account");

                        dsAccount.Tables["Account"].PrimaryKey = new[] { dsAccount.Tables["Account"].Columns["AccountID"] };
                        dsAccount.Tables["Account"].PrimaryKey[0].AutoIncrement = true;
                        dsAccount.Tables["Account"].PrimaryKey[0].AutoIncrementSeed = -1;
                        dsAccount.Tables["Account"].PrimaryKey[0].AutoIncrementStep = -1;
                    }

                    using (SqlDataAdapter daClientAccount = new SqlDataAdapter("ClientAccount_Select", cnSselData))
                    {
                        daClientAccount.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daClientAccount.SelectCommand.Parameters.AddWithValue("@Action", "WithAccountName");
                        daClientAccount.Fill(dsAccount, "ClientAccount");

                        dsAccount.Tables["ClientAccount"].PrimaryKey = new[] { dsAccount.Tables["ClientAccount"].Columns["ClientAccountID"] };
                        dsAccount.Tables["ClientAccount"].PrimaryKey[0].AutoIncrement = true;
                        dsAccount.Tables["ClientAccount"].PrimaryKey[0].AutoIncrementSeed = -1;
                        dsAccount.Tables["ClientAccount"].PrimaryKey[0].AutoIncrementStep = -1;
                    }

                    //2007-02-03 add this new table to dataset to check if an account still have people who has only this account
                    //This is used in account delete button to make sure the client won't delete the account that still has people whose account is only the deleting one
                    using (SqlDataAdapter daOneClientAccount = new SqlDataAdapter("ClientAccount_Select", cnSselData))
                    {
                        daOneClientAccount.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daOneClientAccount.SelectCommand.Parameters.AddWithValue("@Action", "WithOnlyOneAccount");
                        daOneClientAccount.Fill(dsAccount, "OneClientAccount");

                        // get client data - used to check if all accounts disabled
                        SqlDataAdapter daClient = new SqlDataAdapter("Client_Select", cnSselData);
                        daClient.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daClient.SelectCommand.Parameters.AddWithValue("@Action", "AllWithStatus");
                        daClient.Fill(dsAccount, "Client");

                        dsAccount.Tables["Client"].PrimaryKey = new[] { dsAccount.Tables["Client"].Columns["ClientID"] };
                    }

                    // get Org info
                    using (SqlDataAdapter daOrg = new SqlDataAdapter("Org_Select", cnSselData))
                    {
                        daOrg.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daOrg.SelectCommand.Parameters.AddWithValue("@Action", "All");
                        daOrg.FillSchema(dsAccount, SchemaType.Mapped, "Org"); // to speed table lookups
                        daOrg.Fill(dsAccount, "Org");
                    }

                    // get managers
                    using (SqlDataAdapter daManagerOrg = new SqlDataAdapter("ClientOrg_Select", cnSselData))
                    {
                        daManagerOrg.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daManagerOrg.SelectCommand.Parameters.AddWithValue("@Action", "OrgManager");
                        daManagerOrg.SelectCommand.Parameters.AddWithValue("@OrgID", Session["OrgID"]);
                        daManagerOrg.Fill(dsAccount, "ManagerOrg");

                        // this facilitates the datagrid binding
                        dsAccount.Tables["ManagerOrg"].PrimaryKey = new[] { dsAccount.Tables["ManagerOrg"].Columns["ClientOrgID"] };
                    }

                    // manager info
                    using (SqlDataAdapter daClientManager = new SqlDataAdapter("ClientManager_Select", cnSselData))
                    {
                        daClientManager.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daClientManager.SelectCommand.Parameters.AddWithValue("@Action", "ByOrg");
                        daClientManager.SelectCommand.Parameters.AddWithValue("@OrgID", Session["OrgID"]);
                        daClientManager.Fill(dsAccount, "ClientManager");
                    }

                    // display name column is appended to facilitate manager display
                    using (SqlDataAdapter daClientOrg = new SqlDataAdapter("ClientOrg_Select", cnSselData))
                    {
                        daClientOrg.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daClientOrg.SelectCommand.Parameters.AddWithValue("@Action", "All");
                        daClientOrg.Fill(dsAccount, "ClientOrg");
                        dsAccount.Tables["ClientOrg"].Columns.Add("Reactivated", typeof(bool));

                        dsAccount.Tables["ClientOrg"].PrimaryKey = new[] { dsAccount.Tables["ClientOrg"].Columns["ClientOrgID"] };
                    }

                    // fill in the ddl's
                    using (SqlCommand cmdFundingSource = new SqlCommand("Global_Select", cnSselData))
                    {
                        cmdFundingSource.CommandType = CommandType.StoredProcedure;
                        cmdFundingSource.Parameters.AddWithValue("@TableName", "FundingSource");

                        using (var reader = cmdFundingSource.ExecuteReader())
                        {
                            ddlFundingSource.DataSource = reader;
                            ddlFundingSource.DataTextField = "FundingSource";
                            ddlFundingSource.DataValueField = "FundingSourceID";
                            ddlFundingSource.DataBind();
                        }
                    }

                    using (SqlCommand cmdTechnicalField = new SqlCommand("Global_Select", cnSselData))
                    {
                        cmdTechnicalField.CommandType = CommandType.StoredProcedure;
                        cmdTechnicalField.Parameters.AddWithValue("@TableName", "TechnicalField");

                        using (var reader = cmdTechnicalField.ExecuteReader())
                        {
                            ddlTechnicalField.DataSource = reader;
                            ddlTechnicalField.DataTextField = "TechnicalField";
                            ddlTechnicalField.DataValueField = "TechnicalFieldID";
                            ddlTechnicalField.DataBind();
                        }
                    }

                    using (SqlCommand cmdSpecialTopic = new SqlCommand("Global_Select", cnSselData))
                    {
                        cmdSpecialTopic.CommandType = CommandType.StoredProcedure;
                        cmdSpecialTopic.Parameters.AddWithValue("@TableName", "SpecialTopic");

                        using (var reader = cmdSpecialTopic.ExecuteReader())
                        {
                            ddlSpecialTopic.DataSource = reader;
                            ddlSpecialTopic.DataTextField = "SpecialTopic";
                            ddlSpecialTopic.DataValueField = "SpecialTopicID";
                            ddlSpecialTopic.DataBind();
                        }
                    }

                    // Get the Address
                    using (SqlDataAdapter daAddress = new SqlDataAdapter("Address_Select", cnSselData))
                    {
                        daAddress.SelectCommand.CommandType = CommandType.StoredProcedure;
                        daAddress.SelectCommand.Parameters.AddWithValue("@Action", "All");
                        daAddress.FillSchema(dsAccount, SchemaType.Mapped, "Address");
                        daAddress.Fill(dsAccount, "Address");

                        dsAccount.Tables["Address"].Columns.Add("AddressType", typeof(string));
                        dsAccount.Tables["Address"].Columns.Add("AddDelete", typeof(bool));
                    }

                    //Declare default sort parameter and sort direction
                    ViewState["dgAccountSortCol"] = "Name";
                    ViewState["dgAccountSortDir"] = " ASC";

                    CacheManager.Current.CacheData(dsAccount);

                    SetPageControlsAndBind(false, false, false);

                    cnSselData.Close();
                }
            }
        }

        private void SetPageControlsAndBind(bool showAddEditPanel, bool showAddExistingPanel, bool inlineAddressEdit)
        {
            bool panelVis = showAddEditPanel | showAddExistingPanel;

            // determine which panel to display
            pAccountList.Visible = !panelVis;
            pAddEdit.Visible = showAddEditPanel;
            pExisting.Visible = showAddExistingPanel;

            DataRow dr = dsAccount.Tables["Org"].Rows.Find(Session["OrgID"]);

            // set page header label
            if (pAccountList.Visible)
            {
                lblHeader.Text = "Configure Accounts for " + dr["OrgName"].ToString();

                pIntAccount.Visible = false;
                pExtAccount.Visible = false;

                DataView dv = dsAccount.Tables["Account"].DefaultView;
                dv.Sort = ViewState["dgAccountSortCol"].ToString() + ViewState["dgAccountSortDir"].ToString();
                dv.RowFilter = "Active = 1";
                dgAccount.DataSource = dv;
                dgAccount.DataBind();

                if (OrgIsInternal()) // yuck - what if the number of columns change?
                {
                    dgAccount.Columns[2].Visible = true;
                    dgAccount.Columns[3].Visible = true;
                }
                else
                {
                    dgAccount.Columns[2].Visible = false;
                    dgAccount.Columns[3].Visible = false;
                }

                // now, add to ddlPager for paging
                ddlPager.Items.Clear();
                int pSize = dgAccount.PageSize;
                for (int i = 0; i < dv.Count; i += pSize)
                {
                    ListItem pagerItem = new ListItem();
                    pagerItem.Value = (i / pSize).ToString();
                    int index = (i + (pSize - 1) >= dv.Count) ? dv.Count - 1 : i + (pSize - 1);
                    pagerItem.Text = dv[i].Row[ViewState["dgAccountSortCol"].ToString()].ToString() + " ... " + dv[index].Row[ViewState["dgAccountSortCol"].ToString()].ToString();
                    ddlPager.Items.Add(pagerItem);
                }
                ddlPager.SelectedValue = dgAccount.CurrentPageIndex.ToString();
            }
            else if (pAddEdit.Visible)
            {
                DataRow adr = dsAccount.Tables["Account"].Rows.Find(AccountID);

                if (AccountID == 0)
                    lblHeader.Text = "Add new Account";
                else
                    lblHeader.Text = string.Format("Configure Account {0}", adr["Name"]);

                if (OrgIsInternal())
                {
                    pIntAccount.Visible = true;
                    pExtAccount.Visible = false;
                    SetFocus(txtAccount);
                }
                else
                {
                    pIntAccount.Visible = false;
                    pExtAccount.Visible = true;
                    SetFocus(txtNumber);
                }

                var addressItems = GetAddressManagerDataSource();

                // turn off dgAddress control when editing an address
                if (inlineAddressEdit || addressItems.Count == AddressManager1.AddressTypes.Count)
                    AddressManager1.ShowFooter = false;
                else
                    AddressManager1.ShowFooter = true;

                AddressManager1.Fill(addressItems);

                // filter the managers
                DataView dvm = dsAccount.Tables["ClientAccount"].DefaultView;
                dvm.RowFilter = string.Format("Active = 1 AND Manager = 1 AND AccountID = {0}", AccountID);
                dgAccountManager.DataSource = dvm;
                dgAccountManager.DataBind();
            }
            else if (pExisting.Visible)
                lblHeader.Text = "Add existing Account to " + dr["OrgName"].ToString();
        }

        private IList<AddressItem> GetAddressManagerDataSource()
        {
            List<AddressItem> result = new List<AddressItem>();
            DataView dv = GetAddressDataSource();

            if (dv != null)
            {
                foreach (DataRowView drv in dv)
                {
                    string typeColumn = drv["AddressType"].ToString();
                    string typeName = AddressManager1.AddressTypes.First(x => x.Column == typeColumn).Name;

                    result.Add(new AddressItem()
                    {
                        AddressID = Convert.ToInt32(drv["AddressID"]),
                        Type = new AddressType() { Column = typeColumn, Name = typeName },
                        AttentionLine = drv["InternalAddress"].ToString(),
                        StreetAddressLine1 = drv["StrAddress1"].ToString(),
                        StreetAddressLine2 = drv["StrAddress2"].ToString(),
                        City = drv["City"].ToString(),
                        State = drv["State"].ToString(),
                        Zip = drv["Zip"].ToString(),
                        Country = drv["Country"].ToString()
                    });
                }
            }

            return result;
        }

        private DataView GetAddressDataSource()
        {
            // filter the addresses
            DataView dv = dsAccount.Tables["Address"].DefaultView;

            if (AccountID == 0)
            {
                // a new client must have a newly added address, if any
                dv.RowFilter = "AddDelete = 1";
            }
            else
            {
                // set addresstype and create row filter
                DataRow adr = dsAccount.Tables["Account"].Rows.Find(AccountID);

                string filter = string.Empty;
                MakeAddrFilter(ref filter, adr, "BillAddressID");
                MakeAddrFilter(ref filter, adr, "ShipAddressID");

                // if affiliated with this org, this row must exist
                dv.RowFilter = string.Format("AddDelete = 1{0}", filter);
            }

            return dv;
        }

        private void MakeAddrFilter(ref string filter, DataRow dr, string addrType)
        {
            if (Convert.ToInt32(dr[addrType]) != 0)
            {
                DataRow adr = dsAccount.Tables["Address"].Rows.Find(dr[addrType]);
                if (adr != null)
                {
                    adr["AddressType"] = addrType;
                    // to prevent unnecessary writes to the DB
                    if (adr.RowState == DataRowState.Unchanged)
                        adr.AcceptChanges();
                    filter += string.Format(" OR AddressID = {0}", dr[addrType]);
                }
            }
        }

        protected void dgAccount_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            DataRow dr;

            switch (e.CommandName)
            {
                case "AddNew":
                    txtAccount.Text = string.Empty;
                    txtFund.Text = string.Empty;
                    txtDepartment.Text = string.Empty;
                    txtProgram.Text = string.Empty;
                    txtClass.Text = string.Empty;
                    txtProject.Text = string.Empty;
                    txtNumber.Text = string.Empty;
                    txtShortCode.Text = string.Empty;

                    txtName.Text = string.Empty;
                    ddlFundingSource.ClearSelection();
                    ddlTechnicalField.ClearSelection();
                    ddlSpecialTopic.ClearSelection();

                    txtInvoiceNumber.Text = string.Empty;
                    txtInvoiceLine1.Text = string.Empty;
                    txtInvoiceLine2.Text = string.Empty;
                    txtPoEndDate.Text = string.Empty;
                    txtPoInitialFunds.Text = string.Empty;

                    //2010-03-07 add account type to distinguish between regular, limited and IOF only
                    if (cklistAccountType.Items.Count == 0)
                        cklistAccountType.DataBind();

                    cklistAccountType.Items[0].Selected = true;

                    // remove any existing rows with AddDelete == true
                    DataRow[] rows = dsAccount.Tables["Address"].Select("AddDelete = 1");

                    foreach (DataRow drAddr in rows)
                    {
                        drAddr.Delete();
                    }

                    // if org has DefBillAddr or DefShipAddr, add row to addr table with this info
                    DataRow drOrg = dsAccount.Tables["Org"].Rows.Find(Session["OrgID"]);

                    foreach (var addrType in AddressManager1.AddressTypes)
                    {
                        string defaultColumn = string.Format("Def{0}", addrType.Column);

                        if (Convert.ToInt32(drOrg[defaultColumn]) != 0)
                        {
                            DataRow ndr = dsAccount.Tables["Address"].NewRow();
                            int addressId = Convert.ToInt32(ndr["AddressID"]);
                            ndr.ItemArray = dsAccount.Tables["Address"].Rows.Find(drOrg[defaultColumn]).ItemArray;
                            ndr["AddressID"] = addressId;
                            ndr["AddressType"] = addrType.Column;
                            ndr["AddDelete"] = true;
                            dsAccount.Tables["Address"].Rows.Add(ndr);
                        }
                    }

                    AccountID = 0;
                    btnAccountStore.Text = "Store New Account";

                    CacheManager.Current.CacheData(dsAccount);

                    SetPageControlsAndBind(true, false, false);
                    break;
                case "Edit":
                    int accountId = Convert.ToInt32(dgAccount.DataKeys[e.Item.ItemIndex]);
                    dr = dsAccount.Tables["Account"].Rows.Find(accountId);

                    if (OrgIsInternal())
                    {
                        txtAccount.Text = dr["Number"].ToString().Substring(0, 6);
                        txtFund.Text = dr["Number"].ToString().Substring(6, 5);
                        txtDepartment.Text = dr["Number"].ToString().Substring(11, 6);
                        txtProgram.Text = dr["Number"].ToString().Substring(17, 5);
                        txtClass.Text = dr["Number"].ToString().Substring(22, 5);
                        txtProject.Text = dr["Number"].ToString().Substring(27, 7);
                        txtShortCode.Text = WebUtility.FillField(dr["ShortCode"], string.Empty);
                    }
                    else
                    {
                        txtNumber.Text = dr["Number"].ToString().Substring(8);
                        txtInvoiceNumber.Text = WebUtility.FillField(dr["InvoiceNumber"], string.Empty);
                        txtInvoiceLine1.Text = WebUtility.FillField(dr["InvoiceLine1"], string.Empty);
                        txtInvoiceLine2.Text = WebUtility.FillField(dr["InvoiceLine2"], string.Empty);
                        txtPoEndDate.Text = WebUtility.FillField(dr["PoEndDate"], string.Empty);
                        txtPoInitialFunds.Text = WebUtility.FillField(dr["PoInitialFunds"], string.Empty);
                    }

                    txtName.Text = dr["Name"].ToString();
                    ddlFundingSource.SelectedValue = dr["FundingSourceID"].ToString();
                    ddlTechnicalField.SelectedValue = dr["TechnicalFieldID"].ToString();
                    ddlSpecialTopic.SelectedValue = dr["SpecialTopicID"].ToString();

                    int pVal;
                    if (cklistAccountType.Items.Count == 0)
                        cklistAccountType.DataBind();

                    foreach (ListItem item in cklistAccountType.Items)
                    {
                        pVal = Convert.ToInt32(item.Value);
                        item.Selected = (Convert.ToInt32(dr["AccountTypeID"]) == pVal);
                    }

                    AccountID = accountId;
                    btnAccountStore.Text = "Store Modified Data";

                    CacheManager.Current.CacheData(dsAccount);

                    SetPageControlsAndBind(true, false, false);
                    break;
                case "AddExisting":
                    ddlAccount.Enabled = true;
                    SetAccountDDL();
                    SetPageControlsAndBind(false, true, false);
                    break;
                case "Delete":
                    accountId = Convert.ToInt32(dgAccount.DataKeys[e.Item.ItemIndex]);
                    dr = dsAccount.Tables["Account"].Rows.Find(accountId);
                    DataRow[] adr = dsAccount.Tables["Address"].Select(string.Format("AddressID = {0} OR AddressID = {1}", dr["BillAddressID"], dr["ShipAddressID"]));

                    if (dr.RowState == DataRowState.Added)
                    {
                        for (int i = 0; i < adr.Length; i++)
                            dsAccount.Tables["Address"].Rows.Remove(adr[i]); // if it is a new account, we don't need its addresses
                    }

                    DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("Active = 1 AND AccountID = {0}", accountId));
                    for (int i = 0; i < cadrs.Length; i++)
                    {
                        if (dr.RowState == DataRowState.Added)
                        {
                            DataRow tempdr = cadrs[i];
                            if (dsAccount.Tables["Address"].Rows.IndexOf(tempdr) > -1)
                                dsAccount.Tables["Address"].Rows.Remove(tempdr); // if it is a new account, we don't need its managers
                        }
                        else
                        {
                            DataUtility.SetActiveFalse(cadrs[i]); // disable access to this account

                            // check that all clients still have another account
                            int cadrs_ClientOrgID = Convert.ToInt32(cadrs[i]["ClientOrgID"]);
                            DataRow drHasActiveAcct = dsAccount.Tables["ClientOrg"].Rows.Find(cadrs_ClientOrgID);
                            DataRow clientDataRow = null;
                            bool bHasActiveAcct = false;

                            if (drHasActiveAcct != null)
                            {
                                int clientId = Convert.ToInt32(dsAccount.Tables["ClientOrg"].Rows.Find(cadrs_ClientOrgID)["ClientID"]);
                                clientDataRow = dsAccount.Tables["Client"].Rows.Find(clientId);
                                if (clientDataRow == null)
                                    throw new Exception(string.Format("No record found for ClientID = {0}", clientId));

                                // check if client has any active accounts
                                bHasActiveAcct = DataUtility.HasActiveAccount(clientDataRow, dsAccount.Tables["ClientOrg"], dsAccount.Tables["ClientAccount"]);
                            }

                            if (!bHasActiveAcct)
                                if (clientDataRow != null) clientDataRow["EnableAccess"] = false;
                        }
                    }

                    if (dr.RowState == DataRowState.Added)
                        dsAccount.Tables["Account"].Rows.Remove(dr);
                    else
                        DataUtility.SetActiveFalse(dr);

                    CacheManager.Current.CacheData(dsAccount);

                    SetPageControlsAndBind(false, false, false);
                    break;
            }
        }

        protected void dgAccount_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((Label)e.Item.FindControl("lblName")).Text = drv["Name"].ToString();
                if (OrgIsInternal())
                {
                    string strNumber;
                    strNumber = drv["Number"].ToString().Substring(0, 6) + "-";
                    strNumber += drv["Number"].ToString().Substring(6, 5) + "-";
                    strNumber += drv["Number"].ToString().Substring(11, 6) + "-";
                    strNumber += drv["Number"].ToString().Substring(17, 5) + "-";
                    strNumber += drv["Number"].ToString().Substring(22, 5) + "-";
                    strNumber += drv["Number"].ToString().Substring(27, 7);

                    ((Label)e.Item.FindControl("lblNumber")).Text = strNumber;
                    ((Label)e.Item.FindControl("lblProject")).Text = drv["Project"].ToString();
                    if (drv["ShortCode"] == DBNull.Value)
                        ((Label)e.Item.FindControl("lblShortCode")).Text = string.Empty;
                    else
                        ((Label)e.Item.FindControl("lblShortCode")).Text = drv["ShortCode"].ToString();
                }
                else
                    ((Label)e.Item.FindControl("lblNumber")).Text = drv["Number"].ToString();

                //2007-02-03 Post the javascript to warn user if they are deleting accounts that still have clientOrgs who has only one single account in this org
                int AccountID = Convert.ToInt32(dgAccount.DataKeys[e.Item.ItemIndex]);
                DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("Active = 1 AND AccountID = {0}", AccountID));
                if (cadrs.Length > 0)
                {
                    foreach (DataRow dr in cadrs)
                    {
                        DataRow[] OneAccountUser = dsAccount.Tables["OneClientAccount"].Select(string.Format("ClientOrgID = {0}", dr["ClientOrgID"]));
                        if (OneAccountUser.Length >= 1)
                        {
                            string script = string.Format("return confirm('There are still {0} client(s) associated ONLY with this account, are you sure you want to delete this account?');", OneAccountUser.Length);
                            ImageButton lbtn = (ImageButton)e.Item.FindControl("btnDelete");
                            lbtn.OnClientClick = script;
                        }
                    }
                }
            }
        }

        protected void ddlPager_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgAccount.CurrentPageIndex = Convert.ToInt32(ddlPager.SelectedValue); // selectedItem should also work
            SetPageControlsAndBind(false, false, false);
        }

        protected void dgAccount_SortCommand(object source, DataGridSortCommandEventArgs e)
        {
            if (ViewState["dgAccountSortCol"].ToString() == e.SortExpression)
            {
                //Flip-flop sort direction 
                if (ViewState["dgAccountSortDir"].ToString() == " ASC")
                    ViewState["dgAccountSortDir"] = " DESC";
                else
                    ViewState["dgAccountSortDir"] = " ASC";
            }
            else
            {
                ViewState["dgAccountSortCol"] = e.SortExpression;
                ViewState["dgAccountSortDir"] = " ASC";
            }

            SetPageControlsAndBind(false, false, false);
        }

        protected void rblAcctDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAccountDDL();
        }

        private void SetAccountDDL()
        {
            DataView dv = dsAccount.Tables["Account"].DefaultView;
            dv.Sort = rblAcctDisplay.SelectedValue + " ASC";
            dv.RowFilter = "Active = 0";

            ddlAccount.DataSource = dv;
            ddlAccount.DataTextField = rblAcctDisplay.SelectedValue;
            ddlAccount.DataValueField = "AccountID";
            ddlAccount.DataBind();

            // now, go through and remove items that do not meet the filters
            for (int i = ddlAccount.Items.Count - 1; i >= 0; i--)
            {
                // only match specified text
                if (txtSearch.Text.Trim().Length > 0)
                    if (!ddlAccount.Items[i].Text.ToLower().Contains(txtSearch.Text.Trim().ToLower()))
                        ddlAccount.Items.Remove(ddlAccount.Items[i]);
            }

            // now add a blank item so SelectedIndexChnaged works for the first name in the list
            ListItem blankItem = new ListItem();
            blankItem.Value = "0";
            blankItem.Text = string.Empty;
            ddlAccount.Items.Insert(0, blankItem);
        }

        protected void ddlAccount_PreRender(object sender, EventArgs e)
        {
            if (rblAcctDisplay.SelectedValue == "Number")
            {
                foreach (ListItem li in ddlAccount.Items)
                {
                    if (li.Text.Length > 0)
                        li.Text = li.Text.Substring(0, 6) + "-" + li.Text.Substring(6, 5) + "-" + li.Text.Substring(11, 6) + "-" + li.Text.Substring(17, 5) + "-" + li.Text.Substring(22, 5) + "-" + li.Text.Substring(27, 7);
                }
            }
        }

        protected void btnAccountReenable_Click(object sender, EventArgs e)
        {
            DataRow adr = dsAccount.Tables["Account"].Rows.Find(ddlAccount.SelectedValue);
            if (adr.RowState == DataRowState.Modified) // must have just been disabled
            {
                adr.RejectChanges();
                DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0}", ddlAccount.SelectedValue));
                for (int i = 0; i < cadrs.Length; i++)
                {
                    if (cadrs[i].RowState == DataRowState.Modified)
                        cadrs[i].RejectChanges();
                }
            }
            else
            {
                adr["Active"] = true; // set account active

                // now, reestablish the manager(s)
                SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                // get DisableDate
                SqlDataAdapter daLogRecordByRecord = new SqlDataAdapter("ActiveLog_Select", cnSselData);
                daLogRecordByRecord.SelectCommand.CommandType = CommandType.StoredProcedure;
                daLogRecordByRecord.SelectCommand.Parameters.AddWithValue("@Action", "ByRecord");
                daLogRecordByRecord.SelectCommand.Parameters.AddWithValue("@TableName", "Account");
                daLogRecordByRecord.SelectCommand.Parameters.AddWithValue("@Record", adr["AccountID"]);

                DataTable dtLogRecByRec = new DataTable();
                daLogRecordByRecord.Fill(dtLogRecByRec); // should be exactly one row

                // find all other records that were disabled at the same time from ClientAccount
                if (dtLogRecByRec.Rows.Count == 1)
                {
                    SqlDataAdapter daLogRecordByDate = new SqlDataAdapter("ActiveLog_Select", cnSselData);
                    daLogRecordByDate.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daLogRecordByDate.SelectCommand.Parameters.AddWithValue("@Action", "ByDate");
                    daLogRecordByDate.SelectCommand.Parameters.AddWithValue("@DisableDate", dtLogRecByRec.Rows[0]["DisableDate"]);
                    daLogRecordByDate.SelectCommand.Parameters.AddWithValue("@TableName", "ClientAccount");
                    DataTable dtClientAcctRec = new DataTable();
                    daLogRecordByDate.Fill(dtClientAcctRec);

                    DataRow cadr;

                    foreach (DataRow dr in dtClientAcctRec.Rows)
                    {
                        cadr = dsAccount.Tables["ClientAccount"].Select(string.Format("ClientAccountID = {0}", dr["Record"]))[0];
                        if (Convert.ToInt32(cadr["AccountID"]) == Convert.ToInt32(adr["AccountID"]) && Convert.ToBoolean(cadr["Manager"]))
                            cadr["Active"] = true;
                    }
                }
            }

            CacheManager.Current.CacheData(dsAccount);

            SetPageControlsAndBind(false, false, false);
        }

        protected void btnAccountReenableQuit_Click(object sender, EventArgs e)
        {
            SetPageControlsAndBind(false, false, false);
        }

        protected void lbtnReactivateByShortCode_Click(object sender, EventArgs e)
        {
            ReactivateByShortCode(txtShortCode.Text.Trim());
        }

        private void ReactivateByShortCode(string sc)
        {
            string shortcodeVal = sc;

            txtSearch.Text = shortcodeVal;
            rblAcctDisplay.SelectedValue = "ShortCode";

            ddlAccount.Enabled = true;

            //load the ddlAccount with data
            SetAccountDDL();

            if (ddlAccount.Items.Count == 1)
                ddlAccount.SelectedIndex = 0;
            else if (ddlAccount.Items.Count > 1)
                ddlAccount.SelectedIndex = 1;

            pIntAccount.Visible = false;
            SetPageControlsAndBind(false, true, false);
        }

        private bool IsAccountActivatedAndNotSavedYet(string shortcodeVal)
        {
            // check if the Account is already activated in Cache  IMPORTANT this checks only in CACHE, not in the actual DB
            //Dim dv As DataView = dsAccount.Tables("Account").DefaultView
            //Dim dr As DataRow = dsAccount.Tables("Account").Rows.Find(AccountID)
            //dv.RowFilter = "Active=True"
            bool activeState = false;
            //Dim dr As DataRow = dsAccount.Tables("Account").Rows.Find(shortcodeVal)
            DataRow[] rows = dsAccount.Tables["Account"].Select(string.Format("ShortCode = '{0}'", shortcodeVal));
            if (rows.Length > 0)
                activeState = Convert.ToBoolean(rows[0]["Active"]);

            return activeState;
        }

        protected void btnAccountStore_Click(object sender, EventArgs e)
        {
            AlertInfo[] strValidate;

            if (pIntAccount.Visible)
            {
                strValidate = new AlertInfo[7];

                strValidate[1].Data = txtAccount.Text.Trim();
                strValidate[1].Field = "the account";
                strValidate[1].RequiredLength = 6;
                strValidate[1].RequiredType = RequiredDataType.NumberOnly;
                strValidate[2].Data = txtFund.Text.Trim();
                strValidate[2].Field = "the fund";
                strValidate[2].RequiredLength = 5;
                strValidate[2].RequiredType = RequiredDataType.NumberOnly;
                strValidate[3].Data = txtDepartment.Text.Trim();
                strValidate[3].Field = "the department";
                strValidate[3].RequiredLength = 6;
                strValidate[3].RequiredType = RequiredDataType.NumberOnly;
                strValidate[4].Data = txtProgram.Text.Trim();
                strValidate[4].Field = "the program";
                strValidate[4].RequiredLength = 5;
                strValidate[4].RequiredType = RequiredDataType.AlphaNumeric;
                strValidate[5].Data = txtClass.Text.Trim();
                strValidate[5].Field = "the class";
                strValidate[5].RequiredLength = 5;
                strValidate[5].RequiredType = RequiredDataType.NumberOnly;
                strValidate[6].Data = txtProject.Text.Trim();
                strValidate[6].Field = "the project/grant";
                strValidate[6].RequiredLength = 7;
                strValidate[6].RequiredType = RequiredDataType.AlphaNumeric;
            }
            else
            {
                strValidate = new AlertInfo[2];

                strValidate[1].Data = txtNumber.Text.Trim();
                strValidate[1].Field = "an account.";
            }

            strValidate[0].Data = txtName.Text.Trim();
            strValidate[0].Field = "the account name.";

            if (ValidateField(strValidate))
            {
                // add rows to Client, ClientSite and ClientOrg for new entries
                bool isNewEntry = AccountID == 0;

                DataRow dr;

                if (isNewEntry)
                {
                    // add an entry to the client table
                    dr = dsAccount.Tables["Account"].NewRow();
                    dr["OrgID"] = Session["OrgID"];

                    if (pIntAccount.Visible)
                        dr["Project"] = txtProject.Text.Trim();
                    else
                        dr["Project"] = string.Empty;
                }
                else
                {
                    // get the entry in the client table
                    dr = dsAccount.Tables["Account"].Rows.Find(AccountID);
                }

                // if entering new or modifying, update the fields
                if (pIntAccount.Visible)
                {
                    dr["Number"] = txtAccount.Text.Trim() + txtFund.Text.Trim() + txtDepartment.Text.Trim() + txtProgram.Text.Trim() + txtClass.Text.Trim() + txtProject.Text.Trim();
                    dr["ShortCode"] = txtShortCode.Text.Trim();
                }
                else
                {
                    DateTime endDate;
                    if (!DateTime.TryParse(txtPoEndDate.Text.Trim(), out endDate))
                        endDate = DateTime.Now.AddYears(5);

                    double funds;
                    if (!double.TryParse(txtPoInitialFunds.Text.Trim(), out funds))
                        funds = 0.0;

                    dr["ShortCode"] = string.Empty;
                    dr["Number"] = "999999EX" + txtNumber.Text.Trim();
                    dr["InvoiceNumber"] = txtInvoiceNumber.Text.Trim();
                    dr["InvoiceLine1"] = txtInvoiceLine1.Text.Trim();
                    dr["InvoiceLine2"] = txtInvoiceLine2.Text.Trim();
                    dr["PoEndDate"] = endDate;
                    dr["PoInitialFunds"] = funds;
                    dr["PoRemainingFunds"] = dr["PoInitialFunds"];
                }

                dr["Name"] = txtName.Text.Trim();
                dr["FundingSourceID"] = ddlFundingSource.SelectedValue;
                dr["TechnicalFieldID"] = ddlTechnicalField.SelectedValue;
                dr["SpecialTopicID"] = ddlSpecialTopic.SelectedValue;
                dr["AccountTypeID"] = cklistAccountType.SelectedValue;
                dr["Active"] = true;

                // find any address that need to be dealt with
                string addressType;
                DataRow[] fdr = dsAccount.Tables["Address"].Select("AddDelete IS NOT NULL");
                for (int i = 0; i < fdr.Length; i++)
                {
                    addressType = fdr[i]["AddressType"].ToString();
                    if (Convert.ToBoolean(fdr[i]["AddDelete"])) //' addr was added
                    {
                        dr[addressType] = fdr[i]["AddressID"];
                        fdr[i]["AddDelete"] = DBNull.Value;
                    }
                    else
                    {
                        dr[addressType] = 0;
                        fdr[i].Delete();
                    }
                }

                // set unused AddressID's to 0
                foreach (var addrType in AddressManager1.AddressTypes)
                {
                    if (dr[addrType.Column] == DBNull.Value)
                        dr[addrType.Column] = 0;
                }

                // update rows in ClientAccount as needed
                DataRow[] cmdr = dsAccount.Tables["Clientaccount"].Select("AccountID = 0");
                for (int i = 0; i < cmdr.Length; i++)
                    cmdr[i]["AccountID"] = dr["AccountID"];

                if (isNewEntry)
                    dsAccount.Tables["Account"].Rows.Add(dr);

                CacheManager.Current.CacheData(dsAccount);

                SetPageControlsAndBind(false, false, false);
            }
        }

        private int CalculateAccountTypeSum()
        {
            int sum = 0;

            foreach (ListItem item in cklistAccountType.Items)
            {
                if (item.Selected)
                    sum += Convert.ToInt32(item.Value);
            }

            return sum;
        }

        protected void btnAccountStoreQuit_Click(object sender, EventArgs e)
        {
            DataRow[] fdr;

            // remove any addresses that were added
            fdr = dsAccount.Tables["Address"].Select("AddDelete = 1");
            if (fdr.Length == 1)
                fdr[0].Delete();

            // unmark any addresses that were removed - only need to check for non-new entries
            if (AccountID > 0)
            {
                DataRow dr = dsAccount.Tables["Account"].Rows.Find(AccountID);
                fdr = dsAccount.Tables["Address"].Select("AddDelete = 0");

                DataRowState drs;
                for (int i = 0; i < fdr.Length; i++)
                {
                    drs = fdr[i].RowState;
                    string addressType = fdr[i]["AddressType"].ToString();
                    dr[addressType] = fdr[i]["AddressID"];
                    fdr[i]["AddDelete"] = DBNull.Value;
                    fdr[i]["AddressType"] = DBNull.Value;

                    if (drs == DataRowState.Unchanged)
                        fdr[i].AcceptChanges();
                }
            }

            CacheManager.Current.CacheData(dsAccount);

            SetPageControlsAndBind(false, false, false);
        }

        protected void ddlAddrType_Changed(object sender, EventArgs e)
        {
            DropDownList ddlAddrType = (DropDownList)sender;
            PopulateAddressWithDefault(ddlAddrType.SelectedValue, (DataGridItem)ddlAddrType.NamingContainer);
        }

        //protected void dgAddress_ItemCommand(object source, DataGridCommandEventArgs e)
        //{
        //    AlertInfo[] strValidate = new AlertInfo[3];
        //    DataRow dr;
        //    int AddressID;

        //    // use AddDelete to mark new and deleted rows only
        //    // changes that are saved cannot be undone by quiting at the org layer
        //    switch (e.CommandName)
        //    {
        //        case "AddNewRow":
        //            strValidate[0].Data = ((TextBox)e.Item.FindControl("txtStrAddress1F")).Text;
        //            strValidate[0].Field = "a street address.";
        //            strValidate[1].Data = ((TextBox)e.Item.FindControl("txtCityF")).Text;
        //            strValidate[1].Field = "a city.";
        //            strValidate[2].Data = ((TextBox)e.Item.FindControl("txtStateF")).Text;
        //            strValidate[2].Field = "a state.";
        //            if (ValidateField(strValidate))
        //            {
        //                dr = dsAccount.Tables["Address"].NewRow();
        //                dr["AddressType"] = ((DropDownList)e.Item.FindControl("ddlTypeF")).SelectedValue;
        //                dr["InternalAddress"] = ((TextBox)e.Item.FindControl("txtInternalAddressF")).Text;
        //                dr["StrAddress1"] = ((TextBox)e.Item.FindControl("txtStrAddress1F")).Text;
        //                dr["StrAddress2"] = ((TextBox)e.Item.FindControl("txtStrAddress2F")).Text;
        //                dr["City"] = ((TextBox)e.Item.FindControl("txtCityF")).Text;
        //                dr["State"] = ((TextBox)e.Item.FindControl("txtStateF")).Text;
        //                dr["Zip"] = ((TextBox)e.Item.FindControl("txtZipF")).Text;
        //                dr["Country"] = ((TextBox)e.Item.FindControl("txtCountryF")).Text;
        //                dr["AddDelete"] = true;
        //                dsAccount.Tables["Address"].Rows.Add(dr);

        //                Cache.Insert(DataUtility.CacheID, dsAccount, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
        //                SetPageControlsAndBind(true, false, false);
        //            }
        //            break;
        //        case "Edit":
        //            //Datagrid in edit mode, hide footer section 
        //            //dgAddress.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
        //            AddressManager1.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
        //            SetPageControlsAndBind(true, false, true);
        //            break;
        //        case "Cancel":
        //            //Datagrid leaving edit mode 
        //            AddressManager1.EditItemIndex = -1;
        //            SetPageControlsAndBind(true, false, false);
        //            break;
        //        case "Update":
        //            strValidate[0].Data = ((TextBox)e.Item.FindControl("txtStrAddress1")).Text;
        //            strValidate[0].Field = "a street address.";
        //            strValidate[1].Data = ((TextBox)e.Item.FindControl("txtCity")).Text;
        //            strValidate[1].Field = "a city.";
        //            strValidate[2].Data = ((TextBox)e.Item.FindControl("txtState")).Text;
        //            strValidate[2].Field = "a state.";
        //            if (ValidateField(strValidate))
        //            {
        //                AddressID = Convert.ToInt32(AddressManager1.DataKeys[e.Item.ItemIndex]);
        //                dr = dsAccount.Tables["Address"].Rows.Find(AddressID);
        //                dr["AddressType"] = ((DropDownList)e.Item.FindControl("ddlType")).SelectedValue;
        //                dr["InternalAddress"] = ((TextBox)e.Item.FindControl("txtInternalAddress")).Text;
        //                dr["StrAddress1"] = ((TextBox)e.Item.FindControl("txtStrAddress1")).Text;
        //                dr["StrAddress2"] = ((TextBox)e.Item.FindControl("txtStrAddress2")).Text;
        //                dr["City"] = ((TextBox)e.Item.FindControl("txtCity")).Text;
        //                dr["State"] = ((TextBox)e.Item.FindControl("txtState")).Text;
        //                dr["Zip"] = ((TextBox)e.Item.FindControl("txtZip")).Text;
        //                dr["Country"] = ((TextBox)e.Item.FindControl("txtCountry")).Text;

        //                Cache.Insert(DataUtility.CacheID, dsAccount, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
        //                dgAddress.EditItemIndex = -1;
        //                SetPageControlsAndBind(true, false, false);
        //            }
        //            break;
        //        case "Delete":
        //            AddressID = Convert.ToInt32(dgAddress.DataKeys[e.Item.ItemIndex]);
        //            dr = dsAccount.Tables["Address"].Rows.Find(AddressID);

        //            if (dr["AddDelete"] == DBNull.Value) // untouched - mark for deletion
        //            {
        //                dr["AddDelete"] = false; //' will set rowstate to modified

        //                int AccountID = Convert.ToInt32(hidAccountID.Value);
        //                DataRow adr = dsAccount.Tables["Account"].Rows.Find(AccountID);
        //                foreach (DictionaryEntry addrType in addressTypes)
        //                {
        //                    string columnName = addrType.Key.ToString();
        //                    if (Convert.ToInt32(adr[columnName]) == AddressID)
        //                        adr[columnName] = 0;
        //                }
        //            }
        //            else
        //            {
        //                if (Convert.ToBoolean(dr["AddDelete"])) // was just added, so simply remove it
        //                    dr.Delete();
        //            }

        //            Cache.Insert(DataUtility.CacheID, dsAccount, null, DateTime.MaxValue, TimeSpan.FromMinutes(20));
        //            SetPageControlsAndBind(true, false, false);
        //            break;
        //    }
        //}

        //protected void dgAddress_ItemDataBound(object sender, DataGridItemEventArgs e)
        //{
        //    DataRowView drv = (DataRowView)e.Item.DataItem;

        //    if (e.Item.ItemType == ListItemType.EditItem)
        //    {
        //        // show current item and all unused items
        //        // must be handled differently from footer since entire grid not yet populated
        //        int AccountID = Convert.ToInt32(btnAccountStore.CommandArgument);
        //        DropDownList ddlType = (DropDownList)e.Item.FindControl("ddlType");
        //        DataRow dr = dsAccount.Tables["Account"].Rows.Find(AccountID);

        //        if (dr == null)
        //            ddlType.Enabled = false;
        //        else
        //        {
        //            foreach (DictionaryEntry addrType in addressTypes)
        //            {
        //                string columnName = addrType.Key.ToString();
        //                if (Convert.ToInt32(dr[columnName]) != 0 && columnName != drv["AddressType"].ToString())
        //                {
        //                    ListItem fItem = ddlType.Items.FindByValue(columnName);
        //                    ddlType.Items.Remove(fItem);
        //                }
        //            }
        //        }

        //        ((DropDownList)e.Item.FindControl("ddlType")).SelectedValue = drv["AddressType"].ToString();
        //        ((TextBox)e.Item.FindControl("txtInternalAddress")).Text = drv["InternalAddress"].ToString();
        //        ((TextBox)e.Item.FindControl("txtStrAddress1")).Text = drv["StrAddress1"].ToString();
        //        ((TextBox)e.Item.FindControl("txtStrAddress2")).Text = drv["StrAddress2"].ToString();
        //        ((TextBox)e.Item.FindControl("txtCity")).Text = drv["City"].ToString();
        //        ((TextBox)e.Item.FindControl("txtState")).Text = drv["State"].ToString();
        //        ((TextBox)e.Item.FindControl("txtZip")).Text = drv["Zip"].ToString();
        //        ((TextBox)e.Item.FindControl("txtCountry")).Text = drv["Country"].ToString();
        //    }
        //    else if (e.Item.ItemType == ListItemType.Footer)
        //    {
        //        // show only unused items
        //        if (dgAddress.ShowFooter)
        //        {
        //            if (dgAddress.Items.Count == addressTypes.Count)
        //                dgAddress.ShowFooter = false;
        //            else
        //            {
        //                DropDownList ddlType = (DropDownList)e.Item.FindControl("ddlTypeF");

        //                foreach (DataGridItem dgi in dgAddress.Items)
        //                {
        //                    foreach (DictionaryEntry addrType in addressTypes)
        //                    {
        //                        string columnName = addrType.Key.ToString();
        //                        string label = addrType.Value.ToString();
        //                        Label lblType = dgi.FindControl("lblType") as Label;
        //                        if (lblType != null && lblType.Text == label)
        //                        {
        //                            ListItem fItem = ddlType.Items.FindByValue(columnName);
        //                            ddlType.Items.Remove(fItem);
        //                            break;
        //                        }
        //                    }
        //                }

        //                if (ddlType.Items.Count > 0)
        //                {
        //                    ddlType.SelectedIndex = 0;
        //                    PopulateAddressWithDefault(ddlType.SelectedValue, e.Item);
        //                }
        //                else
        //                    ((TextBox)e.Item.FindControl("txtCountryF")).Text = "US";
        //            }
        //        }
        //    }
        //    else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
        //    {
        //        ((Label)e.Item.FindControl("lblType")).Text = addressTypes[drv["AddressType"]].ToString();
        //        ((Label)e.Item.FindControl("lblInternalAddress")).Text = drv["InternalAddress"].ToString();
        //        ((Label)e.Item.FindControl("lblStrAddress1")).Text = drv["StrAddress1"].ToString();
        //        ((Label)e.Item.FindControl("lblStrAddress2")).Text = drv["StrAddress2"].ToString();
        //        ((Label)e.Item.FindControl("lblCity")).Text = drv["City"].ToString();
        //        ((Label)e.Item.FindControl("lblState")).Text = drv["State"].ToString();
        //        ((Label)e.Item.FindControl("lblZip")).Text = drv["Zip"].ToString();
        //        ((Label)e.Item.FindControl("lblCountry")).Text = drv["Country"].ToString();
        //    }
        //}

        private void PopulateAddressWithDefault(string type, DataGridItem dgi)
        {
            DataRow drOrg = dsAccount.Tables["Org"].Rows.Find(Session["OrgID"]);
            int AddressID = Convert.ToInt32(drOrg["Def" + type]);
            if (AddressID != 0)
            {
                DataRow drAddr = dsAccount.Tables["Address"].Rows.Find(AddressID);

                ((TextBox)dgi.FindControl("txtInternalAddressF")).Text = drAddr["InternalAddress"].ToString();
                ((TextBox)dgi.FindControl("txtStrAddress1F")).Text = drAddr["StrAddress1"].ToString();
                ((TextBox)dgi.FindControl("txtStrAddress2F")).Text = drAddr["StrAddress2"].ToString();
                ((TextBox)dgi.FindControl("txtCityF")).Text = drAddr["City"].ToString();
                ((TextBox)dgi.FindControl("txtStateF")).Text = drAddr["State"].ToString();
                ((TextBox)dgi.FindControl("txtZipF")).Text = drAddr["Zip"].ToString();
                ((TextBox)dgi.FindControl("txtCountryF")).Text = drAddr["Country"].ToString();
            }
        }

        protected void dgAccountManager_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            // check if client manager relationship exists
            bool isNewEntry = false;

            switch (e.CommandName)
            {
                case "AddNew":
                    DropDownList ddlMgr = (DropDownList)e.Item.FindControl("ddlMgr");
                    if (ddlMgr.SelectedItem != null)
                    {
                        DataRow dr;
                        DataRow[] cadr = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0} AND ClientOrgID = {1}", AccountID, ddlMgr.SelectedValue));

                        if (cadr.Length == 1) // relationship already exists
                            dr = cadr[0];
                        else
                        {
                            isNewEntry = true;
                            dr = dsAccount.Tables["ClientAccount"].NewRow();
                            dr["AccountID"] = AccountID;
                            dr["ClientOrgID"] = ddlMgr.SelectedValue;
                        }
                        dr["Active"] = true;
                        dr["Manager"] = true;

                        if (isNewEntry)
                            dsAccount.Tables["ClientAccount"].Rows.Add(dr);

                        CacheManager.Current.CacheData(dsAccount);

                        SetPageControlsAndBind(true, false, false);
                    }
                    break;
                case "Delete":
                    // remove access for manager's clients unless another manager has client and account
                    int ManagerOrgID = Convert.ToInt32(dgAccountManager.DataKeys[e.Item.ItemIndex]);

                    DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0} AND ClientOrgID = {1}", AccountID, ManagerOrgID));
                    if (cadrs[0].RowState == DataRowState.Added)
                        cadrs[0].Delete();
                    else
                    {
                        cadrs[0]["Manager"] = false;

                        bool disableAcct; // need to do this because of default account
                        DataRow[] madrs;
                        DataRow[] cmdrs = dsAccount.Tables["ClientManager"].Select(string.Format("ManagerOrgID = {0} AND Active = 1", ManagerOrgID));

                        for (int i = 0; i < cmdrs.Length; i++) // for all clients serviced by this manager
                        {
                            cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0} AND ClientOrgID = {1}", AccountID, cmdrs[i]["ClientOrgID"]));
                            if (cadrs.Length == 1) // if the client has access to this account
                            {
                                disableAcct = true;
                                DataRow[] cmdrs2 = dsAccount.Tables["ClientManager"].Select(string.Format("ClientOrgID = {0} AND Active = 1", cmdrs[i]["ClientOrgID"]));
                                for (int j = 0; j < cmdrs2.Length; j++) // for all of the client's managers
                                {
                                    madrs = dsAccount.Tables["ClientAccount"].Select(string.Format("Active = 1 AND Manager = 1 AND AccountID = {0} AND ClientOrgID = {1}", AccountID, cmdrs2[j]["ManagerOrgID"]));
                                    if (madrs.Length == 1) // another of this clients manager is managing this account
                                        disableAcct = false;
                                }
                                if (disableAcct)
                                    DataUtility.SetActiveFalse(cadrs[0]);
                            }
                        }
                    }

                    CacheManager.Current.CacheData(dsAccount);

                    SetPageControlsAndBind(true, false, false);
                    break;
            }
        }

        protected void dgAccountManager_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;

            if (e.Item.ItemType == ListItemType.Footer)
            {
                // populate ddlMgr
                DropDownList ddlMgr = (DropDownList)e.Item.FindControl("ddlMgr");
                DataView dv = dsAccount.Tables["ManagerOrg"].DefaultView;

                dv.Sort = "DisplayName";
                ddlMgr.DataSource = dv;
                ddlMgr.DataTextField = "DisplayName";
                ddlMgr.DataValueField = "ClientOrgID";
                ddlMgr.DataBind();

                // remove managers from ddl that have already been selected
                DataRow dr;
                for (int i = 0; i < dgAccountManager.DataKeys.Count; i++)
                {
                    dr = dsAccount.Tables["ManagerOrg"].Rows.Find(dgAccountManager.DataKeys[i]);
                    if (dr != null)
                        ddlMgr.Items.Remove(ddlMgr.Items.FindByValue(dr["ClientOrgID"].ToString()));
                }
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                // instead of adding columns, just get the needed value

                int coID = Convert.ToInt32(drv["ClientOrgID"]);
                DataRow dr = dsAccount.Tables["ManagerOrg"].Rows.Find(coID);
                Label lblMgr = (Label)e.Item.FindControl("lblMgr");
                if (dr != null)
                    lblMgr.Text = dr["DisplayName"].ToString();
                else
                {
                    DataUtility.ClientInfo client = DataUtility.GetClientInfo(coID);
                    lblMgr.Text = client.DisplayName + @"<div style=""color: #FF0000; font-weight: bold;"">[Client is not a Technical Manager!]</div>";
                }
            }
        }

        private bool ValidateField(AlertInfo[] strVal)
        {
            string strAlert = string.Empty;
            for (int i = 0; i < strVal.Length; i++)
            {
                if (strAlert.Length > 0)
                    strAlert += "\\n";

                if (strVal[i].Data.Length == 0)
                    strAlert += "Please specify " + strVal[i].Field;

                if (strVal[i].RequiredLength != 0)
                {
                    if (strVal[i].RequiredLength != strVal[i].Data.Length)
                        strAlert += string.Format("Data length for " + strVal[i].Field + " is incorrect. The required length is {1} characters.", strVal[i].Field, strVal[i].RequiredLength);
                    else
                    {
                        if (strVal[i].RequiredType == RequiredDataType.NumberOnly)
                        {
                            if (!Regex.IsMatch(strVal[i].Data, @"^\d{" + strVal[i].RequiredLength.ToString() + "}$"))
                                strAlert += "Only numeric characters are allowed for " + strVal[i].Field;
                        }
                        else if (strVal[i].RequiredType == RequiredDataType.AlphaNumeric)
                        {
                            if (!Regex.IsMatch(strVal[i].Data, @"^[0-9a-zA-Z]{" + strVal[i].RequiredLength.ToString() + "}$"))
                                strAlert += "Only numeric characters are allowed for " + strVal[i].Field;
                        }
                    }
                }
            }

            if (strAlert.Length > 0)
            {
                string strScript = @"<script type=""text/javascript"" id=""showWarning""> ";
                strScript += string.Format(@"alert(""{0}"");", strAlert);
                strScript += @"</script>";

                if (!Page.ClientScript.IsStartupScriptRegistered("showWarning"))
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "showWarning", strScript);

                return false;
            }
            else
                return true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            // update the address table - use handler to update ClientOrg table
            SqlDataAdapter daAddress = new SqlDataAdapter();
            daAddress.RowUpdating += daAddress_RowUpdating;
            daAddress.RowUpdated += daAddress_RowUpdated;

            daAddress.InsertCommand = new SqlCommand("Address_Insert", cnSselData);
            daAddress.InsertCommand.CommandType = CommandType.StoredProcedure;
            daAddress.InsertCommand.Parameters.Add("@InternalAddress", SqlDbType.NVarChar, 45, "InternalAddress");
            daAddress.InsertCommand.Parameters.Add("@StrAddress1", SqlDbType.NVarChar, 45, "StrAddress1");
            daAddress.InsertCommand.Parameters.Add("@StrAddress2", SqlDbType.NVarChar, 45, "StrAddress2");
            daAddress.InsertCommand.Parameters.Add("@City", SqlDbType.NVarChar, 35, "City");
            daAddress.InsertCommand.Parameters.Add("@State", SqlDbType.NVarChar, 2, "State");
            daAddress.InsertCommand.Parameters.Add("@Zip", SqlDbType.NVarChar, 10, "Zip");
            daAddress.InsertCommand.Parameters.Add("@Country", SqlDbType.NVarChar, 50, "Country");

            daAddress.UpdateCommand = new SqlCommand("Address_Update", cnSselData);
            daAddress.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daAddress.UpdateCommand.Parameters.Add("@AddressID", SqlDbType.Int, 4, "AddressID");
            daAddress.UpdateCommand.Parameters.Add("@InternalAddress", SqlDbType.NVarChar, 45, "InternalAddress");
            daAddress.UpdateCommand.Parameters.Add("@StrAddress1", SqlDbType.NVarChar, 45, "StrAddress1");
            daAddress.UpdateCommand.Parameters.Add("@StrAddress2", SqlDbType.NVarChar, 45, "StrAddress2");
            daAddress.UpdateCommand.Parameters.Add("@City", SqlDbType.NVarChar, 35, "City");
            daAddress.UpdateCommand.Parameters.Add("@State", SqlDbType.NVarChar, 2, "State");
            daAddress.UpdateCommand.Parameters.Add("@Zip", SqlDbType.NVarChar, 10, "Zip");
            daAddress.UpdateCommand.Parameters.Add("@Country", SqlDbType.NVarChar, 50, "Country");

            daAddress.DeleteCommand = new SqlCommand("Address_Delete", cnSselData);
            daAddress.DeleteCommand.CommandType = CommandType.StoredProcedure;
            daAddress.DeleteCommand.Parameters.Add("@AddressID", SqlDbType.Int, 4, "AddressID");

            daAddress.Update(dsAccount, "Address");

            // update the Account table - use handler to update ClientOrg table
            SqlDataAdapter daClient = new SqlDataAdapter();
            daClient.RowUpdating += daClient_RowUpdating;
            daClient.RowUpdated += daClient_RowUpdated;

            daClient.InsertCommand = new SqlCommand("Account_Insert", cnSselData);
            daClient.InsertCommand.CommandType = CommandType.StoredProcedure;
            daClient.InsertCommand.Parameters.Add("@OrgID", SqlDbType.Int, 4, "OrgID");
            daClient.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50, "Name");
            daClient.InsertCommand.Parameters.Add("@Number", SqlDbType.NVarChar, 50, "Number");
            daClient.InsertCommand.Parameters.Add("@ShortCode", SqlDbType.NChar, 6, "ShortCode");
            daClient.InsertCommand.Parameters.Add("@FundingSourceID", SqlDbType.Int, 4, "FundingSourceID");
            daClient.InsertCommand.Parameters.Add("@TechnicalFieldID", SqlDbType.Int, 4, "TechnicalFieldID");
            daClient.InsertCommand.Parameters.Add("@SpecialTopicID", SqlDbType.Int, 4, "SpecialTopicID");
            daClient.InsertCommand.Parameters.Add("@AccountTypeID", SqlDbType.Int, 4, "AccountTypeID");
            daClient.InsertCommand.Parameters.Add("@BillAddressID", SqlDbType.Int, 4, "BillAddressID");
            daClient.InsertCommand.Parameters.Add("@ShipAddressID", SqlDbType.Int, 4, "ShipAddressID");
            daClient.InsertCommand.Parameters.Add("@InvoiceNumber", SqlDbType.NVarChar, 50, "InvoiceNumber");
            daClient.InsertCommand.Parameters.Add("@InvoiceLine1", SqlDbType.NVarChar, 50, "InvoiceLine1");
            daClient.InsertCommand.Parameters.Add("@InvoiceLine2", SqlDbType.NVarChar, 50, "InvoiceLine2");
            daClient.InsertCommand.Parameters.Add("@PoEndDate", SqlDbType.DateTime, 8, "PoEndDate");
            daClient.InsertCommand.Parameters.Add("@PoInitialFunds", SqlDbType.Money, 8, "PoInitialFunds");
            daClient.InsertCommand.Parameters.Add("@PoRemainingFunds", SqlDbType.Money, 8, "PoRemainingFunds");
            daClient.InsertCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            daClient.UpdateCommand = new SqlCommand("Account_Update", cnSselData);
            daClient.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClient.UpdateCommand.Parameters.Add("@AccountID", SqlDbType.Int, 4, "AccountID");
            daClient.UpdateCommand.Parameters.Add("@OrgID", SqlDbType.Int, 4, "OrgID");
            daClient.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50, "Name");
            daClient.UpdateCommand.Parameters.Add("@Number", SqlDbType.NVarChar, 50, "Number");
            daClient.UpdateCommand.Parameters.Add("@ShortCode", SqlDbType.NChar, 6, "ShortCode");
            daClient.UpdateCommand.Parameters.Add("@FundingSourceID", SqlDbType.Int, 4, "FundingSourceID");
            daClient.UpdateCommand.Parameters.Add("@TechnicalFieldID", SqlDbType.Int, 4, "TechnicalFieldID");
            daClient.UpdateCommand.Parameters.Add("@SpecialTopicID", SqlDbType.Int, 4, "SpecialTopicID");
            daClient.UpdateCommand.Parameters.Add("@AccountTypeID", SqlDbType.Int, 4, "AccountTypeID");
            daClient.UpdateCommand.Parameters.Add("@BillAddressID", SqlDbType.Int, 4, "BillAddressID");
            daClient.UpdateCommand.Parameters.Add("@ShipAddressID", SqlDbType.Int, 4, "ShipAddressID");
            daClient.UpdateCommand.Parameters.Add("@InvoiceNumber", SqlDbType.NVarChar, 50, "InvoiceNumber");
            daClient.UpdateCommand.Parameters.Add("@InvoiceLine1", SqlDbType.NVarChar, 50, "InvoiceLine1");
            daClient.UpdateCommand.Parameters.Add("@InvoiceLine2", SqlDbType.NVarChar, 50, "InvoiceLine2");
            daClient.UpdateCommand.Parameters.Add("@PoEndDate", SqlDbType.DateTime, 8, "PoEndDate");
            daClient.UpdateCommand.Parameters.Add("@PoInitialFunds", SqlDbType.Money, 8, "PoInitialFunds");
            daClient.UpdateCommand.Parameters.Add("@PoRemainingFunds", SqlDbType.Money, 8, "PoRemainingFunds");
            daClient.UpdateCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            daClient.Update(dsAccount, "Account");

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

            daClientAccount.Update(dsAccount, "ClientAccount");

            // this is used to turn on/off card access to the lab - for existing clients only

            SqlDataAdapter daClientAccess = new SqlDataAdapter();

            daClientAccess.UpdateCommand = new SqlCommand("Client_AuxDBUpdate", cnSselData);
            daClientAccess.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClientAccess.UpdateCommand.Parameters.Add("@ClientID", SqlDbType.Int, 4, "ClientID");
            daClientAccess.UpdateCommand.Parameters.Add("@EnableAccess", SqlDbType.Bit, 1, "EnableAccess");

            daClientAccess.Update(dsAccount, "Client");

            btnDiscard_Click(sender, e);
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
                DataRow[] fdr = dsAccount.Tables["Account"].Select(string.Format("{0} = {1}", e.Row["AddressType"], oldAddressID));
                string addressType = e.Row["AddressType"].ToString();
                fdr[0][addressType] = e.Row["AddressID"];
            }
        }

        private void daClient_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
                oldAccountID = Convert.ToInt32(e.Row["AccountID"]);
        }

        private void daClient_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                // update AccountOrg
                DataRow[] fdr = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0}", oldAccountID));
                for (int i = 0; i < fdr.Length; i++)
                    fdr[i]["AccountID"] = e.Row["AccountID"];
            }
        }

        protected void btnDiscard_Click(object sender, EventArgs e)
        {
            CacheManager.Current.RemoveCacheData(); // remove anything left in cache
            Response.Redirect("~");
        }

        private bool OrgIsInternal()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgID"]);
                var org = DA.Current.Single<repo.Org>(orgId);
                return org.IsInternal();
            }
            catch
            {
                return false;
            }
        }

        protected void AddressManager1_UpdateAddress(object sender, UpdateAddressEventArgs e)
        {
            DataRow dr = dsAccount.Tables["Address"].Rows.Find(e.Item.AddressID);

            dr["AddressType"] = e.Item.Type.Column;
            dr["InternalAddress"] = e.Item.AttentionLine;
            dr["StrAddress1"] = e.Item.StreetAddressLine1;
            dr["StrAddress2"] = e.Item.StreetAddressLine2;
            dr["City"] = e.Item.City;
            dr["State"] = e.Item.State;
            dr["Zip"] = e.Item.Zip;
            dr["Country"] = e.Item.Country;

            if (AccountID > 0)
            {
                DataRow acct = dsAccount.Tables["Account"].Rows.Find(AccountID);
                acct[e.Item.Type.Column] = e.Item.AddressID;
            }

            CacheManager.Current.CacheData(dsAccount);

            SetPageControlsAndBind(true, false, false);
        }

        protected void AddressManager1_CreateAddress(object sender, CreateAddressEventArgs e)
        {
            var ndr = dsAccount.Tables["Address"].NewRow();

            ndr["AddressType"] = e.NewItem.Type.Column;
            ndr["InternalAddress"] = e.NewItem.AttentionLine;
            ndr["StrAddress1"] = e.NewItem.StreetAddressLine1;
            ndr["StrAddress2"] = e.NewItem.StreetAddressLine2;
            ndr["City"] = e.NewItem.City;
            ndr["State"] = e.NewItem.State;
            ndr["Zip"] = e.NewItem.Zip;
            ndr["Country"] = e.NewItem.Country;

            dsAccount.Tables["Address"].Rows.Add(ndr);

            if (AccountID > 0)
            {
                DataRow dr = dsAccount.Tables["Account"].Rows.Find(AccountID);
                dr[e.NewItem.Type.Column] = ndr["AddressID"];
            }

            CacheManager.Current.CacheData(dsAccount);

            SetPageControlsAndBind(true, false, false);
        }

        protected void AddressManager1_EditAddress(object sender, EditAddressEventArgs e)
        {
            // Event that fires when an address edit is started. Nothing needs to be done here accept load all the addresses (and other controls)
            SetPageControlsAndBind(true, false, false);
        }

        protected void AddressManager1_DeleteAddress(object sender, EditAddressEventArgs e)
        {
            var dr = dsAccount.Tables["Address"].Rows.Find(e.AddressID);
            if (dr != null) dr.Delete();
            SetPageControlsAndBind(true, false, false);
        }
    }
}
using LNF.Data;
using LNF.Web;
using LNF.Web.Content;
using sselData.AppCode;
using sselData.Controls;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using repo = LNF.Impl.Repository.Data;

namespace sselData
{
    public partial class Account : OnlineServicesPage
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
            get { return Session["EditAccountID"] == null ? 0 : (int)Session["EditAccountID"]; }
            set { Session["EditAccountID"] = value; }
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
                dsAccount = ContextBase.GetCacheData();

                if (dsAccount == null)
                    Response.Redirect("~");
                else if (dsAccount.DataSetName != "Account")
                    Response.Redirect("~");
            }
            else
            {
                ContextBase.RemoveCacheData(); // remove anything left in cache

                AccountID = 0;

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

                        var dtClient = dsAccount.Tables["Client"];
                        dtClient.PrimaryKey = new[] { dtClient.Columns["ClientID"] };
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
                    }

                    //Declare default sort parameter and sort direction
                    ViewState["dgAccountSortCol"] = "Name";
                    ViewState["dgAccountSortDir"] = " ASC";

                    ContextBase.SetCacheData(dsAccount);

                    SetPageControlsAndBind(false, false, false);

                    cnSselData.Close();
                }
            }
        }

        private void SetPageControlsAndBind(bool showAddEditPanel, bool showAddExistingPanel, bool inlineAddressEdit)
        {
            if (inlineAddressEdit)
                throw new Exception("This feature is not supported yet.");

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
                AccountDataGrid.DataSource = dv;
                AccountDataGrid.DataBind();

                if (OrgIsInternal()) // yuck - what if the number of columns change?
                {
                    AccountDataGrid.Columns[2].Visible = true;
                    AccountDataGrid.Columns[3].Visible = true;
                }
                else
                {
                    AccountDataGrid.Columns[2].Visible = false;
                    AccountDataGrid.Columns[3].Visible = false;
                }

                // now, add to ddlPager for paging
                PagerDropDownList.Items.Clear();
                int pSize = AccountDataGrid.PageSize;
                for (int i = 0; i < dv.Count; i += pSize)
                {
                    ListItem pagerItem = new ListItem { Value = (i / pSize).ToString() };
                    int index = (i + (pSize - 1) >= dv.Count) ? dv.Count - 1 : i + (pSize - 1);
                    pagerItem.Text = dv[i].Row[ViewState["dgAccountSortCol"].ToString()].ToString() + " ... " + dv[index].Row[ViewState["dgAccountSortCol"].ToString()].ToString();
                    PagerDropDownList.Items.Add(pagerItem);
                }
                PagerDropDownList.SelectedValue = AccountDataGrid.CurrentPageIndex.ToString();
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

                //var addressItems = GetAddressManagerDataSource();

                // turn off dgAddress control when editing an address
                //if (inlineAddressEdit || addressItems.Count == AddressManager1.AddressTypes.Count)
                //    AddressManager1.ShowFooter = false;
                //else
                //    AddressManager1.ShowFooter = true;

                //AddressManager1.Fill(addressItems);

                // filter the managers
                DataView dvm = dsAccount.Tables["ClientAccount"].DefaultView;
                dvm.RowFilter = $"Active = 1 AND Manager = 1 AND AccountID = {AccountID}";
                AccountManagerDataGrid.DataSource = dvm;
                AccountManagerDataGrid.DataBind();
            }
            else if (pExisting.Visible)
                lblHeader.Text = "Add existing Account to " + dr["OrgName"].ToString();
        }

        protected void AccountDataGrid_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            DataRow dr;

            switch (e.CommandName)
            {
                case "AddNew":
                    if (AccountID == 0)
                    {
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

                        var newAccountRow = dsAccount.Tables["Account"].NewRow();
                        newAccountRow.SetField("OrgID", Convert.ToInt32(Session["OrgID"]));
                        newAccountRow.SetField("Name", "");
                        newAccountRow.SetField("AccountTypeID", 1);
                        newAccountRow.SetField("Number", "");
                        newAccountRow.SetField("ShortCode", "");
                        newAccountRow.SetField("FundingSourceID", 1);
                        newAccountRow.SetField("TechnicalFieldID", 1);
                        newAccountRow.SetField("SpecialTopicID", 1);
                        newAccountRow.SetField("BillAddressID", 0);
                        newAccountRow.SetField("ShipAddressID", 0);
                        newAccountRow.SetField("InvoiceNumber", "");
                        newAccountRow.SetField("InvoiceLine1", "");
                        newAccountRow.SetField("InvoiceLine2", "");
                        newAccountRow.SetField("PoEndDate", DBNull.Value);
                        newAccountRow.SetField("PoInitialFunds", DBNull.Value);
                        newAccountRow.SetField("PoRemainingFunds", DBNull.Value);
                        newAccountRow.SetField("Active", true);
                        dsAccount.Tables["Account"].Rows.Add(newAccountRow);

                        // remove any added rows
                        // if we are adding mulitple accounts before the final save, we don't want to remove addresses that were added.
                        //var rows = dsAccount.Tables["Address"].AsEnumerable().Where(x => x.RowState == DataRowState.Added).ToArray();
                        //foreach (DataRow drAddr in rows)
                        //{
                        //    drAddr.RejectChanges();
                        //}

                        // if org has DefBillAddr or DefShipAddr, add row to addr table with this info
                        DataRow drOrg = dsAccount.Tables["Org"].Rows.Find(Session["OrgID"]);
                        string[] addressTypes = { "BillAddressID", "ShipAddressID" };

                        foreach (var addrType in addressTypes)
                        {
                            string defaultColumn = string.Format("Def{0}", addrType);

                            if (drOrg.Field<int>(defaultColumn) > 0)
                            {
                                var newAddressRow = dsAccount.Tables["Address"].NewRow();
                                var defaultAddress = dsAccount.Tables["Address"].Rows.Find(drOrg.Field<int>(defaultColumn));
                                if (defaultAddress != null)
                                {
                                    newAddressRow.SetField("InternalAddress", defaultAddress.Field<string>("InternalAddress"));
                                    newAddressRow.SetField("StrAddress1", defaultAddress.Field<string>("StrAddress1"));
                                    newAddressRow.SetField("StrAddress2", defaultAddress.Field<string>("StrAddress2"));
                                    newAddressRow.SetField("City", defaultAddress.Field<string>("City"));
                                    newAddressRow.SetField("State", defaultAddress.Field<string>("State"));
                                    newAddressRow.SetField("Zip", defaultAddress.Field<string>("Zip"));
                                    newAddressRow.SetField("Country", defaultAddress.Field<string>("Country"));
                                    newAddressRow.SetField("AddressType", addrType);
                                    dsAccount.Tables["Address"].Rows.Add(newAddressRow);
                                    newAccountRow.SetField(addrType, newAddressRow.Field<int>("AddressID"));
                                }
                            }
                        }

                        AccountID = newAccountRow.Field<int>("AccountID");
                        AccountStoreButton.Text = "Store New Account";

                        ContextBase.SetCacheData(dsAccount);
                    }

                    SetPageControlsAndBind(true, false, false);
                    break;
                case "Edit":
                    int accountId = Convert.ToInt32(AccountDataGrid.DataKeys[e.Item.ItemIndex]);
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
                        txtPoInitialFunds.Text = WebUtility.FillField<decimal>(dr["PoInitialFunds"], 0, "{0:0.00}");
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
                    AccountStoreButton.Text = "Store Modified Data";

                    ContextBase.SetCacheData(dsAccount);

                    SetPageControlsAndBind(true, false, false);
                    break;
                case "AddExisting":
                    AccountDropDownList.Enabled = true;
                    SetAccountDDL();
                    SetPageControlsAndBind(false, true, false);
                    break;
                case "Delete":
                    accountId = Convert.ToInt32(AccountDataGrid.DataKeys[e.Item.ItemIndex]);
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

                    ContextBase.SetCacheData(dsAccount);

                    SetPageControlsAndBind(false, false, false);
                    break;
            }
        }

        protected void AccountDataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
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
                int AccountID = Convert.ToInt32(AccountDataGrid.DataKeys[e.Item.ItemIndex]);
                DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select($"Active = 1 AND AccountID = {AccountID}");
                if (cadrs.Length > 0)
                {
                    foreach (DataRow dr in cadrs)
                    {
                        DataRow[] OneAccountUser = dsAccount.Tables["OneClientAccount"].Select($"ClientOrgID = {dr["ClientOrgID"]}");
                        if (OneAccountUser.Length >= 1)
                        {
                            string script = $"return confirm('There are still {OneAccountUser.Length} client(s) associated ONLY with this account, are you sure you want to delete this account?');";
                            ImageButton lbtn = (ImageButton)e.Item.FindControl("btnDelete");
                            lbtn.OnClientClick = script;
                        }
                    }
                }
            }
        }

        protected void PagerDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            AccountDataGrid.CurrentPageIndex = Convert.ToInt32(PagerDropDownList.SelectedValue); // selectedItem should also work
            SetPageControlsAndBind(false, false, false);
        }

        protected void AccountDataGrid_SortCommand(object source, DataGridSortCommandEventArgs e)
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

        protected void AcctDisplayRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAccountDDL();
        }

        private void SetAccountDDL()
        {
            DataView dv = dsAccount.Tables["Account"].DefaultView;
            dv.Sort = AcctDisplayRadioButtonList.SelectedValue + " ASC";
            dv.RowFilter = "Active = 0";

            AccountDropDownList.DataSource = dv;
            AccountDropDownList.DataTextField = AcctDisplayRadioButtonList.SelectedValue;
            AccountDropDownList.DataValueField = "AccountID";
            AccountDropDownList.DataBind();

            // now, go through and remove items that do not meet the filters
            for (int i = AccountDropDownList.Items.Count - 1; i >= 0; i--)
            {
                // only match specified text
                if (txtSearch.Text.Trim().Length > 0)
                    if (!AccountDropDownList.Items[i].Text.ToLower().Contains(txtSearch.Text.Trim().ToLower()))
                        AccountDropDownList.Items.Remove(AccountDropDownList.Items[i]);
            }

            // now add a blank item so SelectedIndexChnaged works for the first name in the list
            ListItem blankItem = new ListItem { Value = "0", Text = string.Empty };
            AccountDropDownList.Items.Insert(0, blankItem);
        }

        protected void AccountDropDownList_PreRender(object sender, EventArgs e)
        {
            if (AcctDisplayRadioButtonList.SelectedValue == "Number")
            {
                foreach (ListItem li in AccountDropDownList.Items)
                {
                    if (li.Text.Length > 0)
                        li.Text = li.Text.Substring(0, 6) + "-" + li.Text.Substring(6, 5) + "-" + li.Text.Substring(11, 6) + "-" + li.Text.Substring(17, 5) + "-" + li.Text.Substring(22, 5) + "-" + li.Text.Substring(27, 7);
                }
            }
        }

        protected void AccountReenableButton_Click(object sender, EventArgs e)
        {
            DataRow adr = dsAccount.Tables["Account"].Rows.Find(AccountDropDownList.SelectedValue);
            if (adr.RowState == DataRowState.Modified) // must have just been disabled
            {
                adr.RejectChanges();
                DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0}", AccountDropDownList.SelectedValue));
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

            ContextBase.SetCacheData(dsAccount);

            SetPageControlsAndBind(false, false, false);
        }

        protected void AccountReenableQuitButton_Click(object sender, EventArgs e)
        {
            SetPageControlsAndBind(false, false, false);
        }

        protected void ReactivateByShortCodeLinkButton_Click(object sender, EventArgs e)
        {
            ReactivateByShortCode(txtShortCode.Text.Trim());
        }

        private void ReactivateByShortCode(string sc)
        {
            string shortcodeVal = sc;

            txtSearch.Text = shortcodeVal;
            AcctDisplayRadioButtonList.SelectedValue = "ShortCode";

            AccountDropDownList.Enabled = true;

            //load the ddlAccount with data
            SetAccountDDL();

            if (AccountDropDownList.Items.Count == 1)
                AccountDropDownList.SelectedIndex = 0;
            else if (AccountDropDownList.Items.Count > 1)
                AccountDropDownList.SelectedIndex = 1;

            pIntAccount.Visible = false;
            SetPageControlsAndBind(false, true, false);
        }

        protected void AccountStoreButton_Click(object sender, EventArgs e)
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

                DataRow accountRow;

                if (isNewEntry)
                {
                    // add an entry to the client table
                    accountRow = dsAccount.Tables["Account"].NewRow();
                    accountRow.SetField("OrgID", Convert.ToInt32(Session["OrgID"]));
                    accountRow.SetField("BillAddressID", 0);
                    accountRow.SetField("ShipAddressID", 0);

                    if (pIntAccount.Visible)
                        accountRow.SetField("Project", txtProject.Text.Trim());
                    else
                        accountRow.SetField("Project", string.Empty);
                }
                else
                {
                    // get the entry in the client table
                    accountRow = dsAccount.Tables["Account"].Rows.Find(AccountID);
                }

                // if entering new or modifying, update the fields
                if (pIntAccount.Visible)
                {
                    accountRow.SetField("Number", txtAccount.Text.Trim() + txtFund.Text.Trim() + txtDepartment.Text.Trim() + txtProgram.Text.Trim() + txtClass.Text.Trim() + txtProject.Text.Trim());
                    accountRow.SetField("ShortCode", txtShortCode.Text.Trim());
                }
                else
                {
                    if (!DateTime.TryParse(txtPoEndDate.Text.Trim(), out DateTime endDate))
                        endDate = DateTime.Now.AddYears(5);

                    if (!double.TryParse(txtPoInitialFunds.Text.Trim(), out double funds))
                        funds = 0.0;

                    accountRow.SetField("ShortCode", string.Empty);
                    accountRow.SetField("Number", "999999EX" + txtNumber.Text.Trim());
                    accountRow.SetField("InvoiceNumber", txtInvoiceNumber.Text.Trim());
                    accountRow.SetField("InvoiceLine1", txtInvoiceLine1.Text.Trim());
                    accountRow.SetField("InvoiceLine2", txtInvoiceLine2.Text.Trim());
                    accountRow.SetField("PoEndDate", endDate);
                    accountRow.SetField("PoInitialFunds", funds);
                    accountRow.SetField("PoRemainingFunds", accountRow.Field<decimal>("PoInitialFunds"));
                }

                accountRow.SetField("Name", txtName.Text.Trim());
                accountRow.SetField("AccountTypeID", cklistAccountType.SelectedValue);
                accountRow.SetField("FundingSourceID", ddlFundingSource.SelectedValue);
                accountRow.SetField("TechnicalFieldID", ddlTechnicalField.SelectedValue);
                accountRow.SetField("SpecialTopicID", ddlSpecialTopic.SelectedValue);
                accountRow.SetField("Active", true);

                // Handle new addresses. We need to do this here because if default addresses were added they must now be linked to the account.
                var newAddresses = dsAccount.Tables["Address"].AsEnumerable().Where(x => x.RowState == DataRowState.Added);
                foreach (var addr in newAddresses)
                {
                    var addressType = addr.Field<string>("AddressType");
                    accountRow.SetField(addressType, addr.Field<int>("AddressID"));
                }

                // update rows in ClientAccount as needed
                var cmdr = dsAccount.Tables["ClientAccount"].Select("AccountID = 0");
                foreach (var dr in cmdr)
                    dr.SetField("AccountID", accountRow.Field<int>("AccountID"));

                if (isNewEntry)
                    dsAccount.Tables["Account"].Rows.Add(accountRow);

                ContextBase.SetCacheData(dsAccount);

                SetPageControlsAndBind(false, false, false);
            }
        }

        protected void AccountStoreQuitButton_Click(object sender, EventArgs e)
        {
            // undo all changes
            var accountRow = dsAccount.Tables["Account"].Select($"AccountID = {AccountID}").FirstOrDefault();

            if (accountRow != null)
            {
                DataRow addressRow;

                addressRow = dsAccount.Tables["Address"].Select($"AddressID = {accountRow["BillAddressID"]}").FirstOrDefault();
                if (addressRow != null) addressRow.RejectChanges();
                addressRow = dsAccount.Tables["Address"].Select($"AddressID = {accountRow["ShipAddressID"]}").FirstOrDefault();
                if (addressRow != null) addressRow.RejectChanges();

                var clientAccountRows = dsAccount.Tables["ClientAccount"].Select($"AccountID = {AccountID}");
                foreach (var dr in clientAccountRows)
                    dr.RejectChanges();

                accountRow.RejectChanges();

                ContextBase.SetCacheData(dsAccount);
            }

            AccountID = 0;

            SetPageControlsAndBind(false, false, false);
        }

        protected void AccountManagerDataGrid_ItemCommand(object source, DataGridCommandEventArgs e)
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

                        ContextBase.SetCacheData(dsAccount);

                        SetPageControlsAndBind(true, false, false);
                    }
                    break;
                case "Delete":
                    // remove access for manager's clients unless another manager has client and account
                    int managerOrgId = Convert.ToInt32(AccountManagerDataGrid.DataKeys[e.Item.ItemIndex]);

                    DataRow[] cadrs = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0} AND ClientOrgID = {1}", AccountID, managerOrgId));
                    if (cadrs[0].RowState == DataRowState.Added)
                        cadrs[0].Delete();
                    else
                    {
                        cadrs[0]["Manager"] = false;

                        bool disableAcct; // need to do this because of default account
                        DataRow[] madrs;
                        DataRow[] cmdrs = dsAccount.Tables["ClientManager"].Select(string.Format("ManagerOrgID = {0} AND Active = 1", managerOrgId));

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

                    ContextBase.SetCacheData(dsAccount);

                    SetPageControlsAndBind(true, false, false);
                    break;
            }
        }

        protected void AccountManagerDataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
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
                for (int i = 0; i < AccountManagerDataGrid.DataKeys.Count; i++)
                {
                    dr = dsAccount.Tables["ManagerOrg"].Rows.Find(AccountManagerDataGrid.DataKeys[i]);
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

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            HandleSave();
            DiscardButton_Click(sender, e);
        }

        protected void HandleSave()
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            // update the address table - use handler to update ClientOrg table
            SqlDataAdapter daAddress = new SqlDataAdapter();
            daAddress.RowUpdating += (sender, e) =>
            {
                if (e.StatementType == StatementType.Insert)
                    oldAddressID = Convert.ToInt32(e.Row["AddressID"]);
            };

            daAddress.RowUpdated += (sender, e) =>
            {
                if (e.StatementType == StatementType.Insert)
                {
                    // exactly one row will match
                    DataRow[] fdr = dsAccount.Tables["Account"].Select(string.Format("{0} = {1}", e.Row["AddressType"], oldAddressID));
                    string addressType = e.Row["AddressType"].ToString();
                    fdr[0][addressType] = e.Row["AddressID"];
                }
            };

            daAddress.InsertCommand = new SqlCommand("Address_Insert", cnSselData) { CommandType = CommandType.StoredProcedure };
            daAddress.InsertCommand.Parameters.Add("@InternalAddress", SqlDbType.NVarChar, 45, "InternalAddress");
            daAddress.InsertCommand.Parameters.Add("@StrAddress1", SqlDbType.NVarChar, 45, "StrAddress1");
            daAddress.InsertCommand.Parameters.Add("@StrAddress2", SqlDbType.NVarChar, 45, "StrAddress2");
            daAddress.InsertCommand.Parameters.Add("@City", SqlDbType.NVarChar, 35, "City");
            daAddress.InsertCommand.Parameters.Add("@State", SqlDbType.NVarChar, 2, "State");
            daAddress.InsertCommand.Parameters.Add("@Zip", SqlDbType.NVarChar, 10, "Zip");
            daAddress.InsertCommand.Parameters.Add("@Country", SqlDbType.NVarChar, 50, "Country");

            daAddress.UpdateCommand = new SqlCommand("Address_Update", cnSselData) { CommandType = CommandType.StoredProcedure };
            daAddress.UpdateCommand.Parameters.Add("@AddressID", SqlDbType.Int, 4, "AddressID");
            daAddress.UpdateCommand.Parameters.Add("@InternalAddress", SqlDbType.NVarChar, 45, "InternalAddress");
            daAddress.UpdateCommand.Parameters.Add("@StrAddress1", SqlDbType.NVarChar, 45, "StrAddress1");
            daAddress.UpdateCommand.Parameters.Add("@StrAddress2", SqlDbType.NVarChar, 45, "StrAddress2");
            daAddress.UpdateCommand.Parameters.Add("@City", SqlDbType.NVarChar, 35, "City");
            daAddress.UpdateCommand.Parameters.Add("@State", SqlDbType.NVarChar, 2, "State");
            daAddress.UpdateCommand.Parameters.Add("@Zip", SqlDbType.NVarChar, 10, "Zip");
            daAddress.UpdateCommand.Parameters.Add("@Country", SqlDbType.NVarChar, 50, "Country");

            daAddress.DeleteCommand = new SqlCommand("Address_Delete", cnSselData) { CommandType = CommandType.StoredProcedure };
            daAddress.DeleteCommand.Parameters.Add("@AddressID", SqlDbType.Int, 4, "AddressID");

            daAddress.Update(dsAccount, "Address");

            // update the Account table - use handler to update ClientOrg table
            SqlDataAdapter daClient = new SqlDataAdapter();
            daClient.RowUpdating += (sender, e) =>
            {
                if (e.StatementType == StatementType.Insert)
                    oldAccountID = Convert.ToInt32(e.Row["AccountID"]);
            };

            daClient.RowUpdated += (sender, e) =>
            {
                if (e.StatementType == StatementType.Insert)
                {
                    // update AccountOrg
                    DataRow[] fdr = dsAccount.Tables["ClientAccount"].Select(string.Format("AccountID = {0}", oldAccountID));
                    for (int i = 0; i < fdr.Length; i++)
                        fdr[i]["AccountID"] = e.Row["AccountID"];
                }
            };

            daClient.InsertCommand = new SqlCommand("Account_Insert", cnSselData) { CommandType = CommandType.StoredProcedure };
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

            daClient.UpdateCommand = new SqlCommand("Account_Update", cnSselData) { CommandType = CommandType.StoredProcedure };
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
            SqlDataAdapter daClientAccount = new SqlDataAdapter
            {
                InsertCommand = new SqlCommand("ClientAccount_Insert", cnSselData) { CommandType = CommandType.StoredProcedure }
            };

            daClientAccount.InsertCommand.Parameters.Add("@ClientOrgID", SqlDbType.Int, 4, "ClientOrgID");
            daClientAccount.InsertCommand.Parameters.Add("@AccountID", SqlDbType.Int, 4, "AccountID");
            daClientAccount.InsertCommand.Parameters.Add("@Manager", SqlDbType.Bit, 1, "Manager");
            daClientAccount.InsertCommand.Parameters.Add("@Active", SqlDbType.Bit, 1, "Active");

            daClientAccount.UpdateCommand = new SqlCommand("ClientAccount_Update", cnSselData) { CommandType = CommandType.StoredProcedure };
            daClientAccount.UpdateCommand.Parameters.Add("@ClientAccountID", SqlDbType.Int, 4, "ClientAccountID");
            daClientAccount.UpdateCommand.Parameters.Add("@Manager", SqlDbType.Bit, 1, "Manager");
            daClientAccount.UpdateCommand.Parameters.Add("@Active", SqlDbType.Int, 4, "Active");

            daClientAccount.Update(dsAccount, "ClientAccount");

            // this is used to turn on/off card access to the lab - for existing clients only

            SqlDataAdapter daClientAccess = new SqlDataAdapter { UpdateCommand = new SqlCommand("Client_AuxDBUpdate", cnSselData) };
            daClientAccess.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daClientAccess.UpdateCommand.Parameters.Add("@ClientID", SqlDbType.Int, 4, "ClientID");
            daClientAccess.UpdateCommand.Parameters.Add("@EnableAccess", SqlDbType.Bit, 1, "EnableAccess");

            daClientAccess.Update(dsAccount, "Client");
        }

        protected void DiscardButton_Click(object sender, EventArgs e)
        {
            AccountID = 0;
            ContextBase.RemoveCacheData(); // remove anything left in cache
            Response.Redirect("~");
        }

        private bool OrgIsInternal()
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgID"]);
                var org = DataSession.Single<repo.Org>(orgId);
                return org.IsInternal();
            }
            catch
            {
                return false;
            }
        }

        protected void AddressManager1_UpdateAddress(object _, UpdateAddressEventArgs e)
        {
            DataRow dr = dsAccount.Tables["Address"].Rows.Find(e.Item.AddressID);

            dr["AddressType"] = e.Item.AddressType;
            dr["InternalAddress"] = e.Item.Attention;
            dr["StrAddress1"] = e.Item.AddressLine1;
            dr["StrAddress2"] = e.Item.AddressLine2;
            dr["City"] = e.Item.City;
            dr["State"] = e.Item.State;
            dr["Zip"] = e.Item.Zip;
            dr["Country"] = e.Item.Country;

            if (AccountID > 0)
            {
                DataRow acct = dsAccount.Tables["Account"].Rows.Find(AccountID);
                acct[e.Item.AddressType] = e.Item.AddressID;
            }

            ContextBase.SetCacheData(dsAccount);

            SetPageControlsAndBind(true, false, false);
        }

        protected void AddressManager1_CreateAddress(object _, CreateAddressEventArgs e)
        {
            var ndr = dsAccount.Tables["Address"].NewRow();

            ndr["AddressType"] = e.NewItem.AddressType;
            ndr["InternalAddress"] = e.NewItem.Attention;
            ndr["StrAddress1"] = e.NewItem.AddressLine1;
            ndr["StrAddress2"] = e.NewItem.AddressLine2;
            ndr["City"] = e.NewItem.City;
            ndr["State"] = e.NewItem.State;
            ndr["Zip"] = e.NewItem.Zip;
            ndr["Country"] = e.NewItem.Country;

            dsAccount.Tables["Address"].Rows.Add(ndr);

            if (AccountID > 0)
            {
                DataRow dr = dsAccount.Tables["Account"].Rows.Find(AccountID);
                dr[e.NewItem.AddressType] = ndr["AddressID"];
            }

            ContextBase.SetCacheData(dsAccount);

            SetPageControlsAndBind(true, false, false);
        }

        protected void AddressManager1_EditAddress(object _1, EditAddressEventArgs _2)
        {
            // Event that fires when an address edit is started. Nothing needs to be done here accept load all the addresses (and other controls)
            SetPageControlsAndBind(true, false, false);
        }

        protected void AddressManager1_DeleteAddress(object _, EditAddressEventArgs e)
        {
            var dr = dsAccount.Tables["Address"].Rows.Find(e.AddressID);
            if (dr != null) dr.Delete();
            SetPageControlsAndBind(true, false, false);
        }
    }
}
using LNF.Data;
using LNF.Web.Content;
using sselData.AppCode;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class OrgType : OnlineServicesPage
    {
        private DataSet dsOrgType;
        private SqlConnection cnSselData;

        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
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
                dsOrgType = (DataSet)Cache.Get(DataUtility.CacheID);
                if (dsOrgType == null)
                    Response.Redirect("~");
                else if (dsOrgType.DataSetName != "OrgType")
                    Response.Redirect("~");
            }
            else
            {
                Cache.Remove(DataUtility.CacheID); // remove anything left in cache
                cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                dsOrgType = new DataSet("OrgType");

                SqlDataAdapter daOrgType = new SqlDataAdapter("OrgType_Select", cnSselData);
                daOrgType.SelectCommand.CommandType = CommandType.StoredProcedure;
                daOrgType.FillSchema(dsOrgType, SchemaType.Mapped, "OrgType");
                daOrgType.Fill(dsOrgType, "OrgType");

                // get account and clientAccount info
                SqlDataAdapter daAccount = new SqlDataAdapter("ChargeType_Select", cnSselData);
                daAccount.SelectCommand.CommandType = CommandType.StoredProcedure;
                daAccount.SelectCommand.Parameters.AddWithValue("@Action", "All");
                daAccount.FillSchema(dsOrgType, SchemaType.Mapped, "ChargeType");
                daAccount.Fill(dsOrgType, "ChargeType");

                Cache.Insert(DataUtility.CacheID, dsOrgType);
                BindData();
            }
        }

        private void BindData()
        {
            dsOrgType.Tables["OrgType"].DefaultView.Sort = "OrgType";
            dgOrgType.DataSource = dsOrgType.Tables["OrgType"];
            dgOrgType.DataBind();
        }

        protected void dgOrgType_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            DataRow dr;

            switch (e.CommandName)
            {
                case "AddANewRow":
                    dr = dsOrgType.Tables["OrgType"].NewRow();
                    dr["OrgType"] = ((TextBox)e.Item.FindControl("txtOrgTypeF")).Text;
                    dr["ChargeTypeID"] = ((DropDownList)e.Item.FindControl("ddlCTCF")).SelectedValue;
                    dsOrgType.Tables["OrgType"].Rows.Add(dr);
                    Cache.Insert(DataUtility.CacheID, dsOrgType);
                    break;
                case "Edit":
                    //Datagrid in edit mode, hide footer section 
                    dgOrgType.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
                    dgOrgType.ShowFooter = false;
                    break;
                case "Update":
                    int ItemID = Convert.ToInt32(dgOrgType.DataKeys[e.Item.ItemIndex]);
                    dr = dsOrgType.Tables["OrgType"].Rows.Find(ItemID);
                    dr["OrgType"] = ((TextBox)e.Item.FindControl("txtOrgType")).Text;
                    dr["ChargeTypeID"] = ((DropDownList)e.Item.FindControl("ddlCTC")).SelectedValue;
                    Cache.Insert(DataUtility.CacheID, dsOrgType);
                    // Quit in-line-editing mode.
                    dgOrgType.EditItemIndex = -1;
                    dgOrgType.ShowFooter = true;
                    break;
                case "Cancel":
                    //Quit in-line-editing mode 
                    dgOrgType.EditItemIndex = -1;
                    dgOrgType.ShowFooter = true;
                    break;
            }

            BindData();
        }

        protected void dgOrgType_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.EditItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((TextBox)e.Item.FindControl("txtOrgType")).Text = drv["OrgType"].ToString();

                DropDownList ddlCTC = (DropDownList)e.Item.FindControl("ddlCTC");
                ddlCTC.DataSource = dsOrgType.Tables["ChargeType"];
                ddlCTC.DataTextField = "ChargeType";
                ddlCTC.DataValueField = "ChargeTypeID";
                ddlCTC.DataBind();
                ddlCTC.SelectedValue = drv["ChargeTypeID"].ToString();
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((Label)e.Item.FindControl("lblOrgType")).Text = drv["OrgType"].ToString();
                ((Label)e.Item.FindControl("lblCTC")).Text = dsOrgType.Tables["ChargeType"].Rows.Find(drv["ChargeTypeID"])["ChargeType"].ToString();
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                DropDownList ddlCTC = (DropDownList)e.Item.FindControl("ddlCTCF");
                ddlCTC.DataSource = dsOrgType.Tables["ChargeType"];
                ddlCTC.DataTextField = "ChargeType";
                ddlCTC.DataValueField = "ChargeTypeID";
                ddlCTC.DataBind();
                ddlCTC.ClearSelection();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e) //Handles butSave.Click
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            using (var insert = new SqlCommand("sselData.dbo.OrgType_Insert", conn) { CommandType = CommandType.StoredProcedure })
            using (var update = new SqlCommand("sselData.dbo.OrgType_Update", conn) { CommandType = CommandType.StoredProcedure })
            using (var adap = new SqlDataAdapter() { InsertCommand = insert, UpdateCommand = update })
            {
                insert.Parameters.Add("OrgType", SqlDbType.NVarChar, 50, "OrgType");
                insert.Parameters.Add("ChargeTypeID", SqlDbType.Int, 4, "ChargeTypeID");

                update.Parameters.Add("OrgTypeID", SqlDbType.Int, 4, "OrgTypeID");
                update.Parameters.Add("OrgType", SqlDbType.NVarChar, 50, "OrgType");
                update.Parameters.Add("ChargeTypeID", SqlDbType.Int, 4, "ChargeTypeID");

                adap.Update(dsOrgType, "OrgType");

                btnDiscard_Click(sender, e);
            }
        }

        protected void btnDiscard_Click(object sender, EventArgs e) //Handles butDiscard.Click
        {
            Cache.Remove(DataUtility.CacheID);
            Response.Redirect("~");
        }
    }
}
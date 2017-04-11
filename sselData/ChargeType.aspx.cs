using LNF.Models.Data;
using sselData.AppCode;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class ChargeType : LNF.Web.Content.LNFPage
    {
        DataSet dsChargeType;

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
                dsChargeType = (DataSet)Cache.Get(DataUtility.CacheID);
                if (dsChargeType == null)
                    Response.Redirect("~");
                else if (dsChargeType.DataSetName != "ChargeType")
                    Response.Redirect("~");
            }
            else
            {
                Cache.Remove(DataUtility.CacheID); // remove anything left in cache
                SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                dsChargeType = new DataSet("ChargeType");

                SqlDataAdapter daChargeType = new SqlDataAdapter("ChargeType_Select", cnSselData);
                daChargeType.SelectCommand.CommandType = CommandType.StoredProcedure;
                daChargeType.SelectCommand.Parameters.AddWithValue("@Action", "All");

                daChargeType.Fill(dsChargeType, "ChargeType");

                DataColumn[] pk = new DataColumn[1];
                pk[0] = dsChargeType.Tables["ChargeType"].Columns["ChargeTypeID"];
                dsChargeType.Tables["ChargeType"].PrimaryKey = pk;

                SqlDataAdapter daAccount = new SqlDataAdapter("Account_Select", cnSselData);
                daAccount.SelectCommand.CommandType = CommandType.StoredProcedure;
                daAccount.SelectCommand.Parameters.AddWithValue("@Action", "AllByOrg");
                daAccount.Fill(dsChargeType, "Account");

                DataColumn[] pka = new DataColumn[1];
                pka[0] = dsChargeType.Tables["Account"].Columns["AccountID"];
                dsChargeType.Tables["Account"].PrimaryKey = pka;

                Cache.Insert(DataUtility.CacheID, dsChargeType);

                BindData();
            }
        }

        private void BindData()
        {
            dsChargeType.Tables["ChargeType"].DefaultView.Sort = "ChargeTypeID";
            dgChargeType.DataKeyField = "ChargeTypeID";
            dgChargeType.DataSource = dsChargeType.Tables["ChargeType"];
            dgChargeType.DataBind();
        }

        protected void dgChargeType_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            DataRow dr;
            int ChargeTypeID;

            switch (e.CommandName)
            {
                case "AddANewRow":
                    dr = dsChargeType.Tables["ChargeType"].NewRow();
                    dr["ChargeTypeID"] = ((TextBox)e.Item.FindControl("txtCTCIdF")).Text;
                    dr["ChargeType"] = ((TextBox)e.Item.FindControl("txtCTCF")).Text;
                    dr["AccountID"] = ((DropDownList)e.Item.FindControl("ddlAccountIDF")).SelectedValue;
                    dsChargeType.Tables["ChargeType"].Rows.Add(dr);
                    Cache.Insert(DataUtility.CacheID, dsChargeType);
                    break;
                case "Edit":
                    //Datagrid in edit mode, hide footer section 
                    dgChargeType.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
                    dgChargeType.ShowFooter = false;
                    break;
                case "Update":
                    ChargeTypeID = Convert.ToInt32(dgChargeType.DataKeys[e.Item.ItemIndex]);
                    dr = dsChargeType.Tables["ChargeType"].Rows.Find(ChargeTypeID);
                    dr["ChargeTypeID"] = ((TextBox)e.Item.FindControl("txtCTCId")).Text;
                    dr["ChargeType"] = ((TextBox)e.Item.FindControl("txtCTC")).Text;
                    dr["AccountID"] = ((DropDownList)e.Item.FindControl("ddlAccountID")).SelectedValue;
                    Cache.Insert(DataUtility.CacheID, dsChargeType);
                    // Quit in-line-editing mode.
                    dgChargeType.EditItemIndex = -1;
                    dgChargeType.ShowFooter = true;
                    break;
                case "Delete":
                    ChargeTypeID = Convert.ToInt32(dgChargeType.DataKeys[e.Item.ItemIndex]);
                    dr = dsChargeType.Tables["ChargeType"].Rows.Find(ChargeTypeID);
                    dr.Delete();
                    Cache.Insert(DataUtility.CacheID, dsChargeType);
                    break;
                case "Cancel":
                    //Quit in-line-editing mode 
                    dgChargeType.EditItemIndex = -1;
                    dgChargeType.ShowFooter = true;
                    break;
            }

            BindData();
        }

        protected void dgChargeType_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.EditItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((TextBox)e.Item.FindControl("txtCTCId")).Text = drv["ChargeTypeID"].ToString();
                ((TextBox)e.Item.FindControl("txtCTC")).Text = drv["ChargeType"].ToString();
                DropDownList ddlAccountID = (DropDownList)e.Item.FindControl("ddlAccountID");
                ddlAccountID.DataSource = dsChargeType.Tables["Account"];
                ddlAccountID.DataValueField = "AccountID";
                ddlAccountID.DataTextField = "Project";
                ddlAccountID.DataBind();
                ddlAccountID.SelectedValue = drv["AccountID"].ToString();
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                ((Label)e.Item.FindControl("lblCTCid")).Text = drv["ChargeTypeID"].ToString();
                ((Label)e.Item.FindControl("lblCTC")).Text = drv["ChargeType"].ToString();
                ((Label)e.Item.FindControl("lblAccountID")).Text = dsChargeType.Tables["Account"].Rows.Find(drv["AccountID"])["Project"].ToString();
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                DropDownList ddlAccountID = (DropDownList)e.Item.FindControl("ddlAccountIDF");
                ddlAccountID.DataSource = dsChargeType.Tables["Account"];
                ddlAccountID.DataValueField = "AccountID";
                ddlAccountID.DataTextField = "Project";
                ddlAccountID.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            SqlDataAdapter daChargeType = new SqlDataAdapter();

            daChargeType.InsertCommand = new SqlCommand("ChargeType_Insert", cnSselData);
            daChargeType.InsertCommand.CommandType = CommandType.StoredProcedure;
            daChargeType.InsertCommand.Parameters.Add("@ChargeTypeID", SqlDbType.Int, 4, "ChargeTypeID");
            daChargeType.InsertCommand.Parameters.Add("@ChargeType", SqlDbType.NVarChar, 50, "ChargeType");
            daChargeType.InsertCommand.Parameters.Add("@AccountID", SqlDbType.Int, 50, "AccountID");

            daChargeType.UpdateCommand = new SqlCommand("ChargeType_Update", cnSselData);
            daChargeType.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daChargeType.UpdateCommand.Parameters.Add("@ChargeTypeID", SqlDbType.Int, 4, "ChargeTypeID");
            daChargeType.UpdateCommand.Parameters.Add("@ChargeType", SqlDbType.NVarChar, 50, "ChargeType");
            daChargeType.UpdateCommand.Parameters.Add("@AccountID", SqlDbType.Int, 50, "AccountID");

            daChargeType.Update(dsChargeType, "ChargeType");

            btnDiscard_Click(sender, e);
        }

        protected void btnDiscard_Click(object sender, EventArgs e)
        {
            Cache.Remove(DataUtility.CacheID);
            Response.Redirect("~");
        }
    }
}
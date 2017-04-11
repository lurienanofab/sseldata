using LNF.Models.Data;
using LNF.Web.Content;
using sselData.AppCode;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace sselData
{
    public partial class GlobalConfig : LNFPage
    {
        DataSet dsGlobal;
        string strGlobalItem;

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
                dsGlobal = (DataSet)Cache.Get(DataUtility.CacheID);
                if (dsGlobal == null)
                    Response.Redirect("~");
                else if (dsGlobal.DataSetName != "globalConfig")
                    Response.Redirect("~");
            }
            else
            {
                Cache.Remove(DataUtility.CacheID); // remove anything left in cache
                SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

                dsGlobal = new DataSet("globalConfig");

                strGlobalItem = Request.QueryString["ConfigItem"];
                lblConfigItem.Text = "Configure " + Request.QueryString["ConfigItem"];

                SqlDataAdapter daGlobal = new SqlDataAdapter("Global_Select", cnSselData);
                daGlobal.SelectCommand.CommandType = CommandType.StoredProcedure;
                daGlobal.SelectCommand.Parameters.AddWithValue("@TableName", strGlobalItem);
                daGlobal.FillSchema(dsGlobal, SchemaType.Mapped, strGlobalItem);
                daGlobal.Fill(dsGlobal, strGlobalItem);
                Cache.Insert(DataUtility.CacheID, dsGlobal);

                BindData();
            }

            strGlobalItem = lblConfigItem.Text.Substring(10); // instead of referencing through the label each time
        }

        private void BindData()
        {
            dgGlobal.Columns[0].HeaderText = strGlobalItem; // seems hackish, but not sure how else to do this
            dsGlobal.Tables[strGlobalItem].DefaultView.Sort = strGlobalItem;
            dgGlobal.DataKeyField = strGlobalItem + "ID";
            dgGlobal.DataSource = dsGlobal.Tables[strGlobalItem];
            dgGlobal.DataBind();
        }

        protected void dgGlobal_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            DataRow dr;

            switch (e.CommandName)
            {
                case "AddANewRow":
                    dr = dsGlobal.Tables[strGlobalItem].NewRow();
                    dr[strGlobalItem] = ((TextBox)e.Item.FindControl("tbReqTextF")).Text;
                    dsGlobal.Tables[strGlobalItem].Rows.Add(dr);
                    Cache.Insert(DataUtility.CacheID, dsGlobal);
                    break;
                case "Edit":
                    //Datagrid in edit mode, hide footer section
                    dgGlobal.EditItemIndex = Convert.ToInt32(e.Item.ItemIndex);
                    dgGlobal.ShowFooter = false;
                    break;
                case "Update":
                    int ItemID = Convert.ToInt32(dgGlobal.DataKeys[e.Item.ItemIndex]);
                    dr = dsGlobal.Tables[strGlobalItem].Rows.Find(ItemID);
                    dr[strGlobalItem] = ((TextBox)e.Item.FindControl("tbReqText")).Text;
                    Cache.Insert(DataUtility.CacheID, dsGlobal);
                    // Quit in-line-editing mode.
                    dgGlobal.EditItemIndex = -1;
                    dgGlobal.ShowFooter = true;
                    break;
                case "Cancel":
                    //Quit in-line-editing mode
                    dgGlobal.EditItemIndex = -1;
                    dgGlobal.ShowFooter = true;
                    break;
            }

            BindData();
        }

        protected void dgGlobal_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.EditItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                TextBox tbReqText = (TextBox)e.Item.FindControl("tbReqText");
                tbReqText.Text = drv[strGlobalItem].ToString();
            }
            else if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                Label lblReqText = (Label)e.Item.FindControl("lblReqText");
                lblReqText.Text = drv[strGlobalItem].ToString();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);

            SqlDataAdapter daGlobal = new SqlDataAdapter();

            daGlobal.InsertCommand = new SqlCommand("Global_Insert", cnSselData);
            daGlobal.InsertCommand.CommandType = CommandType.StoredProcedure;
            daGlobal.InsertCommand.Parameters.AddWithValue("@TableName", strGlobalItem);
            daGlobal.InsertCommand.Parameters.Add("@Value", SqlDbType.NVarChar, 50, strGlobalItem);

            daGlobal.UpdateCommand = new SqlCommand("Global_Update", cnSselData);
            daGlobal.UpdateCommand.CommandType = CommandType.StoredProcedure;
            daGlobal.UpdateCommand.Parameters.AddWithValue("@TableName", strGlobalItem);
            daGlobal.UpdateCommand.Parameters.Add("@ValueID", SqlDbType.Int, 4, strGlobalItem + "ID");
            daGlobal.UpdateCommand.Parameters.Add("@Value", SqlDbType.NVarChar, 50, strGlobalItem);

            daGlobal.Update(dsGlobal, strGlobalItem);

            btnDiscard_Click(sender, e);
        }

        protected void btnDiscard_Click(object sender, EventArgs e)
        {
            Cache.Remove(DataUtility.CacheID);
            Response.Redirect("~");
        }
    }
}
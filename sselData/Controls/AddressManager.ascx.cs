using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using LNF.Repository.Data;
using System.Data;

namespace sselData.Controls
{
    [ParseChildren(true)]
    public partial class AddressManager : UserControl
    {
        public event EventHandler<CreateAddressEventArgs> CreateAddress;
        public event EventHandler<UpdateAddressEventArgs> UpdateAddress;
        public event EventHandler<EditAddressEventArgs> EditAddress;
        public event EventHandler<EditAddressEventArgs> DeleteAddress;

        [NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<AddressType> AddressTypes { get; set; }

        public List<AddressItem> Items { get; private set; }

        public int EditItemIndex
        {
            get { return Convert.ToInt32(hidEditItemIndex.Value); }
            set { hidEditItemIndex.Value = value.ToString(); }
        }

        public bool ShowFooter
        {
            get { return bool.Parse(hidShowFooter.Value); }
            set { hidShowFooter.Value = value.ToString().ToLower(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private AddressItem GetUpdateItem()
        {
            var repeaterItem = rptAddressManager.Items[EditItemIndex];

            var ddlType = ((DropDownList)repeaterItem.FindControl("ddlType"));
            var txtAttentionLine = ((TextBox)repeaterItem.FindControl("txtAttentionLine"));
            var txtStreetAddressLine1 = ((TextBox)repeaterItem.FindControl("txtStreetAddressLine1"));
            var txtStreetAddressLine2 = ((TextBox)repeaterItem.FindControl("txtStreetAddressLine2"));
            var txtCity = ((TextBox)repeaterItem.FindControl("txtCity"));
            var txtState = ((TextBox)repeaterItem.FindControl("txtState"));
            var txtZip = ((TextBox)repeaterItem.FindControl("txtZip"));
            var txtCountry = ((TextBox)repeaterItem.FindControl("txtCountry"));
            var hidAddressID = (HiddenField)repeaterItem.FindControl("hidAddressID");

            var addrType = AddressTypes.First(x => x.Column == ddlType.SelectedValue);

            var result = new AddressItem()
            {
                AddressID = int.Parse(hidAddressID.Value),
                Type = addrType,
                AttentionLine = txtAttentionLine.Text,
                StreetAddressLine1 = txtStreetAddressLine1.Text,
                StreetAddressLine2 = txtStreetAddressLine2.Text,
                City = txtCity.Text,
                State = txtState.Text,
                Zip = txtZip.Text,
                Country = txtCountry.Text
            };

            return result;
        }

        private int GetAddressID(int index)
        {
            var repeaterItem = rptAddressManager.Items[index];
            var hidAddressID = (HiddenField)repeaterItem.FindControl("hidAddressID");
            int result = int.Parse(hidAddressID.Value);
            return result;
        }

        protected void Item_Command(object sender, CommandEventArgs e)
        {
            int index;

            switch (e.CommandName)
            {
                case "edit":
                    EditItemIndex = Convert.ToInt32(e.CommandArgument);
                    EditAddress?.Invoke(this, new EditAddressEventArgs() { ItemIndex = EditItemIndex, AddressID = GetAddressID(EditItemIndex) });
                    break;
                case "cancel":
                    EditItemIndex = -1;
                    EditAddress?.Invoke(this, new EditAddressEventArgs() { ItemIndex = EditItemIndex, AddressID = 0 });
                    break;
                case "update":
                    var item = GetUpdateItem();
                    index = EditItemIndex;
                    EditItemIndex = -1;
                    UpdateAddress?.Invoke(this, new UpdateAddressEventArgs() { ItemIndex = index, Item = item });
                    break;
                case "delete":
                    index = Convert.ToInt32(e.CommandArgument);
                    EditItemIndex = -1;
                    DeleteAddress?.Invoke(this, new EditAddressEventArgs() { ItemIndex = index, AddressID = GetAddressID(index) });
                    break;
            }
        }

        protected void rptAddressManager_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.Footer:
                    DropDownList ddlTypeF = (DropDownList)e.Item.FindControl("ddlTypeF");
                    ddlTypeF.DataSource = FilteredAddressTypes().OrderBy(x => x.Name);
                    ddlTypeF.DataBind();
                    break;
            }
        }

        private IEnumerable<AddressType> FilteredAddressTypes()
        {
            List<AddressType> result = new List<AddressType>();
            string[] columns = Items.Select(x => x.Type.Column).ToArray();
            result.AddRange(AddressTypes.Where(x => !columns.Contains(x.Column)));
            return result;
        }

        private void CancelEdit()
        {
            if (EditItemIndex > -1)
            {
                RepeaterItem item = rptAddressManager.Items[EditItemIndex];
                HtmlTableRow tr = (HtmlTableRow)item.FindControl("trItem");
                tr.Attributes.Remove("class");
                SetVisibility(item, "lblAttentionLine", true);
                SetVisibility(item, "txtAttentionLine", false);
                SetVisibility(item, "lblStreetAddressLine1", true);
                SetVisibility(item, "txtStreetAddressLine1", false);
                SetVisibility(item, "lblStreetAddressLine2", true);
                SetVisibility(item, "txtStreetAddressLine2", false);
                SetVisibility(item, "lblCity", true);
                SetVisibility(item, "txtCity", false);
                SetVisibility(item, "lblState", true);
                SetVisibility(item, "txtState", false);
                SetVisibility(item, "lblZip", true);
                SetVisibility(item, "txtZip", false);
                SetVisibility(item, "lblCountry", true);
                SetVisibility(item, "txtCountry", false);
                SetVisibility(item, "btnEdit", true);
                SetVisibility(item, "btnDelete", true);
                SetVisibility(item, "btnUpdate", false);
                SetVisibility(item, "btnCancel", false);
                SetVisibility(item, "lblType", true);
                SetVisibility(item, "ddlType", false);
            }
        }

        private void BeginEdit()
        {
            if (EditItemIndex > -1)
            {
                RepeaterItem item = rptAddressManager.Items[EditItemIndex];
                HtmlTableRow tr = (HtmlTableRow)item.FindControl("trItem");
                tr.Attributes.Add("class", "editing");
                SetVisibility(item, "lblAttentionLine", false);
                SetVisibility(item, "txtAttentionLine", true);
                SetVisibility(item, "lblStreetAddressLine1", false);
                SetVisibility(item, "txtStreetAddressLine1", true);
                SetVisibility(item, "lblStreetAddressLine2", false);
                SetVisibility(item, "txtStreetAddressLine2", true);
                SetVisibility(item, "lblCity", false);
                SetVisibility(item, "txtCity", true);
                SetVisibility(item, "lblState", false);
                SetVisibility(item, "txtState", true);
                SetVisibility(item, "lblZip", false);
                SetVisibility(item, "txtZip", true);
                SetVisibility(item, "lblCountry", false);
                SetVisibility(item, "txtCountry", true);
                SetVisibility(item, "btnEdit", false);
                SetVisibility(item, "btnDelete", false);
                SetVisibility(item, "btnUpdate", true);
                SetVisibility(item, "btnCancel", true);

                IList<AddressType> dataSource = FilteredAddressTypes().ToList();

                //we need to add this item's address type back in
                AddressItem dataItem = Items[item.ItemIndex];
                dataSource.Add(dataItem.Type);

                Label lblType = (Label)item.FindControl("lblType");
                DropDownList ddlType = (DropDownList)item.FindControl("ddlType");
                ddlType.DataSource = dataSource.OrderBy(x => x.Name);
                ddlType.DataBind();
                ddlType.SelectedValue = ddlType.Items.FindByText(lblType.Text).Value;

                lblType.Visible = false;
                ddlType.Visible = true;
            }
        }

        private void SetVisibility(RepeaterItem item, string id, bool visible)
        {
            if (item == null) return;
            Control ctrl = item.FindControl(id);
            if (ctrl != null)
                ctrl.Visible = visible;
        }

        /// <summary>
        /// Create the structure based on the AddressTypes without loading any data. Used for 'Add New' situations where no addresses have been defined yet.
        /// </summary>
        public void Fill()
        {
            Items = new List<AddressItem>();
            rptAddressManager.DataSource = Items;
            rptAddressManager.DataBind();
            ToggleFooter();
            CancelEdit();
            BeginEdit();
        }

        public void Fill(IEnumerable<AddressItem> items)
        {
            Items = new List<AddressItem>();

            foreach (var addrType in AddressTypes)
            {
                Items.AddRange(items.Where(x => x.Type.Column == addrType.Column));
            }

            rptAddressManager.DataSource = Items;
            rptAddressManager.DataBind();

            ToggleFooter();
            CancelEdit();
            BeginEdit();
        }

        private void ToggleFooter()
        {
            var footer = GetFooter();
            var trFooter = (HtmlTableRow)footer.FindControl("trFooter");
            trFooter.Visible = ShowFooter;
        }

        private Control GetFooter()
        {
            return rptAddressManager.Controls[rptAddressManager.Controls.Count - 1].Controls[0];
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            var footer = GetFooter();
            var ddlTypeF = (DropDownList)footer.FindControl("ddlTypeF");
            var txtAttentionLineF = (TextBox)footer.FindControl("txtAttentionLineF");
            var txtStreetAddressLine1F = (TextBox)footer.FindControl("txtStreetAddressLine1F");
            var txtStreetAddressLine2F = (TextBox)footer.FindControl("txtStreetAddressLine2F");
            var txtCityF = (TextBox)footer.FindControl("txtCityF");
            var txtStateF = (TextBox)footer.FindControl("txtStateF");
            var txtZipF = (TextBox)footer.FindControl("txtZipF");
            var txtCountryF = (TextBox)footer.FindControl("txtCountryF");

            if (!string.IsNullOrEmpty(txtStreetAddressLine1F.Text))
            {
                var type = AddressTypes.First(x => x.Column == ddlTypeF.SelectedValue);

                AddressItem item = new AddressItem()
                {
                    AddressID = 0,
                    Type = type,
                    AttentionLine = txtAttentionLineF.Text,
                    StreetAddressLine1 = txtStreetAddressLine1F.Text,
                    StreetAddressLine2 = txtStreetAddressLine2F.Text,
                    City = txtCityF.Text,
                    State = txtStateF.Text,
                    Zip = txtZipF.Text,
                    Country = txtCountryF.Text
                };

                CreateAddress?.Invoke(this, new CreateAddressEventArgs() { NewItem = item });
            }
        }
    }
}
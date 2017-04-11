using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using LNF.CommonTools;

namespace sselData.AppCode
{
    public class DynamicCheckboxTemplate : ITemplate
    {
        private string _ID;
        private IDynamicCheckBoxContainer _Provider;

        public DynamicCheckboxTemplate(IDynamicCheckBoxContainer provider, string id)
        {
            _ID = id;
            _Provider = provider;
        }

        public void InstantiateIn(Control container)
        {
            CheckBox chk = new CheckBox();
            chk.ID = _ID;
            chk.AutoPostBack = true;
            chk.CheckedChanged += _Provider.DynamicCheckBox_CheckChanged;
            chk.DataBinding += BindCheckBox;
            container.Controls.Add(chk);
        }

        public void BindCheckBox(object sender, EventArgs e)
        {
            CheckBox myCheckBox = (CheckBox)sender;
            DataGridItem container = (DataGridItem)myCheckBox.NamingContainer;
            DataItemHelper helper = new DataItemHelper(container.DataItem);
            myCheckBox.Checked = helper.Value(_ID, false);
        }
    }
}
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace sselData.AppCode
{
    public class DynamicLabelTemplate : ITemplate
    {
        private string _ID;

        public DynamicLabelTemplate(string id)
        {
            _ID = id;
        }

        public void InstantiateIn(Control container)
        {
            Label myLabel = new Label();
            myLabel.ID = _ID;
            container.Controls.Add(myLabel);
        }
    }
}
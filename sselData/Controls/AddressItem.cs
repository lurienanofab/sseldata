using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sselData.Controls
{
    [Serializable]
    public class AddressItem
    {
        public AddressType Type { get; set; }
        public int AddressID { get; set; }
        public string AttentionLine { get; set; }
        public string StreetAddressLine1 { get; set; }
        public string StreetAddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}
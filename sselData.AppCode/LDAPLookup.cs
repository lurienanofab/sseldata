using System;
using System.Xml;

namespace sselData.AppCode
{
    public class LDAPLookup
    {
        public static LDAPUserInfo GetUser(string uniqueID)
        {
            LDAPUserInfo result;
            string debug = string.Empty;

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(string.Format("http://lnf.umich.edu/?uniqname={0}&action=umich-directory-search&format=xml", uniqueID));

            result = new LDAPUserInfo(xdoc);

            return result;
        }
    }

    public class LDAPUserInfo
    {
        private string _UID;
        private string[] _DisplayName;
        private string _Title;
        private string _Email;
        private string[] _PostalAddress;
        private string _Phone;
        private Exception _LDAPException;

        public string Debug { get; set; }

        public LDAPUserInfo(XmlDocument xdoc)
        {
            _UID = GetStringProperty(xdoc, "UID");
            _DisplayName = GetArrayProperty(xdoc, "DisplayName");
            _Title = GetStringProperty(xdoc, "Title");
            _Email = GetStringProperty(xdoc, "Email");
            _PostalAddress = GetArrayProperty(xdoc, "PostalAddress");
            _Phone = GetStringProperty(xdoc, "Phone");
            _LDAPException = GetExeptionProperty(xdoc);

            Debug = GetDebugProperty(xdoc);

            xdoc = null;
        }

        public string UID { get { return _UID; } }

        public string[] DisplayName { get { return _DisplayName; } }

        public string Title { get { return _Title; } }

        public string Email { get { return _Email; } }

        public string[] PostalAddress { get { return _PostalAddress; } }

        public string Phone { get { return _Phone; } }

        public Exception LDAPException { get { return _LDAPException; } }

        private string GetStringProperty(XmlDocument xdoc, string propname)
        {
            XmlNode node = xdoc.SelectSingleNode(string.Format("/ldap/property[@name='{0}']", propname));
            if (node != null)
                return node.Attributes["value"].Value;
            else
                return string.Empty;
        }

        private string[] GetArrayProperty(XmlDocument xdoc, string propname)
        {
            XmlNode node = xdoc.SelectSingleNode(string.Format("/ldap/property[@name='{0}']", propname));

            if (node != null)
            {
                XmlNodeList children = node.SelectNodes("add");
                string[] result = new string[children.Count];
                foreach (XmlNode c in children)
                {
                    int index = Convert.ToInt32(c.Attributes["index"].Value);
                    result[index] = c.Attributes["value"].Value;
                }
                return result;
            }
            else
                return new string[] { string.Empty };
        }

        private Exception GetExeptionProperty(XmlDocument xdoc)
        {
            Exception result = null;
            XmlNode node = xdoc.SelectSingleNode("/ldap/exception");
            if (node.Attributes["type"].Value == "null")
                result = null;
            else
                result = new Exception(node.SelectSingleNode("message").Attributes["value"].Value);
            return result;
        }

        private string GetDebugProperty(XmlDocument xdoc)
        {
            string result = string.Empty;

            XmlNode node = xdoc.SelectSingleNode("/ldap/debug");

            if (node != null)
            {
                try
                {
                    XmlCDataSection cdata = (XmlCDataSection)node.FirstChild;
                    result = cdata.Value;
                }
                catch
                {
                    result = string.Empty;
                }
            }

            return result;
        }
    }
}
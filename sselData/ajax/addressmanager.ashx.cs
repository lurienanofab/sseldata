﻿using LNF;
using LNF.Cache;
using sselData.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace sselData.Ajax
{
    /// <summary>
    /// Summary description for addressmanager
    /// </summary>
    public class AddressManager : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            var json = string.Empty;

            try
            {
                context.Response.ContentType = "application/json";

                var command = context.Request.QueryString["command"];

                switch (command)
                {
                    case "get-addresses":
                        json = HandleGetAddresses(context);
                        break;
                    case "update-address":
                        json = HandleUpdateAddress(context);
                        break;
                    case "delete-address":
                        json = HandleDeleteAddress(context);
                        break;
                    case "add-address":
                        json = HandleAddAddress(context);
                        break;
                    case "get-default-addresses":
                        json = HandleGetDefaultAddress(context);
                        break;
                    case "table-dump":
                        json = HandleTableDump(context);
                        break;
                    default:
                        throw new AjaxException(400, "Invalid command.");
                }
            }
            catch (AjaxException ajaxEx)
            {
                context.Response.StatusCode = ajaxEx.StatusCode;
                json = ServiceProvider.Current.Serialization.Json.SerializeObject(new { error = ajaxEx.Message });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                json = ServiceProvider.Current.Serialization.Json.SerializeObject(new { error = ex.Message });
            }

            context.Response.Write(json);
        }

        private string HandleTableDump(HttpContext context)
        {
            string name = context.Request.QueryString["name"];

            if (string.IsNullOrEmpty(name))
                throw new AjaxException(400, "Invalid name.");

            var ds = GetDataSet();

            if (!ds.Tables.Contains(name))
                throw new AjaxException(400, string.Format("Table '{0}' does not exist.", name));

            string html = "<!DOCTYPE html>"
                + "<html><head><title>Accounts</title><link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\"></head><body><div class=\"container-fluid\">{0}</div><script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js\"></script><script src=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js\"></script></body></html>";

            var table = LNF.CommonTools.Utility.TableDump(ds.Tables[name], true, "html-bootstrap");

            string result = string.Format(html, table);

            context.Response.ContentType = "text/html";

            return result;
        }

        private string HandleGetDefaultAddress(HttpContext context)
        {
            var addressType = context.Request.QueryString["addressType"];
            var orgId = Convert.ToInt32(context.Session["OrgID"]);
            var addr = GetDefaultAddress(addressType, orgId);
            return ServiceProvider.Current.Serialization.Json.SerializeObject(addr);
        }

        private AddressItem GetDefaultAddress(string addressType, int orgId)
        {
            var ds = GetDataSet();

            DataRow org = ds.Tables["Org"].Rows.Find(orgId);

            int addressId = org.Field<int>(string.Format("Def{0}", addressType));

            var result = new AddressItem();

            if (addressId != 0)
            {
                DataRow addr = ds.Tables["Address"].Rows.Find(addressId);

                if (addr != null)
                {
                    result.AddressType = addressType;
                    result.AddressID = addr.Field<int>("AddressID");
                    result.Attention = addr.Field<string>("InternalAddress");
                    result.AddressLine1 = addr.Field<string>("StrAddress1");
                    result.AddressLine2 = addr.Field<string>("StrAddress2");
                    result.City = addr.Field<string>("City");
                    result.State = addr.Field<string>("State");
                    result.Zip = addr.Field<string>("Zip");
                    result.Country = addr.Field<string>("Country");
                }
            }

            return result;
        }

        private string HandleAddAddress(HttpContext context)
        {
            if (context.Request.HttpMethod != "POST")
                throw new AjaxException(405, "This command must use POST.");

            var type = GetContextType(context);

            switch (type)
            {
                case "account":
                    var accountId = GetContextId(context);
                    var item = GetContextAddressItem(context);
                    AddAccountAddress(accountId, item);
                    return HandleGetAddresses(context);
                default:
                    throw new NotImplementedException();
            }
        }

        private void AddAccountAddress(int accountId, AddressItem item)
        {
            if (accountId > 0)
            {
                var dsAccount = GetDataSet();

                DataRow ndr = dsAccount.Tables["Address"].NewRow();

                ndr.SetField("AddressType", item.AddressType);
                ndr.SetField("InternalAddress", item.Attention);
                ndr.SetField("StrAddress1", item.AddressLine1);
                ndr.SetField("StrAddress2", item.AddressLine2);
                ndr.SetField("City", item.City);
                ndr.SetField("State", item.State);
                ndr.SetField("Zip", item.Zip);
                ndr.SetField("Country", item.Country);

                dsAccount.Tables["Address"].Rows.Add(ndr);
                int addressId = ndr.Field<int>("AddressID");
                DataRow acct = dsAccount.Tables["Account"].Rows.Find(accountId);
                acct.SetField(item.AddressType, addressId);

                CacheManager.Current.CacheData(dsAccount);
            }
        }

        private string HandleDeleteAddress(HttpContext context)
        {
            var type = GetContextType(context);

            if (int.TryParse(context.Request.QueryString["addressId"], out int addressId))
            {
                switch (type)
                {
                    case "account":
                        var accountId = GetContextId(context);
                        DeleteAccountAddress(accountId, addressId);
                        return HandleGetAddresses(context);
                    default:
                        throw new NotImplementedException();
                }
            }
            else
                throw new AjaxException(400, "Invalid addressId.");
        }

        private void DeleteAccountAddress(int accountId, int addressId)
        {
            var dsAccount = GetDataSet();

            DataRow addressRow = dsAccount.Tables["Address"].Rows.Find(addressId);
            string addressType = addressRow.Field<string>("AddressType");

            addressRow.Delete();

            // accountId is zero for new accounts
            if (accountId > 0)
            {
                DataRow accountRow = dsAccount.Tables["Account"].Rows.Find(accountId);
                accountRow.SetField(addressType, 0);
            }
        }

        private string HandleUpdateAddress(HttpContext context)
        {
            if (context.Request.HttpMethod != "POST")
                throw new AjaxException(405, "This command must use POST.");

            var type = GetContextType(context);

            switch (type)
            {
                case "account":
                    var accountId = GetContextId(context);
                    var item = GetContextAddressItem(context);
                    UpdateAccountAddress(accountId, item);
                    return HandleGetAddresses(context);
                default:
                    throw new NotImplementedException();
            }
        }

        protected void UpdateAccountAddress(int accountId, AddressItem item)
        {
            var dsAccount = GetDataSet();

            DataRow addr;

            if (item.AddressID == 0)
                addr = dsAccount.Tables["Address"].NewRow();
            else
                addr = dsAccount.Tables["Address"].Rows.Find(item.AddressID);

            addr.SetField("AddressType", item.AddressType);
            addr.SetField("InternalAddress", item.Attention);
            addr.SetField("StrAddress1", item.AddressLine1);
            addr.SetField("StrAddress2", item.AddressLine2);
            addr.SetField("City", item.City);
            addr.SetField("State", item.State);
            addr.SetField("Zip", item.Zip);
            addr.SetField("Country", item.Country);

            if (item.AddressID == 0)
                dsAccount.Tables["Address"].Rows.Add(addr);

            var acct = dsAccount.Tables["Account"].Rows.Find(accountId);
            acct[item.AddressType] = addr["AddressID"];

            CacheManager.Current.CacheData(dsAccount);
        }

        private string HandleGetAddresses(HttpContext context)
        {
            var type = GetContextType(context);

            IEnumerable<AddressItem> items;

            switch (type)
            {
                case "account":
                    var accountId = GetContextId(context);
                    items = GetAddresses(accountId);
                    return ServiceProvider.Current.Serialization.Json.SerializeObject(items);
                default:
                    throw new NotImplementedException();
            }
        }

        private IList<AddressItem> GetAddresses(int accountId)
        {
            List<AddressItem> result = new List<AddressItem>();
            DataRow[] rows = GetAddressDataSource(accountId);

            if (rows != null)
            {
                foreach (DataRow dr in rows)
                {
                    string addressType = dr.Field<string>("AddressType");

                    result.Add(new AddressItem()
                    {
                        AddressID = dr.Field<int>("AddressID"),
                        AddressType = addressType,
                        Attention = dr.Field<string>("InternalAddress"),
                        AddressLine1 = dr.Field<string>("StrAddress1"),
                        AddressLine2 = dr.Field<string>("StrAddress2"),
                        City = dr.Field<string>("City"),
                        State = dr.Field<string>("State"),
                        Zip = dr.Field<string>("Zip"),
                        Country = dr.Field<string>("Country")
                    });
                }
            }

            return result;
        }

        private DataRow[] GetAddressDataSource(int accountId)
        {
            DataSet dsAccount = GetDataSet();

            // filter the addresses
            var addressTable = dsAccount.Tables["Address"];

            EnumerableRowCollection<DataRow> result;

            if (accountId == 0)
            {
                // a new client must have a newly added address, if any (we are looking for default addresses that were added based on org here)
                result = addressTable.AsEnumerable().Where(x => x.RowState == DataRowState.Added);
            }
            else
            {
                // set addresstype and create row filter
                var accountRow = dsAccount.Tables["Account"].Rows.Find(accountId);
                var filter = new List<int>();

                if (accountRow != null)
                {
                    MakeAddrFilter(filter, addressTable, accountRow, "BillAddressID");
                    MakeAddrFilter(filter, addressTable, accountRow, "ShipAddressID");
                }

                // if affiliated with this org, this row must exist
                result = addressTable.AsEnumerable().Where(x => x.RowState == DataRowState.Added || (x.RowState != DataRowState.Deleted && filter.Contains(x.Field<int>("AddressID"))));
            }

            return result.ToArray();
        }

        private void MakeAddrFilter(IList<int> filter, DataTable addressTable, DataRow accountRow, string addrType)
        {
            // get the AddressID for this address type (BillAddressID or ShipAddressID)
            var addressId = accountRow.Field<int>(addrType);

            if (addressId > 0)
            {
                // the AddressID has been assigned so get a row from the address table
                var addressRow = addressTable.Rows.Find(addressId);

                if (addressRow != null && addressRow.RowState != DataRowState.Deleted)
                {
                    // we found an address, flag it with this address type
                    addressRow.SetField("AddressType", addrType);

                    // add the AddressID to the filter so it will be included
                    filter.Add(addressId);
                }
            }
        }

        private string GetAddressTypeText(string value)
        {
            switch (value)
            {
                case "ShipAddressID":
                    return "Shipping";
                case "BillAddressID":
                    return "Billing";
                default:
                    throw new NotImplementedException();
            }
        }

        private DataSet GetDataSet()
        {
            var ds = CacheManager.Current.CacheData();
            if (ds == null)
                throw new AjaxException(400, "No data found in cache.");
            return ds;
        }

        private int GetContextId(HttpContext context)
        {
            if (int.TryParse(context.Request.QueryString["id"], out int accountId))
                return accountId;
            else
                throw new AjaxException(400, "Invalid id.");
        }

        private string GetContextType(HttpContext context)
        {
            var type = context.Request.QueryString["type"];
            return type;
        }

        private AddressItem GetContextAddressItem(HttpContext context)
        {
            if (int.TryParse(context.Request.Form["addressId"], out int addressId))
            {
                var item = new AddressItem()
                {
                    AddressID = addressId,
                    AddressType = context.Request.Form["addressType"],
                    Attention = context.Request.Form["attention"],
                    AddressLine1 = context.Request.Form["addressLine1"],
                    AddressLine2 = context.Request.Form["addressLine2"],
                    City = context.Request.Form["city"],
                    State = context.Request.Form["state"],
                    Zip = context.Request.Form["zip"],
                    Country = context.Request.Form["country"]
                };

                return item;
            }
            else
            {
                throw new AjaxException(400, "Invalid value for addressId.");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }

    public class AjaxException : Exception
    {
        private string _message;

        public int StatusCode { get; }
        public override string Message => _message;

        public AjaxException(int statusCode, string message)
        {
            StatusCode = statusCode;
            _message = message;
        }
    }
}

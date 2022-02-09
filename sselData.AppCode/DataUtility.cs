using LNF.CommonTools;
using LNF.Data;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace sselData.AppCode
{
    public class DataUtility
    {
        public struct ClientInfo
        {
            public int ClientOrgID;
            public int ClientID;
            public string DisplayName;

        }
        public static string CacheID
        {
            get
            {
                if (HttpContext.Current.Session["Cache"] == null)
                    HttpContext.Current.Session["Cache"] = Guid.NewGuid().ToString();
                return HttpContext.Current.Session["Cache"].ToString();
            }
        }

        public static void SetActiveFalse(DataRow dr)
        {
            dr["Active"] = false;
        }

        public static bool IsReactivated(DataRow dr)
        {
            return dr["Reactivated"] != DBNull.Value && dr.Field<bool>("Reactivated");
        }

        public static bool HasActiveAccount(DataRow cdr, DataTable dtClientOrg, DataTable dtClientAccount)
        {
            if (cdr == null)
                throw new ArgumentNullException("cdr");
            if (dtClientOrg == null)
                throw new ArgumentNullException("dtClientOrg");
            if (dtClientAccount == null)
                throw new ArgumentNullException("dtClientAccount");

            int uPriv = Convert.ToInt32(cdr["Privs"]);
            int storePriv = (int)ClientPrivilege.StoreUser;
            if ((uPriv & storePriv) == storePriv) uPriv -= storePriv;

            int accessPriv = (int)ClientPrivilege.PhysicalAccess;
            if ((uPriv & accessPriv) == accessPriv) uPriv -= accessPriv;

            accessPriv = (int)ClientPrivilege.OnlineAccess;
            if ((uPriv & accessPriv) == accessPriv) uPriv -= accessPriv;

            if (uPriv == (int)ClientPrivilege.LabUser) // only affects Lab User only clients
            {
                DataRow[] codrs = dtClientOrg.Select(string.Format("ClientID = {0} AND Active = 1", cdr["ClientID"]));
                for (int i = 0; i < codrs.Length; i++) // check client in all orgs
                {
                    DataRow[] cadrs = dtClientAccount.Select(string.Format("ClientOrgID = {0} AND Active = 1", codrs[i]["ClientOrgID"]));
                    if (cadrs.Length > 0) // for active accounts
                        return true;
                }

                return false;
            }
            else
                return true;
        }

        public static ClientInfo GetClientInfo(int clientOrgID)
        {
            ClientInfo result = new ClientInfo();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT co.ClientOrgID, c.ClientID, DisplayName = c.LName + ', ' + c.FName FROM Client c INNER JOIN ClientOrg co ON co.ClientID = c.ClientID WHERE co.ClientOrgID = @ClientOrgID", conn))
                {
                    cmd.Parameters.AddWithValue("@ClientOrgID", clientOrgID);
                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adap.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        result.ClientOrgID = Convert.ToInt32(dr["ClientOrgID"]);
                        result.ClientID = Convert.ToInt32(dr["ClientID"]);
                        result.DisplayName = dr["DisplayName"].ToString();
                    }
                }
            }

            return result;
        }

        public static void GetOrgData(DataSet ds)
        {
            if (ds.Tables.Contains("Org"))
                ds.Tables.Remove("Org");

            DataCommand.Create().MapSchema().Param("Action", "All").FillDataSet(ds, "sselData.dbo.Org_Select", "Org"); // to speed table lookups

            ds.Tables["Org"].PrimaryKey = new[] { ds.Tables["Org"].Columns["OrgID"] };
        }

        public static void GetAccountData(DataSet ds, int orgId)
        {
            if (ds.Tables.Contains("Account"))
                ds.Tables.Remove("Account");

            DataCommand.Create().MapSchema().Param(new { Action = "AllByOrg", OrgID = orgId }).FillDataSet(ds, "Account_Select", "Account");
        }

        public static void GetClientData(DataSet ds)
        {
            if (ds.Tables.Contains("Client"))
                ds.Tables.Remove("Client");

            DataCommand.Create().Param(new { Action = "All", sDate = DateTime.Parse("2000-01-01") }).FillDataSet(ds, "Client_Select", "Client");
            ds.Tables["Client"].Columns.Add("DisplayInfo", typeof(string));
            ds.Tables["Client"].Columns.Add("Reactivated", typeof(bool));
            ds.Tables["Client"].Columns.Add("EnableAccess", typeof(bool));
            ds.Tables["Client"].PrimaryKey = new[] { ds.Tables["Client"].Columns["ClientID"] };
            ds.Tables["Client"].PrimaryKey[0].AutoIncrement = true;
            ds.Tables["Client"].PrimaryKey[0].AutoIncrementSeed = -1;
            ds.Tables["Client"].PrimaryKey[0].AutoIncrementStep = -1;
        }

        public static void GetClientOrgData(DataSet ds)
        {
            if (ds.Tables.Contains("ClientOrg"))
                ds.Tables.Remove("ClientOrg");

            DataCommand.Create().Param(new { Action = "All" }).FillDataSet(ds, "ClientOrg_Select", "ClientOrg");
            ds.Tables["ClientOrg"].Columns.Add("Reactivated", typeof(bool));
            ds.Tables["ClientOrg"].PrimaryKey = new[] { ds.Tables["ClientOrg"].Columns["ClientOrgID"] };
            ds.Tables["ClientOrg"].PrimaryKey[0].AutoIncrement = true;
            ds.Tables["ClientOrg"].PrimaryKey[0].AutoIncrementSeed = -1;
            ds.Tables["ClientOrg"].PrimaryKey[0].AutoIncrementStep = -1;
        }

        public static void GetClientAccountData(DataSet ds)
        {
            if (ds.Tables.Contains("ClientAccount"))
                ds.Tables.Remove("ClientAccount");

            DataCommand.Create().Param(new { Action = "WithAccountName" }).FillDataSet(ds, "ClientAccount_Select", "ClientAccount");
            ds.Tables["ClientAccount"].Columns.Add("Reactivated", typeof(bool));
            ds.Tables["ClientAccount"].PrimaryKey = new[] { ds.Tables["ClientAccount"].Columns["ClientAccountID"] };
            ds.Tables["ClientAccount"].PrimaryKey[0].AutoIncrement = true;
            ds.Tables["ClientAccount"].PrimaryKey[0].AutoIncrementSeed = -1;
            ds.Tables["ClientAccount"].PrimaryKey[0].AutoIncrementStep = -1;
        }

        public static void GetClientManagerData(DataSet ds, int orgId)
        {
            if (ds.Tables.Contains("ClientManager"))
                ds.Tables.Remove("ClientManager");

            DataCommand.Create().Param(new { Action = "ByOrg", OrgID = orgId }).FillDataSet(ds, "ClientManager_Select", "ClientManager");
            ds.Tables["ClientManager"].Columns.Add("Reactivated", typeof(bool));
            ds.Tables["ClientManager"].PrimaryKey = new[] { ds.Tables["ClientManager"].Columns["ClientManagerID"] };
            ds.Tables["ClientManager"].PrimaryKey[0].AutoIncrement = true;
            ds.Tables["ClientManager"].PrimaryKey[0].AutoIncrementSeed = -1;
            ds.Tables["ClientManager"].PrimaryKey[0].AutoIncrementStep = -1;
        }

        public static void GetAddressData(DataSet ds)
        {
            if (ds.Tables.Contains("Address"))
                ds.Tables.Remove("Address");

            // AddDelete is normally null
            // when an address is added, it is set to true
            // when an address is deleted, it is set to false
            // when saving, save address with true and delete those with false
            // when quiting, delete address with false and save those with true
            DataCommand.Create().MapSchema().Param(new { Action = "All" }).FillDataSet(ds, "Address_Select", "Address");
            ds.Tables["Address"].Columns.Add("AddDelete", typeof(bool));
            ds.Tables["Address"].PrimaryKey = new[] { ds.Tables["Address"].Columns["AddressID"] };
            ds.Tables["Address"].PrimaryKey[0].AutoIncrement = true;
            ds.Tables["Address"].PrimaryKey[0].AutoIncrementSeed = -1;
            ds.Tables["Address"].PrimaryKey[0].AutoIncrementStep = -1;
        }

        public static void GetDryBoxData(DataSet ds)
        {
            if (ds.Tables.Contains("DryBox"))
                ds.Tables.Remove("DryBox");

            DataCommand.Create(CommandType.Text)
                .FillDataSet(ds, "SELECT * FROM dbo.v_DryBoxCurrentAssignments", "DryBox");
        }

        public static DataSet GetClientDataSet(int orgId)
        {
            DataSet ds = new DataSet("Client");

            // get client data
            GetClientData(ds);

            // display name column is appended to facilitate manager display
            GetClientOrgData(ds);

            // manager info
            GetClientManagerData(ds, orgId);

            // get org info
            GetOrgData(ds);

            // get account info
            GetAccountData(ds, orgId);

            // get client account info
            GetClientAccountData(ds);

            // Get the Address
            GetAddressData(ds);

            // Get drybox data
            GetDryBoxData(ds);

            return ds;
        }

        public static void StoreClientInfo(DataTable dtClient, DataTable dtClientOrg, DataTable dtClientAccount, DataTable dtClientManager, DataTable dtAddress, bool isNewClientEntry, bool addExistingClient, int orgId, string username, string lname, string fname, string mname, int demCitizenId, int demEthnicId, int demRaceId, int demGenderId, int demDisabilityId, int privs, int communities, int technicalInterestId, int roleId, int departmentId, string email, string phone, bool isManager, bool isFinManager, DateTime subsidyStartDate, DateTime newFacultyStartDate, ref int clientId, ref int clientOrgId, out string alertMsg, out bool enableAccessError)
        {
            // add rows to Client, ClientSite and ClientOrg for new entries
            bool isNewClientOrgEntry = false;

            DataRow cdr;

            if (isNewClientEntry)
            {
                // add an entry to the client table
                cdr = dtClient.NewRow();
                cdr.SetField("DisplayName", string.Format("{0}, {1}", lname, fname)); // has to be done here
                clientId = cdr.Field<int>("ClientID");
            }
            else
            {
                // get the entry in the client table
                cdr = dtClient.Rows.Find(clientId);
            }

            // if entering new or modifying, update the fields
            if (!addExistingClient)
            {
                cdr.SetField("FName", fname);
                cdr.SetField("LName", lname);

                // strip period if entered
                if (mname.Length > 0)
                {
                    if (mname.EndsWith("."))
                        mname = mname.Remove(mname.Length - 1, 1);
                }
                cdr.SetField("MName", mname);

                cdr.SetField("UserName", username);

                //Encryption enc = new Encryption();
                //cdr.SetField("Password", enc.EncryptText(username));
                cdr.SetField("Password", DBNull.Value);
                cdr.SetField("PasswordHash", DBNull.Value);

                cdr.SetField("DemCitizenID", demCitizenId);
                cdr.SetField("DemEthnicID", demEthnicId);
                cdr.SetField("DemRaceID", demRaceId);
                cdr.SetField("DemGenderID", demGenderId);
                cdr.SetField("DemDisabilityID", demDisabilityId);

                // store Privs's
                cdr.SetField("Privs", privs);

                cdr.SetField("Communities", communities);

                cdr.SetField("TechnicalInterestID", technicalInterestId);
            }

            // next the ClientOrg table
            DataRow codr;
            DataRow[] codrs = dtClientOrg.Select(string.Format("ClientID = {0} AND OrgID = {1}", clientId, orgId));

            if (codrs.Length == 0) // need new row in clientOrg
            {
                isNewClientOrgEntry = true;
                codr = dtClientOrg.NewRow();
                codr.SetField("ClientID", clientId);
                codr.SetField("OrgID", orgId);
                codr.SetField("Active", true);
                codr.SetField("ClientAddressID", 0);
            }
            else
            {
                codr = codrs[0];
                clientOrgId = codr.Field<int>("ClientOrgID");
                if (!codr.Field<bool>("Active"))
                    codr.SetField("Active", true);
            }

            codr.SetField("RoleID", roleId);
            codr.SetField("DepartmentID", departmentId);
            codr.SetField("Email", email);
            codr.SetField("Phone", phone);
            codr.SetField("IsManager", isManager);
            codr.SetField("IsFinManager", isFinManager);
            codr.SetField("SubsidyStartDate", subsidyStartDate);
            codr.SetField("NewFacultyStartDate", newFacultyStartDate);

            // find any address that need to be dealt with
            DataRow[] sdrs = dtAddress.Select("AddDelete IS NOT NULL");
            for (int i = 0; i < sdrs.Length; i++)
            {
                if (sdrs[i].Field<bool>("AddDelete")) // addr was added
                {
                    int addressId = 0;
                    if (sdrs[i]["AddressID"] != DBNull.Value)
                        addressId = sdrs[i].Field<int>("AddressID");
                    codr.SetField("ClientAddressID", addressId);
                    sdrs[i].SetField("AddDelete", DBNull.Value);
                }
                else
                {
                    codr.SetField("ClientAddressID", 0);
                    sdrs[i].Delete();
                }
            }

            if (isNewClientOrgEntry)
                dtClientOrg.Rows.Add(codr);

            // update rows in ClientManager as needed
            DataRow[] cmdrs = dtClientManager.Select("ClientOrgID = 0");
            for (int i = 0; i < cmdrs.Length; i++)
                cmdrs[i].SetField("ClientOrgID", codr.Field<int>("ClientOrgID"));

            // update rows in ClientAccount as needed
            DataRow[] cadrs = dtClientAccount.Select("ClientOrgID = 0");
            for (int i = 0; i < cadrs.Length; i++)
                cadrs[i].SetField("ClientOrgID", codr.Field<int>("ClientOrgID"));

            // done here after ClientAccount has been updated
            if (addExistingClient) // reenabling a client
                cdr.SetField("EnableAccess", PrivUtility.HasPriv((ClientPrivilege)cdr.Field<int>("Privs"), ClientPrivilege.PhysicalAccess));
            else
                cdr.SetField("EnableAccess", PrivUtility.HasPriv((ClientPrivilege)privs, ClientPrivilege.PhysicalAccess));

            alertMsg = string.Empty;
            // for clients who have Lab User Privs only, only allow access if s/he has an active account
            // if access is not enabled, show an alert
            if (cdr.Field<bool>("EnableAccess"))
            {
                if (!HasActiveAccount(cdr, dtClientOrg, dtClientAccount))
                {
                    cdr.SetField("EnableAccess", false);
                    alertMsg = "Store and physical access disabled for this client - no active accounts.";
                }
            }

            // if client has been disabled for a 'long time', do not enable access and alert user
            enableAccessError = false;
            if (addExistingClient && cdr.Field<bool>("EnableAccess"))
            {
                try
                {
                    int cid = cdr.Field<int>("ClientID");
                    bool result = DataCommand.Create()
                        .Param("Action", "AllowReenable")
                        .Param("ClientID", cid)
                        .ExecuteScalar<bool>("NexWatch_Select").Value;
                    cdr.SetField("EnableAccess", result);
                }
                catch (Exception ex)
                {
                    enableAccessError = true;
                    alertMsg = ex.Message;
                    return;
                }

                if (!cdr.Field<bool>("EnableAccess"))
                {
                    int p = cdr.Field<int>("Privs") - (int)ClientPrivilege.PhysicalAccess;
                    cdr.SetField("Privs", p);
                    alertMsg += "Note that this client has been inactive for so long that access is not automatically reenabled. Please see the Lab Manager.";
                }
            }

            if (isNewClientEntry)
                dtClient.Rows.Add(cdr);
        }
    }
}
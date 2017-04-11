using LNF.Repository;
using System.Data;

namespace sselData.AppCode.DAL
{
    public class BillingTypeDA
    {
        public static DataTable GetAllBillingTypes()
        {
            using (var dba = DA.Current.GetAdapter())
                return dba.ApplyParameters(new { Action = "All" }).FillDataTable("BillingType_Select");
        }

        public static int GetBillingTypeID(int clientOrgId)
        {
            using (var dba = DA.Current.GetAdapter())
                return dba.ApplyParameters(new { Action = "GetCurrentTypeID", ClientOrgID = clientOrgId }).ExecuteScalar<int>("ClientOrgBillingTypeTS_Select");
        }

        public static bool SetBillingTypeID(int clientOrgId, int billingTypeId)
        {
            //For some reason, it's possible ClientOrgID is less than 1.  We have to catch that here
            if (clientOrgId < 1) return false;

            using (var dba = DA.Current.GetAdapter())
            { 
                dba.AddParameter("@ClientOrgID", clientOrgId);
                dba.AddParameter("@BillingTypeID", billingTypeId);
                int result = dba.ExecuteNonQuery("ClientOrgBillingTypeTS_Insert");
                return result == 1;
            }
        }
    }
}
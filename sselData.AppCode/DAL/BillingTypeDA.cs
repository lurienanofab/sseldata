using LNF.Repository;
using System.Data;

namespace sselData.AppCode.DAL
{
    public class BillingTypeDA
    {
        public static DataTable GetAllBillingTypes()
        {
            return DataCommand.Create()
                .Param("Action", "All")
                .FillDataTable("BillingType_Select");
        }

        public static int GetBillingTypeID(int clientOrgId)
        {
            return DataCommand.Create()
                .Param("Action", "GetCurrentTypeID")
                .Param("ClientOrgID", clientOrgId)
                .ExecuteScalar<int>("ClientOrgBillingTypeTS_Select").Value;
        }

        public static bool SetBillingTypeID(int clientOrgId, int billingTypeId)
        {
            //For some reason, it's possible ClientOrgID is less than 1.  We have to catch that here
            if (clientOrgId < 1) return false;

            var result = DataCommand.Create()
                .Param("ClientOrgID", clientOrgId)
                .Param("BillingTypeID", billingTypeId)
                .ExecuteNonQuery("ClientOrgBillingTypeTS_Insert").Value;

            return result == 1;
        }
    }
}
using LNF.Repository;
using System.Data;

namespace sselData.AppCode.DAL
{
    public class AccountTypeDA
    {
        public static DataTable GetAllAccountTypes()
        {
            using (var dba = DA.Current.GetAdapter())
                return dba.ApplyParameters(new { Action = "All" }).FillDataTable("AccountType_Select");
        }
    }
}
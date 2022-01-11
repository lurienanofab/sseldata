using LNF.Repository;
using System.Data;

namespace sselData.AppCode.DAL
{
    public class AccountTypeDA
    {
        public static DataTable GetAllAccountTypes()
        {
            return DataCommand.Create()
                .Param("Action", "All")
                .FillDataTable("AccountType_Select");
        }
    }
}
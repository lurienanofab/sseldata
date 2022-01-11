using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Data;

namespace sselData.AppCode.DAL
{
    public class UserGroupDA
    {
        public static DataTable GetAllUserGroups()
        {
            return DataCommand.Create()
                .Param("Action", "All")
                .FillDataTable("UserGroup_Select");
        }

        [Obsolete("No longer used?")]
        public static List<int> GetUserGroupIDs()
        {
            List<int> result = new List<int>();
            var cmd = DataCommand.Create().Param("Action", "All");
            using (var reader = cmd.ExecuteReader("ClientOrgUserGroupTS"))
            {
                while (reader.Read())
                    result.Add(reader.Value("UserGroupID", 0));
            }
            return result;
        }
    }
}
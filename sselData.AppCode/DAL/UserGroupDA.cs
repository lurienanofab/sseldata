using LNF.CommonTools;
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
            using (var dba = DA.Current.GetAdapter())
                return dba.ApplyParameters(new { Action = "All" }).FillDataTable("UserGroup_Select");
        }

        [Obsolete("No longer used?")]
        public static List<int> GetUserGroupIDs()
        {
            using (var dba = DA.Current.GetAdapter())
            {
                List<int> result = new List<int>();
                IDataReader reader = dba.ApplyParameters(new { Action = "All" }).ExecuteReader("ClientOrgUserGroupTS");
                while (reader.Read())
                    result.Add(reader.Value("UserGroupID", 0));
                return result;
            }
        }
    }
}
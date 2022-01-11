using LNF.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sselData.AppCode;
using System;
using System.Data;

namespace sselData.Tests
{
    [TestClass]
    public class AppCode_DataUtility_Tests
    {
        [TestMethod]
        public void CanStoreClientInfo()
        {
            DataSet dsClient = DataUtility.GetClientDataSet(17);
            int clientId = 0;
            int selectedClientOrgId = 0;

            DataUtility.StoreClientInfo(
                dtClient: dsClient.Tables["Client"],
                dtClientOrg: dsClient.Tables["ClientOrg"],
                dtClientAccount: dsClient.Tables["ClientAccount"],
                dtClientManager: dsClient.Tables["ClientManager"],
                dtAddress: dsClient.Tables["Address"],
                isNewClientEntry: true,
                addExistingClient: false,
                orgId: 17,
                username: "test" + DateTime.Now.Ticks.ToString(),
                lname: "user",
                fname: "test",
                mname: string.Empty,
                demCitizenId: 1,
                demEthnicId: 1,
                demRaceId: 1,
                demGenderId: 1,
                demDisabilityId: 1,
                privs: PrivUtility.CalculatePriv(ClientPrivilege.LabUser | ClientPrivilege.PhysicalAccess | ClientPrivilege.OnlineAccess),
                communities: 0,
                technicalInterestId: 1,
                roleId: 1,
                departmentId: 1,
                email: "test@test.com",
                phone: string.Empty,
                isManager: false,
                isFinManager: false,
                subsidyStartDate: DateTime.Now,
                newFacultyStartDate: DateTime.Now,
                clientId: ref clientId,
                clientOrgId: ref selectedClientOrgId,
                alertMsg: out string alertMsg,
                enableAccessError: out bool enableAccessError);
        }
    }
}

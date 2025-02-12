
using SuperTransp.Models;

namespace SuperTransp.Core
{
    public class Interfaces
    {
        public interface ISecurity
        {
            public SecurityUserModel GetValidUser(string login, string password);
            public List<SecurityUserModel> GetAllUsers();
            public List<SecurityUserModel> GetAllUsersByStateId(int stateId);
            public SecurityUserModel GetUserById(int securityUserId);
            public bool GroupModuleHasAccess(int securityGroupId, int securityModuleId);
			public List<SecurityGroupModel> GetAllGroups();
			public List<SecurityGroupModel> GetGroupById(int groupId);
            public List<SecurityModuleModel> GetModuleById(int securityModuleId);
            public List<SecurityModuleModel> GetAllModules();
			public List<SecurityStatusUserModel> GetAllUsersStatus();
			public List<SecurityAccessTypeModel> GetAllAccessTypes();
            public List<SecurityGroupModuleModel> GetAllSecurityGroupModuleDetail();
			public int AddOrEditUser(SecurityUserModel model);
			public int RegisteredUser(string paramValue, string verifyBy);
			public int AddOrEditGroup(SecurityGroupModel model);
            public int AddOrEditModule(SecurityModuleModel model);
            public int AddOrEditGroupModules(SecurityGroupModuleModel model);
            public int DeleteGroupModules(int securityGroupModuleId);
		}
        public interface IGeography
        {
            public List<GeographyModel> GetAllStates();
            public List<GeographyModel> GetStateById(int stateId);
		}
	}
}

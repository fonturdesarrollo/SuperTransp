
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
			public List<SecurityStatusUserModel> GetAllUsersStatus();
            public int AddOrEdit(SecurityUserModel model);
			public int RegisteredUser(string paramValue, string verifyBy);
		}
        public interface IGeography
        {
            public List<GeographyModel> GetAllStates();
            public List<GeographyModel> GetStateById(int stateId);
		}
	}
}

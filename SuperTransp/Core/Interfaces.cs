
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
            public bool GroupHasAccessToModule(int securityGroupId, int securityModuleId);
			public List<SecurityGroupModel> GetAllGroups();
			public List<SecurityGroupModel> GetGroupById(int groupId);
            public List<SecurityModuleModel> GetModuleById(int securityModuleId);
            public List<SecurityModuleModel> GetAllModules();
			public List<SecurityStatusUserModel> GetAllUsersStatus();
			public List<SecurityAccessTypeModel> GetAllAccessTypes();
            public List<SecurityGroupModuleModel> GetAllSecurityGroupModuleDetail();
			public List<SecurityModuleModel> GetModulesByGroupId(int groupId);
			public int AddOrEditUser(SecurityUserModel model);
			public int RegisteredUser(string paramValue, string verifyBy);
			public int AddOrEditGroup(SecurityGroupModel model);
            public int AddOrEditModule(SecurityModuleModel model);
            public int AddOrEditGroupModules(SecurityGroupModuleModel model);
            public int DeleteGroupModules(int securityGroupModuleId);
			public int ChangePassword(SecurityUserModel model);
			public bool OldPasswordValid(int securityUserId, string oldPassword);
			public string? Encrypt(string plainText);
			public string? Decrypt(string encryptedText);
		}
        public interface IGeography
        {
            public List<GeographyModel> GetAllStates();
            public List<GeographyModel> GetStateById(int stateId);
			public List<GeographyModel> GetAllMunicipalities();
			public List<GeographyModel> GetMunicipalityByStateId(int stateId);
		}

        public interface IDesignation
        {
			public List<DesignationViewModel> GetAll();
		}

		public interface IMode
		{
			public List<ModeViewModel> GetAll();
		}

		public interface IUnion
		{
			public List<UnionViewModel> GetAll();
		}

		public interface IPublicTransportGroup
        {
			public int AddOrEdit(PublicTransportGroupModel model);
			public PublicTransportGroupModel GetPublicTransportGroupById(int publicTransportGroupId);
			public string? RegisteredRif(string publicTransportGroupRif);
			public List<PublicTransportGroupModel> GetAll();
			public List<PublicTransportGroupModel> GetByStateId(int stateId);
		}
	}
}


using SuperTransp.Models;

namespace SuperTransp.Core
{
	public class Interfaces
	{
		public interface ISecurity
		{
			public SecurityUserViewModel GetValidUser(string login, string password);
			public List<SecurityUserViewModel> GetAllUsers();
			public List<SecurityUserViewModel> GetAllUsersByStateId(int stateId);
			public List<SecurityUserViewModel> GetAllUsersByGroupId(int securityGroupId);
			public SecurityUserViewModel GetUserById(int securityUserId);
			public bool GroupHasAccessToModule(int securityGroupId, int securityModuleId);
			public List<SecurityGroupModel> GetAllGroups();
			public List<SecurityGroupModel> GetGroupById(int groupId);
			public List<SecurityModuleModel> GetModuleById(int securityModuleId);
			public List<SecurityModuleModel> GetAllModules();
			public List<SecurityStatusUserModel> GetAllUsersStatus();
			public List<SecurityAccessTypeModel> GetAllAccessTypes();
			public List<SecurityGroupModuleModel> GetAllSecurityGroupModuleDetail();
			public List<SecurityModuleModel> GetModulesByGroupId(int groupId);
			public int AddOrEditUser(SecurityUserViewModel model);
			public int RegisteredUser(string paramValue, string verifyBy);
			public int AddOrEditGroup(SecurityGroupModel model);
			public int AddOrEditModule(SecurityModuleModel model);
			public int AddOrEditGroupModules(SecurityGroupModuleModel model);
			public int DeleteGroupModules(int securityGroupModuleId);
			public int ChangePassword(SecurityUserViewModel model);
			public bool OldPasswordValid(int securityUserId, string oldPassword);
			public string? Encrypt(string plainText);
			public string? Decrypt(string encryptedText);
			public int AddLogbook(int processId, bool isDeleteAction, string actionDescription);
			public bool IsTotalAccess(int securityModuleId);
			public bool IsUpdateAccess(int securityModuleId);
			public bool BlockLogin(string login);
			public bool IsBlockedLogin(string login);
			public bool IsInactiveLogin(string login);
			public string GeneratePublicKey();
			public bool ValidateKey(string key, byte[] sign);
			public List<SecurityLogbookModel> GetLogbookByStateName(string userState, string filterType);
			public List<SecurityLogbookModel> GetLogbookAllExceptAdminByStateName(string selectStateName, string filterType);
			public List<SecurityLogbookModel> GetLogbookAllBySelectedStateName(string selectedStateName, string filterType);
		}
		public interface IGeography
		{
			public List<GeographyViewModel> GetAllStates();
			public List<GeographyViewModel> GetStateById(int stateId);
			public List<GeographyViewModel> GetAllMunicipalities();
			public List<GeographyViewModel> GetMunicipalityByStateId(int stateId);
		}

		public interface IDesignation
		{
			public List<DesignationViewModel> GetAll();
			public int AddOrEdit(DesignationViewModel model);
		}

		public interface IMode
		{
			public int AddOrEdit(ModeViewModel model);
			public List<ModeViewModel> GetAll();
		}

		public interface IUnion
		{
			public int AddOrEdit(UnionViewModel model);
			public List<UnionViewModel> GetAll();
			public List<UnionViewModel> GetByStateId(int stateId);
		}

		public interface IVehicleData
		{
			public List<VehicleDataViewModel> GetAll();
		}

		public interface IPublicTransportGroup
		{
			public int AddOrEdit(PublicTransportGroupViewModel model);
			public PublicTransportGroupViewModel GetPublicTransportGroupById(int publicTransportGroupId);
			public string? RegisteredRif(string publicTransportGroupRif);
			public List<PublicTransportGroupViewModel> GetAll();
			public List<PublicTransportGroupViewModel> GetAllByStateId(int stateId);
			public List<PublicTransportGroupViewModel> GetAllStatisticsByStateId(int stateId);
			public List<PublicTransportGroupViewModel> GetAllStatistics();
			public List<PublicTransportGroupViewModel> GetAllBySupervisedDriversAndStateIdAndNotSummaryAdded(int stateId);
			public List<PublicTransportGroupViewModel> GetAllBySupervisedDriversAndNotSummaryAdded();
			public PublicTransportGroupViewModel GetByGUIDId(string guidId);
		}
		public interface IDriver
		{
			public int AddOrEdit(DriverViewModel model);
			public List<DriverViewModel> GetByPublicTransportGroupId(int publicTransportGroupId);
			public DriverViewModel GetByDriverPublicTransportGroupId(int driverPublicTransportGroupId);
			public DriverViewModel GetByIdentityDocument(int driverIdentityDocument);
			public bool RegisteredDocumentId(int driverIdentityDocument, int publicTransportGroupId);
			public bool RegisteredPhone(string driverPhone, int publicTransportGroupId);
			public bool RegisteredPartnerNumber(int partnerNumber, int publicTransportGroupId);
			public DriverViewModel GetById(int driverId);
			public bool Delete(int driverId);
			public int TotalDriversByPublicTransportGroupId(int publicTransportGroupId);
		}

		public interface ISupervision
		{
			public int AddOrEdit(SupervisionViewModel model);
			public int AddSimple(SupervisionViewModel model);
			public int AddOrEditRound(SupervisionRoundModel model);
			public SupervisionViewModel GetById(int supervisionId);
			public SupervisionViewModel GetByPublicTransportGroupIdAndDriverIdAndPartnerNumberStateId(int publicTransportGroupId, int driverId, int partnerNumber, int stateId);
			public SupervisionViewModel GetByPublicTransportGroupGUIDAndPartnerNumber(string publicTransportGroupGUID, int partnerNumber);
			public List<PublicTransportGroupViewModel> GetDriverPublicTransportGroupByStateIdAndPTGRif(int stateId, string ptgRif);
			public List<PublicTransportGroupViewModel> GetDriverPublicTransportGroupByPtgId(int publicTransportGroupId);
			public List<PublicTransportGroupViewModel> GetAllDriverPublicTransportGroup(string ptgRif);
			public List<PublicTransportGroupViewModel> RegisteredPlate(string plate);
			public int AddOrEditSummary(SupervisionSummaryViewModel model);
			public List<SupervisionSummaryViewModel> GetAllSupervisionSummary();
			public List<SupervisionSummaryViewModel> GetSupervisionSummaryByStateId(int stateId);
			public SupervisionSummaryViewModel GetSupervisionSummaryById(int supervisionSummaryId);
			public bool IsActiveSupervisionRoundByStateMonthAndYear(int stateId, int month, int year);
			public bool IsFinishedSupervisionRoundByStateMonthAndYear(int stateId, int month, int year);
			public bool IsSupervisionSummaryDoneByPtgId(int publicTransportGroupId);
			public bool IsUserSupervisingPublicTransportGroup(int securityUserId, int publicTransportGroupId);
			public bool DeletePicturesByPTGIdAndPartnerNumber(int publicTransportGroupId, int partnerNumber);
			public List<SupervisionPictures> GetPicturesByPTGIdAndPartnerNumber(int publicTransportGroupId, int partnerNumber);
		}

		public interface ICommonData
		{
			public List<CommonDataViewModel> GetYesNo();
			public List<CommonDataViewModel> GetYears();
			public List<CommonDataViewModel> GetCurrentYears();
			public List<CommonDataViewModel> GetMonthNames();
			public List<CommonDataViewModel> GetMakesByYear(int year);
			public List<CommonDataViewModel> GetModelsByYearAndMake(int year, string make);
			public CommonDataViewModel GetVehicleDataById(int? vehicleDataId);
			public List<CommonDataViewModel> GetPassengers();
			public List<CommonDataViewModel> GetRims();
			public List<CommonDataViewModel> GetWheels();
			public List<CommonDataViewModel> GetFuelTypes();
			public List<CommonDataViewModel> GetTankCapacity();
			public List<CommonDataViewModel> GetBatteries();
			public List<CommonDataViewModel> GetNumberOfBatteries();
			public List<CommonDataViewModel> GetMotorOil();
			public List<CommonDataViewModel> GetOilLitters();
			public List<CommonDataViewModel> GetFailureType();
			public int AddOrEditMakeModel(CommonDataViewModel model);
			public CommonDataViewModel GetCommonDataValueByName(string commonDataName);
			public List<CommonDataViewModel> GetSex();
		}

		public interface IReport
		{
			public List<PublicTransportGroupViewModel> GetAllSupervisedVehiclesStatistics();
			public List<PublicTransportGroupViewModel> GetAllSupervisedVehiclesStatisticsByStateId(int stateId);
			public List<PublicTransportGroupViewModel> GetAllSupervisedDriversStatisticsInEstate();
			public List<PublicTransportGroupViewModel> GetAllSupervisedDriversStatisticsInEstateByStateId(int stateId);
		}
		public interface IFtpService
		{
			Task DeleteFilesInFolderAsync(string folderPath);
			Task UploadFileAsync(Stream fileStream, string folderPath, string fileName);
			Task<List<string>> ListFilesAsync(string folderPath);
			Task<bool> FolderExistsAsync(string folderPath);
			Task CreateFolderAsync(string folderPath);
			Task DeleteFolderAsync(string folderPath);
			Task TransferFileAsync(string sourcePath, string destinationPath);
			Task DeleteFileAsync(string filePath);
		}
	}
}

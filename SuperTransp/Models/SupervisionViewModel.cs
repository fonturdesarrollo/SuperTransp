using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace SuperTransp.Models
{
	public class SupervisionViewModel
	{
		[Key]
		public int SupervisionId { get; set; }
		public bool InPerson { get; set; }
		public bool DriverWithVehicle { get; set; }
		public bool WorkingVehicle { get; set; }
		public int DriverId { get; set; }
		public string? Plate { get; set; }
		public int? VehicleDataId { get; set; }
		public int? Passengers { get; set; }
		public int? RimId { get; set; }
		public int? Wheels { get; set; }
		public int? MotorOilId { get; set; }
		public int? Liters { get; set; }
		public int? FuelTypeId { get; set; }
		public int? TankCapacity { get; set; }
		public int? BatteryId { get; set; }
		public int? NumberOfBatteries { get; set; }
		public int? FailureTypeId { get; set; }	
		public bool FingerprintTrouble { get; set; }
		public string? Remarks { get; set; }
		public DateTime SupervisionDateAdded { get; set; }
		public int SecurityUserId { get; set; }
		public string? PTGCompleteName { get; set; }
		public int PublicTransportGroupId { get; set; }
		public int DriverIdentityDocument { get; set; }
		public string? DriverFullName { get; set; }
		public int PartnerNumber { get; set; }
		public string? StateName { get; set; }
		public string? PublicTransportGroupRif { get; set; }
		public bool? SupervisionStatus { get; set; }
		public string? VehicleImageUrl { get; set; }
		public int ModeId { get; set; }
		public string? ModeName { get; set; }
		public string? SupervisionStatusName { get; set; }
		public int TotalDrivers { get; set; }
		public int TotalSupervisedDrivers { get; set; }
		public int? Year { get; set; }
		public string? Make { get; set; }
		public string? Model { get; set; }
		public string? RimName { get; set; }
		public string? MotorOilName { get; set; }
		public string? FuelTypeName { get; set; }
		public string? BatteryName { get; set; }
		public string? FailureTypeName { get; set; }
		public int StateId { get; set; }
		public int DriverPublicTransportGroupId { get; set; }
		public List<SupervisionPictures>? Pictures { get; set; }
	}

	public class SupervisionPictures
	{
		[Key]
		public int SupervisionPictureId { get; set; }
		public int PublicTransportGroupId { get; set; }
		public int PartnerNumber { get; set; }
		public int DriverId { get; set; }
		public string? VehicleImageUrl { get; set; }
		public DateTime SupervisionPictureDateAdded { get; set; }
	}

	public class SupervisionRoundModel
	{
		[Key]
		public int SupervisionRoundId { get; set; }
		public int StateId { get; set; }
		public DateTime SupervisionRoundStartDate { get; set; }
		public string? SupervisionRoundStartDescription { get; set; }
		public DateTime? SupervisionRoundEndDate { get; set; }
		public string? SupervisionRoundEndDescription { get; set; }
		public bool SupervisionRoundStatus { get; set; }
		public int TotalPTG { get; set; }
		public int TotalPartners { get; set; }
		public int TotalSupervisedDrivers { get; set; }
		public int TotalWorkingVehicles { get; set; }
		public int TotalNotInOperationVehicles { get; set; }
		public int TotalPartersWithoutVehicle { get; set; }
		public int TotalAbsentDrivers { get; set; }
	}

	public class PublicTransportGroupDriverListPageVM
	{
		public string PTGRifName { get; set; } = "";
		public List<PublicTransportGroupViewModel> Items { get; set; } = new();
	}
}

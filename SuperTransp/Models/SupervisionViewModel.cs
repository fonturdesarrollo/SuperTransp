using System.ComponentModel.DataAnnotations;

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
		public int Year { get; set; }
		public string? Make { get; set; }
		public string? Model { get; set; }
		public string? RimName { get; set; }
		public string? MotorOilName { get; set; }
		public string? FuelTypeName { get; set; }
		public string? BatteryName { get; set; }
		public string? FailureTypeName { get; set; }
	}
}

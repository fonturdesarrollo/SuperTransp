using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class SupervisionViewModel
	{
		[Key]
		public int SupervisionId { get; set; }
		public int SupervisionSummaryId { get; set; }
		public int InPerson { get; set; }
		public int DriverWithVehicle { get; set; }
		public int WorkingVehicle { get; set; }
		public int DriverId { get; set; }
		public string? Plate { get; set; }
		public int VehicleDataId { get; set; }
		public int Passengers { get; set; }
		public int RimId { get; set; }
		public int Wheels { get; set; }
		public int MotorOilId { get; set; }
		public int Liters { get; set; }
		public int FuelTypeId { get; set; }
		public int TankCapacity { get; set; }
		public int Operational { get; set; }
		public int FailureTypeId { get; set; }
		public int FingerprintTrouble { get; set; }
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
		public int? SupervisionStatus { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class PublicTransportGroupViewModel
	{
		[Key]
		public int PublicTransportGroupId { get; set; }
		[Required(ErrorMessage = "Rif es requerido")]
		public string? PublicTransportGroupRif { get; set; }
		[Required(ErrorMessage = "Entidad legal es requerida")]
		[Range(1, int.MaxValue, ErrorMessage = "La Entidad legal debe agegarse")]
		public int DesignationId { get; set; }
		[Required(ErrorMessage = "Nombre de la organización es requerida")]
		public string? PublicTransportGroupName { get; set; }
		[Required(ErrorMessage = "Modalidad es requerida")]
		public int ModeId { get; set; }
		[Required(ErrorMessage = "Gremio es requerido")]
		public int UnionId { get; set; }
		[Required(ErrorMessage = "Datos de la ubicación son requeridos")]
		public int MunicipalityId { get; set; }
		public int StateId { get; set; }
		[Required(ErrorMessage = "Cedula es requerida")]
		public int? RepresentativeIdentityDocument { get; set; }
		[Required(ErrorMessage = "Nombre del representante es requerido")]
		public string? RepresentativeName { get; set; }
		[Required(ErrorMessage = "Teléfono del representante es requerido")]
		public string? RepresentativePhone { get; set; }
		public DateTime PublicTransportGroupIdModifiedDate { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? StateName { get; set; }
		public string? MunicipalityName { get; set; }
		public string? DesignationName { get; set; }
		public string? ModeName { get; set; }
		public string? UnionName { get; set; }
		public int DriverId { get; set; }
		public int DriverIdentityDocument { get; set; }
		public string? DriverFullName { get; set; }
		public int PartnerNumber { get; set; }
		public string? DriverPhone { get; set; }
		public string? SexName { get; set; }
		public DateTime? BirthDate { get; set; }
		public bool? SupervisionStatus { get; set; }
		public string? SupervisionStatusName { get; set; }
		public string? SupervisionPostCloseStatusName { get; set; }
		public int TotalDrivers { get; set; }
		public int TotalSupervisedDrivers { get; set; }
		public int? SupervisionId { get; set; }
		public bool DriverWithVehicle { get; set; }
		public bool WorkingVehicle { get; set; }
		public bool InPerson { get; set; }
		public string? Plate { get; set; }
		public int Year { get; set; }
		public string? Make { get; set; }
		public string? Model { get; set; }
		public int Passengers { get; set; }
		public string? RimName { get; set; }
		public int Wheels { get; set; }
		public string? MotorOilName { get; set; }
		public int Liters { get; set; }
		public string? FuelTypeName { get; set; }
		public int TankCapacity { get; set; }
		public string? BatteryName { get; set; }
		public int NumberOfBatteries { get; set; }
		public string? FailureTypeName { get; set; }
		public bool FingerprintTrouble { get; set; }
		public string? Remarks { get; set; }
		public string? VehicleImageUrl { get; set; }
		public int? Partners { get; set; }
		public string? PublicTransportGroupGUID { get; set; }
		public int? SupervisionSummaryId { get; set; }
		public  string? UserFullName { get; set; }
		public  int? SecurityUserId { get; set; }
		public List<SupervisionPictures>? Pictures { get; set; }
		public int TotalWorkingVehicles { get; set; }
		public int TotalNotWorkingVehicles { get; set; }
		public int TotalWithVehicle { get; set; }
		public int TotalWithoutVehicle { get; set; }
		public int TotalDriverInPerson { get; set; }
		public int TotalDriverNotInPerson { get; set; }
		public int TotalPTGInState { get; set; }
		public int TotaPartnersByPTG { get; set; }
		public int TotalAddedPartners { get; set; }
		public int TotalUniversePTGInState { get; set; }
		public int TotalUniverseDriversInState { get; set; }
		public int DriverPublicTransportGroupId { get; set; }
		public int TotalShowedUpDrivers { get; set; }
		public DateTime? SupervisionLastRoundDate { get; set; }
	}
}

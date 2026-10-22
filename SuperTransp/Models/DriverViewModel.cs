using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class DriverViewModel
	{
		[Key]
		public int DriverId { get; set; }
		public int NewPlateDriverId { get; set; }
		public int PublicTransportGroupId { get; set; }
		public int DriverPublicTransportGroupId { get; set; }
		[Required(ErrorMessage = "La cédula es requerida")]
		public int? DriverIdentityDocument { get; set; }
		[Required(ErrorMessage = "El nombre completo es requerido")]
		public string? DriverFullName { get; set; }
		[Required(ErrorMessage = "El número de asociado es requerido")]
		public int? PartnerNumber { get; set; }
		[Required(ErrorMessage = "El teléfono es requerido")]
		public string? DriverPhone { get; set; }
		// TEMP-EMAIL: EmailAddress removed temporarily (empty email is saved as N/D). Restore later.
		[StringLength(80, ErrorMessage = "El email no puede exceder 80 caracteres")]
		public string? DriverEmail { get; set; }
		public DateTime DriverModifiedDate { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? PublicTransportGroupGUID { get; set; }
		public int SexId { get; set; }
		public string? SexName { get; set; }
		[Microsoft.AspNetCore.Mvc.ModelBinder(typeof(SuperTransp.Core.DayFirstDateModelBinder))]
		public DateTime Birthdate { get; set; }
		public string? StateName { get; set; }
		public string? PublicTransportGroupRif { get; set; }
		public int StateId { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class DriverViewModel
	{
		[Key]
		public int DriverId { get; set; }
		public int PublicTransportGroupId { get; set; }
		public int DriverPublicTransportGroupId { get; set; }
		[Required(ErrorMessage = "La cédula es requerida")]
		public int? DriverIdentityDocument { get; set; }
		[Required(ErrorMessage = "El nombre completo es requerido")]
		public string? DriverFullName { get; set; }
		[Required(ErrorMessage = "El número de socio es requerido")]
		public int? PartnerNumber { get; set; }
		[Required(ErrorMessage = "El teléfono es requerido")]
		public string? DriverPhone { get; set; }
		public DateTime DriverModifiedDate { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? PublicTransportGroupGUID { get; set; }
	}
}

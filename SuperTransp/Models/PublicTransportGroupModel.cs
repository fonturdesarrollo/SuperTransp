using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class PublicTransportGroupModel
	{
		[Key]
		public int PublicTransportGroupId { get; set; }
		[Required(ErrorMessage = "Rif es requerido")]
		public string? PublicTransportGroupRif { get; set; }
		[Required(ErrorMessage = "Nomenclatura es requerida")]
		public int DesignationId { get; set; }
		[Required(ErrorMessage = "Nombre de la línea es requerida")]
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
	}
}

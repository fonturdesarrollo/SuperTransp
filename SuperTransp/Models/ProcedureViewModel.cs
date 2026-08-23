using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class ProcedureViewModel
	{
		[Required]
		public int ProcedureId { get; set; }
		[Required(ErrorMessage = "La categoría es requerida")]
		[Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
		public int ProcedureCategoryId { get; set; }
		public string? ProcedureCategoryName { get; set; }
		[Required(ErrorMessage = "El concepto es requerido")]
		public string? ProcedureConcept { get; set; }
		[Required(ErrorMessage = "La frecuencia es requerida")]
		[Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una frecuencia")]
		public int ProcedureFrequencyId { get; set; }
		public string? ProcedureFrequencyName { get; set; }
		[Required(ErrorMessage = "La tarifa es requerida")]
		[Range(1, int.MaxValue, ErrorMessage = "La tarifa debe ser mayor a cero")]
		public decimal Tariff { get; set; }
	}
}

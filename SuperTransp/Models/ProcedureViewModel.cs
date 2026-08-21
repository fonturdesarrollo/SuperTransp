using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class ProcedureViewModel
	{
		[Required]
		public int ProcedureId { get; set; }
		public int ProcedureCategoryId { get; set; }
		public string? ProcedureCategoryName { get; set; }
		public string? ProcedureConcept { get; set; }
		public int ProcedureFrequencyId { get; set; }
		public string? ProcedureFrequencyName { get; set; }
		public decimal Tariff { get; set; }
	}
}

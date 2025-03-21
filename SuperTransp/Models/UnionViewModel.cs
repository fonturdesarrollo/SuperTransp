using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class UnionViewModel
	{
		public int UnionId { get; set; }
		[Required(ErrorMessage = "Estado es requerido")]
		public int StateId { get; set; }
		[Required(ErrorMessage = "Nombre del gremio es requerid")]
		public string? UnionName { get; set; }
	}
}

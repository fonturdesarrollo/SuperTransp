using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
	public class VehicleDataViewModel
	{
		[Key]
		public int VehicleDataId { get; set; }
		public int Year { get; set; }
		public string? Make { get; set; }
		public string? Model { get; set; }
	}
}

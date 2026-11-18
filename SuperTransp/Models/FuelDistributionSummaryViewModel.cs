namespace SuperTransp.Models
{
	// Latest fuel distribution ("operativo de combustible") of a state: open if there is one, else the last closed;
	// states without any operativo are included with no status/date
	public class FuelDistributionSummaryViewModel
	{
		public int StateId { get; set; }
		public string? StateName { get; set; }
		public string? Status { get; set; }
		public DateTime? StartDate { get; set; }
		public string? Remarks { get; set; }
		// False for a state that has never created an operativo (status and date are empty)
		public bool HasOperativo => StartDate.HasValue;
		public bool IsOpen => string.Equals(Status, "Abierto", StringComparison.OrdinalIgnoreCase);
	}
}

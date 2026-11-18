namespace SuperTransp.Models
{
	// Mirrors Reports/Dashboard: the 5 summary cards plus one row per state with the 7 indicators
	public class DashboardApiModel
	{
		public DateTime GeneratedAt { get; set; }
		public DashboardTotalsApiModel Totals { get; set; } = new();
		public List<DashboardStateApiModel> States { get; set; } = new();
	}

	public class DashboardTotalsApiModel
	{
		public int SupervisedDrivers { get; set; }
		public int WorkingVehicles { get; set; }
		public int NotWorkingVehicles { get; set; }
		public int DriversWithoutVehicle { get; set; }
		public int AbsentDrivers { get; set; }
	}

	public class DashboardStateApiModel
	{
		public int StateId { get; set; }
		public string StateName { get; set; } = string.Empty;
		public int SupervisedDrivers { get; set; }
		public int WorkingVehicles { get; set; }
		public int NotWorkingVehicles { get; set; }
		public int DriversWithVehicle { get; set; }
		public int DriversWithoutVehicle { get; set; }
		public int PresentDrivers { get; set; }
		public int AbsentDrivers { get; set; }
	}

	public class ApiTokenRequest
	{
		public string? Username { get; set; }
		public string? Password { get; set; }
	}
}

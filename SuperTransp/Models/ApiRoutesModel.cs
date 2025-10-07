namespace SuperTransp.Models
{
	public class ApiRoutesModel
	{
	}

	public class RoutesModel
	{
		public RoutesDescription[] ptgData { get; set; }
	}

	public class RoutesDescription
	{
		public int superRouteId { get; set; }
		public string name { get; set; }
		public string rif { get; set; }
	}
}

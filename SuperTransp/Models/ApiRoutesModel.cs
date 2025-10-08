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


	public class RoutesByRifModel
	{
		public Superroute superRoute { get; set; }
		public RouteFleet[] fleet { get; set; }
	}

	public class Superroute
	{
		public int superRouteId { get; set; }
		public string name { get; set; }
		public string rif { get; set; }
	}

	public class RouteFleet
	{
		public int busId { get; set; }
		public string licensePlate { get; set; }
		public string unitName { get; set; }
		public string brand { get; set; }
		public string model { get; set; }
		public int year { get; set; }
		public RouteOwner owner { get; set; }
	}

	public class RouteOwner
	{
		public string idCard { get; set; }
		public string firstName { get; set; }
	}
}

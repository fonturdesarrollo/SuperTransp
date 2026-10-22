using System.ComponentModel.DataAnnotations;

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

	public class RepresentativeModel
	{
		public int RepresentativeIdentityDocument { get; set; }
		public string? RepresentativeName { get; set; }
		public string? RepresentativePhone { get; set; }
		public string? RepresentativeEmail { get; set; }
		public int PublicTransportGroupId { get; set; }
		public string? PublicTransportGroupRif { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? StateName { get; set; }
		public string? MunicipalityName { get; set; }
		public string? UnionName { get; set; }
		public string? ModeName { get; set; }
	}

	public class DriverModel
	{
		public int DriverId { get; set; }
		public int PublicTransportGroupId { get; set; }
		public int DriverPublicTransportGroupId { get; set; }
		public int? DriverIdentityDocument { get; set; }
		public string? DriverFullName { get; set; }
		public int? PartnerNumber { get; set; }
		public string? DriverPhone { get; set; }
		public string? DriverEmail { get; set; }
		public DateTime DriverModifiedDate { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? ModeName { get; set; }
		public string? UnionName { get; set; }
		public string? MunicipalityName { get; set; }
		public int SexId { get; set; }
		public string? SexName { get; set; }
		public DateTime Birthdate { get; set; }
		public string? StateName { get; set; }
		public string? PublicTransportGroupRif { get; set; }
		public int StateId { get; set; }
		public int VehicleYear { get; set; }
		public string? VehiclePlate { get; set; }
		public string? VehicleMake { get; set; }
		public string? VehicleModel { get; set; }
		public int VehiclePassengers { get; set; }
	}

	public class DriverByPTGModel
	{
		public int DriverId { get; set; }
		public int PublicTransportGroupId { get; set; }
		public int? DriverIdentityDocument { get; set; }
		public string? DriverFullName { get; set; }
		public string? DriverPhone { get; set; }
		public string? DriverEmail { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? SexName { get; set; }
		public DateTime Birthdate { get; set; }
		public string? StateName { get; set; }
		public string? MunicipalityName { get; set; }
		public string? PublicTransportGroupRif { get; set; }
		public string? ModeName { get; set; }
		public string? UnionName { get; set; }
	}
}

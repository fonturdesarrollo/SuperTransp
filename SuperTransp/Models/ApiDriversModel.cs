namespace SuperTransp.Models
{
	public class ApiDriversModel
	{
	}
}
public class PersonalData
{
	public string IdCard { get; set; }
	public string FirstName { get; set; }
	public string PrimaryPhone { get; set; }
	public string Gender { get; set; }
	public string BirthDate { get; set; }
}

public class SuperRoute
{
	public int SuperRouteId { get; set; }
	public string Name { get; set; }
}

public class Bus
{
	public int BusId { get; set; }
	public string LicensePlate { get; set; }
	public string? PhotoUrl { get; set; }
	public string Brand { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public int SeatCapacity { get; set; }
	public string FuelType { get; set; }
	public List<SuperRoute> SuperRoute { get; set; }
}

public class Owner
{
	public int OwnerId { get; set; }
	public List<Bus> Buses { get; set; }
}

public class Roles
{
	public Owner Owner { get; set; }
}

public class DriversModel
{
	public PersonalData PersonalData { get; set; }
	public Roles Roles { get; set; }
}
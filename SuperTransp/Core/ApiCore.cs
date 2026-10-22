using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuperTransp.Core
{
	public class ApiCore : IApiCore
	{
		private readonly ISupervision _supervision;
		private IPublicTransportGroup _publicTransportGroup;
		private IDriver _driver;
		private readonly ISecurity _security;
		public ApiCore(ISupervision supervision, IPublicTransportGroup publicTransportGroup, ISecurity security, IDriver driver)
		{
			_supervision = supervision;
			_publicTransportGroup = publicTransportGroup;
			_driver = driver;
			_security = security;
		}

		public DriversModel? MapToDriversModel(int idCard)
		{
			var drivers = _supervision.GetDriverByDriverIdentityDocument(idCard);

			if (drivers == null || !drivers.Any())
				return null;

			var firstDriver = drivers.FirstOrDefault();

			var picture = _supervision
				.GetPicturesByPTGIdAndDriverPublicTransportGroupId(firstDriver.PublicTransportGroupId, firstDriver.DriverPublicTransportGroupId)
				?.FirstOrDefault();

			var buses = drivers.Select(driver =>
			{
				bool hasSupervision = driver.SupervisionId > 0;
				bool hasVehicle = !string.IsNullOrEmpty(driver.Plate);

				return new Bus
				{
					BusId = (int)driver.SupervisionId,
					LicensePlate = hasSupervision && !hasVehicle ? "SOCIO SIN VEHICULO" : driver.Plate ?? "SIN PLACA",
					PhotoUrl = hasSupervision && picture?.VehicleImageUrl == null ? "SOCIO SIN VEHICULO" : picture?.VehicleImageUrl ?? "",
					Brand = hasSupervision && string.IsNullOrEmpty(driver.Make) ? "SOCIO SIN VEHICULO" : driver.Make ?? "SIN MARCA",
					Model = hasSupervision && string.IsNullOrEmpty(driver.Model) ? "SOCIO SIN VEHICULO" : driver.Model ?? "SIN MODELO",
					Year = driver.Year > 1900 ? driver.Year : 0,
					SeatCapacity = driver.Passengers > 0 ? driver.Passengers : 0,
					FuelType = hasSupervision && string.IsNullOrEmpty(driver.FuelTypeName) ? "SOCIO SIN VEHICULO" : driver.FuelTypeName ?? "SIN COMBUSTIBLE",
					SuperRoute = new List<SuperRoute>
			{
				new SuperRoute
				{
					SuperRouteId = driver.PublicTransportGroupId,
					Name = string.IsNullOrEmpty(driver.PTGCompleteName) ? "SIN NOMBRE DE RUTA" : driver.PTGCompleteName
				}
			}
				};
			}).ToList();

			var owner = new Owner
			{
				OwnerId = (int)firstDriver.SecurityUserId,
				Buses = buses
			};

			var roles = new Roles
			{
				Owner = owner
			};

			var personalData = new PersonalData
			{
				IdCard = firstDriver.DriverIdentityDocument.ToString(),
				FirstName = string.IsNullOrEmpty(firstDriver.DriverFullName) ? "Sin nombre" : firstDriver.DriverFullName,
				PrimaryPhone = string.IsNullOrEmpty(firstDriver.DriverPhone) ? "0000000000" : firstDriver.DriverPhone,
				Gender = string.IsNullOrEmpty(firstDriver.SexName) ? "Sin género" : firstDriver.SexName,
				BirthDate = firstDriver.BirthDate?.ToString("yyyy-MM-dd") ?? "1900-01-01"
			};

			_security.AddLogbook(0, false, $"API GET Consulta de datos del socio por cedula: {idCard}");

			return new DriversModel
			{
				PersonalData = personalData,
				Roles = roles
			};
		}

		public RoutesModel? MapToRoutesModel()
		{
			var ptgData = _publicTransportGroup.GetAll();

			if (ptgData == null || !ptgData.Any())
				return null;

			var routes = ptgData.Select(route => new RoutesDescription
			{
				superRouteId = route.PublicTransportGroupId,
				name = string.IsNullOrEmpty(route.PTGCompleteName) ? "SIN NOMBRE DE RUTA" : route.PTGCompleteName,
				rif = string.IsNullOrEmpty(route.PublicTransportGroupRif) ? "SIN RIF" : route.PublicTransportGroupRif
			}).ToArray();

			_security.AddLogbook(0, false, $"API GET Consulta de datos de todas las organizaciones");

			return new RoutesModel
			{
				ptgData = routes
			};
		}

		public RoutesByRifModel? MapToRoutesByRifModel(string publicTransportGroupRif)
		{
			var ptgData = _supervision.GetAllDriverPublicTransportGroup(publicTransportGroupRif);

			if (ptgData == null || ptgData.Count == 0)
				return null;

			var first = ptgData.First();

			var model = new RoutesByRifModel
			{
				superRoute = new Superroute
				{
					superRouteId = first.PublicTransportGroupId,
					name = first.PTGCompleteName,
					rif = first.PublicTransportGroupRif
				},
				fleet = ptgData
					.Select(x => new RouteFleet
					{
						busId = x.DriverId,
						licensePlate = x.Plate,
						unitName = string.Empty,
						brand = x.Make,
						model = x.Model,
						year = x.Year,
						owner = new RouteOwner
						{
							idCard = $"V-{x.DriverIdentityDocument.ToString()}" ,
							firstName = x.DriverFullName
						}
					}).ToArray()
			};

			_security.AddLogbook(0, false, $"API GET Consulta de datos de la organizacion por RIF: {publicTransportGroupRif}");

			return model;
		}

		public List<RepresentativeModel> MapToRepresentativeByIdentityDocument(int representativeIdentityDocument)
		{
			var ptgData = _publicTransportGroup.GetAllRepresentativesByIdentityDocument(representativeIdentityDocument);

			if (ptgData == null || ptgData.Count == 0)
				return null;

			var model = ptgData
				.GroupBy(x => x.PublicTransportGroupId)
				.Select(g => g.First())
				.Select(x => new RepresentativeModel
				{
					RepresentativeIdentityDocument = x.RepresentativeIdentityDocument ?? 0,
					RepresentativeName = x.RepresentativeName,
					RepresentativePhone = x.RepresentativePhone,
					RepresentativeEmail = x.RepresentativeEmail,
					PublicTransportGroupId = x.PublicTransportGroupId,
					PublicTransportGroupRif = x.PublicTransportGroupRif,
					PTGCompleteName = x.PTGCompleteName,
					StateName = x.StateName,
					MunicipalityName = x.MunicipalityName,
					UnionName = x.UnionName,
					ModeName = x.ModeName
				}).ToList();

			_security.AddLogbook(0, false, $"API GET Consulta de datos del representante por cedula: {representativeIdentityDocument}");

			return model;
		}

		public List<DriverModel> MapToDriverByIdentityDocument(int driverIdentityDocument)
		{
			var driverData = _supervision.GetDriverByDriverIdentityDocument(driverIdentityDocument);

			if (driverData == null || driverData.Count == 0)
				return null;

			var model = driverData.Select(x => new DriverModel
			{
				DriverId = x.DriverId,
				PublicTransportGroupId = x.PublicTransportGroupId,
				DriverPublicTransportGroupId = x.DriverPublicTransportGroupId,
				DriverIdentityDocument = x.DriverIdentityDocument,
				DriverFullName = x.DriverFullName,
				PartnerNumber = x.PartnerNumber,
				DriverPhone = x.DriverPhone,
				DriverEmail = x.DriverEmail,
				PTGCompleteName = x.PTGCompleteName,
				ModeName = x.ModeName,
				UnionName = x.UnionName,
				SexName = x.SexName,
				Birthdate = (DateTime)x.BirthDate,
				StateName = x.StateName,
				MunicipalityName = x.MunicipalityName,
				PublicTransportGroupRif = x.PublicTransportGroupRif,
				StateId = x.StateId,
				VehiclePlate = x.Plate,
				VehicleYear = x.Year,
				VehicleMake = x.Make,
				VehicleModel = x.Model,
				VehiclePassengers = x.Passengers,
			}).ToList();

			_security.AddLogbook(0, false, $"API GET Consulta de datos del socio por cedula: {driverIdentityDocument}");

			return model;
		}

		public List<DriverByPTGModel> MapToDriverPTGByIdentityDocument(int driverIdentityDocument)
		{
			var driverData = _supervision.GetDriverPTGByIdentityDocument(driverIdentityDocument);

			if (driverData == null || driverData.Count == 0)
				return null;

			var model = driverData.Select(x => new DriverByPTGModel
			{
				DriverId = x.DriverId,
				PublicTransportGroupId = x.PublicTransportGroupId,
				DriverIdentityDocument = x.DriverIdentityDocument,
				DriverFullName = x.DriverFullName,
				DriverPhone = x.DriverPhone,
				DriverEmail = x.DriverEmail,
				PTGCompleteName = x.PTGCompleteName,
				SexName = x.SexName,
				Birthdate = (DateTime)x.BirthDate,
				StateName = x.StateName,
				PublicTransportGroupRif = x.PublicTransportGroupRif,
				ModeName = x.ModeName,
				UnionName = x.UnionName,
				MunicipalityName = x.MunicipalityName,
			}).ToList();

			_security.AddLogbook(0, false, $"API GET Consulta de datos del socio por cedula: {driverIdentityDocument}");

			return model;
		}
	}
}

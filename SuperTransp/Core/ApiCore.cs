using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuperTransp.Core
{
	public class ApiCore : IApiCore
	{
		private readonly ISupervision _supervision;
		private IPublicTransportGroup _publicTransportGroup;
		private readonly ISecurity _security;
		public ApiCore(ISupervision supervision, IPublicTransportGroup publicTransportGroup, ISecurity security)
		{
			_supervision = supervision;
			_publicTransportGroup = publicTransportGroup;
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
	}
}

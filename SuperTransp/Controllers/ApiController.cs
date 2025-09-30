using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperTransp.Core;
using SuperTransp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using static SuperTransp.Core.Interfaces;

[ApiController]
[Authorize]
[Route("api/transport")]
public class ApiController : ControllerBase
{
	private readonly ISupervision _supervision;
	public ApiController(ISupervision supervision)
	{
		_supervision = supervision;
	}

	[HttpGet("person/{idCard}")]
	public IActionResult GetAll(string idCard)
	{
		try
		{
			if (!int.TryParse(idCard, out _))
			{
				return BadRequest("La cedula permitida debe consistir solo de numeros.");
			}

			var result = _supervision.GetDriverByDriverIdentityDocument(int.Parse(idCard));

			if (result.Any())
			{
				var model = MapToRootObject(result);

				return Ok(model);
			}
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}

		return BadRequest("No existe el numero de cedula indicado.");
	}

	public RootObject MapToRootObject(List<PublicTransportGroupViewModel> drivers)
	{
		if (drivers == null || !drivers.Any())
			return null;

		var firstDriver = drivers.First();

		var picture = _supervision
			.GetPicturesByPTGIdAndDriverId(firstDriver.PublicTransportGroupId, firstDriver.DriverId)
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

		return new RootObject
		{
			PersonalData = personalData,
			Roles = roles
		};
	}
}
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QRCoder;
using SuperTransp.Core;
using SuperTransp.Models;
using SuperTransp.Utils;
using System;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Net;
using System.Reflection.Metadata;
using System.Security;
using System.Text.RegularExpressions;	
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class PublicTransportGroupController : Controller
	{
		private ISecurity _security;
		private IGeography _geography;
		private IDesignation _designation;
		private IUnion _union;
		private IMode _mode;
		private IPublicTransportGroup _publicTransportGroup;
		private IDriver _driver;
		private IConfiguration _configuration;
		private readonly ICommonData _commonData;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _memoryCache;
		private readonly IOptionsSnapshot<MaintenanceSettings> _settings;

		public PublicTransportGroupController(IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography, IDesignation designation, 
			IUnion union, IMode mode, IDriver driver, IConfiguration configuration, IOptionsSnapshot<MaintenanceSettings> settings, ICommonData commonData,
			IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
		{
			_publicTransportGroup = publicTransportGroup;
			_security = security;
			_geography = geography;
			_designation = designation;
			_union = union;
			_mode = mode;
			_driver = driver;
			_configuration = configuration;
			_settings = settings;
			_commonData = commonData;
			_httpClientFactory = httpClientFactory;
			_memoryCache = memoryCache;
		}

		public IActionResult Index()
		{
			var result = CheckSessionAndPermission(1);
			if (result != null) return result;

			SetCommonViewBag();

			return View();
		}

		public IActionResult Add()
		{
			try
			{
				var result = CheckSessionAndPermission(1);
				if (result != null) return result;

				var model = new PublicTransportGroupViewModel
				{
					PublicTransportGroupId = 0,
					PublicTransportGroupIdModifiedDate = DateTime.Now
				};

				SetCommonViewBag();

				ViewBag.IsTotalAccess = false;
				ViewBag.IsDeleteAccess = false;
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");

				if (securityGroupId.HasValue)
				{
					if (securityGroupId != 1)
					{
						if (HasModuleAccess(6))
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
							ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
							ViewBag.GasStations = new SelectList(_commonData.GetGasStationsAll(), "GasStationId", "GasStationDisplayName");

							if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
							{
								ViewBag.IsTotalAccess = true;
							}
						}
						else
						{
							if (stateId.HasValue)
							{
								ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
								ViewBag.Union = new SelectList(_union.GetByStateId((int)stateId), "UnionId", "UnionName");
								ViewBag.GasStations = new SelectList(_commonData.GetGasStationsByStateId((int)stateId), "GasStationId", "GasStationDisplayName");

								if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
								{
									ViewBag.IsTotalAccess = true;
								}
							}
						}
					}
					else
					{
						ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
						ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
						ViewBag.GasStations = new SelectList(_commonData.GetGasStationsAll(), "GasStationId", "GasStationDisplayName");
						ViewBag.IsTotalAccess = true;
					}
				}

				List <DesignationViewModel> designations = _designation.GetAll();

				ViewBag.Designation = new SelectList(designations, "DesignationId", "DesignationName");				
				ViewBag.Mode = new SelectList(_mode.GetAll(), "ModeId", "ModeName");
				ViewBag.DesignationList = Designations(designations);

				return View(model);
			}

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpPost]
		public IActionResult Add(PublicTransportGroupViewModel model)
		{
			try
			{
				var result = CheckSessionAndPermission(1);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

				if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1) || securityGroupId == 1)
				{
					if(model.DesignationId == 0)
					{
						TempData["SuccessMessage"] = "Debe seleccionar la entidad legal de la lista";

						return RedirectToAction("Add");
					}

					if (!model.AssemblyDate.HasValue)
					{
						TempData["SuccessMessage"] = "Debe colocar la fecha de la asamblea";

						return RedirectToAction("Add");
					}

					var emailError = ValidateRepresentativeEmailUnique(model.RepresentativeEmail, model.RepresentativeIdentityDocument); // TEMP-EMAIL-UNIQUE
					if (emailError != null)
					{
						TempData["SuccessMessage"] = emailError;

						return RedirectToAction("Add");
					}

					var routeError = ValidateRoute(model);
					if (routeError != null)
					{
						TempData["SuccessMessage"] = routeError;

						return RedirectToAction("Add");
					}

					int publicTransportGroupId = _publicTransportGroup.AddOrEdit(model);

					if (publicTransportGroupId > 0)
					{
						TempData["SuccessMessage"] = "Datos actualizados correctamente";

						return RedirectToAction("Add");
					}
				}				

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult Edit(int publicTransportGroupId)
		{
			var result = CheckSessionAndPermission(1);
			if (result != null) return result;

			ViewBag.IsTotalAccess = false;
			ViewBag.IsDeleteAccess = false;
			var model = _publicTransportGroup.GetPublicTransportGroupById(publicTransportGroupId);
			int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
			int? stateId = HttpContext.Session.GetInt32("StateId");

			SetCommonViewBag();

			if (securityGroupId.HasValue)
			{
				if (securityGroupId != 1)
				{
					if(HasModuleAccess(6))
					{
						ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
						ViewBag.Union = new SelectList(_union.GetByStateId((int)model.StateId), "UnionId", "UnionName");
						ViewBag.GasStations = new SelectList(_commonData.GetGasStationsByStateId((int)model.StateId), "GasStationId", "GasStationDisplayName");

						if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
						{
							ViewBag.IsTotalAccess = true;
						}
					}
					else
					{
						if (stateId.HasValue)
						{
							ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
							ViewBag.Union = new SelectList(_union.GetByStateId((int)model.StateId), "UnionId", "UnionName");
							ViewBag.GasStations = new SelectList(_commonData.GetGasStationsByStateId((int)model.StateId), "GasStationId", "GasStationDisplayName");

							if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
							{
								ViewBag.IsTotalAccess = true;
							}
						}
					}
				}
				else
				{
					ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
					ViewBag.Union = new SelectList(_union.GetByStateId((int)model.StateId), "UnionId", "UnionName");
					ViewBag.GasStations = new SelectList(_commonData.GetGasStationsByStateId(model.StateId), "GasStationId", "GasStationDisplayName");
					ViewBag.IsTotalAccess = true;
				}

				List<DesignationViewModel> designations = _designation.GetAll();

				ViewBag.Designation = new SelectList(designations, "DesignationId", "DesignationName");
				ViewBag.DesignationList = Designations(designations);
				ViewBag.Mode = new SelectList(_mode.GetAll(), "ModeId", "ModeName");
				ViewBag.Municipality = new SelectList(_geography.GetMunicipalityByStateId(model.StateId), "MunicipalityId", "MunicipalityName");
				ViewBag.ExistingGasStations = _commonData.GetGasStationsByPTGId(publicTransportGroupId);
				ViewBag.GasStationsFull = _commonData.GetGasStationsByStateId(model.StateId);
			}

			return View(model);
		}

		public IActionResult Edit(PublicTransportGroupViewModel model)
		{
			try
			{
				var result = CheckSessionAndPermission(1);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

				if (model.DesignationId == 0)
				{
					TempData["SuccessMessage"] = "Debe seleccionar la entidad legal de la lista";

					return RedirectToAction("Add");
				}

				if (!model.AssemblyDate.HasValue)
				{
					TempData["SuccessMessage"] = "Debe colocar la fecha de la asamblea";

					return RedirectToAction("Edit", new { publicTransportGroupId = model.PublicTransportGroupId });
				}

				var routeError = ValidateRoute(model);
				if (routeError != null)
				{
					TempData["SuccessMessage"] = routeError;

					return RedirectToAction("Edit", new { publicTransportGroupId = model.PublicTransportGroupId });
				}

				var emailError = ValidateRepresentativeEmailUnique(model.RepresentativeEmail, model.RepresentativeIdentityDocument); // TEMP-EMAIL-UNIQUE
				if (emailError != null)
				{
					TempData["SuccessMessage"] = emailError;

					return RedirectToAction("Edit", new { publicTransportGroupId = model.PublicTransportGroupId });
				}

				if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1) || securityGroupId == 1)
				{
					_publicTransportGroup.AddOrEdit(model);
				}

				TempData["SuccessMessage"] = "Datos actualizados correctamente";

				return RedirectToAction("Edit", new { publicTransportGroupId = model.PublicTransportGroupId});	
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult PublicTransportGroupList()
		{
			try
			{
				var result = CheckSessionAndPermission(1);
				if (result != null) return result;

				List<PublicTransportGroupViewModel> model = new();

				SetCommonViewBag();

				return View();		
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult GetPublicTransportGroup()
		{
			try
			{
				var result = CheckSessionAndPermission(1);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");
				List<PublicTransportGroupViewModel> ptgData = new List<PublicTransportGroupViewModel>();

				if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
				{
					ptgData = _publicTransportGroup.GetAllByStateId((int)stateId);
				}
				else
				{
					ptgData = _publicTransportGroup.GetAll();
				}

				var list = ptgData.Select(ptg => new {
					nombre = ptg.PTGCompleteName,
					tipo = ptg.ModeName,
					rif = ptg.PublicTransportGroupRif,
					cupos = ptg.Partners,
					cargados = ptg.TotalDrivers,
					estado = ptg.StateName,
					id = ptg.PublicTransportGroupId
				});

				return Json(new { data = list });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpGet]
		public JsonResult GetMunicipality(int stateId)
		{
			var municipality = _geography.GetMunicipalityByStateId(stateId).ToList();

			return Json(municipality);
		}

		[HttpGet]
		public JsonResult GetUnion(int stateId)
		{
			var union = _union.GetByStateId(stateId);

				return Json(union);
		}

		// Venezuela bounding box (min lon, min lat, max lon, max lat) used to limit place search and route points
		private const double VenezuelaMinLng = -73.4;
		private const double VenezuelaMinLat = 0.6;
		private const double VenezuelaMaxLng = -59.8;
		private const double VenezuelaMaxLat = 12.3;

		private record PlaceResult(string Label, double Lat, double Lng);

		[HttpGet]
		public async Task<IActionResult> SearchPlaces(string? q)
		{
			if (HttpContext.Session.GetInt32("SecurityGroupId") == null)
				return Unauthorized();

			q = (q ?? string.Empty).Trim();

			if (q.Length < 3 || q.Length > 100)
				return Json(new List<PlaceResult>());

			var cacheKey = $"ptg-places:{q.ToLowerInvariant()}";

			if (_memoryCache.TryGetValue(cacheKey, out List<PlaceResult>? cached) && cached != null)
				return Json(cached);

			try
			{
				var client = _httpClientFactory.CreateClient("geo");
				var url = string.Create(CultureInfo.InvariantCulture,
					$"https://photon.komoot.io/api/?q={Uri.EscapeDataString(q)}&limit=10&bbox={VenezuelaMinLng},{VenezuelaMinLat},{VenezuelaMaxLng},{VenezuelaMaxLat}");

				using var response = await client.GetAsync(url);

				if (!response.IsSuccessStatusCode)
					return StatusCode(502, new { error = "El servicio de búsqueda de lugares no está disponible en este momento." });

				using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
				var places = new List<PlaceResult>();

				foreach (var feature in document.RootElement.GetProperty("features").EnumerateArray())
				{
					var properties = feature.GetProperty("properties");

					if (!properties.TryGetProperty("countrycode", out var countryCode) || countryCode.GetString() != "VE")
						continue;

					string? Value(string name) => properties.TryGetProperty(name, out var value) ? value.GetString() : null;

					var street = Value("street");
					if (street != null && Value("housenumber") != null)
						street = $"{street} {Value("housenumber")}";

					var parts = new[] { Value("name"), street, Value("district"), Value("city"), Value("county"), Value("state") }
						.Where(part => !string.IsNullOrWhiteSpace(part))
						.Distinct()
						.ToList();

					if (parts.Count == 0)
						continue;

					var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
					var label = string.Join(", ", parts);

					if (places.Any(place => place.Label == label))
						continue;

					places.Add(new PlaceResult(label, coordinates[1].GetDouble(), coordinates[0].GetDouble()));

					if (places.Count == 6)
						break;
				}

				_memoryCache.Set(cacheKey, places, TimeSpan.FromHours(6));

				return Json(places);
			}
			catch (Exception ex)
			{
				return StatusCode(502, new { error = $"No se pudo consultar los lugares: {ex.Message}" });
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetRoute(string? startLat, string? startLng, string? endLat, string? endLng)
		{
			if (HttpContext.Session.GetInt32("SecurityGroupId") == null)
				return Unauthorized();

			if (!TryParseVenezuelaPoint(startLat, startLng, out var startLatValue, out var startLngValue)
				|| !TryParseVenezuelaPoint(endLat, endLng, out var endLatValue, out var endLngValue))
			{
				return BadRequest(new { error = "Las coordenadas del inicio o de la última parada no son válidas." });
			}

			var cacheKey = string.Create(CultureInfo.InvariantCulture, $"ptg-route:{startLatValue:F6},{startLngValue:F6};{endLatValue:F6},{endLngValue:F6}");

			if (_memoryCache.TryGetValue(cacheKey, out object? cached) && cached != null)
				return Json(cached);

			try
			{
				var client = _httpClientFactory.CreateClient("geo");
				var url = string.Create(CultureInfo.InvariantCulture,
					$"https://router.project-osrm.org/route/v1/driving/{startLngValue:F6},{startLatValue:F6};{endLngValue:F6},{endLatValue:F6}?overview=full&geometries=polyline");

				using var response = await client.GetAsync(url);

				if (!response.IsSuccessStatusCode)
					return StatusCode(502, new { error = "El servicio de rutas no está disponible en este momento." });

				using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
				var root = document.RootElement;

				if (root.GetProperty("code").GetString() != "Ok" || root.GetProperty("routes").GetArrayLength() == 0)
					return NotFound(new { error = "No se encontró una ruta entre los dos puntos seleccionados." });

				var route = root.GetProperty("routes")[0];
				var result = new
				{
					geometry = route.GetProperty("geometry").GetString(),
					distanceMeters = (int)Math.Round(route.GetProperty("distance").GetDouble()),
					durationSeconds = (int)Math.Round(route.GetProperty("duration").GetDouble())
				};

				_memoryCache.Set(cacheKey, result, TimeSpan.FromHours(1));

				return Json(result);
			}
			catch (Exception ex)
			{
				return StatusCode(502, new { error = $"No se pudo calcular la ruta: {ex.Message}" });
			}
		}

		private static bool TryParseVenezuelaPoint(string? lat, string? lng, out double latValue, out double lngValue)
		{
			latValue = 0;
			lngValue = 0;

			return double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out latValue)
				&& double.TryParse(lng, NumberStyles.Float, CultureInfo.InvariantCulture, out lngValue)
				&& latValue >= VenezuelaMinLat && latValue <= VenezuelaMaxLat
				&& lngValue >= VenezuelaMinLng && lngValue <= VenezuelaMaxLng;
		}

		// At least one route (start, end and full path) is mandatory: returns a message when something is missing
		private string? ValidateRoute(PublicTransportGroupViewModel model)
		{
			if (!_publicTransportGroup.RoutesEnabled || !PublicTransportGroupViewModel.ModeHasRoute(model.ModeId))
			{
				return null;
			}

			if (model.Routes.Count == 0)
			{
				return "Debe agregar al menos una ruta a la lista.";
			}

			for (int i = 0; i < model.Routes.Count; i++)
			{
				var route = model.Routes[i];
				var prefix = $"Ruta {i + 1}: ";

				if (string.IsNullOrWhiteSpace(route.RouteStartName) || string.IsNullOrWhiteSpace(route.RouteEndName)
					|| !TryParseVenezuelaPoint(route.RouteStartLat, route.RouteStartLng, out _, out _)
					|| !TryParseVenezuelaPoint(route.RouteEndLat, route.RouteEndLng, out _, out _))
				{
					return prefix + "debe seleccionar el inicio y la última parada de la lista de sugerencias.";
				}

				if ((route.RouteStartName?.Trim().Length ?? 0) > 250 || (route.RouteEndName?.Trim().Length ?? 0) > 250
					|| (route.RouteMidPoint1?.Trim().Length ?? 0) > 250 || (route.RouteMidPoint2?.Trim().Length ?? 0) > 250)
				{
					return prefix + "los lugares no pueden exceder 250 caracteres.";
				}

				if (string.IsNullOrWhiteSpace(route.RouteGeometry) || route.RouteDistanceMeters <= 0)
				{
					return prefix + "no tiene el trazado calculado, agréguela de nuevo.";
				}
			}

			return null;
		}

		// TEMP-EMAIL-UNIQUE: returns a message when the email is already used by another representative identity document
		private string? ValidateRepresentativeEmailUnique(string? email, int? identityDocument)
		{
			var value = email?.Trim().ToLower();

			if (string.IsNullOrEmpty(value) || value == "n/d" || identityDocument == null)
				return null;

			if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
				return null;

			return _publicTransportGroup.EmailUsedByOtherRepresentative(value, identityDocument.Value)
				? $"El email {value} ya está registrado a otro representante con una cédula diferente."
				: null;
		}

		public JsonResult CheckRifExist(string paramValue1, string? paramValue4 = null, int? paramValue5 = null)
		{
			var result = CheckSessionAndPermission(1);
			if (result != null) Json("ERROR");

			var registeredRif = _publicTransportGroup.RegisteredRif(paramValue1);

			if (!string.IsNullOrEmpty(registeredRif))
			{

				return Json($"La línea con el RIF {paramValue1} ya está registrada.");					
			}

			var emailError = ValidateRepresentativeEmailUnique(paramValue4, paramValue5); // TEMP-EMAIL-UNIQUE
			if (emailError != null)
				return Json(emailError);

			return Json("OK");
		}

		public JsonResult CheckRifExistOnEdit(string paramValue1, int paramValue2, int paramValue3, string? paramValue4 = null, int? paramValue5 = null)
		{
			var result = CheckSessionAndPermission(1);
			if (result != null) return Json("ERROR");

			var registeredRif = _publicTransportGroup.RegisteredRif(paramValue1);

			if(!string.IsNullOrEmpty(registeredRif))
			{
				var currentRif = _publicTransportGroup.GetPublicTransportGroupById(paramValue2);

				if(currentRif.PublicTransportGroupRif != registeredRif)
				{
					return Json($"La línea con el RIF {paramValue1} ya está registrada.");
				}
			}

			var emailError = ValidateRepresentativeEmailUnique(paramValue4, paramValue5); // TEMP-EMAIL-UNIQUE
			if (emailError != null)
				return Json(emailError);

			if (paramValue2 > 0)
			{
				var totalDrivers = _driver.TotalDriversByPublicTransportGroupId(paramValue2);

				if (totalDrivers > paramValue3)
				{
					return Json($"Existen {totalDrivers} transportista(s) cargado(s) a esta línea, si desea reducir la flota habilitada debe eliminar los necesarios.");
				}
			}

			return Json("OK");
		}

		[HttpPost]
		public IActionResult GenerateQR([FromBody] QRRequest request)
		{
			var baseWebSiteUrl = _configuration["FtpSettings:BaseWebSiteUrl"];
			var ptgDataController = $"{baseWebSiteUrl}QR/PublicTransportGroupData?ptgCode={request.ptgGUID}";

			if (request == null || string.IsNullOrEmpty(request.ptgGUID))
			{
				return BadRequest(new { success = false, message = "Se requiere el GUID para generar el código QR." });
			}

			try
			{
				QRCodeGenerator qrGenerator = new QRCodeGenerator();
				QRCodeData qrCodeData = qrGenerator.CreateQrCode(ptgDataController, QRCodeGenerator.ECCLevel.Q);

				using (PngByteQRCode pngByteQRCode = new PngByteQRCode(qrCodeData))
				{
					byte[] qrCodeBytes = pngByteQRCode.GetGraphic(20);
					string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

					return Json(new { success = true, qrImage = "data:image/png;base64," + qrCodeBase64 });
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error al generar el código QR: {ex.Message}");
				return StatusCode(500, new { success = false, message = $"Error interno al generar el código QR: {ex.Message}" });
			}
		}

		private List<string?> Designations(List<DesignationViewModel> designations)
		{
			List<string?> designationValues = new List<string?>();

			foreach (var designation in designations)
			{
				designationValues.Add(designation.DesignationName);
			}

			foreach (var pattern in Patterns.PTGPatterns)
			{
				designationValues.Add(pattern);
			}

			return designationValues;
		}

		private bool HasModuleAccess(int moduleId)
		{
			var groupId = HttpContext.Session.GetInt32("SecurityGroupId");
			return groupId == 1 || _security.GroupHasAccessToModule(groupId ?? 0, moduleId);
		}

		private void SetCommonViewBag()
		{
			ViewBag.EmployeeName = $"{HttpContext.Session.GetString("FullName")} ({HttpContext.Session.GetString("SecurityGroupName")})";
			ViewBag.SecurityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
			ViewBag.SystemVersion = HttpContext.Session.GetString("SystemVersion");

			if (_settings.Value.IsActive)
			{
				ViewBag.MaintenanceMessage = _settings.Value.Message;
			}
		}

		private IActionResult? CheckSessionAndPermission(int requiredModuleId)
		{
			var securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

			if (securityGroupId == null)
				return RedirectToAction("Login", "Security");

			if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, requiredModuleId))
				return RedirectToAction("Login", "Security");

			return null;
		}
	}
	public class QRRequest
	{
		public string ptgGUID { get; set; }
	}
}

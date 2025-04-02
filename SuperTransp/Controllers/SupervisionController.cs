using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using SuperTransp.Core;
using SuperTransp.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class SupervisionController : Controller
	{
		ISupervision _supervision;
		private ISecurity _security;
		private IPublicTransportGroup _publicTransportGroup;
		private ICommonData _commonData;
		private IConfiguration _configuration;

		public SupervisionController(ISupervision supervision, ISecurity security, IPublicTransportGroup publicTransportGroup, ICommonData commonData, IConfiguration configuration )
		{
			_supervision = supervision;
			_security = security;
			_publicTransportGroup = publicTransportGroup;
			_commonData = commonData;
			_configuration = configuration;
		}

		public IActionResult Index()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FullName")) && HttpContext.Session.GetInt32("SecurityGroupId") != null)
			{
				ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

				return View();
			}

			return RedirectToAction("Login", "Security");
		}

		public IActionResult PublicTransportGroupList()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _supervision.GetDriverPublicTransportGroupByStateId((int)stateId);
					}
					else
					{
						model = _supervision.GetAllDriverPublicTransportGroup();
					}

					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult Add(int publicTransportGroupId, string pTGCompleteName, string driverFullName, int partnerNumber, string? publicTransportGroupRif, int driverIdentityDocument, string stateName, int? supervisionStatus, int driverId, int supervisionId)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					var model = new SupervisionViewModel
					{
						PublicTransportGroupId = publicTransportGroupId,
						PTGCompleteName = pTGCompleteName,
						DriverId = driverId,
						DriverFullName = driverFullName,
						PartnerNumber = partnerNumber,
						PublicTransportGroupRif = publicTransportGroupRif,
						DriverIdentityDocument = driverIdentityDocument,
						StateName = stateName,
						SecurityUserId = (int)HttpContext.Session.GetInt32("SecurityUserId"),
						SupervisionId =  supervisionId
					};

					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");


					ViewBag.DriverWithVehicle = new SelectList( _commonData.GetYesNo(), "YesNoId", "YesNoName");
					ViewBag.WorkingVehicle = new SelectList( _commonData.GetYesNo(), "YesNoId", "YesNoName");
					ViewBag.InPerson = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
					ViewBag.Years = new SelectList( _commonData.GetYears(), "YearId", "YearName");
					ViewBag.Passengers = new SelectList( _commonData.GetPassengers(), "PassengerId", "Passengers");
					ViewBag.Rims = new SelectList( _commonData.GetRims(), "RimId", "RimName");
					ViewBag.Wheels = new SelectList( _commonData.GetWheels(), "WheelId", "Wheels");
					ViewBag.FuelTypes = new SelectList( _commonData.GetFuelTypes(), "FuelTypeId", "FuelTypeName");
					ViewBag.TankCapacity = new SelectList( _commonData.GetTankCapacity(), "TankCapacityId", "TankCapacity");
					ViewBag.Batteries = new SelectList( _commonData.GetBatteries(), "BatteryId", "BatteryName");
					ViewBag.NumberOfBatteries = new SelectList( _commonData.GetNumberOfBatteries(), "BatteriesId", "Batteries");
					ViewBag.MotorOil = new SelectList( _commonData.GetMotorOil(), "MotorOilId", "MotorOilName");
					ViewBag.OilLitters = new SelectList( _commonData.GetOilLitters(), "OilLittersId", "OilLitters");
					ViewBag.FailureType = new SelectList( _commonData.GetFailureType(), "FailureTypeId", "FailureTypeName");
					ViewBag.FingerprintProblem = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");

					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpPost]
		public IActionResult Add(SupervisionViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int supervisionId = 0;

					if (model.DriverWithVehicle == 0)
					{
						supervisionId = _supervision.AddSimple(model);	
					}
					else
					{
						model.Remarks = string.IsNullOrEmpty(model.Remarks) ? string.Empty : model.Remarks;
						model.VehicleImageUrl = string.IsNullOrEmpty(model.VehicleImageUrl) ? string.Empty : model.VehicleImageUrl;
						model.SupervisionStatus = 1;
						model.FailureTypeId = model.WorkingVehicle == 1 ? 1 : model.FailureTypeId;
						var imageUrl = SupervisionPictureUrl(model.StateName, model.PublicTransportGroupRif, model.DriverIdentityDocument, model.PartnerNumber);
						model.VehicleImageUrl = imageUrl;
						supervisionId = _supervision.AddOrEdit(model);
					}

					if (supervisionId > 0)
					{
						return RedirectToAction("PublicTransportGroupList");
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
		public JsonResult GetMakes(int yearId)
		{
			var makes = _commonData.GetMakesByYear(yearId).ToList();

			return Json(makes);
		}

		[HttpGet]
		public JsonResult GetModels(int yearId, string make)
		{
			var models = _commonData.GetModelsByYearAndMake(yearId, make).ToList();

			return Json(models);
		}

		[HttpPost]
		public async Task<IActionResult> SaveFiles(IFormFile file, string stateName, string driverIdentityDocument, string partnerNumber, string publicTransportGroupRif)
		{
			if (file?.Length > 0 &&
				!string.IsNullOrEmpty(stateName) &&
				!string.IsNullOrEmpty(driverIdentityDocument) &&
				!string.IsNullOrEmpty(partnerNumber) &&
				!string.IsNullOrEmpty(publicTransportGroupRif))
			{
				var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
				var ftpUsername = _configuration["FtpSettings:Username"];
				var ftpPassword = _configuration["FtpSettings:Password"];

				var newFolderName = $"{stateName.ToUpper().Trim()}";
				var ftpFolderPath = Path.Combine(ftpBaseUrl, newFolderName).Replace("\\", "/");
				var subFolderName = $"{publicTransportGroupRif}-{driverIdentityDocument}-{partnerNumber}";
				var ftpSubFolderPath = Path.Combine(ftpFolderPath, subFolderName).Replace("\\", "/");
				var ftpFileName = Guid.NewGuid().ToString();

				Regex regex = new Regex(@"\.[^\.]+$");
				Match match = regex.Match(file.FileName);

				if (match.Success)
				{
					ftpFileName = $"{ftpFileName}{match.Value}";
				}

				try
				{
					// Verificar si la subcarpeta ESTADO/RIF-CEDULA-SOCIO existe y eliminar su contenido
					FtpWebRequest listSubFolderRequest = (FtpWebRequest)WebRequest.Create(ftpSubFolderPath);
					listSubFolderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
					listSubFolderRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

					try
					{
						using (var listSubFolderResponse = (FtpWebResponse)await listSubFolderRequest.GetResponseAsync())
						using (StreamReader reader = new StreamReader(listSubFolderResponse.GetResponseStream()))
						{
							string line;
							while ((line = reader.ReadLine()) != null)
							{
								// Eliminar cada archivo dentro de la subcarpeta ESTADO/RIF-CEDULA-SOCIO
								var filePath = Path.Combine(ftpSubFolderPath, line).Replace("\\", "/");
								FtpWebRequest deleteFileRequest = (FtpWebRequest)WebRequest.Create(filePath);
								deleteFileRequest.Method = WebRequestMethods.Ftp.DeleteFile;
								deleteFileRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

								using (var deleteFileResponse = (FtpWebResponse)await deleteFileRequest.GetResponseAsync())
								{
									Console.WriteLine($"Archivo eliminado: {deleteFileResponse.StatusDescription}");
								}
							}
						}
					}
					catch (WebException ex)
					{
						var response = (FtpWebResponse)ex.Response;
						// Ignorar error si la subcarpeta ESTADO/RIF-CEDULA-SOCIO no existe
						if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
						{
							throw;
						}
					}

					// Crear la subcarpeta ESTADO dentro de la carpeta principal si no existe
					FtpWebRequest subFolderRequest = (FtpWebRequest)WebRequest.Create(ftpFolderPath);
					subFolderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
					subFolderRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

					try
					{
						using (var subFolderResponse = (FtpWebResponse)await subFolderRequest.GetResponseAsync())
						{
							Console.WriteLine($"Subcarpeta creada: {subFolderResponse.StatusDescription}");
						}
					}
					catch (WebException ex)
					{
						var response = (FtpWebResponse)ex.Response;
						// Ignorar error si la subcarpeta ya existe
						if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
						{
							throw;
						}
					}

					// Crear la subcarpeta ESTADO/RIF-CEDULA-SOCIO dentro de la carpeta ESTADO si no existe
					FtpWebRequest subSubFolderRequest = (FtpWebRequest)WebRequest.Create(ftpSubFolderPath);
					subSubFolderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
					subSubFolderRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

					try
					{
						using (var subFolderResponse = (FtpWebResponse)await subSubFolderRequest.GetResponseAsync())
						{
							Console.WriteLine($"Subcarpeta creada: {subFolderResponse.StatusDescription}");
						}
					}
					catch (WebException ex)
					{
						var response = (FtpWebResponse)ex.Response;
						// Ignorar error si la subcarpeta ya existe
						if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
						{
							throw;
						}
					}

					// Subir el archivo a la subcarpeta ESTADO/RIF-CEDULA-SOCIO
					var ftpFilePath = Path.Combine(ftpSubFolderPath, ftpFileName).Replace("\\", "/");
					FtpWebRequest fileRequest = (FtpWebRequest)WebRequest.Create(ftpFilePath);
					fileRequest.Method = WebRequestMethods.Ftp.UploadFile;
					fileRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

					using (var memoryStream = new MemoryStream())
					{
						await file.CopyToAsync(memoryStream);
						byte[] fileContents = memoryStream.ToArray();

						using (Stream requestStream = fileRequest.GetRequestStream())
						{
							await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
						}
					}

					return Ok(new { message = $"Archivo cargado exitosamente en {ftpFilePath}" });
				}
				catch (Exception ex)
				{
					return BadRequest($"Error al procesar el archivo o la carpeta: {ex.Message}");
				}
			}

			return BadRequest("No se pudo subir el archivo.");
		}

		private string SupervisionPictureUrl(string stateName, string publicTransportGroupRif, int driverIdentityDocument, int partnerNumber)
		{
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var ftpUsername = _configuration["FtpSettings:Username"];
			var ftpPassword = _configuration["FtpSettings:Password"];
			var baseImagesUrl = _configuration["FtpSettings:BaseImagesUrl"];

			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = Path.Combine(ftpBaseUrl, newFolderName).Replace("\\", "/");
			var subFolderName = $"{publicTransportGroupRif}-{driverIdentityDocument}-{partnerNumber}";
			var ftpSubFolderPath = Path.Combine(ftpFolderPath, subFolderName).Replace("\\", "/");
			var filePath = string.Empty;

			try
			{
				// Verificar si la subcarpeta ESTADO/RIF-CEDULA-SOCIO existe y eliminar su contenido
				FtpWebRequest listSubFolderRequest = (FtpWebRequest)WebRequest.Create(ftpSubFolderPath);
				listSubFolderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
				listSubFolderRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

				try
				{
					using (var listSubFolderResponse = (FtpWebResponse)listSubFolderRequest.GetResponse())
					using (StreamReader reader = new StreamReader(listSubFolderResponse.GetResponseStream()))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							filePath = Path.Combine(ftpSubFolderPath, line).Replace("\\", "/").Replace(ftpBaseUrl, baseImagesUrl);
						}
					}
				}
				catch (WebException ex)
				{
					var response = (FtpWebResponse)ex.Response;
					if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
					{
						return filePath;
					}
				}
			}
			catch
			{
				// Manejo de excepciones genéricas
			}

			return filePath;
		}

		public JsonResult CheckExistingPlate(int paramValue1, string paramValue2)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var existingPlate = _supervision.RegisteredPlate(paramValue2);
				if (existingPlate.Any())
				{
					if(!existingPlate.Where(x=> x.DriverId == paramValue1).Any())
					{
						return Json($"El número de placa {paramValue2} ya está asignado al transportista {existingPlate.FirstOrDefault().DriverFullName} línea {existingPlate.FirstOrDefault().PTGCompleteName} estado {existingPlate.FirstOrDefault().StateName.ToUpper()}.");
					}					
				}				

				return Json("OK");
			}

			return Json("ERROR");
		}
	}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using SuperTransp.Core;
using SuperTransp.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
				{
					return RedirectToAction("Login", "Security");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

				return View();
			}

			return RedirectToAction("Login", "Security");
		}

		public IActionResult PublicTransportGroupDriverList(string ptgRifName)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");
					int? userId = HttpContext.Session.GetInt32("SecurityUserId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _supervision.GetDriverPublicTransportGroupByStateIdAndPTGRif((int)stateId, ptgRifName);					
					}
					else
					{
						model = _supervision.GetAllDriverPublicTransportGroup(ptgRifName);
					}

					if(!model.Any())
					{
						TempData["SuccessMessage"] = $"No existe una organización con el RIF {ptgRifName}";
						return RedirectToAction("Index");
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

		public IActionResult Add(int publicTransportGroupId, string pTGCompleteName, string driverFullName, int partnerNumber, string? publicTransportGroupRif, int driverIdentityDocument, string stateName, int? supervisionStatus, int driverId, int supervisionId, int modeId, string modeName)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
					int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
					
					if (securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId, 3) || securityGroupId == 1)
					{
						if(!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
						{
							return RedirectToAction("PublicTransportGroupDriverList");
						}

						if (_supervision.IsSupervisionSummaryDoneByPtgId(publicTransportGroupId))
						{
							if (!_security.GroupHasAccessToModule((int)securityGroupId, 19))
							{
								return RedirectToAction("PublicTransportGroupDriverList");
							}
						}

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
							SecurityUserId = (int)securityUserId,
							SupervisionId = supervisionId,
							ModeId = modeId,
							ModeName = modeName
						};

						ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
						ViewBag.DriverWithVehicle = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
						ViewBag.WorkingVehicle = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
						ViewBag.InPerson = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
						ViewBag.Years = new SelectList(_commonData.GetYears(), "YearId", "YearName");
						ViewBag.Passengers = new SelectList(_commonData.GetPassengers(), "PassengerId", "Passengers");
						ViewBag.Rims = new SelectList(_commonData.GetRims(), "RimId", "RimName");
						ViewBag.Wheels = new SelectList(_commonData.GetWheels(), "WheelId", "Wheels");
						ViewBag.FuelTypes = new SelectList(_commonData.GetFuelTypes(), "FuelTypeId", "FuelTypeName");
						ViewBag.TankCapacity = new SelectList(_commonData.GetTankCapacity(), "TankCapacityId", "TankCapacity");
						ViewBag.Batteries = new SelectList(_commonData.GetBatteries(), "BatteryId", "BatteryName");
						ViewBag.NumberOfBatteries = new SelectList(_commonData.GetNumberOfBatteries(), "BatteriesId", "Batteries");
						ViewBag.MotorOil = new SelectList(_commonData.GetMotorOil(), "MotorOilId", "MotorOilName");
						ViewBag.OilLitters = new SelectList(_commonData.GetOilLitters(), "OilLittersId", "OilLitters");
						ViewBag.FailureType = new SelectList(_commonData.GetFailureType(), "FailureTypeId", "FailureTypeName");
						ViewBag.FingerprintProblem = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");

						var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
						var tempFolderName = $"{publicTransportGroupRif}-{partnerNumber}-supervision_temp";
						string ftpTempFolderPath = Path.Combine(ftpBaseUrl, stateName.ToUpper().Trim(), tempFolderName).Replace("\\", "/");

						DeleteFilesInFTPFolderAsync(ftpTempFolderPath).GetAwaiter().GetResult();

						if (securityGroupId != 1)
						{
							ViewBag.IsTotalAccess = _security.IsTotalAccess(3);
						}
						else
						{

							ViewBag.IsTotalAccess = true;
						}

						return View(model);
					}
				}

				return RedirectToAction("Login", "Security");
			}

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpPost]
		public async Task<IActionResult> Add(SupervisionViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
					int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

					if(securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId,3) || securityGroupId == 1)
					{
						if(_security.IsTotalAccess(3) || securityGroupId == 1)
						{
							int supervisionId = 0;

							if (!model.DriverWithVehicle)
							{
								supervisionId = _supervision.AddSimple(model);
							}
							else
							{
								model.Remarks = string.IsNullOrEmpty(model.Remarks) ? string.Empty : model.Remarks;
								model.VehicleImageUrl = string.IsNullOrEmpty(model.VehicleImageUrl) ? string.Empty : model.VehicleImageUrl;
								model.SupervisionStatus = true;
								model.FailureTypeId = model.WorkingVehicle ? 1 : model.FailureTypeId;
								var imageUrl = await SupervisionPictureUrl(model.StateName, model.PublicTransportGroupRif, model.DriverIdentityDocument, model.PartnerNumber);

								if (imageUrl != null && imageUrl.Any())
								{
									List<SupervisionPictures> pictures = new();
									foreach (var item in imageUrl)
									{
										pictures.Add(new SupervisionPictures
										{
											SupervisionPictureId = 0,
											PublicTransportGroupId = model.PublicTransportGroupId,
											PartnerNumber = model.PartnerNumber,
											VehicleImageUrl = item.VehicleImageUrl,
											SupervisionPictureDateAdded = DateTime.Now,
										});
									}

									model.Pictures?.Clear();
									model.Pictures = pictures;
								}

								supervisionId = _supervision.AddOrEdit(model);
							}

							if (supervisionId > 0)
							{
								return RedirectToAction("PublicTransportGroupDriverList", new { ptgRifName = model.PublicTransportGroupRif });
							}
						}
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult Edit(int publicTransportGroupId, string pTGCompleteName, string driverFullName, int partnerNumber, string? publicTransportGroupRif, int driverIdentityDocument, string stateName, int? supervisionStatus, int driverId, int supervisionId, int modeId, string modeName, int stateId)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
					int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

					if(securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId,3) || securityGroupId == 1)
					{
						if (!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
						{
							return RedirectToAction("PublicTransportGroupDriverList");
						}

						if (_supervision.IsSupervisionSummaryDoneByPtgId(publicTransportGroupId))
						{
							if (!_security.GroupHasAccessToModule((int)securityGroupId, 19))
							{
								return RedirectToAction("PublicTransportGroupDriverList");
							}
						}

						var model = _supervision.GetByPublicTransportGroupIdAndDriverIdAndPartnerNumberStateId(publicTransportGroupId, driverId, partnerNumber, (int)stateId);

						if(model != null)
						{
							model.PublicTransportGroupId = publicTransportGroupId;
							model.PTGCompleteName = pTGCompleteName;
							model.DriverId = driverId;
							model.DriverFullName = driverFullName;
							model.PartnerNumber = partnerNumber;
							model.PublicTransportGroupRif = publicTransportGroupRif;
							model.DriverIdentityDocument = driverIdentityDocument;
							model.StateName = stateName;
							model.SecurityUserId = (int)HttpContext.Session.GetInt32("SecurityUserId");
							model.SupervisionId = supervisionId;
							model.ModeId = modeId;
							model.ModeName = modeName;

							ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
							ViewBag.DriverWithVehicle = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
							ViewBag.WorkingVehicle = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
							ViewBag.InPerson = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");
							ViewBag.Years = new SelectList(_commonData.GetYears(), "YearId", "YearName");
							ViewBag.Passengers = new SelectList(_commonData.GetPassengers(), "PassengerId", "Passengers");
							ViewBag.Rims = new SelectList(_commonData.GetRims(), "RimId", "RimName");
							ViewBag.Wheels = new SelectList(_commonData.GetWheels(), "WheelId", "Wheels");
							ViewBag.FuelTypes = new SelectList(_commonData.GetFuelTypes(), "FuelTypeId", "FuelTypeName");
							ViewBag.TankCapacity = new SelectList(_commonData.GetTankCapacity(), "TankCapacityId", "TankCapacity");
							ViewBag.Batteries = new SelectList(_commonData.GetBatteries(), "BatteryId", "BatteryName");
							ViewBag.NumberOfBatteries = new SelectList(_commonData.GetNumberOfBatteries(), "BatteriesId", "Batteries");
							ViewBag.MotorOil = new SelectList(_commonData.GetMotorOil(), "MotorOilId", "MotorOilName");
							ViewBag.OilLitters = new SelectList(_commonData.GetOilLitters(), "OilLittersId", "OilLitters");
							ViewBag.FailureType = new SelectList(_commonData.GetFailureType(), "FailureTypeId", "FailureTypeName");
							ViewBag.FingerprintProblem = new SelectList(_commonData.GetYesNo(), "YesNoId", "YesNoName");

							ViewBag.Makes = new SelectList(_commonData.GetMakesByYear((int)model.Year).ToList(), "Make", "Make");
							ViewBag.VehicleModel = new SelectList(_commonData.GetModelsByYearAndMake((int)model.Year, model.Make).ToList(), "VehicleDataId", "ModelName");

							var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
							var tempFolderName = $"{publicTransportGroupRif}-{partnerNumber}-supervision_temp";
							string ftpTempFolderPath = Path.Combine(ftpBaseUrl, stateName.ToUpper().Trim(), tempFolderName).Replace("\\", "/");

							DeleteFilesInFTPFolderAsync(ftpTempFolderPath).GetAwaiter().GetResult();

							if (securityGroupId != 1)
							{
								ViewBag.IsTotalAccess = _security.IsTotalAccess(3);
							}
							else
							{

								ViewBag.IsTotalAccess = true;
							}

							return View(model);
						}
					}
				}

				return RedirectToAction("Login", "Security");
			}

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpPost]
		public IActionResult Edit(SupervisionSummaryViewModel model)
		{
			if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
			{
				return RedirectToAction("Login", "Security");
			}

			return View(model);
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
				var subFolderName = $"{publicTransportGroupRif}-{partnerNumber}-supervision_temp";
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

		[HttpPost]
		public async Task<IActionResult> SaveSummaryFiles(IFormFile file, string stateName, string driverIdentityDocument, string partnerNumber, string publicTransportGroupRif)
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
				//var subFolderName = $"Resumen_Supervision";
				var subFolderName = $"{publicTransportGroupRif}-resumen_supervision_temp";
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteFilesInFTPFolderAsync(string ftpSubFolderPath)
		{
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var ftpUsername = _configuration["FtpSettings:Username"];
			var ftpPassword = _configuration["FtpSettings:Password"];

			// Obtener lista de archivos en la carpeta Temp
			FtpWebRequest listFilesRequest = (FtpWebRequest)WebRequest.Create(ftpSubFolderPath);
			listFilesRequest.Method = WebRequestMethods.Ftp.ListDirectory;
			listFilesRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

			try
			{
				using (var listFilesResponse = (FtpWebResponse)await listFilesRequest.GetResponseAsync())
				using (StreamReader reader = new StreamReader(listFilesResponse.GetResponseStream()))
				{
					string fileName;

					while ((fileName = reader.ReadLine()) != null)
					{
						// Construir la ruta de cada archivo dentro de Temp
						var filePath = Path.Combine(ftpSubFolderPath, fileName).Replace("\\", "/");

						FtpWebRequest deleteFileRequest = (FtpWebRequest)WebRequest.Create(filePath);
						deleteFileRequest.Method = WebRequestMethods.Ftp.DeleteFile;
						deleteFileRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

						try
						{
							using (var deleteFileResponse = (FtpWebResponse)await deleteFileRequest.GetResponseAsync())
							{
								Console.WriteLine($"Archivo eliminado: {fileName}");
							}
						}
						catch (WebException ex)
						{
							var response = (FtpWebResponse)ex.Response;
							if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
							{
								throw;
							}
						}
					}
				}
			}
			catch (WebException ex)
			{
				var response = (FtpWebResponse)ex.Response;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
				{
					Console.WriteLine("La carpeta Temp no existe o está vacía.");
				}
				else
				{
					throw;
				}
			}

			return Ok();
		}

		public async Task<JsonResult> DeleteAllSupervisionPictures(string stateName, string publicTransportGroupRif, int partnerNumber, int publicTransportGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
				var ftpUsername = _configuration["FtpSettings:Username"];
				var ftpPassword = _configuration["FtpSettings:Password"];

				var newFolderName = $"{stateName.ToUpper().Trim()}";
				var ftpFolderPath = Path.Combine(ftpBaseUrl, newFolderName).Replace("\\", "/");
				var subFolderName = $"{publicTransportGroupRif}-{partnerNumber}-supervision_temp";
				var ftpSubFolderPath = Path.Combine(ftpFolderPath, subFolderName).Replace("\\", "/");

				// Obtener lista de archivos en la carpeta Temp
				FtpWebRequest listFilesRequest = (FtpWebRequest)WebRequest.Create(ftpSubFolderPath);
				listFilesRequest.Method = WebRequestMethods.Ftp.ListDirectory;
				listFilesRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

				try
				{
					using (var listFilesResponse = (FtpWebResponse)await listFilesRequest.GetResponseAsync())
					using (StreamReader reader = new StreamReader(listFilesResponse.GetResponseStream()))
					{
						string fileName;
						while ((fileName = reader.ReadLine()) != null)
						{
							// Construir la ruta de cada archivo dentro de Temp
							var filePath = Path.Combine(ftpSubFolderPath, fileName).Replace("\\", "/");

							FtpWebRequest deleteFileRequest = (FtpWebRequest)WebRequest.Create(filePath);
							deleteFileRequest.Method = WebRequestMethods.Ftp.DeleteFile;
							deleteFileRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

							try
							{
								using (var deleteFileResponse = (FtpWebResponse)await deleteFileRequest.GetResponseAsync())
								{
									Console.WriteLine($"Archivo eliminado: {fileName}");
								}
							}
							catch (WebException ex)
							{
								var response = (FtpWebResponse)ex.Response;
								if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
								{
									throw;
								}
							}
						}
					}
				}
				catch (WebException ex)
				{
					var response = (FtpWebResponse)ex.Response;
					if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
					{
						Console.WriteLine("La carpeta Temp no existe o está vacía.");
					}
					else
					{
						throw;
					}
				}

				return Json("OK");
			}

			return Json("ERROR");
		}

		private async Task<List<SupervisionPictures>> SupervisionPictureUrl(string stateName, string publicTransportGroupRif, int driverIdentityDocument, int partnerNumber)
		{
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var ftpUsername = _configuration["FtpSettings:Username"];
			var ftpPassword = _configuration["FtpSettings:Password"];
			var baseImagesUrl = _configuration["FtpSettings:BaseImagesUrl"];

			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = $"{ftpBaseUrl}/{newFolderName}";
			var subFolderName = $"{publicTransportGroupRif}-{partnerNumber}-supervision_temp";
			var subFinalFolderName = $"{publicTransportGroupRif}-{partnerNumber}-supervision";
			var ftpSubFolderPath = $"{ftpFolderPath}/{subFolderName}";
			var ftpfinalFolderPath = $"{ftpFolderPath}/{subFinalFolderName}";

			var images = new List<SupervisionPictures>();

			// Paso 1: Verificar si la carpeta destino existe antes de crearla
			if (!await FolderExistsAsync(ftpfinalFolderPath, ftpUsername, ftpPassword))
			{
				await CreateFolderAsync(ftpfinalFolderPath, ftpUsername, ftpPassword);
			}

			try
			{
				// Paso 2: Obtener lista de archivos
				var fileList = await ListFilesAsync(ftpSubFolderPath, ftpUsername, ftpPassword);
				if (fileList.Count == 0) return images;

				// Paso X: Eliminar archivos de la carpeta final
				if (fileList.Any())
				{
					var currentFileList = await ListFilesAsync(ftpfinalFolderPath, ftpUsername, ftpPassword);
					if (currentFileList.Count > 0)
					{

						var deleteTasksX = currentFileList.Select(fileName => DeleteFileAsync($"{ftpfinalFolderPath}/{fileName}", ftpUsername, ftpPassword));
						await Task.WhenAll(deleteTasksX);
					}
				}

				// Paso 3: Transferir archivos a la carpeta final
				var transferTasks = fileList.Select(fileName => TransferFileAsync($"{ftpSubFolderPath}/{fileName}", $"{ftpfinalFolderPath}/{fileName}", ftpUsername, ftpPassword));
				await Task.WhenAll(transferTasks); // Transferencias en paralelo

				// Paso 4: Eliminar archivos de la carpeta temporal
				var deleteTasks = fileList.Select(fileName => DeleteFileAsync($"{ftpSubFolderPath}/{fileName}", ftpUsername, ftpPassword));
				await Task.WhenAll(deleteTasks);

				// Paso 5: Registrar imágenes
				foreach (var fileName in await ListFilesAsync(ftpfinalFolderPath, ftpUsername, ftpPassword))
				{
					var filePath = $"{ftpfinalFolderPath}/{fileName}".Replace(ftpBaseUrl, baseImagesUrl);
					images.Add(new SupervisionPictures { VehicleImageUrl = filePath });
				}
			}
			catch (WebException ex)
			{
				Console.WriteLine($"Error FTP: {((FtpWebResponse)ex.Response).StatusDescription}");
			}

			return images;
		}

		private async Task<List<SupervisionSummaryPictures>> SupervisionSummaryPictureUrlAsync(string stateName, string publicTransportGroupRif)
		{
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var ftpUsername = _configuration["FtpSettings:Username"];
			var ftpPassword = _configuration["FtpSettings:Password"];
			var baseImagesUrl = _configuration["FtpSettings:BaseImagesUrl"];

			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = $"{ftpBaseUrl}/{newFolderName}";
			var subFolderName = $"{publicTransportGroupRif}-resumen_supervision_temp";
			var subFinalFolderName = $"{publicTransportGroupRif}-resumen_supervision";
			var ftpSubFolderPath = $"{ftpFolderPath}/{subFolderName}";
			var ftpfinalFolderPath = $"{ftpFolderPath}/{subFinalFolderName}";

			var images = new List<SupervisionSummaryPictures>();

			// Paso 1: Verificar si la carpeta destino existe antes de crearla
			if (!await FolderExistsAsync(ftpfinalFolderPath, ftpUsername, ftpPassword))
			{
				await CreateFolderAsync(ftpfinalFolderPath, ftpUsername, ftpPassword);
			}

			try
			{
				// Paso 2: Obtener lista de archivos
				var fileList = await ListFilesAsync(ftpSubFolderPath, ftpUsername, ftpPassword);
				if (fileList.Count == 0) return images;

				// Paso 3: Transferir archivos a la carpeta final
				var transferTasks = fileList.Select(fileName => TransferFileAsync($"{ftpSubFolderPath}/{fileName}", $"{ftpfinalFolderPath}/{fileName}", ftpUsername, ftpPassword));
				await Task.WhenAll(transferTasks); // Transferencias en paralelo

				// Paso 4: Eliminar archivos de la carpeta temporal
				var deleteTasks = fileList.Select(fileName => DeleteFileAsync($"{ftpSubFolderPath}/{fileName}", ftpUsername, ftpPassword));
				await Task.WhenAll(deleteTasks);

				// Paso 5: Registrar imágenes
				foreach (var fileName in await ListFilesAsync(ftpfinalFolderPath, ftpUsername, ftpPassword))
				{
					var filePath = $"{ftpfinalFolderPath}/{fileName}".Replace(ftpBaseUrl, baseImagesUrl);
					images.Add(new SupervisionSummaryPictures { SupervisionSummaryPictureUrl = filePath });
				}
			}
			catch (WebException ex)
			{
				Console.WriteLine($"Error FTP: {((FtpWebResponse)ex.Response).StatusDescription}");
			}

			return images;
		}

		private async Task<bool> FolderExistsAsync(string folderPath, string username, string password)
		{
			try
			{
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderPath);
				request.Method = WebRequestMethods.Ftp.ListDirectory;
				request.Credentials = new NetworkCredential(username, password);
				using (await request.GetResponseAsync()) { return true; }
			}
			catch (WebException ex)
			{
				return ((FtpWebResponse)ex.Response).StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable;
			}
		}

		private async Task CreateFolderAsync(string folderPath, string username, string password)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderPath);
			request.Method = WebRequestMethods.Ftp.MakeDirectory;
			request.Credentials = new NetworkCredential(username, password);
			using (await request.GetResponseAsync()) { }
		}

		private async Task<List<string>> ListFilesAsync(string folderPath, string username, string password)
		{
			var files = new List<string>();
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderPath);
			request.Method = WebRequestMethods.Ftp.ListDirectory;
			request.Credentials = new NetworkCredential(username, password);

			using (var response = (FtpWebResponse)await request.GetResponseAsync())
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				string line;
				while ((line = await reader.ReadLineAsync()) != null)
				{
					files.Add(line);
				}
			}

			return files;
		}

		private async Task TransferFileAsync(string sourcePath, string destinationPath, string username, string password)
		{
			FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(sourcePath);
			downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
			downloadRequest.Credentials = new NetworkCredential(username, password);

			FtpWebResponse downloadResponse = (FtpWebResponse)await downloadRequest.GetResponseAsync();
			Stream responseStream = downloadResponse.GetResponseStream();

			FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create(destinationPath);
			uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
			uploadRequest.Credentials = new NetworkCredential(username, password);

			Stream requestStream = await uploadRequest.GetRequestStreamAsync();

			try
			{
				await responseStream.CopyToAsync(requestStream);
			}
			finally
			{
				responseStream.Close();
				requestStream.Close();
				downloadResponse.Close();
			}
		}

		private async Task DeleteFileAsync(string filePath, string username, string password)
		{
			try
			{
				FtpWebRequest deleteRequest = (FtpWebRequest)WebRequest.Create(filePath);
				deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
				deleteRequest.Credentials = new NetworkCredential(username, password);

				using (await deleteRequest.GetResponseAsync()) { } // Confirmación de eliminación
			}
			catch (WebException ex)
			{
				Console.WriteLine($"Error al eliminar {filePath}: {((FtpWebResponse)ex.Response).StatusDescription}");
			}
		}

		public JsonResult CheckExistingPlate(int paramValue1, string paramValue2)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if(!string.IsNullOrEmpty(paramValue2))
				{
					var existingPlate = _supervision.RegisteredPlate(paramValue2);
					if (existingPlate.Any())
					{
						if (!existingPlate.Where(x => x.DriverId == paramValue1).Any())
						{
							return Json($"El número de placa {paramValue2} ya está asignado al transportista {existingPlate.FirstOrDefault().DriverFullName} línea {existingPlate.FirstOrDefault().PTGCompleteName} estado {existingPlate.FirstOrDefault().StateName.ToUpper()}.");
						}
					}

					var plateRule = _commonData.GetCommonDataValueByName("ValidPlateRule");

					if (plateRule != null)
					{
						string regexPlatePattern = plateRule.CommonDataValue;

						Regex regexPlate = new Regex(regexPlatePattern);

						if (!regexPlate.IsMatch(paramValue2))
						{
							return Json($"El número de placa {paramValue2} debe tener un formato válido.");
						}
					}

					return Json("OK");
				}
			}

			return Json("ERROR");
		}

		public IActionResult PublicTransportGroupList()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _publicTransportGroup.GetAllBySupervisedDriversAndStateIdAndNotSummaryAdded((int)stateId);
					}
					else
					{
						model = _publicTransportGroup.GetAllBySupervisedDriversAndNotSummaryAdded();
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

		public async Task<IActionResult> AddSummaryAsync(int publicTransportGroupId)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
					int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");

					if (!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
					{
						return RedirectToAction("PublicTransportGroupList");
					}

					var ptg = _publicTransportGroup.GetPublicTransportGroupById(publicTransportGroupId);
					
					if (ptg != null) 
					{
						var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
						string stateName = ptg.StateName;
						var tempFolderName = $"{ptg.PublicTransportGroupRif}-resumen_supervision_temp";
						string ftpTempFolderPath = Path.Combine(ftpBaseUrl, stateName.ToUpper().Trim(), tempFolderName).Replace("\\", "/");

						await DeleteFilesInFTPFolderAsync(ftpTempFolderPath);

						var model = new SupervisionSummaryViewModel
						{
							PublicTransportGroupId = publicTransportGroupId,
							PTGCompleteName = ptg.PTGCompleteName,
							PublicTransportGroupRif = ptg.PublicTransportGroupRif,
							StateName = ptg.StateName,
							SupervisionDate = DateTime.Now,
							SecurityUserId = (int)HttpContext.Session.GetInt32("SecurityUserId")
						};

						ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

						if (securityGroupId != 1)
						{
							ViewBag.IsTotalAccess = _security.IsTotalAccess(3);
						}
						else
						{

							ViewBag.IsTotalAccess = true;
						}

						return View(model);
					}

					return RedirectToAction("PublicTransportGroupList");
				}

				return RedirectToAction("Login", "Security");
			}

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpPost]
		public async Task<IActionResult> AddSummary(SupervisionSummaryViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int supervisionSummaryId = 0;
					int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
					int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					if (_security.IsTotalAccess(3) || securityGroupId == 1)
					{
						model.Pictures = await SupervisionSummaryPictureUrlAsync(model.StateName, model.PublicTransportGroupRif);

						supervisionSummaryId = _supervision.AddOrEditSummary(model);

						if (supervisionSummaryId > 0)
						{
							return RedirectToAction("SummaryList");
						}
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
			}
		}

		public IActionResult EditSummary(int supervisionSummaryId, int publicTransportGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
				{
					return RedirectToAction("Login", "Security");
				}

				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");

				if (!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
				{
					return RedirectToAction("SummaryList");
				}

				var model = _supervision.GetSupervisionSummaryById(supervisionSummaryId);

				if (securityGroupId != 1)
				{
					ViewBag.IsTotalAccess = _security.IsTotalAccess(3);
				}
				else
				{

					ViewBag.IsTotalAccess = true;
				}

				return View(model);
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpPost]
		public IActionResult EditSummary(IFormCollection form, SupervisionSummaryViewModel model)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
				{
					return RedirectToAction("Login", "Security");
				}

				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
				{
					return RedirectToAction("Login", "Security");
				}

				if (_security.IsTotalAccess(3) || securityGroupId == 1)
				{
					List<SupervisionSummaryPictures> pictures = new List<SupervisionSummaryPictures>();

					foreach (var key in form.Keys)
					{
						if (key.StartsWith("Pictures["))
						{
							foreach (var value in form[key])
							{
								pictures.Add(new SupervisionSummaryPictures { SupervisionSummaryPictureUrl = value });
							}
						}
					}

					if (pictures.Any())
					{
						model.Pictures.AddRange(pictures);
					}

					_supervision.AddOrEditSummary(model);

					return RedirectToAction("EditSummary", new { supervisionSummaryId = model.SupervisionSummaryId });
				}
			}

			return RedirectToAction("Login", "Security");
		}

		public IActionResult SummaryList()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 3))
					{
						return RedirectToAction("Login", "Security");
					}

					List<SupervisionSummaryViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _supervision.GetSupervisionSummaryByStateId((int)stateId);
					}
					else
					{
						model = _supervision.GetAllSupervisionSummary();
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

		[HttpGet]
		public JsonResult CheckPermission(int publicTransportGroupId)
		{
			int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
			int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
			bool hasPermission = true;
			bool summaryDone = false;

			hasPermission = _supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId);

			if (!hasPermission)
			{
				return Json(new { hasPermission, message = "Esta organización está siendo supervisada por otro supervisor." });
			}

			summaryDone = _supervision.IsSupervisionSummaryDoneByPtgId(publicTransportGroupId);

			if (summaryDone)
			{
				if(!_security.GroupHasAccessToModule((int)securityGroupId, 19))
				{
					hasPermission = false;
					return Json(new { hasPermission, message = "Esta organización tiene realizado el resumen de supervisión, no pueden modificarse sus unidades." });
				}
			}

			return Json(new { hasPermission, message = "" });
		}

		[HttpGet]
		public JsonResult CheckPermissionSummary(int publicTransportGroupId)
		{
			int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
			bool hasPermission = true;

			hasPermission = _supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId);

			if (!hasPermission)
			{
				return Json(new { hasPermission, message = "Esta organización está siendo supervisada por otro supervisor." });
			}

			return Json(new { hasPermission, message = "" });
		}
	}
}

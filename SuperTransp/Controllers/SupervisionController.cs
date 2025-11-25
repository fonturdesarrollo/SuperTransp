using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
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
		private readonly ISupervision _supervision;
		private readonly ISecurity _security;
		private readonly IPublicTransportGroup _publicTransportGroup;
		private readonly ICommonData _commonData;
		private readonly IConfiguration _configuration;
		private readonly IGeography _geography;
		private readonly IFtpService _ftpService;
		private readonly IDriver _driver;
		private readonly IOptionsSnapshot<MaintenanceSettings> _settings;

		public SupervisionController(ISupervision supervision, ISecurity security, IPublicTransportGroup publicTransportGroup, 
			ICommonData commonData, IConfiguration configuration, IGeography geography, IFtpService ftpService, IDriver driver, IOptionsSnapshot<MaintenanceSettings> settings)
		{
			_supervision = supervision;
			_security = security;
			_publicTransportGroup = publicTransportGroup;
			_commonData = commonData;
			_configuration = configuration;
			_geography = geography;
			_ftpService = ftpService;
			_driver = driver;
			_settings = settings;
		}

		public IActionResult Index()
		{
			var result = CheckSessionAndPermission(3);
			if (result != null) return result;

			int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
			int? stateId = HttpContext.Session.GetInt32("StateId");

			if (securityGroupId.HasValue)
			{
				if (securityGroupId != 1)
				{
					if (_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
					}
					else
					{
						if (stateId.HasValue)
						{
							ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
						}
					}
				}
				else
				{
					ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");
				ViewBag.Months = new SelectList(_commonData.GetMonthNames(), "MonthId", "MonthName");
				ViewBag.Years = new SelectList(_commonData.GetCurrentYears(), "YearId", "YearName");
				ViewBag.ModulesInGroup = _security.GetModulesByGroupId(ViewBag.SecurityGroupId);
				ViewBag.RoundActive = _supervision.IsActiveSupervisionRoundByStateId((int)stateId);

				var currentRound = _supervision.GetActiveSupervisionRoundByStateId((int)stateId);

				if(currentRound != null)
				{
					ViewBag.CurrentRoundStartDate = $"{currentRound.SupervisionRoundStartDate.ToString("MMMM").ToUpper()} {currentRound.SupervisionRoundStartDate.ToString("yyyy")}";
				}
				else
				{
					ViewBag.CurrentRoundStartDate = "No existe vuelta de supervisión abierta";
				}

				ViewBag.RoundMessage = ViewBag.RoundActive  ? $"Vuelta {ViewBag.CurrentRoundStartDate}" : "No existe vuelta de supervisión abierta";
			}

			if (_settings.Value.IsActive)
			{
				ViewBag.MaintenanceMessage = _settings.Value.Message;
			}

			return View();
		}

		[HttpGet]
		public IActionResult PublicTransportGroupDriverList(string ptgRifName)
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

				var vm = new PublicTransportGroupDriverListPageVM
				{
					PTGRifName = ptgRifName ?? "",
					Items = new List<PublicTransportGroupViewModel>()
				};

				return View(vm);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
			}
		}

		[HttpGet]
		public IActionResult PublicTransportGroupDriverListData(string ptgRifName)
		{
			try
			{
				var perm = CheckSessionAndPermission(3);
				if (perm != null)
				{
					return Json(new { data = new List<object>(), message = "No autorizado" });
				}

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");

				List<PublicTransportGroupViewModel> model;

				if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
				{
					model = _supervision.GetDriverPublicTransportGroupByStateIdAndPTGRif((int)stateId, ptgRifName);
				}
				else
				{
					model = _supervision.GetAllDriverPublicTransportGroup(ptgRifName);
				}

				return Json(new { data = model });
			}
			catch (Exception ex)
			{
				Response.StatusCode = 500;
				return Json(new { data = new List<object>(), error = ex.Message });
			}
		}

		public IActionResult AddSupervisionRound(int monthValue, int yearValue, string supervisionRoundDescription, int stateId)
		{
			var result = CheckSessionAndPermission(24);
			if (result != null) return result;

			DateTime startDate;
			int year = yearValue;
			int month = monthValue;

			startDate = new DateTime(year, month, 1);

			SupervisionRoundModel model = new SupervisionRoundModel
			{
				SupervisionRoundStartDate = startDate,
				StateId = stateId,
				SupervisionRoundStartDescription = supervisionRoundDescription.ToUpper(),
				SupervisionRoundEndDate = startDate,
				SupervisionRoundEndDescription = supervisionRoundDescription,
				SupervisionRoundStatus = true
			};

			_supervision.AddOrEditRound(model);

			TempData["SuccessMessage"] = "Datos actualizados correctamente";

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> AddAsync(int publicTransportGroupId, string pTGCompleteName, string driverFullName, int partnerNumber, string? publicTransportGroupRif, int driverIdentityDocument, string stateName, int? supervisionStatus, int driverId, int supervisionId, int modeId, string modeName, int driverPublicTransportGroupId)
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
				var driver = _driver.GetByDriverPublicTransportGroupId(driverPublicTransportGroupId);

				ViewBag.IsTotalAccess = false;

				if (securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId, 3) || securityGroupId == 1)
				{
					if (!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
					{
						if (!_security.GroupHasAccessToModule((int)securityGroupId, 28))
						{
							TempData["SuccessMessage"] = "Esta organización está siendo supervisada por otro supervisor";
							return RedirectToAction("PublicTransportGroupDriverList", new { ptgRifName = publicTransportGroupRif });
						}
					}

					if (_supervision.IsSupervisionSummaryDoneByPtgId(publicTransportGroupId))
					{
						if (!_security.GroupHasAccessToModule((int)securityGroupId, 19))
						{
							TempData["SuccessMessage"] = "No tiene acceso a este proceso";
							return RedirectToAction("PublicTransportGroupDriverList", new { ptgRifName = publicTransportGroupRif });
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
						ModeName = modeName,
						DriverPublicTransportGroupId = driverPublicTransportGroupId
					};

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

					PopulateViewBagForSupervision();

					var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
					var tempFolderName = $"{publicTransportGroupRif}-{driverPublicTransportGroupId}-supervision_temp";
					string ftpTempFolderPath = Path.Combine(ftpBaseUrl, stateName.ToUpper().Trim(), tempFolderName).Replace("\\", "/");

					await _ftpService.DeleteFilesInFolderAsync(ftpTempFolderPath);

					if (securityGroupId != 1)
					{
						if (_security.IsTotalAccess(3) || _security.IsUpdateAccess(3))
						{
							ViewBag.IsTotalAccess = true;
						}
					}
					else
					{

						ViewBag.IsTotalAccess = true;
					}

					if(driver != null)
					{
						var currentRound = _supervision.GetActiveSupervisionRoundByStateId((int)driver.StateId);

						if (currentRound != null)
						{
							ViewBag.CurrentRoundStartDate = $"{currentRound.SupervisionRoundStartDate.ToString("MMMM").ToUpper()} {currentRound.SupervisionRoundStartDate.ToString("yyyy")}";
						}
						else
						{
							ViewBag.CurrentRoundStartDate = "No existe vuelta de supervisión abierta";
						}
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

		[HttpPost]
		public async Task<IActionResult> Add(SupervisionViewModel model)
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

				if (securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId, 3) || securityGroupId == 1)
				{
					if (_security.IsTotalAccess(3) || securityGroupId == 1)
					{
						int supervisionId = 0;
						var driver = _driver.GetByDriverPublicTransportGroupId(model.DriverPublicTransportGroupId);
						model.StateId = driver.StateId;

						if (!model.DriverWithVehicle || !model.InPerson)
						{
							model.InPerson = !model.DriverWithVehicle ? false : model.InPerson;

							supervisionId = _supervision.AddSimple(model);

							var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
							var folderName = $"{model.PublicTransportGroupRif}-{model.DriverPublicTransportGroupId}-supervision";
							string ftpFolderPath = Path.Combine(ftpBaseUrl, model.StateName.ToUpper().Trim(), folderName).Replace("\\", "/");

							await _ftpService.DeleteFilesInFolderAsync(ftpFolderPath);
						}
						else
						{
							model.Remarks = string.IsNullOrEmpty(model.Remarks) ? string.Empty : model.Remarks;
							model.VehicleImageUrl = string.IsNullOrEmpty(model.VehicleImageUrl) ? string.Empty : model.VehicleImageUrl;
							model.SupervisionStatus = true;
							model.FailureTypeId = model.WorkingVehicle ? 1 : model.FailureTypeId;

							var imageUrl = await SupervisionPictureUrl(model.StateName, model.PublicTransportGroupRif, model.DriverIdentityDocument, model.PartnerNumber, model.DriverPublicTransportGroupId, model.ModeId);

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

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public async Task<IActionResult> EditAsync(int publicTransportGroupId, string pTGCompleteName, string driverFullName, int partnerNumber, string? publicTransportGroupRif, int driverIdentityDocument, string stateName, int? supervisionStatus, int driverId, int supervisionId, int modeId, string modeName, int stateId, int driverPublicTransportGroupId)
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
				var driver = _driver.GetByDriverPublicTransportGroupId(driverPublicTransportGroupId);
				ViewBag.IsTotalAccess = false;

				if (securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId,3) || securityGroupId == 1)
				{
					if (!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
					{
						if (!_security.GroupHasAccessToModule((int)securityGroupId, 28))
						{
							TempData["SuccessMessage"] = "Esta organización está siendo supervisada por otro supervisor";
							return RedirectToAction("PublicTransportGroupDriverList", new { ptgRifName = publicTransportGroupRif });
						}
					}

					if (_supervision.IsSupervisionSummaryDoneByPtgId(publicTransportGroupId))
					{
						if (!_security.GroupHasAccessToModule((int)securityGroupId, 19))
						{
							TempData["SuccessMessage"] = "No tiene acceso a este proceso";
							return RedirectToAction("PublicTransportGroupDriverList", new { ptgRifName = publicTransportGroupRif });
						}
					}

					var model = _supervision.GetByPublicTransportGroupIdAndDriverIdAndPartnerNumberStateId(publicTransportGroupId, driverPublicTransportGroupId, partnerNumber, (int)stateId);

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
						
						PopulateViewBagForSupervision();

						ViewBag.Makes = new SelectList(_commonData.GetMakesByYear((int)model.Year).ToList(), "Make", "Make");
						ViewBag.VehicleModel = new SelectList(_commonData.GetModelsByYearAndMake((int)model.Year, model.Make).ToList(), "VehicleDataId", "ModelName");									

						var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
						var tempFolderName = $"{publicTransportGroupRif}-{driverPublicTransportGroupId}-supervision_temp";
						string ftpTempFolderPath = Path.Combine(ftpBaseUrl, stateName.ToUpper().Trim(), tempFolderName).Replace("\\", "/");

						await _ftpService.DeleteFilesInFolderAsync(ftpTempFolderPath);

						if (securityGroupId != 1)
						{
							if (_security.IsTotalAccess(3) || _security.IsUpdateAccess(3))
							{
								ViewBag.IsTotalAccess = true;
							}
						}
						else
						{

							ViewBag.IsTotalAccess = true;
						}

						if (driver != null)
						{
							var currentRound = _supervision.GetActiveSupervisionRoundByStateId((int)driver.StateId);

							if (currentRound != null)
							{
								ViewBag.CurrentRoundStartDate = $"{currentRound.SupervisionRoundStartDate.ToString("MMMM").ToUpper()} {currentRound.SupervisionRoundStartDate.ToString("yyyy")}";
							}
							else
							{
								ViewBag.CurrentRoundStartDate = "No existe vuelta de supervisión abierta";
							}
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
		public IActionResult Edit(SupervisionSummaryViewModel model)
		{
			var result = CheckSessionAndPermission(3);
			if (result != null) return result;

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
		[RequestSizeLimit(20_000_000)]
		[RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
		public async Task<IActionResult> SaveFiles(IFormFile file, string stateName, string driverIdentityDocument, string partnerNumber, string publicTransportGroupRif, string driverId, int driverPublicTransportGroupId)
		{
			if (file?.Length > 0 &&
				!string.IsNullOrEmpty(stateName) &&
				!string.IsNullOrEmpty(driverIdentityDocument) &&
				!string.IsNullOrEmpty(partnerNumber) &&
				!string.IsNullOrEmpty(publicTransportGroupRif))
			{
				var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
				var newFolderName = $"{stateName.ToUpper().Trim()}";
				var ftpFolderPath = Path.Combine(ftpBaseUrl, newFolderName).Replace("\\", "/");
				var subFolderName = $"{publicTransportGroupRif}-{driverPublicTransportGroupId}-supervision_temp";
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
					// Crear la carpeta principal si no existe
					if (!await _ftpService.FolderExistsAsync(ftpFolderPath))
						await _ftpService.CreateFolderAsync(ftpFolderPath);

					// Crear la subcarpeta si no existe
					if (!await _ftpService.FolderExistsAsync(ftpSubFolderPath))
						await _ftpService.CreateFolderAsync(ftpSubFolderPath);

					// Subir el archivo
					using (var memoryStream = new MemoryStream())
					{
						await file.CopyToAsync(memoryStream);
						memoryStream.Position = 0;
						await _ftpService.UploadFileAsync(memoryStream, ftpSubFolderPath, ftpFileName);
					}

					return Ok(new { message = $"Archivo cargado exitosamente en {ftpSubFolderPath}/{ftpFileName}" });
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
			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = Path.Combine(ftpBaseUrl, newFolderName).Replace("\\", "/");
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
				// Crear la carpeta principal si no existe
				if (!await _ftpService.FolderExistsAsync(ftpFolderPath))
					await _ftpService.CreateFolderAsync(ftpFolderPath);

				// Crear la subcarpeta si no existe
				if (!await _ftpService.FolderExistsAsync(ftpSubFolderPath))
					await _ftpService.CreateFolderAsync(ftpSubFolderPath);

				// Subir el archivo
				using (var memoryStream = new MemoryStream())
				{
					await file.CopyToAsync(memoryStream);
					memoryStream.Position = 0;
					await _ftpService.UploadFileAsync(memoryStream, ftpSubFolderPath, ftpFileName);
				}

				return Ok(new { message = $"Archivo cargado exitosamente en {ftpSubFolderPath}/{ftpFileName}" });
			}
			catch (Exception ex)
			{
				return BadRequest($"Error al procesar el archivo o la carpeta: {ex.Message}");
			}
		}

			return BadRequest("No se pudo subir el archivo.");
		}

		public async Task<JsonResult> DeleteAllSupervisionPictures(string stateName, string publicTransportGroupRif, int partnerNumber, int publicTransportGroupId, int driverId, int driverPublicTransportGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
				var newFolderName = $"{stateName.ToUpper().Trim()}";
				var ftpFolderPath = Path.Combine(ftpBaseUrl, newFolderName).Replace("\\", "/");
				var subFolderName = $"{publicTransportGroupRif}-{driverPublicTransportGroupId}-supervision_temp";
				var ftpSubFolderPath = Path.Combine(ftpFolderPath, subFolderName).Replace("\\", "/");

				await _ftpService.DeleteFilesInFolderAsync(ftpSubFolderPath);

				return Json("OK");
			}

			return Json("ERROR");
		}

		private async Task<List<SupervisionPictures>> SupervisionPictureUrl(string stateName, string publicTransportGroupRif, int driverIdentityDocument, int partnerNumber, int driverId, int ptgMode)
		{
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var baseImagesUrl = _configuration["FtpSettings:BaseImagesUrl"];

			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = $"{ftpBaseUrl}/{newFolderName}";
			var subFolderName = $"{publicTransportGroupRif}-{driverId}-supervision_temp";
			var subFinalFolderName = $"{publicTransportGroupRif}-{driverId}-supervision";
			var ftpSubFolderPath = $"{ftpFolderPath}/{subFolderName}";
			var ftpfinalFolderPath = $"{ftpFolderPath}/{subFinalFolderName}";

			var images = new List<SupervisionPictures>();

			// Paso 1: Verificar si la carpeta destino existe antes de crearla
			if (!await _ftpService.FolderExistsAsync(ftpfinalFolderPath))
			{
				await _ftpService.CreateFolderAsync(ftpfinalFolderPath);
			}

			try
			{
				// Paso 2: Obtener lista de archivos
				var fileList = await _ftpService.ListFilesAsync(ftpSubFolderPath);
				if (fileList.Count == 0) return images;

				// Paso X: Eliminar archivos de la carpeta final
				if (fileList.Any())
				{
					var currentFileList = await _ftpService.ListFilesAsync(ftpfinalFolderPath);
					if (currentFileList.Count > 0)
					{
						var deleteTasksX = currentFileList.Select(fileName => _ftpService.DeleteFileAsync($"{ftpfinalFolderPath}/{fileName}"));
						await Task.WhenAll(deleteTasksX);
					}
				}

				// Paso 2.1: Filtrar archivos según ptgMode
				if (ptgMode != 4 && fileList.Count > 4)
				{
					fileList = fileList.Take(4).ToList();
				}
				else if (ptgMode == 4 && fileList.Count > 2)
				{
					fileList = fileList.Take(2).ToList();
				}

				// Paso 3: Transferir archivos a la carpeta final
				var transferTasks = fileList.Select(fileName =>
					_ftpService.TransferFileAsync($"{ftpSubFolderPath}/{fileName}", $"{ftpfinalFolderPath}/{fileName}"));
				await Task.WhenAll(transferTasks); // Transferencias en paralelo

				// Paso 4: Eliminar archivos de la carpeta temporal
				await _ftpService.DeleteFilesInFolderAsync(ftpSubFolderPath);

				// Paso 5: Registrar imágenes
				foreach (var fileName in await _ftpService.ListFilesAsync(ftpfinalFolderPath))
				{
					var filePath = $"{ftpfinalFolderPath}/{fileName}".Replace(ftpBaseUrl, baseImagesUrl);
					images.Add(new SupervisionPictures { VehicleImageUrl = filePath });
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error FTP: {ex.Message}");
			}

			return images;
		}

		private async Task<List<SupervisionSummaryPictures>> SupervisionSummaryPictureUrlAsync(string stateName, string publicTransportGroupRif)
		{
			int? stateId = HttpContext.Session.GetInt32("StateId");
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var ftpUsername = _configuration["FtpSettings:Username"];
			var ftpPassword = _configuration["FtpSettings:Password"];
			var baseImagesUrl = _configuration["FtpSettings:BaseImagesUrl"];

			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = $"{ftpBaseUrl}/{newFolderName}";
			var subFolderName = $"{publicTransportGroupRif}-resumen_supervision_temp";
			var subFinalFolderName = $"{publicTransportGroupRif}-resumen_supervision";
			var supervisionFolderName = "RESUMEN_DE_SUPERVISION";
			var roundFolderName = string.Empty;
			var ftpSubFolderPath = $"{ftpFolderPath}/{subFolderName}";
			var ftpFinalFolderPath = $"{ftpFolderPath}/{subFinalFolderName}";
			var images = new List<SupervisionSummaryPictures>();

			try
			{
				roundFolderName = $"VUELTA_{_supervision.GetActiveSupervisionRoundByStateId((int)stateId).SupervisionRoundStartDate.ToString("MM-yyyy")}";

				// Verificar si la carpeta RESUMEN_DE_SUPERVISION existe antes de crearla
				if (!await _ftpService.FolderExistsAsync($"{ftpFolderPath}/{supervisionFolderName}"))
				{
					await _ftpService.CreateFolderAsync($"{ftpFolderPath}/{supervisionFolderName}");
				}

				// Verificar si la carpeta VUELTA existe antes de crearla
				if (!await _ftpService.FolderExistsAsync($"{ftpFolderPath}/{supervisionFolderName}/{roundFolderName}"))
				{
					await _ftpService.CreateFolderAsync($"{ftpFolderPath}/{supervisionFolderName}/{roundFolderName}");
				}

				ftpFinalFolderPath = $"{ftpFolderPath}/{supervisionFolderName}/{roundFolderName}/{subFinalFolderName}";
				// Paso 1: Verificar si la carpeta destino existe antes de crearla
				if (!await _ftpService.FolderExistsAsync(ftpFinalFolderPath))
				{
					await _ftpService.CreateFolderAsync(ftpFinalFolderPath);
				}

				// Paso 2: Obtener lista de archivos
				var fileList = await _ftpService.ListFilesAsync(ftpSubFolderPath);
				if (fileList.Count == 0) return images;

				// Paso 3: Transferir archivos a la carpeta final
				var transferTasks = fileList.Select(fileName =>
					_ftpService.TransferFileAsync($"{ftpSubFolderPath}/{fileName}", $"{ftpFinalFolderPath}/{fileName}"));
				await Task.WhenAll(transferTasks);

				// Paso 4: Registrar imágenes
				foreach (var fileName in await _ftpService.ListFilesAsync(ftpSubFolderPath))
				{
					var filePath = $"{ftpFinalFolderPath}/{fileName}".Replace(ftpBaseUrl, baseImagesUrl);
					images.Add(new SupervisionSummaryPictures { SupervisionSummaryPictureUrl = filePath });
				}

				// Paso 5: Eliminar archivos de la carpeta temporal
				var deleteTasks = fileList.Select(fileName => _ftpService.DeleteFileAsync($"{ftpSubFolderPath}/{fileName}"));
				await Task.WhenAll(deleteTasks);

				// Paso 6: Eliminar la carpeta temporal
				if (await _ftpService.FolderExistsAsync(ftpSubFolderPath))
				{
					await _ftpService.DeleteFolderAsync(ftpSubFolderPath);
				}
			}
			catch (WebException ex)
			{
				Console.WriteLine($"Error FTP: {((FtpWebResponse)ex.Response).StatusDescription}");
			}

			return images;
		}

		public JsonResult CheckExistingPlate(int paramValue1, string paramValue2)
		{
			var result = CheckSessionAndPermission(3);
			if (result != null) return Json("ERROR");

			if (!string.IsNullOrEmpty(paramValue2))
			{
				var existingPlate = _supervision.RegisteredPlate(paramValue2);
				if (existingPlate.Any())
				{
					if (!existingPlate.Where(x => x.DriverId == paramValue1).Any())
					{
						return Json($"El número de placa {paramValue2} ya está asignado al socio {existingPlate.FirstOrDefault().DriverFullName} organización {existingPlate.FirstOrDefault().PTGCompleteName} estado {existingPlate.FirstOrDefault().StateName.ToUpper()}.");
					}
				}

				//Esto esta en espera de que se avance mas con la supervision 22-10-2025
				//var plateRule = _commonData.GetCommonDataValueByName("ValidPlateRule");

				//if (plateRule != null)
				//{
				//	string regexPlatePattern = plateRule.CommonDataValue;

				//	Regex regexPlate = new Regex(regexPlatePattern);

				//	if (!regexPlate.IsMatch(paramValue2))
				//	{
				//		return Json($"El número de placa {paramValue2} debe tener un formato válido.");
				//	}
				//}

				return Json("OK");
			}

			return Json("ERROR");
		}

		public IActionResult PublicTransportGroupList()
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

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
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public async Task<IActionResult> AddSummaryAsync(int publicTransportGroupId)
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				ViewBag.IsTotalAccess = false;

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

					await _ftpService.DeleteFilesInFolderAsync(ftpTempFolderPath);

					var model = new SupervisionSummaryViewModel
					{
						PublicTransportGroupId = publicTransportGroupId,
						PTGCompleteName = ptg.PTGCompleteName,
						PublicTransportGroupRif = ptg.PublicTransportGroupRif,
						StateName = ptg.StateName,
						SupervisionDate = DateTime.Now,
						SecurityUserId = (int)HttpContext.Session.GetInt32("SecurityUserId"),
						StateId = ptg.StateId
					};

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

					if (securityGroupId != 1)
					{
						if(_security.IsTotalAccess(3) || _security.IsUpdateAccess(3))
						{
							ViewBag.IsTotalAccess = true;
						}
					}
					else
					{

						ViewBag.IsTotalAccess = true;
					}

					return View(model);
				}

				return RedirectToAction("PublicTransportGroupList");
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
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

				int supervisionSummaryId = 0;
				int supervisionSummaryIdExisting = 0;
				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

				if (_security.IsTotalAccess(3) || securityGroupId == 1)
				{
					supervisionSummaryIdExisting = _supervision.IsSupervisionSummaryDoneByRIF(model.PublicTransportGroupRif);

					model.Pictures = await SupervisionSummaryPictureUrlAsync(model.StateName, model.PublicTransportGroupRif);

					model.SupervisionSummaryId = supervisionSummaryIdExisting;					

					supervisionSummaryId = _supervision.AddOrEditSummary(model);

					if (supervisionSummaryId > 0 || supervisionSummaryIdExisting > 0)
					{
						return RedirectToAction("PublicTransportGroupList");
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
			var result = CheckSessionAndPermission(3);
			if (result != null) return result;

			int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
			int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
			ViewBag.IsTotalAccess = false;
			ViewBag.DeleteSummary = false;

			if (securityGroupId > 2)
			{
				if (!_supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId))
				{
					return RedirectToAction("SummaryList");
				}
			}

			var model = _supervision.GetSupervisionSummaryById(supervisionSummaryId);

			if (securityGroupId != 1)
			{
				if (_security.IsTotalAccess(3) || _security.IsUpdateAccess(3))
				{
					ViewBag.IsTotalAccess = true;

					if (_security.GroupHasAccessToModule((int)securityGroupId, 29))
					{
						ViewBag.DeleteSummary = true;
					}
				}
			}
			else
			{
				ViewBag.IsTotalAccess = true;
				ViewBag.DeleteSummary = true;
			}

			return View(model);
		}

		[HttpPost]
		public IActionResult EditSummary(IFormCollection form, SupervisionSummaryViewModel model, string actionType)
		{
			var result = CheckSessionAndPermission(3);
			if (result != null) return result;

			int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
			int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

			if (_security.IsTotalAccess(3) || securityGroupId == 1)
			{
				if (actionType == "delete")
				{
					return RedirectToAction("DeleteSummary", new { supervisionSummaryId = model.SupervisionSummaryId });
				}

				List<SupervisionSummaryPictures> pictures = new List<SupervisionSummaryPictures>();
				// TODO FIX THIS
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

				TempData["SuccessMessage"] = "Datos actualizados correctamente";

				return RedirectToAction("PublicTransportGroupList");
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpGet]
		public async Task<IActionResult> DeleteSummary(int supervisionSummaryId)
		{
			var summaryData = _supervision.GetSupervisionSummaryById(supervisionSummaryId);
			var summaryFolderName = string.Empty;
			string ftpFilesFolderPath = string.Empty;

			if (summaryData != null)
			{
				var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
				var stateName = summaryData.StateName.ToUpper();
				var summaryParentFolderName = "RESUMEN_DE_SUPERVISION";
				summaryFolderName = $"{summaryData.PublicTransportGroupRif}-resumen_supervision";

				if (summaryData.Pictures.Any())
				{
					if (summaryData.Pictures.Any(p =>
						!string.IsNullOrEmpty(p.SupervisionSummaryPictureUrl) &&
						p.SupervisionSummaryPictureUrl.Contains(summaryParentFolderName)))
					{
						string url = summaryData.Pictures
							.FirstOrDefault(p => !string.IsNullOrEmpty(p.SupervisionSummaryPictureUrl))?
							.SupervisionSummaryPictureUrl;

						if (!string.IsNullOrEmpty(url))
						{
							var match = Regex.Match(url, @"RESUMEN_DE_SUPERVISION/VUELTA_\d{2}-\d{4}");
							if (match.Success)
							{
								ftpFilesFolderPath = Path.Combine(
									ftpBaseUrl,
									stateName.ToUpper().Trim(),
									match.Value,
									summaryFolderName
								).Replace("\\", "/");
							}
						}
					}
				}

				if (!string.IsNullOrEmpty(ftpFilesFolderPath))
				{
					await _ftpService.DeleteFilesInFolderAsync(ftpFilesFolderPath);
				}

				_supervision.DeleteSupervisionSummaryById(supervisionSummaryId);
			}

			return RedirectToAction("SummaryList");
		}

		public IActionResult SummaryList()
		{
			try
			{
				var result = CheckSessionAndPermission(3);
				if (result != null) return result;

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
				if (!_security.GroupHasAccessToModule((int)securityGroupId, 28))
				{
					return Json(new { hasPermission, message = "Esta organización está siendo supervisada por otro supervisor." });
				}
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
			int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
			bool hasPermission = true;

			if(securityGroupId > 2)
			{
				hasPermission = _supervision.IsUserSupervisingPublicTransportGroup((int)securityUserId, publicTransportGroupId);

				if (!hasPermission)
				{
					return Json(new { hasPermission, message = "Esta organización está siendo supervisada por otro supervisor." });
				}
			}

			return Json(new { hasPermission, message = "" });
		}

		[HttpGet]
		public JsonResult CheckValidRound(int stateId, int month, int year)
		{
			bool invalidMonth = true;

			invalidMonth = _supervision.IsActiveSupervisionRoundByStateMonthAndYear((int)stateId, month, year);

			if (invalidMonth)
			{
				return Json(new { invalidMonth, message = $"Esta vuelta de supervisión {month} - {year} está activa" });
			}

			invalidMonth = _supervision.IsFinishedSupervisionRoundByStateMonthAndYear((int)stateId, month, year);

			if (invalidMonth)
			{
				return Json(new { invalidMonth, message = $"Esta vuelta de supervisión {month} - {year} está finalizada" });
			}

			return Json(new { invalidMonth, message = "" });
		}

		private void PopulateViewBagForSupervision()
		{
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
}

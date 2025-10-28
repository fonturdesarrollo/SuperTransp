using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using QRCoder;
using SuperTransp.Core;
using SuperTransp.Models;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class DriverController : Controller
	{
		private readonly ISecurity _security;
		private readonly IGeography _geography;
		private readonly IPublicTransportGroup _publicTransportGroup;
		private readonly IDriver _driver;
		private readonly IConfiguration _configuration;
		private readonly ICommonData _commonData;
		private readonly IFtpService _ftpService;
		private readonly IOptions<MaintenanceSettings> _settings;

		public DriverController(IDriver driver, IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography, 
			IConfiguration configuration, ICommonData commonData, IFtpService ftpService, IOptions<MaintenanceSettings> settings)
		{
			_driver = driver;
			_publicTransportGroup = publicTransportGroup;
			_security = security;
			_geography = geography;
			_configuration = configuration;
			_commonData = commonData;
			_ftpService = ftpService;
			_settings = settings;
		}

		public IActionResult Index()
		{
			var result = CheckSessionAndPermission(2);
			if (result != null) return result;

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
			ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

			if (_settings.Value.IsActive)
			{
				ViewBag.MaintenanceMessage = _settings.Value.Message;
			}

			return View();
		}

		public IActionResult PublicTransportGroupList()
		{
			try
			{
				var result = CheckSessionAndPermission(2);
				if (result != null) return result;

				List<PublicTransportGroupViewModel> model = new();

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

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
				var result = CheckSessionAndPermission(2);
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

		public IActionResult Add(int publicTransportGroupId, string pTGCompleteName)
		{
			try
			{
				var result = CheckSessionAndPermission(2);
				if (result != null) return result;

				ViewBag.IsTotalAccess = false;
				ViewBag.IsDeleteAccess = false;
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				var ptg = _publicTransportGroup.GetPublicTransportGroupById(publicTransportGroupId);
				var model = new DriverViewModel
				{
					PublicTransportGroupId = publicTransportGroupId,
					PTGCompleteName = pTGCompleteName,
					DriverModifiedDate = DateTime.Now,
					Birthdate = DateTime.Now.AddYears(-20)
				};

				ViewBag.Partners = ptg.Partners;
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

				if (securityGroupId != 1)
				{
					if (_security.IsTotalAccess(2) || _security.IsUpdateAccess(2))
					{
						ViewBag.IsTotalAccess = true;
					}

					if (_security.IsTotalAccess(2))
					{
						ViewBag.IsDeleteAccess = true;
					}
				}
				else
				{
					ViewBag.IsTotalAccess = true;
					ViewBag.IsDeleteAccess = true;
				}

				ViewBag.Sex = new SelectList(_commonData.GetSex(), "SexId", "SexName");

				return View(model);
			}

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult GetDriversByPTG(int publicTransportGroupId)
		{
			try
			{
				var result = CheckSessionAndPermission(2);
				if (result != null) return result;

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				var drivers = _driver.GetByPublicTransportGroupId(publicTransportGroupId);
				var isTotalAccess = securityGroupId == 1 || _security.IsTotalAccess(2) || _security.IsUpdateAccess(2);
				var isDeleteAccess = securityGroupId == 1 || _security.IsTotalAccess(2);
				var editControllerUrl = $"{Url.Action("Edit", "Driver")}?driverPublicTransportGroupId=";

				var data = drivers.Select(driver => new
				{
					nombre = driver.DriverFullName,
					cedula = driver.DriverIdentityDocument,
					socio = driver.PartnerNumber,
					telefono = driver.DriverPhone,
					sexo = driver.SexName,
					driverId = driver.DriverId,
					ptgGUID = $"{driver.PublicTransportGroupGUID}|{driver.PartnerNumber}",
					nacimiento = driver.Birthdate.ToString("dd/MM/yyyy"),
					modificar = isTotalAccess ? $@"<a id='btnEdit' href='{editControllerUrl}{driver.DriverPublicTransportGroupId}'>MODIFICAR</a>" : "<span>SOLO LECTURA</span>",
					eliminar = isDeleteAccess ? $@"<a id='btnDelete' href='javascript:void(0);' onclick=""confirmDeletion('/Driver/Delete?driverId={driver.DriverId}&driverPublicTransportGroupId={driver.DriverPublicTransportGroupId}&partnerNumber={driver.PartnerNumber}&publicTransportGroupId={driver.PublicTransportGroupId}&pTGCompleteName={driver.PTGCompleteName}')"">ELIMINAR</a>" : "<span>SOLO LECTURA</span>",
					qr = "<button class='generateQR view-info p-1' type='button'><i class='bi bi-qr-code'></i></button>"
				});

				return Json(new { data });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddWithAjax(DriverViewModel model)
		{
			try
			{
				if (string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) || !ModelState.IsValid)
					return Json(new { success = false, redirect = Url.Action("Login", "Security") });

				int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

				if (groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 2))
					return Json(new { success = false, redirect = Url.Action("Login", "Security") });

				if (_security.IsTotalAccess(2) || _security.IsUpdateAccess(2) || groupId == 1)
				{
					int driverId = _driver.AddOrEdit(model);

					//if (driverId > 0)
					//{
						return Json(new
						{
							success = true,
							message = "Datos actualizados correctamente"
						});
					//}
				}

				return Json(new { success = false, redirect = Url.Action("Login", "Security") });
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult Edit(int driverPublicTransportGroupId)
		{
			var result = CheckSessionAndPermission(2);
			if (result != null) return result;

			ViewBag.IsTotalAccess = false;
			ViewBag.IsDeleteAccess = false;
			var model = _driver.GetByDriverPublicTransportGroupId(driverPublicTransportGroupId);
			var ptg = _publicTransportGroup.GetPublicTransportGroupById(model.PublicTransportGroupId);
			int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
			ViewBag.Partners = ptg.Partners;

			if (securityGroupId != 1)
			{
				if (_security.IsTotalAccess(2) || _security.IsUpdateAccess(2))
				{
					ViewBag.IsTotalAccess = true;
				}
			}
			else
			{
				ViewBag.IsTotalAccess = true;
			}

			ViewBag.Sex = new SelectList(_commonData.GetSex(), "SexId", "SexName");

			return View(model);
		}

		[HttpPost]
		public JsonResult EditWithAjax(DriverViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 2))
					{
						return Json(new
						{
							success = false,
							redirectUrl = Url.Action("Login", "Security")
						});
					}

					if (_security.IsTotalAccess(2) || _security.IsUpdateAccess(2) || securityGroupId == 1)
					{
						_driver.AddOrEdit(model);

						return Json(new
						{
							success = true,
							message = "Datos actualizados correctamente",
							redirectUrl = Url.Action("Add", new { publicTransportGroupId = model.PublicTransportGroupId, pTGCompleteName = model.PTGCompleteName })
						});
					}
				}

				return Json(new
				{
					success = false,
					redirectUrl = Url.Action("Login", "Security")
				});
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<JsonResult> DeleteDriverAjaxAsync(int driverId, int driverPublicTransportGroupId, int partnerNumber)
		{
			try
			{
				if (string.IsNullOrEmpty(HttpContext.Session?.GetString("SecurityUserId")))
					return Json(new { success = false, redirect = Url.Action("Login", "Security") });

				if (!ModelState.IsValid)
					return Json(new { success = false, message = "Datos inválidos para eliminar el socio." });

				var groupId = HttpContext.Session.GetInt32("SecurityGroupId");

				if (groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 2))
					return Json(new { success = false, redirect = Url.Action("Login", "Security") });

				if (groupId == 1 || _security.IsTotalAccess(2))
				{
					var driverData = _driver.GetPartnerById(driverPublicTransportGroupId);
					var result = _driver.DeletePartner(driverId, driverPublicTransportGroupId, partnerNumber);

					if (result)
					{
						await DeleteDriverVehiclePictures(driverData.StateName, driverData.PublicTransportGroupRif, driverPublicTransportGroupId);

						return Json(new
						{
							success = true,
							message = "Registro eliminado correctamente"
						});
					}
				}

				return Json(new { success = false, message = "No tiene permisos suficientes para eliminar el registro." });
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = $"Error al eliminar: {ex.Message}"
				});
			}
		}

		private async Task<bool> DeleteDriverVehiclePictures(string? stateName, string? publicTransportGroupRif, int driverId)
		{
			var ftpBaseUrl = _configuration["FtpSettings:BaseUrl"];
			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = $"{ftpBaseUrl}/{newFolderName}";
			var subFolderName = $"{publicTransportGroupRif}-{driverId}-supervision";
			var ftpSubFolderPath = $"{ftpFolderPath}/{subFolderName}";

			if (await _ftpService.FolderExistsAsync(ftpSubFolderPath))
			{
				try
				{
					var fileList = await _ftpService.ListFilesAsync(ftpSubFolderPath);
					if (fileList.Count == 0) return false;

					var deleteTasks = fileList.Select(fileName => _ftpService.DeleteFileAsync($"{ftpSubFolderPath}/{fileName}"));
					await Task.WhenAll(deleteTasks);

					try
					{
						await _ftpService.DeleteFolderAsync(ftpSubFolderPath);
					}
					catch (WebException ex)
					{
						Console.WriteLine($"Error al eliminar la carpeta FTP: {((FtpWebResponse)ex.Response).StatusDescription}");
					}

				}
				catch (WebException ex)
				{
					Console.WriteLine($"Error FTP: {((FtpWebResponse)ex.Response).StatusDescription}");
				}

				return true;
			}

			return false;
		}

		public JsonResult CheckDocumentIdExist(int paramValue1, int paramValue2)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (_driver.RegisteredDocumentId(paramValue1,paramValue2))
				{
					return Json($"La cédula {paramValue1} ya está registrada a la línea.");
				}

				return Json("OK");
			}

			return Json("ERROR");
		}

		[HttpPost]
		public IActionResult GetDriverDataByIdDocument(int driverIdentityDocument)
		{
			if (driverIdentityDocument > 0)
			{
				var driver = _driver.GetByIdentityDocument(driverIdentityDocument);
				if (driver != null)
				{
					return Json(new { driverId = driver.DriverId, driverFullName = driver.DriverFullName, driverPhone = driver.DriverPhone, driverSexId = driver.SexId, driverBirthDate = driver.Birthdate.ToString("dd/MM/yyyy") });
				}
			}

			return Json(new { driverFullName = "", driverPhone = "" });			
		}

		public JsonResult CheckExistingValues(int paramValue2, int paramValue4)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (_driver.RegisteredPartnerNumber(paramValue4, paramValue2))
				{
					return Json($"El número de socio {paramValue4} ya está registrado a la línea.");
				}

				var ptgPartners = _publicTransportGroup.GetPublicTransportGroupById(paramValue2);
				var totalDrivers = _driver.TotalDriversByPublicTransportGroupId(paramValue2);

				if (totalDrivers >= ptgPartners.Partners)
				{
					return Json($"La línea tiene cupo solo para {ptgPartners.Partners} transportista(s) no puede agregar mas.");
				}

				//ELIMINADA VALIDACIÓN DE NÚMERO DE SOCIO CORRELATIVO
				//if(paramValue4 > ptgPartners.Partners)
				//{
				//	return Json($"La línea tiene cupo solo para {ptgPartners.Partners} transportista(s) no puede agregar un número de socio {paramValue4}.");
				//}

				return Json("OK");
			}

			return Json("ERROR");
		}

		public JsonResult CheckExistingValuesOnEdit(int paramValue2, int paramValue4, int paramValue5)
		{
			var result = CheckSessionAndPermission(2);
			if (result != null)
			{
				return Json(new
				{
					canContinue = false,
					message = "Sesión inválida"
				});
			} 

			if (_driver.RegisteredPartnerNumber(paramValue4, paramValue2))
			{
				var allData = _driver.GetByPublicTransportGroupId(paramValue2);
				var driverData = allData.Where(x => x.PartnerNumber == paramValue4 && x.DriverId == paramValue5).ToList();

				if (!driverData.Any())
				{
					var finalData = allData.Where(x => x.PartnerNumber == paramValue4 && x.DriverId != paramValue5);
					if (finalData.Any())
					{
						return Json(new
						{
							canContinue = false,
							message = $"El número de socio {paramValue4} ya está registrado a la línea."
						});
					}
				}
			}

			//ELIMINADA VALIDACIÓN DE NÚMERO DE SOCIO CORRELATIVO
			//var ptgPartners = _publicTransportGroup.GetPublicTransportGroupById(paramValue2);

			//if (paramValue4 > ptgPartners.Partners)
			//{
			//	return Json(new
			//	{
			//		canContinue = false,
			//		message = $"La línea tiene cupo solo para {ptgPartners.Partners} transportista(s), no puede agregar el número de socio {paramValue4}."
			//	});
			//}

			return Json(new
			{
				canContinue = true,
				message = "OK"
			});	
		}

		[HttpPost]
		public IActionResult GenerateQR([FromBody] QRDriverRequest request)
		{
			var baseWebSiteUrl = _configuration["FtpSettings:BaseWebSiteUrl"];
			var ptgDataController = $"{baseWebSiteUrl}QR/DriverData?driverCode={request.ptgGUID}";

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
		private IActionResult? CheckSessionAndPermission(int requiredModuleId)
		{
			var securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

			if (securityGroupId == null)
				return RedirectToAction("Login", "Security");

			if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, requiredModuleId))
				return RedirectToAction("Login", "Security");

			return null;
		}

		public class QRDriverRequest
		{
			public string ptgGUID { get; set; }
		}
	}
}

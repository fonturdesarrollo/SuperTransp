using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
		private ISecurity _security;
		private IGeography _geography;
		private IPublicTransportGroup _publicTransportGroup;
		private IDriver _driver;
		private IConfiguration _configuration;
		private ICommonData _commonData;

		public DriverController(IDriver driver, IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography, IConfiguration configuration, ICommonData commonData)
		{
			_driver = driver;
			_publicTransportGroup = publicTransportGroup;
			_security = security;
			_geography = geography;
			_configuration = configuration;
			_commonData = commonData;
		}
		public IActionResult Index()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FullName")) && HttpContext.Session.GetInt32("SecurityGroupId") != null)
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
				{
					return RedirectToAction("Login", "Security");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
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
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					}

					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

					return View();
				}

				return RedirectToAction("Login", "Security");
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
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					} 

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

				return RedirectToAction("Login", "Security");
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
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					}

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

					//ViewBag.Drivers = _driver.GetByPublicTransportGroupId(publicTransportGroupId);
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

				return RedirectToAction("Login", "Security");
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
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					}

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
						eliminar = isDeleteAccess ? $@"<a id='btnDelete' href='javascript:void(0);' onclick=""confirmDeletion('/Driver/Delete?driverId={driver.DriverId}&publicTransportGroupId={driver.PublicTransportGroupId}&pTGCompleteName={driver.PTGCompleteName}')"">ELIMINAR</a>" : "<span>SOLO LECTURA</span>",
						qr = "<button class='generateQR view-info p-1' type='button'><i class='bi bi-qr-code'></i></button>"
					});

					return Json(new { data });
				}

				return RedirectToAction("Login", "Security");
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

					if (driverId > 0)
					{
						return Json(new
						{
							success = true,
							message = "Datos actualizados correctamente"
						});
					}
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
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
				{
					return RedirectToAction("Login", "Security");
				}

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

			return RedirectToAction("Login", "Security");
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
		public async Task<JsonResult> DeleteDriverAjaxAsync(int driverId)
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
					var driverData = _driver.GetById(driverId);
					var result = _driver.Delete(driverId);

					if (result)
					{
						await DeleteDriverVehiclePictures(driverData.StateName, driverData.PublicTransportGroupRif, driverData.DriverId);

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
			var ftpUsername = _configuration["FtpSettings:Username"];
			var ftpPassword = _configuration["FtpSettings:Password"];
			var baseImagesUrl = _configuration["FtpSettings:BaseImagesUrl"];

			var newFolderName = $"{stateName.ToUpper().Trim()}";
			var ftpFolderPath = $"{ftpBaseUrl}/{newFolderName}";
			var subFolderName = $"{publicTransportGroupRif}-{driverId}-supervision";
			var ftpSubFolderPath = $"{ftpFolderPath}/{subFolderName}";

			if (await FolderExistsAsync(ftpSubFolderPath, ftpUsername, ftpPassword))
			{
				try
				{
					var fileList = await ListFilesAsync(ftpSubFolderPath, ftpUsername, ftpPassword);
					if (fileList.Count == 0) return false;

					var deleteTasks = fileList.Select(fileName => DeleteFileAsync($"{ftpSubFolderPath}/{fileName}", ftpUsername, ftpPassword));
					await Task.WhenAll(deleteTasks);

					try
					{
						await DeleteFolderAsync(ftpSubFolderPath, ftpUsername, ftpPassword);
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

		private async Task DeleteFolderAsync(string ftpFolderPath, string username, string password)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFolderPath);
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;
			request.Credentials = new NetworkCredential(username, password);
			using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
			{
				Console.WriteLine($"Carpeta eliminada: {response.StatusDescription}");
			}
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
					return Json(new { driverFullName = driver.DriverFullName, driverPhone = driver.DriverPhone, driverSexId = driver.SexId });
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

				if(paramValue4 > ptgPartners.Partners)
				{
					return Json($"La línea tiene cupo solo para {ptgPartners.Partners} transportista(s) no puede agregar un número de socio {paramValue4}.");
				}

				return Json("OK");
			}

			return Json("ERROR");
		}

		public JsonResult CheckExistingValuesOnEdit(int paramValue2, int paramValue4, int paramValue5)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
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

				var ptgPartners = _publicTransportGroup.GetPublicTransportGroupById(paramValue2);

				if (paramValue4 > ptgPartners.Partners)
				{
					return Json(new
					{
						canContinue = false,
						message = $"La línea tiene cupo solo para {ptgPartners.Partners} transportista(s), no puede agregar el número de socio {paramValue4}."
					});
				}

				return Json(new
				{
					canContinue = true,
					message = "OK"
				});
			}

			return Json(new
			{
				canContinue = false,
				message = "Sesión inválida"
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

		public class QRDriverRequest
		{
			public string ptgGUID { get; set; }
		}
	}
}

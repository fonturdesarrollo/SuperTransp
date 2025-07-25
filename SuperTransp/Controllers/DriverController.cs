using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using SuperTransp.Core;
using SuperTransp.Models;
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
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _publicTransportGroup.GetAllByStateId((int)stateId);
					}
					else
					{
						model = _publicTransportGroup.GetAll();
						ViewBag.IsTotalAccess = true;
					}

					ViewBag.IsTotalAccess = _security.IsTotalAccess(2);

					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
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

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					var ptg = _publicTransportGroup.GetPublicTransportGroupById(publicTransportGroupId);
					var model = new DriverViewModel
					{
						PublicTransportGroupId = publicTransportGroupId,
						PTGCompleteName = pTGCompleteName,
						DriverModifiedDate = DateTime.Now,
						Birthdate = DateTime.Now.AddYears(-20)
					};

					ViewBag.Drivers = _driver.GetByPublicTransportGroupId(publicTransportGroupId);
					ViewBag.Partners = ptg.Partners;
					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

					if (securityGroupId != 1)
					{
						ViewBag.IsTotalAccess = _security.IsTotalAccess(2);
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

			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpPost]
		public IActionResult Add(DriverViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (_security.IsTotalAccess(2) || securityGroupId == 1)
					{
						int driverId = _driver.AddOrEdit(model);

						if (driverId > 0)
						{
							TempData["SuccessMessage"] = "Datos actualizados correctamente";

							return RedirectToAction("Add", new { publicTransportGroupId = model.PublicTransportGroupId, pTGCompleteName = model.PTGCompleteName });
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

		[HttpGet]
		public IActionResult Edit(int driverPublicTransportGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
				{
					return RedirectToAction("Login", "Security");
				}

				var model = _driver.GetByDriverPublicTransportGroupId(driverPublicTransportGroupId);
				var ptg = _publicTransportGroup.GetPublicTransportGroupById(model.PublicTransportGroupId);
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.Partners = ptg.Partners;

				if (securityGroupId != 1)
				{
					ViewBag.IsTotalAccess = _security.IsTotalAccess(2);
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

		public IActionResult Edit(DriverViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (_security.IsTotalAccess(2) || securityGroupId == 1)
					{
						_driver.AddOrEdit(model);

						TempData["SuccessMessage"] = "Datos actualizados correctamente";

						return RedirectToAction("Add", new { publicTransportGroupId = model.PublicTransportGroupId, pTGCompleteName = model.PTGCompleteName });
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult Delete(int driverId, int publicTransportGroupId, string pTGCompleteName)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session?.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 2))
					{
						return RedirectToAction("Login", "Security");
					}

					var result = _driver.Delete(driverId);

					if (result)
					{
						TempData["SuccessMessage"] = "Registro eliminado correctamente";

						return RedirectToAction("Add", new { publicTransportGroupId = publicTransportGroupId, pTGCompleteName = pTGCompleteName });
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
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
							return Json($"El número de socio {paramValue4} ya está registrado a la línea.");
						}
					}
				}

				var ptgPartners = _publicTransportGroup.GetPublicTransportGroupById(paramValue2);

				if (paramValue4 > ptgPartners.Partners)
				{
					return Json($"La línea tiene cupo solo para {ptgPartners.Partners} transportista(s) no puede agregar un número de socio {paramValue4}.");
				}

				return Json("OK");
			}

			return Json("ERROR");
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

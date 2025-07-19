using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using SuperTransp.Core;
using SuperTransp.Models;
using System;
using System.Drawing;
using System.Net;
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

		public PublicTransportGroupController(IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography, IDesignation designation, IUnion union, IMode mode, IDriver driver, IConfiguration configuration)
		{
			_publicTransportGroup = publicTransportGroup;
			_security = security;
			_geography = geography;
			_designation = designation;
			_union = union;
			_mode = mode;
			_driver = driver;
			_configuration = configuration;
		}

		public IActionResult Index()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FullName")) && HttpContext.Session.GetInt32("SecurityGroupId") != null)
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 1))
				{
					return RedirectToAction("Login", "Security");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");
				ViewBag.SystemVersion = (string)HttpContext.Session.GetString("SystemVersion");

				return View();
			}

			return RedirectToAction("Login", "Security");
		}

		public IActionResult Add()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 1))
					{
						return RedirectToAction("Login", "Security");
					}

					var model = new PublicTransportGroupViewModel
					{
						PublicTransportGroupId = 0,
						PublicTransportGroupIdModifiedDate = DateTime.Now
					};

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");
					int supervisorsGroupId = 3;

					if (securityGroupId.HasValue)
					{
						if (securityGroupId != 1)
						{
							if (_security.GroupHasAccessToModule((int)securityGroupId, 6))
							{
								ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
								ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
								ViewBag.IsTotalAccess = _security.IsTotalAccess(1);
							}
							else
							{
								if (stateId.HasValue)
								{
									ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
									ViewBag.Union = new SelectList(_union.GetByStateId((int)stateId), "UnionId", "UnionName");
									ViewBag.IsTotalAccess = _security.IsTotalAccess(1);
								}
							}
						}
						else
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
							ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
							ViewBag.IsTotalAccess = true;
						}
					}

					ViewBag.Designation = new SelectList(_designation.GetAll(), "DesignationId", "DesignationName");
					ViewBag.Mode = new SelectList(_mode.GetAll(), "ModeId", "ModeName");

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
		public IActionResult Add(PublicTransportGroupViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 1))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (_security.IsTotalAccess(1) || securityGroupId == 1)
					{
						int publicTransportGroupId = _publicTransportGroup.AddOrEdit(model);

						if (publicTransportGroupId > 0)
						{
							TempData["SuccessMessage"] = "Datos actualizados correctamente";

							return RedirectToAction("Add");
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
		public IActionResult Edit(int publicTransportGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 1))
				{
					return RedirectToAction("Login", "Security");
				}

				var model = _publicTransportGroup.GetPublicTransportGroupById(publicTransportGroupId);
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");
				int supervisorsGroupId = 3;
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

				if (securityGroupId.HasValue)
				{
					if (securityGroupId != 1)
					{
						if (_security.GroupHasAccessToModule((int)securityGroupId, 6))
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
							ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
							ViewBag.IsTotalAccess = _security.IsTotalAccess(1);
						}
						else
						{
							if (stateId.HasValue)
							{
								ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
								ViewBag.Union = new SelectList(_union.GetByStateId((int)stateId), "UnionId", "UnionName");
								ViewBag.IsTotalAccess = _security.IsTotalAccess(1);
							}
						}
					}
					else
					{
						ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
						ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
						ViewBag.IsTotalAccess = true;
					}

					ViewBag.Municipality = new SelectList(_geography.GetMunicipalityByStateId(model.StateId), "MunicipalityId", "MunicipalityName");
					ViewBag.Designation = new SelectList(_designation.GetAll(), "DesignationId", "DesignationName");
					ViewBag.Mode = new SelectList(_mode.GetAll(), "ModeId", "ModeName");
				}

				return View(model);
			}

			return RedirectToAction("Login", "Security");
		}

		public IActionResult Edit(PublicTransportGroupViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 1))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (_security.IsTotalAccess(1) || securityGroupId == 1)
					{
						_publicTransportGroup.AddOrEdit(model);
					}

					TempData["SuccessMessage"] = "Datos actualizados correctamente";

					return RedirectToAction("Edit", new { publicTransportGroupId = model.PublicTransportGroupId});
				}

				return RedirectToAction("Login", "Security");
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
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 1))
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
					}

					ViewBag.IsTotalAccess = _security.IsTotalAccess(1);

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
		public JsonResult GetMunicipality(int stateId)
		{
			var municipality = _geography.GetMunicipalityByStateId(stateId).ToList();

			return Json(municipality);
		}

		public JsonResult CheckRifExist(string paramValue1)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var registeredRif = _publicTransportGroup.RegisteredRif(paramValue1);

				if (!string.IsNullOrEmpty(registeredRif))
				{

					return Json($"La línea con el RIF {paramValue1} ya está registrada.");					
				}

				return Json("OK");
			}

			return Json("ERROR");
		}

		public JsonResult CheckRifExistOnEdit(string paramValue1, int paramValue2, int paramValue3)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var registeredRif = _publicTransportGroup.RegisteredRif(paramValue1);

				if(!string.IsNullOrEmpty(registeredRif))
				{
					var currentRif = _publicTransportGroup.GetPublicTransportGroupById(paramValue2);

					if(currentRif.PublicTransportGroupRif != registeredRif)
					{
						return Json($"La línea con el RIF {paramValue1} ya está registrada.");
					}
				}

				if (paramValue2 > 0)
				{
					var totalDrivers = _driver.TotalDriversByPublicTransportGroupId(paramValue2);

					if (totalDrivers > paramValue3)
					{
						return Json($"Existen {totalDrivers} transportista(s) cargado(s) a esta línea, si desea reducir el cupo debe eliminar los necesarios.");
					}
				}

				return Json("OK");
			}

			return Json("ERROR");
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
	}

	public class QRRequest
	{
		public string ptgGUID { get; set; }
	}
}

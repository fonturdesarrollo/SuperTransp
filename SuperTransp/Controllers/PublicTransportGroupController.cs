using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using QRCoder;
using SuperTransp.Core;
using SuperTransp.Models;
using SuperTransp.Utils;
using System;
using System.Drawing;
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
		private readonly IOptionsSnapshot<MaintenanceSettings> _settings;

		public PublicTransportGroupController(IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography, IDesignation designation, 
			IUnion union, IMode mode, IDriver driver, IConfiguration configuration, IOptionsSnapshot<MaintenanceSettings> settings)
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

								if(_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
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
					ViewBag.IsTotalAccess = true;
				}

				List<DesignationViewModel> designations = _designation.GetAll();

				ViewBag.Designation = new SelectList(designations, "DesignationId", "DesignationName");
				ViewBag.DesignationList = Designations(designations);
				ViewBag.Mode = new SelectList(_mode.GetAll(), "ModeId", "ModeName");
				ViewBag.Municipality = new SelectList(_geography.GetMunicipalityByStateId(model.StateId), "MunicipalityId", "MunicipalityName");
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

		public JsonResult CheckRifExist(string paramValue1)
		{
			var result = CheckSessionAndPermission(1);
			if (result != null) Json("ERROR");

			var registeredRif = _publicTransportGroup.RegisteredRif(paramValue1);

			if (!string.IsNullOrEmpty(registeredRif))
			{

				return Json($"La línea con el RIF {paramValue1} ya está registrada.");					
			}

			return Json("OK");
		}

		public JsonResult CheckRifExistOnEdit(string paramValue1, int paramValue2, int paramValue3)
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

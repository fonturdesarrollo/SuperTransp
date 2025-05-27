using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Core;
using SuperTransp.Models;
using System.Security;
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

		public PublicTransportGroupController(IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography, IDesignation designation, IUnion union, IMode mode, IDriver driver)
		{
			_publicTransportGroup = publicTransportGroup;
			_security = security;
			_geography = geography;
			_designation = designation;
			_union = union;
			_mode = mode;
			_driver = driver;
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

		public IActionResult Add()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					var model = new PublicTransportGroupViewModel
					{
						PublicTransportGroupId = 0,
						PublicTransportGroupIdModifiedDate = DateTime.Now
					};

					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
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
							}
							else
							{
								if (stateId.HasValue)
								{
									ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
									ViewBag.Union = new SelectList(_union.GetByStateId((int)stateId), "UnionId", "UnionName");
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

		[HttpPost]
		public IActionResult Add(PublicTransportGroupViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
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
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var model = _publicTransportGroup.GetPublicTransportGroupById(publicTransportGroupId);
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");
				int supervisorsGroupId = 3;
				ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");

				if (securityGroupId.HasValue)
				{
					if (securityGroupId != 1)
					{
						if (_security.GroupHasAccessToModule((int)securityGroupId, 6))
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
							ViewBag.Union = new SelectList(_union.GetAll(), "UnionId", "UnionName");
						}
						else
						{
							if (stateId.HasValue)
							{
								ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
								ViewBag.Union = new SelectList(_union.GetByStateId((int)stateId), "UnionId", "UnionName");
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
					ViewBag.IsTotalAccess = _security.IsTotalAccess(1);
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
					_publicTransportGroup.AddOrEdit(model);

					return RedirectToAction("PublicTransportGroupList");
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
					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _publicTransportGroup.GetByStateId((int)stateId);
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
	}
}

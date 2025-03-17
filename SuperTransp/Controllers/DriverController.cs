using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

		public DriverController(IDriver driver, IPublicTransportGroup publicTransportGroup, ISecurity security, IGeography geography)
		{
			_driver = driver;
			_publicTransportGroup = publicTransportGroup;
			_security = security;
			_geography = geography;
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
					List<PublicTransportGroupModel> model = new();

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
					var model = new DriverViewModel
					{
						PublicTransportGroupId = publicTransportGroupId,
						PTGCompleteName = pTGCompleteName,
						DriverModifiedDate = DateTime.Now
					};

					ViewBag.Drivers = _driver.GetByPublicTransportGroupId(publicTransportGroupId);
					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");

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
					int driverId = _driver.AddOrEdit(model);

					if (driverId > 0)
					{
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

		[HttpGet]
		public IActionResult Edit(int driverPublicTransportGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var model = _driver.GetByDriverPublicTransportGroupId(driverPublicTransportGroupId);

				ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");

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
					_driver.AddOrEdit(model);

					TempData["SuccessMessage"] = "Datos actualizados correctamente";

					return RedirectToAction("Add", new { publicTransportGroupId = model.PublicTransportGroupId, pTGCompleteName = model.PTGCompleteName });
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
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
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

		public JsonResult CheckExistingValues(int paramValue1, int paramValue2, string paramValue3, int paramValue4, bool isUpdating)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (_driver.RegisteredDocumentId(paramValue1, paramValue2))
				{
					return Json($"La cédula {paramValue1} ya está registrada a la línea.");
				}

				if (_driver.RegisteredPhone(paramValue3, paramValue2))
				{
					return Json($"El número de teléfono {paramValue3} ya está registrado a la línea.");
				}

				if (_driver.RegisteredPartnerNumber(paramValue4, paramValue2))
				{
					return Json($"El número de socio {paramValue4} ya está registrado a la línea.");
				}

				return Json("OK");
			}

			return Json("ERROR");
		}

		public JsonResult CheckExistingValuesOnEdit(int paramValue1, int paramValue2, string paramValue3, int paramValue4, int paramValue5)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (_driver.RegisteredDocumentId(paramValue1, paramValue2))
				{
					var allData = _driver.GetByPublicTransportGroupId(paramValue2);
					var driverData = allData.Where(x => x.DriverIdentityDocument == paramValue1 &&  x.DriverId == paramValue5).ToList();

					if (!driverData.Any()) 
					{
						var finalData = allData.Where(x => x.DriverIdentityDocument == paramValue1 && x.DriverId != paramValue5);
						if (finalData.Any())
						{
							return Json($"La cédula {paramValue1} ya está registrada a la línea.");
						}
					}
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
							return Json($"El número de socio {paramValue4} ya está registrado a la línea.");
						}
					}					
				}

				return Json("OK");
			}

			return Json("ERROR");
		}
	}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Core;
using SuperTransp.Models;
using System;
using System.Reflection;
using System.Xml.Linq;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class CommonDataController : Controller
	{
		private IDesignation _designation;
		private IUnion _union;
		private ISecurity _security;
		private IGeography _geography;

		public CommonDataController(IDesignation designation, IUnion union, ISecurity security, IGeography geography)
		{
			_designation = designation;
			_union = union;
			_security = security;
			_geography = geography;
		}

		public IActionResult AddDesignation()
		{
			ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");

			return View();
		}

		[HttpPost]
		public IActionResult AddDesignation(DesignationViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int designationId = _designation.AddOrEdit(model);

					if (designationId > 0)
					{
						TempData["SuccessMessage"] = "Datos actualizados correctamente";
						return RedirectToAction("AddDesignation");
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult AddUnion()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
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
				}
			}

			return View();
		}

		[HttpPost]
		public IActionResult AddUnion(UnionViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int designationId = _union.AddOrEdit(model);

					if (designationId > 0)
					{
						TempData["SuccessMessage"] = "Datos actualizados correctamente";

						return RedirectToAction("AddUnion");
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}
	}
}

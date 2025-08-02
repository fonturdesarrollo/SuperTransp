﻿using Microsoft.AspNetCore.Mvc;
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
		private ICommonData _commonData;

		public CommonDataController(IDesignation designation, IUnion union, ISecurity security, IGeography geography, ICommonData commonData)
		{
			_designation = designation;
			_union = union;
			_security = security;
			_geography = geography;
			_commonData = commonData;
		}

		public IActionResult AddDesignation()
		{
			if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 15))
			{
				return RedirectToAction("Login", "Security");
			}

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

			return View();
		}

		[HttpPost]
		public IActionResult AddDesignation(DesignationViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 15))
					{
						return RedirectToAction("Login", "Security");
					}

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
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 16))
				{
					return RedirectToAction("Login", "Security");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
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
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 16))
					{
						return RedirectToAction("Login", "Security");
					}

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

		public IActionResult AddMakeModel()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 17))
				{
					return RedirectToAction("Login", "Security");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

				ViewBag.Years = new SelectList(_commonData.GetYears(), "YearId", "YearName");
			}

			return View();
		}

		[HttpPost]
		public IActionResult AddMakeModel(CommonDataViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 17))
					{
						return RedirectToAction("Login", "Security");
					}

					int vehicleDataId = _commonData.AddOrEditMakeModel(model);

					if (vehicleDataId > 0)
					{
						TempData["SuccessMessage"] = "Datos actualizados correctamente";

						return RedirectToAction("AddMakeModel");
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public JsonResult CheckExistingValues(string paramValue1)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var designations = _designation.GetAll();

				if(designations != null && designations.Any())
				{
					var checkDesignation = designations.Where(d => d.DesignationName == paramValue1);

					if (checkDesignation.Any())
					{
						return Json($"La entidad legal {paramValue1} ya existe no puede agregar otra igual.");
					}
				}

				return Json("OK");
			}

			return Json("ERROR");
		}
	}
}

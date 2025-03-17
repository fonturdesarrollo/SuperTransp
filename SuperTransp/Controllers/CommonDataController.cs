using Microsoft.AspNetCore.Mvc;
using SuperTransp.Core;
using SuperTransp.Models;
using System;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class CommonDataController : Controller
	{
		private IDesignation _designation;
		private IUnion _union;

		public CommonDataController(IDesignation designation, IUnion union)
		{
			_designation = designation;
			_union = union;
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
			ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");

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

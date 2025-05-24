using Microsoft.AspNetCore.Mvc;
using SuperTransp.Core;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class SettingsController : Controller
	{
		private readonly ISecurity _security;
		public SettingsController(ISecurity security)
		{
			this._security = security;
		}
		public IActionResult Index()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

				if (ViewBag.SecurityGroupId != 1)
				{
					ViewBag.ModulesInGroup = _security.GetModulesByGroupId(ViewBag.SecurityGroupId);
					ViewBag.IsTotalAccessDesignation = _security.IsTotalAccess(15);
					ViewBag.IsTotalAccessUnion = _security.IsTotalAccess(16);
					ViewBag.IsTotalAccessMakeModel = _security.IsTotalAccess(17);
				}
				else
				{
					ViewBag.ModulesInGroup = _security.GetAllModules();
					ViewBag.IsTotalAccessDesignation = true;
					ViewBag.IsTotalAccessUnion = true;
					ViewBag.IsTotalAccessMakeModel = true;
				}

				return View();
			}

			return RedirectToAction("Login", "Security");
		}
	}
}

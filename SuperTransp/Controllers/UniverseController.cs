using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Core;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class UniverseController : Controller
	{
		private readonly IUniverse _universe;
		private readonly ISecurity _security;
		private readonly IGeography _geography;
		public UniverseController(IUniverse universe, ISecurity security, IGeography geography)
		{
			_universe = universe;
			_security = security;
			_geography = geography;
		}

		public IActionResult Add()
		{
			var model = new UniverseViewModel
			{
				UniverseId = 0,
			};

			ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");

			return View(model);
		}

		[HttpPost]
		public IActionResult Add(UniverseViewModel model)
		{
			var result = CheckSessionAndPermission(26);
			if (result != null) return result;

			int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
			int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");

			if (securityGroupId != null && _security.GroupHasAccessToModule((int)securityGroupId, 26) || securityGroupId == 1)
			{
				if (ModelState.IsValid)
				{
					try
					{
						int universeId = _universe.AddOrEdit(model);
						if (universeId > 0)
						{
							TempData["SuccessMessage"] = "Registros actualizados";
							return RedirectToAction("Add");
						}
					}
					catch (Exception ex)
					{
						return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
					}
				}
			}

			return RedirectToAction("Login", "Security");
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
}

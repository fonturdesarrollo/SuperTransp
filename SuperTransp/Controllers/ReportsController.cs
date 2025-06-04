using Microsoft.AspNetCore.Mvc;
using SuperTransp.Core;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class ReportsController : Controller
	{
		ISupervision _supervision;
		private ISecurity _security;
		private IPublicTransportGroup _publicTransportGroup;

		public ReportsController(ISupervision supervision, IPublicTransportGroup publicTransportGroup, ISecurity security)
		{
			this._security = security;
			this._supervision = supervision;
			this._publicTransportGroup = publicTransportGroup;
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
					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
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
		public IActionResult PublicTransportGroupDriverList(int publicTransportGroupId)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = HttpContext.Session?.GetString("FullName");
					int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session?.GetInt32("StateId");

					model = _supervision.GetDriverPublicTransportGroupByPtgId(publicTransportGroupId);				

					return View(model);
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

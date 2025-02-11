using Microsoft.AspNetCore.Mvc;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class SettingsController : Controller
	{
		public IActionResult Index()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{

				return View();
			}

			return RedirectToAction("Login", "Security");
		}
	}
}

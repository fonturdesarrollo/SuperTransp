using Microsoft.AspNetCore.Mvc;
using SuperTransp.Models;
using System.Diagnostics;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ISecurity _security;

		public HomeController(ILogger<HomeController> logger, ISecurity security)
		{
			_logger = logger;
			_security = security;
		}

		public IActionResult Index()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")}";
					ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

					if (ViewBag.SecurityGroupId != 1)
					{
						ViewBag.ModulesInGroup = _security.GetModulesByGroupId(ViewBag.SecurityGroupId);
					}
					else
					{
						ViewBag.ModulesInGroup = _security.GetAllModules();
					}

					return View();
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			// Puedes pasar informaci�n adicional, como el ID del error
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		//[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		//public IActionResult Error(string errorMessage)
		//{
		//	ViewBag.ErrorMessage = errorMessage;
		//	return View();
		//}
	}
}

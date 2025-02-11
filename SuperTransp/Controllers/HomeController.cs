using Microsoft.AspNetCore.Mvc;
using SuperTransp.Models;
using System.Diagnostics;

namespace SuperTransp.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					ViewBag.EmployeeName = (string)HttpContext.Session.GetString("FullName");
					ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

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
		public IActionResult Error(string errorMessage)
		{
			ViewBag.ErrorMessage = errorMessage;
			return View();
		}
	}
}

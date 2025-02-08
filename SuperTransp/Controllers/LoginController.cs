using SuperTransp.Models;
using Microsoft.AspNetCore.Mvc;
using static SuperTransp.Core.Interfaces;


namespace SuperTransp.Controllers
{
    public class LoginController : Controller
    {
        private readonly ISecurity _security;
        public LoginController(ISecurity security)
        {
            this._security = security;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(SecurityUserModel model)
        {
			try
			{
				var validUser = _security.GetValidUser(model.Login, model.Password);

				if (validUser != null && validUser.SecurityUserId != 0)
				{
					HttpContext.Session.SetInt32("SecurityUserId", validUser.SecurityUserId);
					HttpContext.Session.SetString("FullName", validUser.FullName);
					HttpContext.Session.SetInt32("SecurityGroupId", validUser.SecurityGroupId);

					return RedirectToAction("Index", "Home");
				}

				ViewBag.InvalidUser = "true";

				return View("Login");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}
    }
}

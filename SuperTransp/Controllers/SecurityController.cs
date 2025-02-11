using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class SecurityController : Controller
	{
		private readonly ISecurity _security;
		private readonly IGeography _geography;
		public SecurityController(ISecurity security, IGeography geography)
		{
			this._security = security;
			this._geography = geography;
		}
		public IActionResult Login()
		{
			HttpContext.Session.Remove("SecurityUserId");
			HttpContext.Session.Remove("FullName");
			HttpContext.Session.Remove("SecurityGroupId");
			HttpContext.Session.Remove("StateId");

			return View();
		}

		[HttpPost]
		public IActionResult Login(SecurityUserModel model)
		{
			try
			{
				if(!string.IsNullOrEmpty(model.Login) && !string.IsNullOrEmpty(model.Password))
				{
					var validUser = _security.GetValidUser(model.Login, model.Password);

					if (validUser != null && validUser.SecurityUserId != 0)
					{
						HttpContext.Session.SetInt32("SecurityUserId", validUser.SecurityUserId);
						HttpContext.Session.SetString("FullName", validUser.FullName);
						HttpContext.Session.SetInt32("SecurityGroupId", validUser.SecurityGroupId);
						HttpContext.Session.SetInt32("StateId", validUser.StateId);

						return RedirectToAction("Index", "Home");
					}

					ViewBag.InvalidUser = "true";

					return View();
				}

				ViewBag.InvalidUser = "true";

				return View();

			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult AddUser()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					var model = new SecurityUserModel
					{
						SecurityUserDocumentIdNumber = null
					};

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");
					int supervisorsGroupId = 3;

					if (securityGroupId.HasValue)
					{
						if (_security.GroupModuleHasAccess((int)securityGroupId, 5))
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
						}
						else
						{
							if(stateId.HasValue)
							{
								ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
							}							
						}

						if (_security.GroupModuleHasAccess((int)securityGroupId, 6))
						{
							ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
						}
						else
						{
							//Coordinadores group
							if(securityGroupId == 2)
							{
								ViewBag.Groups = new SelectList(_security.GetGroupById((int)supervisorsGroupId), "SecurityGroupId", "SecurityGroupName");
							}
						}

						ViewBag.Status = new SelectList(_security.GetAllUsersStatus(), "SecurityStatusId", "SecurityStatusName");
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

		[HttpPost]
		public IActionResult AddUser(SecurityUserModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int securityUserId = _security.AddOrEdit(model);

					if (securityUserId > 0)
					{
						return RedirectToAction("EditUser", new { securityUserId = securityUserId });
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult EditUser(int securityUserId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				var model = _security.GetUserById(securityUserId);

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");
				int supervisorsGroupId = 3;

				if (securityGroupId.HasValue)
				{
					if (_security.GroupModuleHasAccess((int)securityGroupId, 5))
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

					if (_security.GroupModuleHasAccess((int)securityGroupId, 6))
					{
						ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
					}
					else
					{
						//Coordinadores group
						if (securityGroupId == 2)
						{
							ViewBag.Groups = new SelectList(_security.GetGroupById((int)supervisorsGroupId), "SecurityGroupId", "SecurityGroupName");
						}
					}

					ViewBag.Status = new SelectList(_security.GetAllUsersStatus(), "SecurityStatusId", "SecurityStatusName");
				}

				return View("EditUser" , model);
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpPost]
		public IActionResult EditUser(SecurityUserModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					var currentPassword = _security.GetUserById(model.SecurityUserId);
					model.Password = currentPassword.Password;
					_security.AddOrEdit(model);

					return RedirectToAction("EditUser", new { securityUserId = model.SecurityUserId });

				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult UserList()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					List<SecurityUserModel> model = _security.GetAllUsers();

					//Coordinadores group
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (securityGroupId == 2)
					{
						model = model.Where(x=> x.SecurityGroupId != 1 && x.SecurityGroupId != 2 && x.SecurityGroupId != 4).ToList();
					}
					/**************************************************************/

					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public JsonResult CheckUserExist(string paramValue1, string paramValue2, string paramValue3)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				string? securityUpdatingUserId = paramValue3;
				var registeredUserIdNumber = _security.RegisteredUser(paramValue1, "SecurityUserDocumentIdNumber");
				var registeredUserLogin = _security.RegisteredUser(paramValue2, "Login");

				if(!string.IsNullOrEmpty(securityUpdatingUserId))
				{
					if (int.Parse(securityUpdatingUserId) != registeredUserIdNumber)
					{
						return Json("El usuario " + paramValue1 + " ya está registrado.");
					}
				}
				else
				{
					if(registeredUserIdNumber != 0)
					{
						return Json("El usuario " + paramValue1 + " ya está registrado.");
					}
				}				

				if(!string.IsNullOrEmpty(securityUpdatingUserId))
				{
					if (int.Parse(securityUpdatingUserId) != registeredUserLogin)
					{
						return Json("El usuario " + paramValue2 + " ya está registrado.");
					}
				}
				else
				{
					if (registeredUserLogin != 0)
					{
						return Json("El usuario " + paramValue2 + " ya está registrado.");
					}
				}				

				return Json("OK");
			}

			return Json("ERROR");
		}
	}
}

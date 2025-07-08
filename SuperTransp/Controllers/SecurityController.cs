using Azure.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Models;
using System.Diagnostics.SymbolStore;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class SecurityController : BaseController
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
			HttpContext.Session.Remove("UserLogin");
			HttpContext.Session.Remove("SecurityGroupId");
			HttpContext.Session.Remove("StateId");
			HttpContext.Session.Remove("StateName");
			HttpContext.Session.Remove("DeviceIP");
			HttpContext.Session.Remove("LoginAttempts");
			HttpContext.Session.Remove("SystemVersion");
			HttpContext.Session.Remove("SecurityGroupName");
			HttpContext.Session.Remove("SecurityGroupDescription");

			return View();
		}

		[HttpPost]
		public IActionResult Login(SecurityUserViewModel model)
		{
			try
			{
				if(!string.IsNullOrEmpty(model.Login) && !string.IsNullOrEmpty(model.Password))
				{
					model.Password = _security.Encrypt(model.Password);

					if (_security.IsInactiveLogin(model.Login))
					{
						ViewBag.InvalidUser = "inactive";

						return View();
					}

					if (_security.IsBlockedLogin(model.Login))
					{
						ViewBag.InvalidUser = "blocked";

						return View();
					}

					var validUser = _security.GetValidUser(model.Login, model.Password);

					if (validUser != null && validUser.SecurityUserId != 0)
					{
						HttpContext.Session.SetInt32("SecurityUserId", validUser.SecurityUserId);
						HttpContext.Session.SetString("FullName", $"{validUser.FullName}");
						HttpContext.Session.SetString("UserLogin", validUser.Login);
						HttpContext.Session.SetInt32("SecurityGroupId", validUser.SecurityGroupId);
						HttpContext.Session.SetInt32("StateId", validUser.StateId);
						HttpContext.Session.SetString("StateName", validUser.StateName);
						HttpContext.Session.SetString("DeviceIP", HttpContext.Connection.RemoteIpAddress?.ToString());
						HttpContext.Session.SetString("SystemVersion", "1.0 r1");
						HttpContext.Session.SetString("SecurityGroupName", validUser.SecurityGroupName);
						HttpContext.Session.SetString("SecurityGroupDescription", validUser.SecurityGroupDescription);

						return RedirectToAction("Index", "Home");
					}

					if (HttpContext.Session.GetInt32("LoginAttempts") == null)
					{
						HttpContext.Session.SetInt32("LoginAttempts", 1);
						
						ViewBag.InvalidUser = "true";
					}
					else
					{
						var attemptCounter =  HttpContext.Session.GetInt32("LoginAttempts");
						
						if(attemptCounter <= 3)
						{
							HttpContext.Session.Remove("LoginAttempts");

							attemptCounter++;

							HttpContext.Session.SetInt32("LoginAttempts", (int)attemptCounter);

							ViewBag.InvalidUser = "true";

							return View();
						}
						else
						{
							_security.BlockLogin(model.Login);
							
							ViewBag.InvalidUser = "blocked";
						}
					}

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
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 8))
					{
						return RedirectToAction("Login", "Security");
					}

					var model = new SecurityUserViewModel
					{
						SecurityUserDocumentIdNumber = null
					};

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId.HasValue)
					{
						if(securityGroupId != 1)
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

							ViewBag.Groups = new SelectList(_security.GetGroupById((int)securityGroupId), "SecurityGroupId", "SecurityGroupName");
						}
						else
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
							ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
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
		public IActionResult AddUser(SecurityUserViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 8))
					{
						return RedirectToAction("Login", "Security");
					}

					model.Password = _security.Encrypt(model.Password);

					int securityUserId = _security.AddOrEditUser(model);

					if (securityUserId > 0)
					{
						TempData["SuccessMessage"] = "Datos actualizados correctamente";

						return RedirectToAction("AddUser");
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
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 9))
				{
					return RedirectToAction("Login", "Security");
				}

				var model = _security.GetUserById(securityUserId);
				int ? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");
				int supervisorsGroupId = 3;

				if (securityGroupId.HasValue)
				{
					if(securityGroupId != 1)
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

						ViewBag.Groups = new SelectList(_security.GetGroupById((int)securityGroupId), "SecurityGroupId", "SecurityGroupName");						
					}
					else
					{
						ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
						ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
					}


					ViewBag.Status = new SelectList(_security.GetAllUsersStatus(), "SecurityStatusId", "SecurityStatusName");
				}

				return View(model);
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpPost]
		public IActionResult EditUser(SecurityUserViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 9))
					{
						return RedirectToAction("Login", "Security");
					}

					var currentPassword = _security.GetUserById(model.SecurityUserId);
					model.Password = currentPassword.Password;

					_security.AddOrEditUser(model);

					TempData["SuccessMessage"] = "Datos actualizados correctamente";

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
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 9))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					List<SecurityUserViewModel> model = _security.GetAllUsersByGroupId((int)securityGroupId);

					if (!_security.GroupHasAccessToModule((int)securityGroupId, 6) && securityGroupId != 1)
					{
						var modelByState = model.Where(x => x.StateId == stateId).ToList();

						return View(modelByState);
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

		public JsonResult CheckUserExist(string paramValue1, string paramValue2, string paramValue3)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				string? securityUpdatingUserId = paramValue3;
				var registeredUserIdNumber = 0;
				var registeredUserLogin = _security.RegisteredUser(paramValue2, "Login");			

				if(!string.IsNullOrEmpty(securityUpdatingUserId))
				{
					if (registeredUserLogin != 0 && int.Parse(securityUpdatingUserId) != registeredUserLogin)
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

		public IActionResult AddGroup()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
					{
						return RedirectToAction("Login", "Security");
					}

					var model = new SecurityGroupModel
					{

					};

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
		public IActionResult AddGroup(SecurityGroupModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
					{
						return RedirectToAction("Login", "Security");
					}

					int securityGroupId = _security.AddOrEditGroup(model);

					if (securityGroupId > 0)
					{
						return RedirectToAction("EditGroup", new { securityGroupId = securityGroupId });
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult EditGroup(int securityGroupId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
				{
					return RedirectToAction("Login", "Security");
				}

				var model = _security.GetGroupById(securityGroupId);

				return View(model);
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpPost]
		public IActionResult EditGroup(SecurityGroupModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
					{
						return RedirectToAction("Login", "Security");
					}

					_security.AddOrEditGroup(model);

					return RedirectToAction("EditGroup", new { securityGroupId = model.SecurityGroupId });

				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult GroupList()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
					{
						return RedirectToAction("Login", "Security");
					}

					List<SecurityGroupModel> model = _security.GetAllGroups();

					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult AddModule()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
					{
						return RedirectToAction("Login", "Security");
					}

					var model = new SecurityModuleModel();
					{

					};

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
		public IActionResult AddModule(SecurityModuleModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
					{
						return RedirectToAction("Login", "Security");
					}

					int securityModuleId = _security.AddOrEditModule(model);

					if (securityModuleId > 0)
					{
						return RedirectToAction("EditModule", new { securityModuleId = securityModuleId });
					}
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult EditModule(int securityModuleId)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
				{
					return RedirectToAction("Login", "Security");
				}

				var model = _security.GetModuleById(securityModuleId);

				return View(model);
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpPost]
		public IActionResult EditModule(SecurityModuleModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					_security.AddOrEditModule(model);

					return RedirectToAction("EditModule", new { securityModuleId = model.SecurityModuleId });

				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult ModuleList()
		{
			try
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
				{
					return RedirectToAction("Login", "Security");
				}

				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					List<SecurityModuleModel> model = _security.GetAllModules();

					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult AddModulesToGroups(int securityGroupModuleId)
		{
			try
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
				{
					return RedirectToAction("Login", "Security");
				}

				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					SecurityGroupModuleModel model = new()
					{

					};

					ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
					ViewBag.Modules = new SelectList(_security.GetAllModules(), "SecurityModuleId", "SecurityModuleName");
					ViewBag.AccessTypes = new SelectList(_security.GetAllAccessTypes(), "SecurityAccessTypeId", "SecurityAccessTypeName");

					ViewBag.SecurityGroupModuleDetail = _security.GetAllSecurityGroupModuleDetail();

					return View(model);
				}
			}
			catch (Exception)
			{

				throw;
			}

			return View();
		}

		[HttpPost]
		public IActionResult AddModulesToGroups(SecurityGroupModuleModel model)
		{
			try
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
				{
					return RedirectToAction("Login", "Security");
				}

				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int securityGroupModuleId = _security.AddOrEditGroupModules(model);

					return RedirectToAction("AddModulesToGroups", new { securityGroupModuleId = securityGroupModuleId });

				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public IActionResult DeleteModuleToGroup(int securityGroupModuleId)
		{
			try
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
				{
					return RedirectToAction("Login", "Security");
				}

				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					int securityGroupModule = _security.DeleteGroupModules(securityGroupModuleId);

					return RedirectToAction("AddModulesToGroups", new { securityGroupModuleId = securityGroupModuleId });

				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult ChangePassword()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					var securityUserId = HttpContext.Session.GetInt32("SecurityUserId");

					var model = _security.GetUserById((int)securityUserId);

					ViewBag.ResultMessage = string.Empty;

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
		public IActionResult ChangePassword(SecurityUserViewModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) && ModelState.IsValid)
				{
					model.NewPassword = _security.Encrypt(model.NewPassword);

					int securityGroupModuleId = _security.ChangePassword(model);

					TempData["SuccessMessage"] = "Clave cambiada correctamente";
					
					return View(model);
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		public JsonResult CheckOldPasswordExist(string paramValue1)
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				int securityUserId = (int)HttpContext.Session.GetInt32("SecurityUserId");

				paramValue1 = _security.Encrypt(paramValue1);

				var oldPasswordValid = _security.OldPasswordValid(securityUserId, paramValue1);

				if(!oldPasswordValid)
				{
					return Json("Clave anterior incorrecta");
				}

				return Json("OK");
			}

			return Json("ERROR");
		}

		public IActionResult Logbook()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				string? stateName = HttpContext.Session.GetString("StateName");
				int? stateId = HttpContext.Session.GetInt32("StateId");

				List<SecurityLogbookModel> model = new List<SecurityLogbookModel>();

				if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 18))
				{
					return RedirectToAction("Login", "Security");
				}

				if (securityGroupId.HasValue)
				{
					if (securityGroupId != 1)
					{
						if (!_security.GroupHasAccessToModule((int)securityGroupId, 6))
						{
							model = _security.GetLogbookByStateName(stateName);
						}
						else
						{
							model = _security.GetLogbookAllExceptAdmin();
						}						
					}
					else
					{
						model = _security.GetLogbookAll();
					}
				}
					
				return View(model);
			}

			return RedirectToAction("Login", "Security");
		}
	}
}

using Azure.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Models;
using System.Net.NetworkInformation;
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

					var validUser = _security.GetValidUser(model.Login, model.Password);

					if (validUser != null && validUser.SecurityUserId != 0)
					{
						HttpContext.Session.SetInt32("SecurityUserId", validUser.SecurityUserId);
						HttpContext.Session.SetString("FullName", validUser.FullName);
						HttpContext.Session.SetString("UserLogin", validUser.Login);
						HttpContext.Session.SetInt32("SecurityGroupId", validUser.SecurityGroupId);
						HttpContext.Session.SetInt32("StateId", validUser.StateId);
						HttpContext.Session.SetString("StateName", validUser.StateName);
						HttpContext.Session.SetString("DeviceIP", HttpContext.Connection.RemoteIpAddress?.ToString());

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
					var model = new SecurityUserViewModel
					{
						SecurityUserDocumentIdNumber = null
					};

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
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

							if (_security.GroupHasAccessToModule((int)securityGroupId, 7))
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
								else
								{
									ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
								}
							}
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

						if (_security.GroupHasAccessToModule((int)securityGroupId, 7))
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
							else
							{
								ViewBag.Groups = new SelectList(_security.GetGroupById((int)securityGroupId), "SecurityGroupId", "SecurityGroupName");
							}
						}
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
					List<SecurityUserViewModel> model = _security.GetAllUsers();

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1)
					{ 						
						if(securityGroupId == 2) //Coordinadores
						{
							model = model.Where(x => x.SecurityGroupId != 1 && x.SecurityGroupId != 4 && x.StateId == stateId).ToList();
						}
						else if(securityGroupId == 4) // Gerentes
						{
							model = model.Where(x => x.SecurityGroupId != 1 && x.StateId == stateId).ToList();
						}
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
	}
}

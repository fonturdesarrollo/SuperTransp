using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
    public class SecurityController : BaseController
    {
        private readonly ISecurity _security;
        private readonly IGeography _geography;

        public SecurityController(ISecurity security, IGeography geography)
        {
            _security = security;
            _geography = geography;
        }

        // Métodos auxiliares para validaciones de sesión y acceso
        private bool IsUserLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId"));
        private int? GetSessionInt(string key) => HttpContext.Session.GetInt32(key);
        private bool HasAccessToModule(int moduleId) =>
            GetSessionInt("SecurityGroupId") == 1 ||
            _security.GroupHasAccessToModule(GetSessionInt("SecurityGroupId") ?? 0, moduleId);

        public IActionResult Login()
        {
            foreach (var key in new[]
            {
                "SecurityUserId", "FullName", "UserLogin", "SecurityGroupId", "StateId", "StateName",
                "DeviceIP", "LoginAttempts", "SystemVersion", "SecurityGroupName", "SecurityGroupDescription"
            })
                HttpContext.Session.Remove(key);

            return View();
        }

        [HttpPost]
        public IActionResult Login(SecurityUserViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password))
                {
                    ViewBag.InvalidUser = "true";
                    return View();
                }

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
                    SetUserSession(validUser);
                    return RedirectToAction("Index", "Home");
                }

                HandleLoginAttempts(model.Login);
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        private void SetUserSession(SecurityUserViewModel user)
        {
            HttpContext.Session.SetInt32("SecurityUserId", user.SecurityUserId);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("UserLogin", user.Login ?? "");
            HttpContext.Session.SetInt32("SecurityGroupId", user.SecurityGroupId);
            HttpContext.Session.SetInt32("StateId", user.StateId);
            HttpContext.Session.SetString("StateName", user.StateName ?? "");
            HttpContext.Session.SetString("DeviceIP", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");
            HttpContext.Session.SetString("SystemVersion", "1.0 r1");
            HttpContext.Session.SetString("SecurityGroupName", user.SecurityGroupName ?? "");
            HttpContext.Session.SetString("SecurityGroupDescription", user.SecurityGroupDescription ?? "");
        }

        private void HandleLoginAttempts(string login)
        {
            var attempts = HttpContext.Session.GetInt32("LoginAttempts") ?? 0;
            attempts++;
            HttpContext.Session.SetInt32("LoginAttempts", attempts);

            if (attempts > 3)
            {
                _security.BlockLogin(login);
                ViewBag.InvalidUser = "blocked";
            }
            else
            {
                ViewBag.InvalidUser = "true";
            }
        }

        public IActionResult AddUser()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                if (!HasAccessToModule(8))
                    return RedirectToAction("Login", "Security");

                var model = new SecurityUserViewModel();
                int? groupId = GetSessionInt("SecurityGroupId");
                int? stateId = GetSessionInt("StateId");

                if (groupId.HasValue)
                {
                    if (groupId != 1)
                    {
                        if (_security.GroupHasAccessToModule(groupId.Value, 6))
                            ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
                        else if (stateId.HasValue)
                            ViewBag.States = new SelectList(_geography.GetStateById(stateId.Value), "StateId", "StateName");

                        ViewBag.Groups = new SelectList(_security.GetGroupById(groupId.Value), "SecurityGroupId", "SecurityGroupName");
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
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddUser(SecurityUserViewModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid || !HasAccessToModule(8))
                    return RedirectToAction("Login", "Security");

                model.Password = _security.Encrypt(model.Password);
                int securityUserId = _security.AddOrEditUser(model);

                if (securityUserId > 0)
                    TempData["SuccessMessage"] = "Datos actualizados correctamente";

                return RedirectToAction("AddUser");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult EditUser(int securityUserId)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Security");

            if (!HasAccessToModule(9))
                return RedirectToAction("Login", "Security");

            var model = _security.GetUserById(securityUserId);
            int? groupId = GetSessionInt("SecurityGroupId");
            int? stateId = GetSessionInt("StateId");

            if (groupId.HasValue)
            {
                if (groupId != 1)
                {
                    if (_security.GroupHasAccessToModule(groupId.Value, 6))
                        ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
                    else if (stateId.HasValue)
                        ViewBag.States = new SelectList(_geography.GetStateById(stateId.Value), "StateId", "StateName");

                    ViewBag.Groups = new SelectList(_security.GetGroupById(groupId.Value), "SecurityGroupId", "SecurityGroupName");
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

        [HttpPost]
        public IActionResult EditUser(SecurityUserViewModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid || !HasAccessToModule(9))
                    return RedirectToAction("Login", "Security");

                var currentPassword = _security.GetUserById(model.SecurityUserId);
                model.Password = currentPassword.Password;

                _security.AddOrEditUser(model);

                TempData["SuccessMessage"] = "Datos actualizados correctamente";

                return RedirectToAction("EditUser", new { securityUserId = model.SecurityUserId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public IActionResult UserList()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                if (!HasAccessToModule(9))
                    return RedirectToAction("Login", "Security");

                int? groupId = GetSessionInt("SecurityGroupId");
                int? stateId = GetSessionInt("StateId");

                List<SecurityUserViewModel> model = _security.GetAllUsersByGroupId((int)groupId);

                if (!_security.GroupHasAccessToModule((int)groupId, 6) && groupId != 1)
                {
                    var modelByState = model.Where(x => x.StateId == stateId).ToList();

                    return View(modelByState);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public JsonResult CheckUserExist(string paramValue1, string paramValue2, string paramValue3)
        {
            if (!IsUserLoggedIn())
                return Json("ERROR");

            string? securityUpdatingUserId = paramValue3;
            var registeredUserIdNumber = 0;
            var registeredUserLogin = _security.RegisteredUser(paramValue2, "Login");

            if (!string.IsNullOrEmpty(securityUpdatingUserId))
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

        public IActionResult AddGroup()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
                {
                    return RedirectToAction("Login", "Security");
                }

                var model = new SecurityGroupModel
                {

                };

                return View(model);
            }

            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddGroup(SecurityGroupModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
                {
                    return RedirectToAction("Login", "Security");
                }

                int securityGroupId = _security.AddOrEditGroup(model);

                if (securityGroupId > 0)
                {
                    return RedirectToAction("EditGroup", new { securityGroupId = securityGroupId });
                }

                return RedirectToAction("Login", "Security");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public IActionResult EditGroup(int securityGroupId)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Security");

            if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
            {
                return RedirectToAction("Login", "Security");
            }

            var model = _security.GetGroupById(securityGroupId);

            return View(model);
        }

        [HttpPost]
        public IActionResult EditGroup(SecurityGroupModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
                {
                    return RedirectToAction("Login", "Security");
                }

                _security.AddOrEditGroup(model);

                return RedirectToAction("EditGroup", new { securityGroupId = model.SecurityGroupId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public IActionResult GroupList()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
                {
                    return RedirectToAction("Login", "Security");
                }

                List<SecurityGroupModel> model = _security.GetAllGroups();

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public IActionResult AddModule()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
                {
                    return RedirectToAction("Login", "Security");
                }

                var model = new SecurityModuleModel();
                {

                };

                return View(model);
            }

            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddModule(SecurityModuleModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
                {
                    return RedirectToAction("Login", "Security");
                }

                int securityModuleId = _security.AddOrEditModule(model);

                if (securityModuleId > 0)
                {
                    return RedirectToAction("EditModule", new { securityModuleId = securityModuleId });
                }

                return RedirectToAction("Login", "Security");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public IActionResult EditModule(int securityModuleId)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Security");

            if (HttpContext.Session.GetInt32("SecurityGroupId") != 1)
            {
                return RedirectToAction("Login", "Security");
            }

            var model = _security.GetModuleById(securityModuleId);

            return View(model);
        }

        [HttpPost]
        public IActionResult EditModule(SecurityModuleModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                _security.AddOrEditModule(model);

                return RedirectToAction("EditModule", new { securityModuleId = model.SecurityModuleId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
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

                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                List<SecurityModuleModel> model = _security.GetAllModules();

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
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

                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Security");

                SecurityGroupModuleModel model = new()
                {

                };

                ViewBag.Groups = new SelectList(_security.GetAllGroups(), "SecurityGroupId", "SecurityGroupName");
                ViewBag.Modules = new SelectList(_security.GetAllModules(), "SecurityModuleId", "SecurityModuleName");
                ViewBag.AccessTypes = new SelectList(_security.GetAllAccessTypes(), "SecurityAccessTypeId", "SecurityAccessTypeName");

                ViewBag.SecurityGroupModuleDetail = _security.GetAllSecurityGroupModuleDetail();

                return View(model);
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

                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                int securityGroupModuleId = _security.AddOrEditGroupModules(model);

                return RedirectToAction("AddModulesToGroups", new { securityGroupModuleId = securityGroupModuleId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
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

                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                int securityGroupModule = _security.DeleteGroupModules(securityGroupModuleId);

                return RedirectToAction("AddModulesToGroups", new { securityGroupModuleId = securityGroupModuleId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                var securityUserId = HttpContext.Session.GetInt32("SecurityUserId");

                var model = _security.GetUserById((int)securityUserId);

                ViewBag.ResultMessage = string.Empty;

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ChangePassword(SecurityUserViewModel model)
        {
            try
            {
                if (!IsUserLoggedIn() || !ModelState.IsValid)
                    return RedirectToAction("Login", "Security");

                model.NewPassword = _security.Encrypt(model.NewPassword);

                int securityGroupModuleId = _security.ChangePassword(model);

                TempData["SuccessMessage"] = "Clave cambiada correctamente";

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
            }
        }

        public JsonResult CheckOldPasswordExist(string paramValue1)
        {
            if (!IsUserLoggedIn())
                return Json("ERROR");

            int securityUserId = (int)HttpContext.Session.GetInt32("SecurityUserId");

            paramValue1 = _security.Encrypt(paramValue1);

            var oldPasswordValid = _security.OldPasswordValid(securityUserId, paramValue1);

            if (!oldPasswordValid)
            {
                return Json("Clave anterior incorrecta");
            }

            return Json("OK");
        }

        public IActionResult Logbook(string selectedStateName, string filterType)
        {
            if (!IsUserLoggedIn() || string.IsNullOrEmpty(selectedStateName))
                return RedirectToAction("Login", "Security");

            ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
            ViewBag.SelectedStateName = selectedStateName;
            ViewBag.FilterType = filterType;

            int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

            if (groupId is null ||
                (groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
                (groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 18)))
            {
                return RedirectToAction("Login", "Security");
            }

            return View();
        }

        [HttpGet]
        public IActionResult GetLogbookData(string selectedStateName, string filterType)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")) || string.IsNullOrEmpty(selectedStateName))
                    return RedirectToAction("Login", "Security");

                int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");
                int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

                if (groupId is null ||
                    (groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
                    (groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 18)))
                    return RedirectToAction("Login", "Security");

                List<SecurityLogbookModel> model;

                if (securityGroupId != 1)
                {
                    if (!_security.GroupHasAccessToModule(securityGroupId.Value, 6))
                        model = _security.GetLogbookByStateName(selectedStateName, filterType);
                    else
                        model = _security.GetLogbookAllExceptAdminByStateName(selectedStateName, filterType);
                }
                else
                {
                    model = _security.GetLogbookAllBySelectedStateName(selectedStateName, filterType);
                }

                var resultado = model.Select(m => new
                {
                    id = m.SecurityLogbookId,
                    fecha = m.SecurityLogbookDate.ToString("dd/MM/yyyy hh:mm tt"),
                    dispositivo = m.DeviceType,
                    os = m.DeviceOperatingSystem,
                    navegador = m.DeviceBrowser,
                    ip = m.DeviceIP,
                    nombre = m.UserFullName,
                    login = m.UserLogin,
                    estado = m.UserState,
                    accion = m.ActionDescription
                });

                return Json(new { data = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

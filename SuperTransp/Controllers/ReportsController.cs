using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperTransp.Core;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class ReportsController : Controller
	{
		private readonly ISupervision _supervision;
		private readonly ISecurity _security;
		private readonly IPublicTransportGroup _publicTransportGroup;
		private readonly IReport _report;
		private readonly IGeography _geography;
		private readonly IExcelExporter _excelExporter;

		public ReportsController(ISupervision supervision, IPublicTransportGroup publicTransportGroup, ISecurity security, IReport report, IGeography geography, IExcelExporter excelExporter)
		{
			_security = security;
			_supervision = supervision;
			_publicTransportGroup = publicTransportGroup;
			_report = report;
			_geography = geography;
			_excelExporter = excelExporter;
		}

		public IActionResult Index()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FullName")) && HttpContext.Session.GetInt32("SecurityGroupId") != null)
			{
				if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 4))
				{
					return RedirectToAction("Login", "Security");
				}

				int? stateId = HttpContext.Session.GetInt32("StateId");
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				List<GeographyViewModel> states = _geography.GetAllStates();

				states.Add(new GeographyViewModel
				{
					StateId = 0,
					StateName = "Todos los estados"
				});

				if (securityGroupId.HasValue)
				{
					if (securityGroupId != 1)
					{
						if (_security.GroupHasAccessToModule((int)securityGroupId, 6))
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateName", "StateName");
							ViewBag.StatisticsStates = new SelectList(states, "StateId", "StateName");
						}
						else
						{
							if (stateId.HasValue)
							{
								ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateName", "StateName");
								ViewBag.StatisticsStates = new SelectList(states.Where(s=> s.StateId == (int)stateId), "StateId", "StateName");

								if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
								{
									ViewBag.IsTotalAccess = true;
								}
							}
						}
					}
					else
					{
						ViewBag.States = new SelectList(_geography.GetAllStates(), "StateName", "StateName");
						
						ViewBag.StatisticsStates = new SelectList(states, "StateId", "StateName");
					}
				}

				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");
				ViewBag.ModulesInGroup = _security.GetModulesByGroupId(ViewBag.SecurityGroupId);
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");
				ViewBag.UserStateId = (int)stateId;

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
					int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (groupId is null ||
						(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
						(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 20)))
					{
						return RedirectToAction("Login", "Security");
					}

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

					return View();
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult GetPublicTransportGroup()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (groupId is null ||
						(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
						(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 20)))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");
					List<PublicTransportGroupViewModel> ptgData = new List<PublicTransportGroupViewModel>();

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						ptgData = _publicTransportGroup.GetAllByStateId((int)stateId);
					}
					else
					{
						ptgData = _publicTransportGroup.GetAll();
					}

					var list = ptgData.Select(ptg => new
					{
						nombre = ptg.PTGCompleteName,
						estado = ptg.StateName,
						modalidad = ptg.ModeName,
						rif = ptg.PublicTransportGroupRif,
						cupos = ptg.Partners,
						cargados = ptg.TotalDrivers,
						supervisados = ptg.TotalSupervisedDrivers,
						estatus = ptg.Partners == ptg.TotalSupervisedDrivers ? "SUPERVISADA" : "PENDIENTE",
						color = ptg.Partners == ptg.TotalSupervisedDrivers ? "green" : "red",
						id = ptg.PublicTransportGroupId
					});

					return Json(new { data = list });
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpGet]
		public IActionResult PublicTransportGroupDriverList(int publicTransportGroupId)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 4))
					{
						return RedirectToAction("Login", "Security");
					}

					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
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

		public IActionResult PublicTransportGroupStatisticsInState(int selectedStatisticsStateId)
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (groupId is null ||
						(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
						(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 22)))
					{
						return RedirectToAction("Login", "Security");
					}

					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = 0;

					if (selectedStatisticsStateId != 0)
					{
						stateId = selectedStatisticsStateId;
					}

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _publicTransportGroup.GetAllStatisticsByStateId((int)stateId);
					}
					else
					{
						if(selectedStatisticsStateId == 0)
						{
							model = _publicTransportGroup.GetAllStatistics();
						}
						else
						{
							model = _publicTransportGroup.GetAllStatisticsByStateId((int)stateId);
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

		public IActionResult PublicTransportGroupSupervisedDriversStatistics()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (groupId is null ||
						(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
						(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 21)))
					{
						return RedirectToAction("Login", "Security");
					}

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

					return View();
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public IActionResult GetPublicTransportGroupSupervisedDriversStatistics()
		{
			try
			{
				List<PublicTransportGroupViewModel> model;
				int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");

				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (groupId is null ||
						(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
						(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 21)))
						{
							return RedirectToAction("Login", "Security");
						}

					if (groupId != 1 && !_security.GroupHasAccessToModule((int)groupId, 6))
						model = _report.GetAllSupervisedVehiclesStatisticsByStateId((int)stateId);
					else
						model = _report.GetAllSupervisedVehiclesStatistics();

					var resultado = model.Select(ptg => new
					{
						nombre = ptg.PTGCompleteName,
						estado = ptg.StateName,
						modalidad = ptg.ModeName,
						rif = ptg.PublicTransportGroupRif,
						supervisados = ptg.TotalSupervisedDrivers,
						operativos = ptg.TotalWorkingVehicles,
						noOperativos = ptg.TotalNotWorkingVehicles,
						conVehiculo = ptg.TotalWithVehicle,
						sinVehiculo = ptg.TotalWithoutVehicle,
						presentes = ptg.TotalDriverInPerson,
						ausentes = ptg.TotalDriverNotInPerson,
						color = ptg.Partners == ptg.TotalSupervisedDrivers ? "green" : "red",
						id = ptg.PublicTransportGroupId
					});

					return Json(new { data = resultado });
				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		public IActionResult SupervisedDriversStatisticsInEstate()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

					if (groupId is null ||
						(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
						(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 23)))
					{
						return RedirectToAction("Login", "Security");
					}

					List<PublicTransportGroupViewModel> model = new();

					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _report.GetAllSupervisedDriversStatisticsInEstateByStateId((int)stateId);
					}
					else
					{
						model = _report.GetAllSupervisedDriversStatisticsInEstate();
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
		public async Task<IActionResult> ExportPublicTransportGroupAndDrivers()
		{
			try
			{
				if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					if (HttpContext.Session.GetInt32("SecurityGroupId") != 1 && !_security.GroupHasAccessToModule((int)HttpContext.Session.GetInt32("SecurityGroupId"), 4))
					{
						return RedirectToAction("Login", "Security");
					}

					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					byte[] content = Array.Empty<byte>();

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						content = await _excelExporter.GenerateExcelPublicTransportGroupAndDriversAsync((int)stateId);
					}
					else
					{
						content = await _excelExporter.GenerateExcelPublicTransportGroupAndDriversAsync(0);
					}					

					return File(content,
								"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
								"OrganizacionesYSocios.xlsx");

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

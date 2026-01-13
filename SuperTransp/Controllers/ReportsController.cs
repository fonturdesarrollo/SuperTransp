using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SuperTransp.Core;
using SuperTransp.Models;
using System.ComponentModel;
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
		private readonly IOptionsSnapshot<MaintenanceSettings> _settings;

		public ReportsController(ISupervision supervision, IPublicTransportGroup publicTransportGroup, ISecurity security, 
			IReport report, IGeography geography, IExcelExporter excelExporter, IOptionsSnapshot<MaintenanceSettings> settings)
		{
			_security = security;
			_supervision = supervision;
			_publicTransportGroup = publicTransportGroup;
			_report = report;
			_geography = geography;
			_excelExporter = excelExporter;
			_settings = settings;
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
				List<GeographyViewModel> statisticsStates = new List<GeographyViewModel>();
				List<GeographyViewModel> movementsStates = new List<GeographyViewModel>();

				statisticsStates.Add(new GeographyViewModel
				{
					StateId = 0,
					StateName = "Todos los Estados"
				});

				movementsStates.Add(new GeographyViewModel
				{
					StateId = 0,
					StateName = "Todos los Estados"
				});

				statisticsStates.AddRange(_geography.GetAllStates());
				movementsStates.AddRange(_geography.GetAllStates());

				if (securityGroupId.HasValue)
				{
					if (securityGroupId != 1)
					{
						if (_security.GroupHasAccessToModule((int)securityGroupId, 6))
						{
							ViewBag.States = new SelectList(_geography.GetAllStates(), "StateName", "StateName");
							ViewBag.StatisticsStates = new SelectList(statisticsStates, "StateId", "StateName");
						}
						else
						{
							if (stateId.HasValue)
							{
								ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateName", "StateName");
								ViewBag.StatisticsStates = new SelectList(statisticsStates.Where(s=> s.StateId == (int)stateId), "StateId", "StateName");

								if (_security.IsTotalAccess(1) || _security.IsUpdateAccess(1))
								{
									ViewBag.IsTotalAccess = true;
								}
							}
						}
					}
					else
					{
						ViewBag.States = new SelectList(movementsStates, "StateName", "StateName");						
						ViewBag.StatisticsStates = new SelectList(statisticsStates, "StateId", "StateName");
					}
				}

				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");
				ViewBag.ModulesInGroup = _security.GetModulesByGroupId(ViewBag.SecurityGroupId);
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");
				ViewBag.UserStateId = (int)stateId;

				if (_settings.Value.IsActive)
				{
					ViewBag.MaintenanceMessage = _settings.Value.Message;
				}

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
					List<PublicTransportGroupViewModel> modelSupervised = new();

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
						modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstateByStateId((int)stateId);
					}
					else
					{
						if(selectedStatisticsStateId == 0)
						{
							model = _publicTransportGroup.GetAllStatistics();
							modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstate();
						}
						else
						{
							model = _publicTransportGroup.GetAllStatisticsByStateId((int)stateId);
							modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstateByStateId((int)stateId);
						}						
					}

					foreach (var supervisionNumbers in modelSupervised)
					{
						model.Where(s => s.StateId == supervisionNumbers.StateId).FirstOrDefault().TotalSupervisedDrivers = supervisionNumbers.TotalSupervisedDrivers;
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

		public IActionResult SummaryList()
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

					List<SupervisionSummaryViewModel> model = new();
					ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
					int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
					int? stateId = HttpContext.Session.GetInt32("StateId");

					if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
					{
						model = _supervision.GetSupervisionSummaryByStateId((int)stateId);
					}
					else
					{
						model = _supervision.GetAllSupervisionSummary();
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

		public IActionResult EditSummary(int supervisionSummaryId, int publicTransportGroupId, bool isClosed = false)
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

				int? securityUserId = HttpContext.Session?.GetInt32("SecurityUserId");
				int? securityGroupId = HttpContext.Session?.GetInt32("SecurityGroupId");
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

				var model = _supervision.GetSupervisionSummaryById(supervisionSummaryId, isClosed);

				return View(model);
			}

			return RedirectToAction("Login", "Security");
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
						socios = ptg.Partners,
						color = ptg.Partners == ptg.TotalSupervisedDrivers ? "green" : "red",
						estatus = ptg.Partners == ptg.TotalSupervisedDrivers ? "SUPERVISADA" : "PENDIENTE",
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

		public IActionResult Dashboard()
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

		public IActionResult DashboardPTG(int selectedStatisticsStateId)
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
					List<PublicTransportGroupViewModel> modelSupervised = new();

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
						modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstateByStateId((int)stateId);
					}
					else
					{
						if (selectedStatisticsStateId == 0)
						{
							model = _publicTransportGroup.GetAllStatistics();
							modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstate();
						}
						else
						{
							model = _publicTransportGroup.GetAllStatisticsByStateId((int)stateId);
							modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstateByStateId((int)stateId);
						}
					}

					foreach (var supervisionNumbers in modelSupervised)
					{
						model.Where(s => s.StateId == supervisionNumbers.StateId).FirstOrDefault().TotalSupervisedDrivers = supervisionNumbers.TotalSupervisedDrivers;
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

		//Sustiuido por el Dashboard
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
		public async Task<IActionResult> ExportPublicTransportGroup()
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
						content = await _excelExporter.GenerateExcelPublicTransportGroupAsync((int)stateId);
					}
					else
					{
						content = await _excelExporter.GenerateExcelPublicTransportGroupAsync(0);
					}

					return File(content,
								"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
								$"Estadisticas_Organizaciones_{DateTime.Now:yyyyMMdd}.xlsx");

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
								$"Estadisticas_OrganizacionesSocios_{DateTime.Now:yyyyMMdd}.xlsx");

				}

				return RedirectToAction("Login", "Security");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message.ToString() });
			}
		}

		[HttpGet]
		public async Task<IActionResult> ExportSupervisionDetail()
		{
			try 
			{ 
				if (string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					return RedirectToAction("Login", "Security");
				}

				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");

				if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 4))
				{
					return RedirectToAction("Login", "Security");
				}

				int idParam = 0;
				if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
				{
					idParam = (int)stateId;
				}

				var content = await _excelExporter.GenerateExcelSupervisionDetailAsync(idParam);

				return File(content,
							"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
							$"Estadisticas_Supervisiones_{DateTime.Now:yyyyMMdd}.xlsx");
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
			}
		}

		public async Task<IActionResult> ExportCurrentAdvance()
		{
			var data = await GetDataForCurrentAdvanceExport();

			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Avance de la Carga");

				worksheet.Cell(1, 1).Value = "Estado";
				worksheet.Cell(1, 2).Value = "Organizaciones Cargadas";
				worksheet.Cell(1, 3).Value = "Universo Organizaciones";
				worksheet.Cell(1, 4).Value = "Socios Totales";
				worksheet.Cell(1, 5).Value = "Socios Cargados";
				worksheet.Cell(1, 6).Value = "Socios Supervisados";
				worksheet.Cell(1, 7).Value = "Universo Socios";


				var headerRange = worksheet.Range("A1:G1");
				headerRange.Style.Font.Bold = true;
				headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

				int row = 2;
				foreach (var item in data)
				{
					worksheet.Cell(row, 1).Value = item.StateName;

					worksheet.Cell(row, 2).Value = item.TotalPTGInState;
					worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";

					worksheet.Cell(row, 3).Value = item.TotalUniversePTGInState;
					worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0";

					worksheet.Cell(row, 4).Value = item.TotaPartnersByPTG;
					worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";

					worksheet.Cell(row, 5).Value = item.TotalAddedPartners;
					worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";

					worksheet.Cell(row, 6).Value = item.TotalSupervisedDrivers;
					worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

					worksheet.Cell(row, 7).Value = item.TotalUniverseDriversInState;
					worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";

					row++;
				}

				worksheet.Columns().AdjustToContents();

				using (var stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					var content = stream.ToArray();

					return File(content,
								"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
								$"EstadisticasOrganizaciones_{DateTime.Now:yyyyMMdd}.xlsx");
				}
			}
		}

		private async Task<List<PublicTransportGroupViewModel>> GetDataForCurrentAdvanceExport()
		{
			List<PublicTransportGroupViewModel> model = new();
			List<PublicTransportGroupViewModel> modelSupervised = new();

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
			int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");
			int? stateId = HttpContext.Session.GetInt32("StateId");

			if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
			{
				model = _publicTransportGroup.GetAllStatisticsByStateId((int)stateId);
				modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstateByStateId((int)stateId);
			}
			else
			{
				model = _publicTransportGroup.GetAllStatistics();
				modelSupervised = _report.GetAllSupervisedDriversStatisticsInEstate();
			}

			foreach (var supervisionNumbers in modelSupervised)
			{
				model.Where(s => s.StateId == supervisionNumbers.StateId).FirstOrDefault().TotalSupervisedDrivers = supervisionNumbers.TotalSupervisedDrivers;
			}

			return model;
		}

		public IActionResult HistoricalData()
		{
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
			{
				int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");
				int? stateId = HttpContext.Session.GetInt32("StateId");

				if (groupId is null ||
					(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
					(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 21)))
				{
					return RedirectToAction("Login", "Security");
				}

				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

				if (_security.GroupHasAccessToModule((int)groupId, 6) || groupId == 1)
				{
					ViewBag.States = new SelectList(_geography.GetAllStates(), "StateId", "StateName");
				}
				else
				{
					ViewBag.States = new SelectList(_geography.GetStateById((int)stateId), "StateId", "StateName");
				}

				return View();
			}

			return RedirectToAction("Login", "Security");
		}

		[HttpGet]
		public JsonResult GetClosedRounds(int stateId)
		{
			var rounds = _supervision.GetClosedRoundsByStateId(stateId);

			return Json(rounds);
		}

		[HttpGet]
		public IActionResult SummaryClosedListAjax(int stateId, int supervisionRoundId)
		{
			try
			{
				if (string.IsNullOrEmpty(HttpContext.Session.GetString("SecurityUserId")))
				{
					return Json(new { data = new List<SupervisionSummaryViewModel>() });
				}

				if (stateId == 0 || supervisionRoundId == 0)
				{
					return Json(new { data = new List<SupervisionSummaryViewModel>() });
				}

				int? groupId = HttpContext.Session.GetInt32("SecurityGroupId");

				if (groupId is null ||
					(groupId != 1 && !_security.GroupHasAccessToModule(groupId.Value, 4)) ||
					(groupId == 1 ? false : !_security.GroupHasAccessToModule(groupId.Value, 21)))
				{
					return Json(new { data = new List<SupervisionSummaryViewModel>() });
				}

				List<SupervisionSummaryViewModel> model = new();
				int? securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

				object roundData = null;

				if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, 6))
				{
					model = _supervision.GetSupervisionSummaryByStateId((int)stateId, supervisionRoundId);

					if (model.Any())
					{
						roundData = _supervision.GetClosedRoundsBySupervisionRoundIdAndStateId(supervisionRoundId, stateId);
					}
				}
				else
				{
					model = _supervision.GetAllSupervisionSummary((int)stateId, supervisionRoundId);

					if (model.Any())
					{
						roundData = _supervision.GetClosedRoundsBySupervisionRoundIdAndStateId(supervisionRoundId, stateId);
					}
				}

				return Json(new
				{
					data = model,
					totals = roundData
				});
			}
			catch (Exception ex)
			{
				return Json(new { error = ex.Message });
			}
		}

		private IActionResult? CheckSessionAndPermission(int requiredModuleId)
		{
			var securityGroupId = HttpContext.Session.GetInt32("SecurityGroupId");

			if (securityGroupId == null)
				return RedirectToAction("Login", "Security");

			if (securityGroupId != 1 && !_security.GroupHasAccessToModule((int)securityGroupId, requiredModuleId))
				return RedirectToAction("Login", "Security");

			return null;
		}
	}
}

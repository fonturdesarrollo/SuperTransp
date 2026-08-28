using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SuperTransp.Models;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class ProceduresController : Controller
	{
		private const int ModuleId = 34;

		private readonly IProcedure _procedure;
		private readonly IPublicTransportGroup _publicTransportGroup;
		private readonly ICommonData _commonData;
		private readonly IExchangeRate _exchangeRate;
		private readonly ISecurity _security;
		private readonly IOptionsSnapshot<MaintenanceSettings> _settings;

		public ProceduresController(IProcedure procedure, IPublicTransportGroup publicTransportGroup, ICommonData commonData, IExchangeRate exchangeRate, ISecurity security, IOptionsSnapshot<MaintenanceSettings> settings)
		{
			_procedure = procedure;
			_publicTransportGroup = publicTransportGroup;
			_commonData = commonData;
			_exchangeRate = exchangeRate;
			_security = security;
			_settings = settings;
		}

		public IActionResult Index()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
			ViewBag.SecurityGroupId = (int)HttpContext.Session.GetInt32("SecurityGroupId");

			if (_settings.Value.IsActive)
			{
				ViewBag.MaintenanceMessage = _settings.Value.Message;
			}

			return View();
		}

		public IActionResult ManagedProcedures()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

			return View();
		}

		public IActionResult RealizedProcedures()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

			return View();
		}

		public IActionResult Add()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
			ViewBag.Categories = new SelectList(_procedure.ProcedureCategoryAll(), "ProcedureCategoryId", "ProcedureCategoryName");
			ViewBag.Frequencies = new SelectList(_procedure.ProcedureFrequencyAll(), "ProcedureFrequencyId", "ProcedureFrequencyName");

			var model = new ProcedureViewModel { ProcedureId = 0 };

			return View(model);
		}

		[HttpPost]
		public IActionResult Add(ProcedureViewModel model)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			if (!ModelState.IsValid)
			{
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.Categories = new SelectList(_procedure.ProcedureCategoryAll(), "ProcedureCategoryId", "ProcedureCategoryName");
				ViewBag.Frequencies = new SelectList(_procedure.ProcedureFrequencyAll(), "ProcedureFrequencyId", "ProcedureFrequencyName");

				return View(model);
			}

			try
			{
				_procedure.AddOrEdit(model);

				TempData["SuccessMessage"] = "Trámite agregado correctamente";
			}
			catch (Exception ex)
			{
				TempData["SuccessMessage"] = $"Error al agregar el trámite: {ex.Message}";
			}

			return RedirectToAction("Add");
		}

		public IActionResult List()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";

			var model = _procedure.GetAll();

			return View(model);
		}

		[HttpGet]
		public IActionResult Edit(int procedureId)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			var model = _procedure.GetAll().FirstOrDefault(p => p.ProcedureId == procedureId);

			if (model == null) return RedirectToAction("List");

			ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
			ViewBag.Categories = new SelectList(_procedure.ProcedureCategoryAll(), "ProcedureCategoryId", "ProcedureCategoryName");
			ViewBag.Frequencies = new SelectList(_procedure.ProcedureFrequencyAll(), "ProcedureFrequencyId", "ProcedureFrequencyName");

			return View(model);
		}

		[HttpPost]
		public IActionResult Edit(ProcedureViewModel model)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return result;

			if (!ModelState.IsValid)
			{
				ViewBag.EmployeeName = $"{(string)HttpContext.Session.GetString("FullName")} ({(string)HttpContext.Session.GetString("SecurityGroupName")})";
				ViewBag.Categories = new SelectList(_procedure.ProcedureCategoryAll(), "ProcedureCategoryId", "ProcedureCategoryName");
				ViewBag.Frequencies = new SelectList(_procedure.ProcedureFrequencyAll(), "ProcedureFrequencyId", "ProcedureFrequencyName");

				return View(model);
			}

			try
			{
				// El SP solo devuelve el id nuevo al insertar; al actualizar no hay resultado
				// (no lanzar excepción ya es señal de éxito).
				_procedure.AddOrEdit(model);

				TempData["SuccessMessage"] = "Trámite actualizado correctamente";
			}
			catch (Exception ex)
			{
				TempData["SuccessMessage"] = $"Error al actualizar el trámite: {ex.Message}";
			}

			return RedirectToAction("Edit", new { procedureId = model.ProcedureId });
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetProcedureStatuses()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			var data = _procedure.GetStatusAll();

			return Json(data.Select(s => new
			{
				procedureStatusId = s.ProcedureStatusId,
				procedureStatusName = s.ProcedureStatusName
			}));
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetProceduresByStatus(int procedureStatusId)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(new { data = Array.Empty<object>() });

			var data = _procedure.GetByPTGByProcedureStatusId(procedureStatusId);

			var list = data.Select(p => new
			{
				procedureByPublicTransportGroupId = p.ProcedureByPublicTransportGroupId,
				procedureStatusId = p.ProcedureStatusId,
				publicTransportGroupRif = p.PublicTransportGroupRif,
				ptgName = p.PublicTransportGroupNameFullName,
				representativeName = p.RepresentativeName,
				representativePhone = p.RepresentativePhone,
				procedureCategoryName = p.ProcedureCategoryName,
				procedureConcept = p.ProcedureConcept,
				tariff = p.Tariff,
				rate = p.Rate,
				bcvPaid = p.BCVPaid,
				accounTypeId = p.AccounTypeId,
				receiverBankAccountTypeName = p.ReceiverBankAccountTypeName,
				receiverBankName = p.ReceiverBankName,
				receiverBankAccountNumber = p.ReceiverBankAccountNumber,
				receiverBankCellPhone = p.ReceiverBankCellPhone,
				receiverBankIdNumber = p.ReceiverBankIdNumber,
				payerBankName = p.PayerBankName,
				payerCellPhoneNumber = p.PayerCellPhoneNumber,
				payerIdNumber = p.PayerIdNumber,
				payerAccountNumber = p.PayerAccountNumber,
				procedureStatusName = p.ProcedureStatusName
			});

			return Json(new { data = list });
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetDriverByPublicTransportGroupRif(string publicTransportGroupRif)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(null);

			var ptg = _publicTransportGroup.GetAll().FirstOrDefault(p => p.PublicTransportGroupRif == publicTransportGroupRif);

			if (ptg == null) return Json(null);

			return Json(new
			{
				publicTransportGroupId = ptg.PublicTransportGroupId,
				publicTransportGroupRif = ptg.PublicTransportGroupRif,
				ptgCompleteName = ptg.PTGCompleteName,
				stateName = ptg.StateName,
				modeName = ptg.ModeName,
				representativeName = ptg.RepresentativeName,
				representativeIdentityDocument = ptg.RepresentativeIdentityDocument,
				representativePhone = ptg.RepresentativePhone,
				partners = ptg.Partners,
				totalDrivers = ptg.TotalDrivers
			});
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetProcedureTypes()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			var data = _procedure.GetAll();

			return Json(data.Select(p => new
			{
				procedureId = p.ProcedureId,
				procedureCategoryName = p.ProcedureCategoryName,
				procedureConcept = p.ProcedureConcept,
				procedureFrequencyName = p.ProcedureFrequencyName,
				tariff = p.Tariff
			}));
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetExchangeRate()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(null);

			var usd = _exchangeRate.GetAll().FirstOrDefault(r => r.CurrencyCode == "USD");

			if (usd == null) return Json(null);

			return Json(new
			{
				currencyCode = usd.CurrencyCode,
				rate = usd.Rate,
				fetchedAt = usd.FetchedAt
			});
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetAccounTypes()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			var data = _commonData.GetAccounTypesAll();

			return Json(data.Select(a => new
			{
				accounTypeId = a.AccounTypeId,
				accounTypeName = a.AccounTypeName
			}));
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetProcedureBankAccounts(int accounTypeId)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			var data = _procedure.GetByBankAccounTypeId(accounTypeId);

			return Json(data.Select(b => new
			{
				procedureBankAccountId = b.ProcedureBankAccountId,
				bankId = b.BankId,
				accounTypeId = b.AccounTypeId,
				accounTypeName = b.AccounTypeName,
				accountNumber = b.AccountNumber,
				cellPhoneNumber = b.CellPhoneNumber,
				idNumber = b.IdNumber,
				bankName = b.BankName,
				sudebanCode = b.SudebanCode
			}));
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetBanks()
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			var data = _commonData.GetBanksAll();

			return Json(data.Select(b => new
			{
				bankId = b.BankId,
				bankName = b.BankName,
				sudebanCode = b.SudebanCode
			}));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public JsonResult SubmitProcedureRequest(ProcedureByPublicTransportGroupViewModel model)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(new { success = false, message = "Sesión inválida" });

			// Los montos se parsean manualmente con cultura invariante: el binder por defecto usa
			// la cultura del servidor, que en instalaciones en español espera coma decimal y
			// rechazaría el punto que envía el formulario (formato "123.45").
			if (model != null)
			{
				if (decimal.TryParse(Request.Form["BCVPaid"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var bcvPaid))
				{
					model.BCVPaid = bcvPaid;
				}

				if (decimal.TryParse(Request.Form["Tariff"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tariff))
				{
					model.Tariff = tariff;
				}

				if (decimal.TryParse(Request.Form["Rate"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rate))
				{
					model.Rate = rate;
				}
			}

			if (model == null || model.ProcedureId <= 0 || model.PublicTransportGroupId <= 0 || model.ProcedureBankAccountId <= 0 || model.BankId <= 0 || model.BCVPaid <= 0 || model.Tariff <= 0 || model.Rate <= 0)
			{
				return Json(new { success = false, message = "Datos incompletos para procesar la solicitud" });
			}

			try
			{
				var procedureByPublicTransportGroupId = _procedure.AddOrEditByPTG(model);

				return Json(new { success = procedureByPublicTransportGroupId > 0, procedureByPublicTransportGroupId });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public JsonResult UpdateProcedureStatuses([FromBody] List<ProcedureStatusChangeItem> changes)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(new { success = false, message = "Sesión inválida" });

			if (changes == null || changes.Count == 0)
			{
				return Json(new { success = false, message = "No hay cambios para actualizar" });
			}

			try
			{
				var updated = 0;

				foreach (var change in changes)
				{
					if (change.ProcedureByPublicTransportGroupId <= 0 || change.ProcedureStatusId <= 0) continue;

					// El SP solo actualiza ProcedureStatusId cuando @ProcedureByPublicTransportGroupId != 0;
					// ExecuteScalar no devuelve resultado en esa rama (no hay excepción = éxito).
					_procedure.AddOrEditByPTG(new ProcedureByPublicTransportGroupViewModel
					{
						ProcedureByPublicTransportGroupId = change.ProcedureByPublicTransportGroupId,
						ProcedureStatusId = change.ProcedureStatusId
					});

					updated++;
				}

				return Json(new { success = true, updated });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		public class ProcedureStatusChangeItem
		{
			public int ProcedureByPublicTransportGroupId { get; set; }
			public int ProcedureStatusId { get; set; }
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

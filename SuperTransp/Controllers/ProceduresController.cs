using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using QRCoder;
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
		private readonly IConfiguration _configuration;

		public ProceduresController(IProcedure procedure, IPublicTransportGroup publicTransportGroup, ICommonData commonData, IExchangeRate exchangeRate, ISecurity security, IOptionsSnapshot<MaintenanceSettings> settings, IConfiguration configuration)
		{
			_procedure = procedure;
			_publicTransportGroup = publicTransportGroup;
			_commonData = commonData;
			_exchangeRate = exchangeRate;
			_security = security;
			_settings = settings;
			_configuration = configuration;
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
				// The SP only returns the new id on insert; there's no result on update
				// (no exception thrown is itself the success signal).
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

			var data = _procedure.GetByDriverByProcedureStatusId(procedureStatusId);

			// JSON names are kept from the by-organization version so the list views keep working:
			// the "organization" fields now carry the driver's organization at request time.
			// Newest requests first
			var list = data.OrderByDescending(p => p.ProcedureByDriverId).Select(p => new
			{
				procedureByPublicTransportGroupId = p.ProcedureByDriverId,
				procedureStatusId = p.ProcedureStatusId,
				publicTransportGroupRif = p.DriverPTGRif,
				ptgName = p.DriverPTGName,
				representativeName = p.DriverFullName,
				representativePhone = p.DriverPhone,
				driverIdentityDocument = p.DriverIdentityDocument,
				vehiclePlate = p.DriverVehiclePlate,
				vehicleDescription = string.Join(" ", new[] { p.DriverVehicleMake, p.DriverVehicleModel, p.DriverVehicleYear > 0 ? p.DriverVehicleYear.ToString() : null }.Where(x => !string.IsNullOrWhiteSpace(x))),
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
				payerReferenceNumber = p.PayerReferenceNumber,
				procedureDate = p.ProcedureDate,
				procedureStatusName = p.ProcedureStatusName
			});

			return Json(new { data = list });
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetDriverVehiclesByIdentityDocument(int driverIdentityDocument)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(null);

			var options = _procedure.GetDriverVehiclesByIdentityDocument(driverIdentityDocument);

			if (options.Count == 0) return Json(null);

			return Json(new
			{
				driverIdentityDocument = options[0].DriverIdentityDocument,
				driverFullName = options[0].DriverFullName,
				vehicles = options.Select(o => new
				{
					driverId = o.DriverId,
					publicTransportGroupId = o.PublicTransportGroupId,
					publicTransportGroupRif = o.PublicTransportGroupRif,
					modeId = o.ModeId,
					modeName = o.ModeName,
					ptgCompleteName = o.PTGCompleteName,
					plate = o.Plate,
					year = o.Year,
					make = o.Make,
					model = o.Model
				})
			});
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetRegistrationRequestData(int procedureByDriverId)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(null);

			var data = _procedure.GetRegistrationRequestData(procedureByDriverId);

			// The request letter can only be issued for VALIDADO (2) procedures
			if (data == null || data.ProcedureStatusId != 2) return Json(null);

			// Same verification URL used by the driver QR (QR/DriverData?driverCode=<ptg guid>|<partner number>)
			var baseWebSiteUrl = _configuration["FtpSettings:BaseWebSiteUrl"];
			var verificationUrl = $"{baseWebSiteUrl}QR/DriverData?driverCode={data.PublicTransportGroupGUID}|{data.PartnerNumber}";

			string qrImage;
			using (var qrGenerator = new QRCodeGenerator())
			using (var qrCodeData = qrGenerator.CreateQrCode(verificationUrl, QRCodeGenerator.ECCLevel.Q))
			using (var pngByteQRCode = new PngByteQRCode(qrCodeData))
			{
				qrImage = "data:image/png;base64," + Convert.ToBase64String(pngByteQRCode.GetGraphic(10));
			}

			return Json(new
			{
				driverFullName = data.DriverFullName,
				driverIdentityDocument = data.DriverIdentityDocument,
				ptgName = data.PTGName,
				partnerNumber = data.PartnerNumber,
				modeName = data.ModeName,
				plate = data.Plate,
				make = data.Make,
				model = data.Model,
				year = data.Year,
				qrImage
			});
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public JsonResult GetProcedureTypes(int modeId, int procedureId = 0)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			// Tariffs depend on the transport mode of the selected vehicle's organization
			var data = _procedure.GetAllByMode(modeId, procedureId);

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
		public JsonResult GetProcedureById(int procedureId)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(Array.Empty<object>());

			var data = _procedure.GetById(procedureId);

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
		public JsonResult GetMonthlyFeePendingsByPTG(int publicTransportGroupId)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(0);

			return Json(_procedure.GetMonthlyFeePendingsByPTG(publicTransportGroupId));
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
		public JsonResult SubmitProcedureRequest(ProcedureByDriverViewModel model)
		{
			var result = CheckSessionAndPermission(ModuleId);
			if (result != null) return Json(new { success = false, message = "Sesión inválida" });

			// Amounts are parsed manually with invariant culture: the default binder uses
			// the server's culture, which on Spanish-locale installs expects a decimal comma
			// and would reject the dot the form sends (format "123.45").
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

			if (model == null || model.ProcedureId <= 0 || model.DriverId <= 0 || string.IsNullOrWhiteSpace(model.DriverVehiclePlate) || model.ProcedureBankAccountId <= 0 || model.BankId <= 0 || model.BCVPaid <= 0 || model.Tariff <= 0 || model.Rate <= 0)
			{
				return Json(new { success = false, message = "Datos incompletos para procesar la solicitud" });
			}

			// Same rules as the wizard: mobile payment (3) needs a valid phone, any other type needs a 20-digit account
			if (model.AccounTypeId == 3)
			{
				if (!System.Text.RegularExpressions.Regex.IsMatch(model.PayerCellPhoneNumber?.Trim() ?? string.Empty, @"^(0412|0414|0416|0424|0426)\d{7}$"))
				{
					return Json(new { success = false, message = "El teléfono debe comenzar con 0412, 0414, 0416, 0424 o 0426 y tener 11 dígitos." });
				}
			}
			else if (!System.Text.RegularExpressions.Regex.IsMatch(model.PayerAccountNumber?.Trim() ?? string.Empty, @"^\d{20}$"))
			{
				return Json(new { success = false, message = "El número de cuenta debe tener 20 dígitos." });
			}

			try
			{
				var procedureByDriverId = _procedure.AddOrEditByDriver(model);

				// The SP's INSERT ignores @ProcedureStatusId (the table default is 1), so new requests are
				// saved as VALIDADO (2) through the SP's update branch right after the insert.
				if (procedureByDriverId > 0)
				{
					_procedure.AddOrEditByDriver(new ProcedureByDriverViewModel
					{
						ProcedureByDriverId = procedureByDriverId,
						ProcedureStatusId = 2
					});

					// The request is already saved: a logbook failure must not turn it into an error for the user
					try
					{
						_procedure.LogProcedureRequest(procedureByDriverId);
					}
					catch
					{
					}
				}

				return Json(new { success = procedureByDriverId > 0, procedureByDriverId });
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

					// The SP only updates ProcedureStatusId when @ProcedureByPublicTransportGroupId != 0;
					// ExecuteScalar returns nothing on that branch (no exception = success).
					_procedure.AddOrEditByDriver(new ProcedureByDriverViewModel
					{
						ProcedureByDriverId = change.ProcedureByPublicTransportGroupId,
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

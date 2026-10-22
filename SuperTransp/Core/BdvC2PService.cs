using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuperTransp.Core
{
	/// <summary>Settings of the Banco de Venezuela C2P (Cuentas Múltiples) integration, section "Bdv".</summary>
	public class BdvC2POptions
	{
		/// <summary>Off by default: while false no call is made to the bank.</summary>
		public bool Enabled { get; set; }
		public string BaseUrl { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;
		public int TimeoutSeconds { get; set; } = 30;
		/// <summary>Only for the QA host if its certificate is not trusted. Never enable it in production.</summary>
		public bool SkipCertificateValidation { get; set; }
	}

	public class BdvC2PResult
	{
		public bool Success { get; set; }
		/// <summary>Bank code ("1000" = done) or a local code (DISABLED, INVALID, TIMEOUT, ERROR).</summary>
		public string Code { get; set; } = string.Empty;
		/// <summary>Message in Spanish, ready to show to the user.</summary>
		public string Message { get; set; } = string.Empty;
		public string? Reference { get; set; }
		public string? EndToEndId { get; set; }
		public string? Date { get; set; }
	}

	public class BdvC2PChargeRequest
	{
		/// <summary>With nationality letter, e.g. V12345678.</summary>
		public string CustomerDocumentId { get; set; } = string.Empty;
		/// <summary>Payer's mobile phone (11 digits).</summary>
		public string CustomerNumberInstrument { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		/// <summary>Sudeban code of the payer's bank (4 digits).</summary>
		public string CustomerBankCode { get; set; } = string.Empty;
		public string Concept { get; set; } = "Pago";
		/// <summary>Payment key (OTP) of the payer. It is never stored nor logged.</summary>
		public string Otp { get; set; } = string.Empty;
		/// <summary>Mobile phone of the commerce: selects the account that receives the money.</summary>
		public string CommerceNumberInstrument { get; set; } = string.Empty;
	}

	/// <summary>
	/// Client of the BDV C2P Cuentas Múltiples API: payment key (OTP) generation, charge and annulment.
	/// Nothing calls the charge or the annulment yet: the wizard still uses the simulated bank validation.
	/// </summary>
	public class BdvC2PService
	{
		private static readonly Regex DocumentIdRegex = new(@"^[VEP]\d{4,8}$", RegexOptions.Compiled); // the bank accepts a letter plus 4 to 8 digits
		private static readonly Regex PhoneRegex = new(@"^\d{11}$", RegexOptions.Compiled);
		private static readonly Regex BankCodeRegex = new(@"^\d{4}$", RegexOptions.Compiled);

		// Error table of the service documentation
		private static readonly Dictionary<string, string> ErrorMessages = new()
		{
			["1002"] = "Ha ocurrido un error en el envío al conector del banco.",
			["1006"] = "El RIF suministrado no es comercio afiliado.",
			["1013"] = "Monto inválido.",
			["1014"] = "El beneficiario no está afiliado a Pago Móvil BDV.",
			["1015"] = "No está afiliado a Clave Móvil BDV.",
			["1026"] = "Referencia o monto duplicado.",
			["1034"] = "Saldo insuficiente.",
			["1041"] = "El servicio está inactivo.",
			["1050"] = "La solicitud superó el tiempo de espera.",
			["1055"] = "La clave de pago no existe.",
			["1056"] = "El número de teléfono no corresponde con el titular.",
			["1061"] = "El monto supera el límite diario.",
			["1062"] = "La cuenta tiene inconvenientes.",
			["1065"] = "Se superó la cantidad de transacciones permitidas.",
			["1080"] = "Documento de identidad inválido.",
			["1091"] = "El banco destino está inactivo.",
			["1092"] = "El banco destino no está afiliado.",
			["1094"] = "Operación duplicada."
		};

		// One payment key request per document per minute (avoids flooding the payer with messages)
		private static readonly ConcurrentDictionary<string, DateTime> LastKeyRequests = new();
		private static readonly TimeSpan KeyRequestInterval = TimeSpan.FromMinutes(1);

		private readonly HttpClient _http;
		private readonly BdvC2POptions _options;

		public BdvC2PService(HttpClient http, Microsoft.Extensions.Options.IOptions<BdvC2POptions> options)
		{
			_http = http;
			_options = options.Value;
		}

		public bool IsEnabled => _options.Enabled && !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.ApiKey);

		/// <summary>Step 1: the bank sends a payment key (OTP) to the payer.</summary>
		public async Task<BdvC2PResult> RequestPaymentKeyAsync(string customerDocumentId)
		{
			if (!IsEnabled) return Disabled();

			customerDocumentId = (customerDocumentId ?? string.Empty).Trim().ToUpperInvariant();
			if (!DocumentIdRegex.IsMatch(customerDocumentId)) return Invalid("El documento de identidad debe llevar la letra V, E o P seguida de 4 a 8 dígitos.");

			var now = DateTime.UtcNow;
			if (LastKeyRequests.TryGetValue(customerDocumentId, out var last) && now - last < KeyRequestInterval)
			{
				return Invalid("Ya se solicitó una clave de pago. Espere un minuto antes de solicitar otra.");
			}
			LastKeyRequests[customerDocumentId] = now;

			return await PostAsync("paymentkey/v2", new { customerDocumentId });
		}

		/// <summary>Step 2: charges the payer's account and credits the commerce account.</summary>
		public async Task<BdvC2PResult> ChargeAsync(BdvC2PChargeRequest request)
		{
			if (!IsEnabled) return Disabled();

			var documentId = (request.CustomerDocumentId ?? string.Empty).Trim().ToUpperInvariant();
			if (!DocumentIdRegex.IsMatch(documentId)) return Invalid("El documento de identidad debe llevar la letra V, E o P seguida de 4 a 8 dígitos.");
			if (!PhoneRegex.IsMatch(request.CustomerNumberInstrument ?? string.Empty)) return Invalid("El teléfono del pagador debe tener 11 dígitos.");
			if (!PhoneRegex.IsMatch(request.CommerceNumberInstrument ?? string.Empty)) return Invalid("El teléfono del comercio debe tener 11 dígitos.");
			if (!BankCodeRegex.IsMatch(request.CustomerBankCode ?? string.Empty)) return Invalid("El código del banco del pagador no es válido.");
			if (request.Amount <= 0) return Invalid("El monto debe ser mayor a cero.");
			if (string.IsNullOrWhiteSpace(request.Otp)) return Invalid("Debe indicar la clave de pago.");

			return await PostAsync("process/v2", new
			{
				customerDocumentId = documentId,
				customerNumberInstrument = request.CustomerNumberInstrument,
				amount = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
				customerBankCode = request.CustomerBankCode,
				concept = string.IsNullOrWhiteSpace(request.Concept) ? "Pago" : request.Concept,
				otp = request.Otp,
				coinType = "VES",
				operationType = "CELE",
				commerceNumberInstrument = request.CommerceNumberInstrument
			});
		}

		/// <summary>Step 3: annuls a charge using the endToEndId returned by the charge.</summary>
		public async Task<BdvC2PResult> AnnulAsync(string endToEndId, string? referenceOrigin = null)
		{
			if (!IsEnabled) return Disabled();
			if (string.IsNullOrWhiteSpace(endToEndId)) return Invalid("Falta el identificador de la operación a anular.");

			return await PostAsync("annulment/v2", new { endToEndId, referenceOrigin });
		}

		private async Task<BdvC2PResult> PostAsync(string path, object body)
		{
			try
			{
				var url = new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), path);
				using var message = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
				};
				message.Headers.Add("X-API-Key", _options.ApiKey);
				message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				using var response = await _http.SendAsync(message);
				var text = await response.Content.ReadAsStringAsync();

				using var doc = JsonDocument.Parse(text);
				var root = doc.RootElement;

				var code = root.TryGetProperty("code", out var codeProp) ? codeProp.ToString() : string.Empty;
				var bankMessage = root.TryGetProperty("message", out var messageProp) ? messageProp.GetString() : null;

				var result = new BdvC2PResult { Code = code, Success = code == "1000" };

				if (result.Success)
				{
					result.Message = "Operación realizada.";

					if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
					{
						result.Reference = ReadString(data, "referencia");
						result.EndToEndId = ReadString(data, "endToEndId");
						result.Date = ReadString(data, "date");
					}
				}
				else
				{
					result.Message = ErrorMessages.TryGetValue(code, out var known)
						? known
						: (string.IsNullOrWhiteSpace(bankMessage) ? "El banco no pudo procesar la operación." : bankMessage);
				}

				return result;
			}
			catch (TaskCanceledException)
			{
				return new BdvC2PResult { Code = "TIMEOUT", Message = ErrorMessages["1050"] };
			}
			catch (Exception)
			{
				// The details are not returned to the user (they could expose the bank's host or the key)
				return new BdvC2PResult { Code = "ERROR", Message = "No se pudo comunicar con el banco. Intente nuevamente." };
			}
		}

		private static string? ReadString(JsonElement element, string name)
		{
			return element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value.ToString() : null;
		}

		private static BdvC2PResult Disabled() => new() { Code = "DISABLED", Message = "El cobro con el banco aún no está habilitado." };

		private static BdvC2PResult Invalid(string message) => new() { Code = "INVALID", Message = message };
	}
}

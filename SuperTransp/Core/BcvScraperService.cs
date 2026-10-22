using System.Globalization;
using System.Text.RegularExpressions;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class BcvScraperService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<BcvScraperService> _logger;

		private static readonly TimeSpan _interval = TimeSpan.FromHours(6);

		// bcv.org.ve has a problematic SSL certificate; validation is disabled only for this host
		private static readonly HttpClient _httpClient = new(new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		})
		{
			DefaultRequestHeaders = { { "User-Agent", "Mozilla/5.0" } },
			Timeout = TimeSpan.FromSeconds(30)
		};

		public BcvScraperService(IServiceScopeFactory scopeFactory, ILogger<BcvScraperService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await FetchNowAsync();

			using var timer = new PeriodicTimer(_interval);
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				await FetchNowAsync();
			}
		}

		public async Task FetchNowAsync()
		{
			try
			{
				var html = await _httpClient.GetStringAsync("https://www.bcv.org.ve/");
				var rates = ParseRates(html);

				if (rates.Count == 0)
				{
					_logger.LogWarning("BCV: no rates found in the HTML");
					return;
				}

				using var scope = _scopeFactory.CreateScope();
				var dal = scope.ServiceProvider.GetRequiredService<IExchangeRate>();

				foreach (var (code, rate) in rates)
				{
					dal.Save(code, rate);
				}

				_logger.LogInformation("BCV rates updated: {Rates}",
					string.Join(", ", rates.Select(r => $"{r.Key}={r.Value:F4}")));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "BCV: error fetching rates");
			}
		}

		private static Dictionary<string, decimal> ParseRates(string html)
		{
			var result = new Dictionary<string, decimal>();
			var targets = new[] { ("dolar", "USD"), ("euro", "EUR") };

			foreach (var (divId, code) in targets)
			{
				// Looks for <div id="dolar"...> ... <strong>XX,XXXX</strong>
				var match = Regex.Match(html,
					$@"id=""{divId}""[^>]*>.*?<strong[^>]*>\s*([\d\.,]+)\s*</strong>",
					RegexOptions.Singleline | RegexOptions.IgnoreCase);

				if (!match.Success) continue;

				// Venezuelan number format: dot = thousands separator, comma = decimal → "91,5234" or "1.091,5234"
				var raw = match.Groups[1].Value.Trim()
					.Replace(".", "")
					.Replace(",", ".");

				if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate) && rate > 0)
				{
					result[code] = rate;
				}
			}

			return result;
		}
	}
}

namespace SuperTransp.Models
{
	public class ExchangeRateViewModel
	{
		public string? CurrencyCode { get; set; }
		public decimal Rate { get; set; }
		public DateTime FetchedAt { get; set; }
	}
}

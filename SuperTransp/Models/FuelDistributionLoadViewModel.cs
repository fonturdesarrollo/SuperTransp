namespace SuperTransp.Models
{
	public class FuelDistributionLoadViewModel
	{
		public int FuelDistributionId { get; set; }
		public int StateId { get; set; }
		public DateTime FuelDistributionStartDate { get; set; }
		public string? FuelDistributionRemarks { get; set; }
		public DateTime ProccessDate { get; set; }
		public int MunicipalityId { get; set; }
		public int DistrictId { get; set; }
		public int GasStationId { get; set; }
		public string? GasStationName { get; set; }
		public string? MunicipalityName { get; set; }
		public string? DistrictName { get; set; }
		public int FuelTypeId { get; set; }
		public string? FuelTypeName { get; set; }
		public int TrucksReceived { get; set; }
		public string? TrucksRemarks { get; set; }
		public int LitersReceived { get; set; }
		public int LitersDispatched { get; set; }
		public int UrbanAttended { get; set; }
		public int SubUrbanAttended { get; set; }
		public int TaxiAttended { get; set; }
		public int MotoTaxiAttended { get; set; }
		public string? DayRemarks { get; set; }
	}
}

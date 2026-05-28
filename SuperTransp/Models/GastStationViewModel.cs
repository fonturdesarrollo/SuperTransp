namespace SuperTransp.Models
{
	public class GastStationViewModel
	{
		public int GasStationId { get; set; }
		public int MunicipalityId { get; set; }
		public int StateId { get; set; }
		public string? GasStationName { get; set; }
		public string? GasStationAddress { get; set; }
		public string? StateName { get; set; }
		public string? MunicipalityName { get; set; }
		public int TotalByMunicipality { get; set; }
		public int TotalByPTG { get; set; }
		public int TotalByGasStation { get; set; }
		public int PublicTransportGroupId { get; set; }
		public string GasStationDisplayName => $"{MunicipalityName} - {GasStationName}";
	}
}

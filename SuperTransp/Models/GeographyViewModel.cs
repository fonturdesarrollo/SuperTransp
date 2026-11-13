namespace SuperTransp.Models
{
	public class GeographyViewModel
	{
        public int StateId { get; set; }
        public string? StateName { get; set; }
		public int MunicipalityId { get; set; }
		public string? MunicipalityName { get; set; }
		public int DistrictId { get; set; }
		public string? DistrictName { get; set; }
	}
}

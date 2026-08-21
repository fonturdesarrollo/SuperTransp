namespace SuperTransp.Models
{
	public class ProcedureBankAccountViewModel
	{
		public int ProcedureBankAccountId { get; set; }
		public int BankId { get; set; }
		public int AccounTypeId { get; set; }
		public string? AccounTypeName { get; set; }
		public string? AccountNumber { get; set; }
		public string? CellPhoneNumber { get; set; }
		public string? IdNumber { get; set; }
		public string? BankName { get; set; }
		public string? SudebanCode { get; set; }
	}
}

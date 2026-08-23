namespace SuperTransp.Models
{
	public class ProcedureByDriverViewModel
	{
		public int ProcedureByDriverId { get; set; }
		public int ProcedureId { get; set; }
		public int DriverId { get; set; }
		public int ProcedureBankAccountId { get; set; }
		public int AccounTypeId { get; set; }
		public int BankId { get; set; }
		public string? PayerCellPhoneNumber { get; set; }
		public string? PayerIdNumber { get; set; }
		public string? PayerAccountNumber { get; set; }
		public string? PayerReferenceNumber { get; set; }
		public decimal BCVPaid { get; set; }
		public decimal Tariff { get; set; }
		public decimal Rate { get; set; }
		public int DriverIdentityDocument { get; set; }
		public string? DriverFullName { get; set; }
		public string? DriverPhone { get; set; }
		public string? ReceiverBankAccountTypeName { get; set; }
		public string? ReceiverBankName { get; set; }
		public string? ReceiverBankAccountNumber { get; set; }
		public string? ReceiverBankCellPhone { get; set; }
		public string? ReceiverBankIdNumber { get; set; }
		public string? PayerBankName { get; set; }
		public string? ProcedureStatusName { get; set; }
		public int ProcedureStatusId { get; set; }
		public string? ProcedureConcept { get; set; }
		public string? ProcedureCategoryName { get; set; }
		public string? DriverPTGRif { get; set; }
		public string? DriverPTGName { get; set; }
		public string? DriverVehiclePlate { get; set; }
		public int DriverVehicleYear { get; set; }
		public string? DriverVehicleMake { get; set; }
		public string? DriverVehicleModel { get; set; }
	}
}

namespace SuperTransp.Models
{
	public class ProcedureByPublicTransportGroupViewModel
	{
		public int ProcedureByPublicTransportGroupId { get; set; }
		public int ProcedureId { get; set; }
		public int PublicTransportGroupId { get; set; }
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
		public string? PublicTransportGroupRif { get; set; }
		public string? PublicTransportGroupNameFullName { get; set; }
		public string? RepresentativePhone { get; set; }
		public string? RepresentativeIdentityDocument { get; set; }
		public string? RepresentativeName { get; set; }
		public int Partners { get; set; }
		public int TotalDrivers { get; set; }
		public int TotalSupervisedDrivers { get; set; }
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
	}
}

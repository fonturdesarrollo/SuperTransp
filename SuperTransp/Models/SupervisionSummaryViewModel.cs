using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;

namespace SuperTransp.Models
{
	public class SupervisionSummaryViewModel
	{
		[Key]
		public int SupervisionSummaryId { get; set; }
		public DateTime SupervsionDate { get; set; }
		public int PublicTransportGroupId { get; set; }
		public string? SupervisionAddress { get; set; }
		public string? SupervisionRemarks { get; set; }
		public int SupervisionStatusId { get; set; }
		public int SecurityUserId { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;

namespace SuperTransp.Models
{
	public class SupervisionSummaryViewModel
	{
		[Key]
		public int SupervisionSummaryId { get; set; }
		public DateTime SupervisionDate { get; set; }
		public int PublicTransportGroupId { get; set; }
		public string? PublicTransportGroupRif { get; set; }
		public string? SupervisionAddress { get; set; }
		public string? SupervisionSummaryRemarks { get; set; }
		public int SupervisionStatusId { get; set; }
		public int SecurityUserId { get; set; }
		public string? PTGCompleteName { get; set; }
		public string? StateName { get; set; }
		public int StateId { get; set; }
		public string? ModeName { get; set; }
		public string? UserFullName { get; set; }
		public List<SupervisionSummaryPictures>? Pictures { get; set; }
	}

	public class SupervisionSummaryPictures
	{
		[Key]
		public int SupervisionSummaryPictureId { get; set; }
		public int SupervisionSummaryId { get; set; }
		public string? SupervisionSummaryPictureUrl { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
    public class SecurityUserModel
    {
        [Key]
        public int SecurityUserId { get; set; }		

		[Required(ErrorMessage = "Cedula es requerida")]
		public int? SecurityUserDocumentIdNumber { get; set; }

		[Required(ErrorMessage = "Login es requerido")]
        public string? Login { get; set; }

        public string? Password { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "El grupo es requerido")]
        public int SecurityGroupId { get; set; }
        public string? SecurityGroupName { get; set; }
        public int SecurityStatusId { get; set; }
        public string? SecurityStatusName { get; set; }
		public int StateId { get; set; }
		public string? StateName { get; set; }
		public int SecurityAccessTypeId { get; set; }
		public string? SecurityAccessTypeName { get; set; }
	}

    public class SecurityGroupModel
    {
        public int SecurityGroupId { get; set; }
        public string? SecurityGroupName { get; set; }
        public string? SecurityGroupDescription { get; set; }
    }

	public class SecurityStatusUserModel
	{
		public int SecurityStatusId { get; set; }
		public string? SecurityStatusName { get; set; }
	}
}

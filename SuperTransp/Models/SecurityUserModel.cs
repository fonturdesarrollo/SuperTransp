using System.ComponentModel.DataAnnotations;

namespace SuperTransp.Models
{
    public class SecurityUserModel
    {
        [Key]
        public int SecurityUserId { get; set; }

        [Required(ErrorMessage = "Login es requerido")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "El password es requerido")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "El grupo es requerido")]
        public int SecurityGroupId { get; set; }
        public string? SecurityGroupName { get; set; }

        public int TenantId { get; set; }
        public string? TenantName { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }

    public class EmployeeModel
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Cédula es requerida")]
        public int IdentificationNumber { get; set; }
        public string? EmployeeName { get; set; }
        public int EmployeeTypeId { get; set; }
        public int ManagementId { get; set; }
        public string? ManagementName { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? ExtensionNumber { get; set; }
        public int FloorId { get; set; }
        public int FloorNumber { get; set; }
    }
}

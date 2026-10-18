using System.ComponentModel.DataAnnotations;

namespace EmployeeBase.Shared.DTOs
{
    public class LoginRequestDto
    {
        [Required] public string Login { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
}
